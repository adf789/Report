using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh 기반 플레이어 이동 제어
/// NavMeshLink를 통해 연결된 맵들을 자동으로 이동
/// </summary>
public class Player : MonoBehaviour
{
    public bool IsNavOn { get; private set; }

    [Header("네비 메쉬 대상")]
    [SerializeField] private NavMeshAgent _agent;

    [Header("경로 렌더링")]
    [SerializeField] private Transform _destinationTarget;
    [SerializeField] private LineRenderer _pathRenderer;

    private NavMeshPath _path;
    private PrintPlayerInfoParam _playerInfoParam;
    private SetDestinationParam _setDestinationParam;
    private LoadMapParam _loadMapParam;

    void Awake()
    {
        _path = new NavMeshPath();
        _playerInfoParam = new PrintPlayerInfoParam();
        _setDestinationParam = new SetDestinationParam();
        _loadMapParam = new LoadMapParam()
        {
            CenterTransform = transform,
            Bounds = default
        };
    }

    void Start()
    {
        InitializeRequestMapLoadRange();
    }

    void Update()
    {
        RenderNavPath();

        NotifyPrintPlayerInfo();
        NotifyLoadMap();
    }

    /// <summary>
    /// 네비게이션 경로 출력
    /// </summary>
    private void RenderNavPath()
    {
        IsNavOn = _agent
        && _agent.isOnNavMesh
        && !_agent.isStopped
        && _agent.pathStatus != NavMeshPathStatus.PathInvalid
        && _agent.velocity != Vector3.zero;

        _destinationTarget.gameObject.SetActive(IsNavOn);
        _pathRenderer.gameObject.SetActive(IsNavOn);

        if (IsNavOn)
        {
            _destinationTarget.position = _setDestinationParam.DestinationPoint;
            _pathRenderer.positionCount = _agent.path.corners.Length;

            for (int i = 0; i < _pathRenderer.positionCount; i++)
            {
                if (i == 0)
                    _pathRenderer.SetPosition(i, transform.position);
                else if (i == _pathRenderer.positionCount - 1)
                    _pathRenderer.SetPosition(i, _setDestinationParam.DestinationPoint);
                else
                    _pathRenderer.SetPosition(i, _agent.path.corners[i]);
            }
        }
    }

    /// <summary>
    /// 플레이어 정보 표기
    /// </summary>
    private void NotifyPrintPlayerInfo()
    {
        _playerInfoParam.MoveState = IsNavOn ? MoveState.Moving : MoveState.Idle;
        _playerInfoParam.CurrentPosition = transform.position;
        _playerInfoParam.DestinationPosition = _agent.destination;

        _playerInfoParam.CurrentPosition.y = 0;
        _playerInfoParam.DestinationPosition.y = 0;

        ObserverManager.Instance.NotifyObserver(_playerInfoParam);
    }

    private void NotifyLoadMap()
    {
        ObserverManager.Instance.NotifyObserver(_loadMapParam);
    }

    /// <summary>
    /// 목적지로 이동
    /// </summary>
    public void SetMove(Vector3 destination)
    {
        if (!_agent)
        {
            Debug.LogError("NavMeshAgent가 할당되지 않았습니다!");
            return;
        }

        if (NavMesh.CalculatePath(_agent.transform.position, destination, _agent.areaMask, _path))
        {
            SetDestination(destination);
        }
        else
        {
            Debug.LogWarning($"목적지 {destination}가 NavMesh 위에 없습니다!");
        }
    }

    /// <summary>
    /// 이동 지점 설정
    /// </summary>
    private void SetDestination(in Vector3 destination)
    {
        // 자동 이동 목표지점 지정
        _agent.SetDestination(destination);

        // 이동 목표지점 수정 시 Notify 전송
        _setDestinationParam.DestinationPoint = destination;
        ObserverManager.Instance.NotifyObserver(_setDestinationParam);
    }

    /// <summary>
    /// 메인 카메라의 뷰 범위에 맞춰 NavMesh Volume 크기를 설정
    /// </summary>
    private void InitializeRequestMapLoadRange()
    {
        if (Camera.main == null)
            return;

        Camera mainCamera = Camera.main;

        // 카메라가 비추는 지면 범위 계산
        Plane groundPlane = new(Vector3.up, Vector3.zero);

        // 카메라 뷰포트의 여러 포인트에서 레이 생성 (더 정확한 범위 계산)
        Vector3[] viewportPoints = new Vector3[]
        {
            new Vector3(0, 0, 0),    // 좌하단
            new Vector3(1, 0, 0),    // 우하단
            new Vector3(0, 1, 0),    // 좌상단
            new Vector3(1, 1, 0),    // 우상단
            new Vector3(0.5f, 0, 0), // 중앙하단 (추가)
            new Vector3(0, 0.5f, 0), // 좌중앙 (추가)
            new Vector3(1, 0.5f, 0), // 우중앙 (추가)
        };

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        int validPointCount = 0;

        // 모든 뷰포트 포인트에서 레이캐스트
        foreach (var viewportPoint in viewportPoints)
        {
            Ray ray = mainCamera.ViewportPointToRay(viewportPoint);

            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                min = Vector3.Min(min, hitPoint);
                max = Vector3.Max(max, hitPoint);
                validPointCount++;
            }
        }

        if (validPointCount == 0)
        {
            Debug.LogWarning("카메라 뷰가 지면과 교차하지 않습니다.");
            return;
        }

        // NavMesh Volume 중심과 크기 설정
        Vector3 center = (min + max) / 2f;
        Vector3 size = max - min;

        // 카메라의 forward 방향을 고려하여 하단 방향으로 추가 여유 공간
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // 카메라가 보는 방향으로 중심을 이동시켜 하단 커버리지 확대
        center += cameraForward;

        // 높이는 임의의 값으로 설정 (필요에 따라 조정)
        size.y = 10f;
        center.y = 0;

        _loadMapParam.Bounds = new Bounds(center, size);
    }
}
