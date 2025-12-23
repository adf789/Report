using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapLoader : MonoBehaviour
{
    [Header("맵 로드 영역")]
    [SerializeField] private float _loadMapPadding = 0f;
    [SerializeField] private float _unloadMapPadding = 1f;

    private Dictionary<Vector2Int, MapGridData> _allMaps; // 로드 가능한 모든 맵
    private Dictionary<Vector2Int, NavMeshSurface> _loadedMaps; // 현재 로드된 맵
    private Queue<AsyncOperationCallback> _loadingOperationCallbacks; // 맵 로딩 후 콜백 이벤트
    private bool _isInitialized = false;
    private bool _isWaitLoadMap = false;
    private bool _isReadyChangeNavMesh = false;

    private Bounds _mapLoadBounds;
    private Bounds _mapUnloadBounds;
    private PrintEnvironmentInfoParam _environtmentInfoParam;

    void Awake()
    {
        InitializeValues();
    }

    void OnEnable()
    {
        ObserverManager.Instance.AddObserver<LoadMapParam>(OnUpdateLoadMap);
        ObserverManager.Instance.AddObserver<SetDestinationParam>(OnModifyTargetArea);
    }

    void OnDisable()
    {
        ObserverManager.Instance.RemoveObserver<LoadMapParam>(OnUpdateLoadMap);
        ObserverManager.Instance.RemoveObserver<SetDestinationParam>(OnModifyTargetArea);

        _isInitialized = false;
    }

    void Start()
    {
        InitilaizeAllMaps();
    }

    /// <summary>
    /// 기본 값 초기화
    /// </summary>
    private void InitializeValues()
    {
        _allMaps = new Dictionary<Vector2Int, MapGridData>();
        _loadedMaps = new Dictionary<Vector2Int, NavMeshSurface>();
        _loadingOperationCallbacks = new Queue<AsyncOperationCallback>();

        _mapLoadBounds = new Bounds();
        _mapUnloadBounds = new Bounds();
        _environtmentInfoParam = new PrintEnvironmentInfoParam();
    }

    /// <summary>
    /// 모든 맵을 그리드 기반으로 로드
    /// </summary>
    private void InitilaizeAllMaps()
    {
        using (MapRegistry mapRegistry = Resources.Load<MapRegistry>("ScriptableObject/MapRegistry"))
        {
            if (mapRegistry == null || mapRegistry.MapGridConfigs == null)
                return;

            foreach (var mapConfig in mapRegistry.MapGridConfigs)
            {
                _allMaps[mapConfig.Grid] = mapConfig;
            }
        }

        _isInitialized = true;
    }

    /// <summary>
    ///  위치 기반에 의한 맵 로드
    /// </summary>
    private void OnUpdateLoadMap(LoadMapParam param)
    {
        if (!_isInitialized)
            return;

        // 로드 영역 설정
        SetLoadMapRange(in param);

        // 맵 언로드 영역 체크
        CheckUnloadArea();

        // 맵 로드 영역 체크
        CheckLoadArea();

        // 맵 로딩이 완료되면 네비게이션 빌드
        if (_loadingOperationCallbacks.Count > 0)
            StartCoroutine(WaitForMapLoadingCallback());

        // 현재 지형 정보 수정
        OnModifyCurrentArea(param.CenterTransform.position);
    }

    /// <summary>
    /// 언로드 영역을 벗어난 지형이 있는지 체크 후 언로드
    /// </summary>
    private void CheckUnloadArea()
    {
        List<Vector2Int> unloadMaps = null;

        foreach (var loadedMap in _loadedMaps)
        {
            if (!_allMaps.TryGetValue(loadedMap.Key, out var mapData))
                continue;

            Bounds mapRect = mapData.GetBounds();

            // 언로드 영역을 벗어났는지, 언로드가 가능한 지형인지 체크
            if (!IsAABBOverlap(in _mapUnloadBounds, in mapRect)
            && TryUnloadMap(mapData))
            {
                if (unloadMaps == null)
                    unloadMaps = new List<Vector2Int>() { mapData.Grid };
                else
                    unloadMaps.Add(mapData.Grid);
            }
        }

        // 로드된 지형 캐싱에서 삭제
        if (unloadMaps != null && unloadMaps.Count > 0)
        {
            foreach (var unloadMap in unloadMaps)
            {
                _loadedMaps.Remove(unloadMap);
            }
        }
    }

    /// <summary>
    /// 로드 영역 내에 로드되지 않은 지형 로드 시도
    /// </summary>
    private void CheckLoadArea()
    {
        // 맵 로드 영역 체크 (y좌표 무시, 영역 내 그리드만 탐색)
        Vector2Int minGrid = ConvertGrid(new Vector3(_mapLoadBounds.min.x, 0, _mapLoadBounds.min.z));
        Vector2Int maxGrid = ConvertGrid(new Vector3(_mapLoadBounds.max.x, 0, _mapLoadBounds.max.z));

        for (int x = minGrid.x; x <= maxGrid.x; x++)
        {
            for (int y = minGrid.y; y <= maxGrid.y; y++)
            {
                Vector2Int grid = new Vector2Int(x, y);

                if (_allMaps.TryGetValue(grid, out MapGridData mapData))
                {
                    LoadMap(mapData);
                }
            }
        }
    }

    /// <summary>
    /// 위치에 따른 맵 로드
    /// </summary>
    private void LoadMap(MapGridData mapData)
    {
        if (_loadedMaps.ContainsKey(mapData.Grid))
            return;

        var scene = SceneManager.GetSceneByName(mapData.SceneName);

        if (scene.isLoaded)
            return;

        // 같은 그리드가 재로드되지 않게 방지
        _loadedMaps[mapData.Grid] = null;

        // 씬 로드 및 콜백 이벤트 등록
        var asyncOperation = SceneManager.LoadSceneAsync(mapData.SceneName, LoadSceneMode.Additive);
        var asyncOperationCallback = new AsyncOperationCallback(asyncOperation, () => OnAfterLoadMap(mapData));

        _loadingOperationCallbacks.Enqueue(asyncOperationCallback);
    }

    /// <summary>
    /// 로드된 맵 언로드
    /// </summary>
    private bool TryUnloadMap(MapGridData mapData)
    {
        if (!_loadedMaps.ContainsKey(mapData.Grid))
            return false;

        var scene = SceneManager.GetSceneByName(mapData.SceneName);

        if (scene.isLoaded)
            SceneManager.UnloadSceneAsync(mapData.SceneName);

        return true;
    }

    private void OnAfterLoadMap(MapGridData mapData)
    {
        var scene = SceneManager.GetSceneByName(mapData.SceneName);

        if (!scene.isLoaded || scene.rootCount == 0)
            return;

        if (scene.GetRootGameObjects()[0].TryGetComponent(out NavMeshSurface navMeshSurface))
        {
            _loadedMaps[mapData.Grid] = navMeshSurface;
        }
    }

    /// <summary>
    /// 현재 영역 설정
    /// </summary>
    private void OnModifyCurrentArea(Vector3 position)
    {
        // 현재 지형 정보 가져옴
        var mapData = GetMapData(position);
        _environtmentInfoParam.CurrentArea = mapData.GroundName;
        _environtmentInfoParam.CurrentGrid = mapData.Grid;

        // 네비게이션 영역 갱신이 필요한 경우
        if (_isReadyChangeNavMesh
        && _environtmentInfoParam.CurrentArea == _environtmentInfoParam.TargetArea)
        {
            _isReadyChangeNavMesh = false;

            // 영역 갱신
            StartCoroutine(WaitForUpdateNavArea(position));
        }

        ObserverManager.Instance.NotifyObserver(_environtmentInfoParam);
    }

    /// <summary>
    /// 목표 지역 설정
    /// </summary>
    private void OnModifyTargetArea(SetDestinationParam param)
    {
        // 목표 지형 정보 가져옴
        var mapData = GetMapData(param.DestinationPoint);
        _environtmentInfoParam.TargetArea = mapData.GroundName;
        _environtmentInfoParam.TargetGrid = mapData.Grid;

        // 네비게이션 갱신 준비상태로 변경
        _isReadyChangeNavMesh = _environtmentInfoParam.CurrentGrid != _environtmentInfoParam.TargetGrid;

        ObserverManager.Instance.NotifyObserver(_environtmentInfoParam);
    }

    /// <summary>
    /// 네비게이션 영역 갱신
    /// </summary>
    private IEnumerator WaitForUpdateNavArea(Vector3 position)
    {
        foreach (var loadedMap in _loadedMaps)
        {
            // 목표 구역만 활성화
            if (!loadedMap.Value)
                continue;

            var center = loadedMap.Value.transform.position + loadedMap.Value.center;
            var size = loadedMap.Value.size;

            if (IsInside(in center, in size, in position))
                loadedMap.Value.enabled = loadedMap.Key == _environtmentInfoParam.TargetGrid;
        }

        // 한 프레임 대기
        yield return null;

        foreach (var loadedMap in _loadedMaps)
        {
            // 모든 구역 활성화
            if (loadedMap.Value)
                loadedMap.Value.enabled = true;
        }
    }

    /// <summary>
    /// 맵 로드에 필요한 영역 설정
    /// </summary>
    private void SetLoadMapRange(in LoadMapParam param)
    {
        Vector3 center = param.CenterTransform.position + param.Bounds.center;
        Vector3 loadRange = param.Bounds.size + Vector3.one * _loadMapPadding;
        Vector3 unloadRange = loadRange + Vector3.one * _unloadMapPadding;

        // 맵 로드 범위 설정
        _mapLoadBounds.center = center;
        _mapLoadBounds.size = loadRange;

        // 맵 언로드 범위 설정
        _mapUnloadBounds.center = center;
        _mapUnloadBounds.size = unloadRange;
    }

    /// <summary>
    /// 맵 로딩 완료 후 콜백 이벤트 호출
    /// </summary>
    private IEnumerator WaitForMapLoadingCallback()
    {
        if (_isWaitLoadMap)
            yield break;

        _isWaitLoadMap = true;

        // 맵 로딩이 완료되면 네비게이션 빌드
        while (_loadingOperationCallbacks.Count > 0)
        {
            var loadingOperationCallback = _loadingOperationCallbacks.Dequeue();

            // 맵 로드 완료까지 대기
            yield return new WaitUntil(() => loadingOperationCallback.AsyncOperation.isDone);

            // 콜백 호출
            loadingOperationCallback.Callback?.Invoke();
        }

        _isWaitLoadMap = false;
    }

    private MapGridData GetMapData(Vector3 position)
    {
        var grid = ConvertGrid(position);

        return _allMaps.TryGetValue(grid, out var data) ? data : default;
    }

    private Vector2Int ConvertGrid(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x * FloatDefine.INVERSE_MAP_UNIT_SIZE);
        int y = Mathf.FloorToInt(position.z * FloatDefine.INVERSE_MAP_UNIT_SIZE);

        return new Vector2Int(x, y);
    }

    private bool IsAABBOverlap(in Bounds a, in Bounds b)
    {
        if (a.max.x < b.min.x || a.min.x > b.max.x)
        {
            return false;
        }

        if (a.max.y < b.min.y || a.min.y > b.max.y)
        {
            return false;
        }

        if (a.max.z < b.min.z || a.min.z > b.max.z)
        {
            return false;
        }

        return true;
    }

    private bool IsInside(in Vector3 center, in Vector3 size, in Vector3 position)
    {
        Vector3 halfSize = size * 0.5f;

        return position.x >= center.x - halfSize.x && position.x <= center.x + halfSize.x
                     && position.y >= center.y - halfSize.y && position.y <= center.y + halfSize.y
                     && position.z >= center.z - halfSize.z && position.z <= center.z + halfSize.z;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_mapLoadBounds.center, _mapLoadBounds.size);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_mapUnloadBounds.center, _mapUnloadBounds.size);
    }
#endif
}
