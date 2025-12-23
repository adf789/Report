
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MapRegistry", menuName = "Scriptable Objects/MapRegistry")]
public class MapRegistry : ScriptableObject, IDisposable
{
    public MapGridData[] MapGridConfigs => _mapGridConfigs;

    private readonly string PATH = "ScriptableObject/MapRegistry";

    [Header("Map Configuration")]
    [SerializeField] private MapGridData[] _mapGridConfigs; // Inspector에서 설정할 맵 배치 정보

    public void Dispose()
    {

    }

#if UNITY_EDITOR
    [ContextMenu("Preview Map")]
    public async void PreviewMap()
    {
        using (MapRegistry mapRegistry = Resources.Load<MapRegistry>(PATH))
        {
            if (mapRegistry == null || mapRegistry.MapGridConfigs == null)
                return;

            foreach (var mapConfig in mapRegistry.MapGridConfigs)
            {
                string scenePath = $"Assets/{mapConfig.SceneName}.unity";
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);

                if (scene.isLoaded)
                    continue;

                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath,
                UnityEditor.SceneManagement.OpenSceneMode.Additive);
            }
        }
    }

    [ContextMenu("Unload Map")]
    public async void UnloadMaps()
    {
        using (MapRegistry mapRegistry = Resources.Load<MapRegistry>(PATH))
        {
            if (mapRegistry == null || mapRegistry.MapGridConfigs == null)
                return;

            foreach (var mapConfig in mapRegistry.MapGridConfigs)
            {
                string scenePath = $"Assets/{mapConfig.SceneName}.unity";
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);

                if (!scene.isLoaded)
                    continue;

                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }
        }
    }

    [ContextMenu("Save Map Grid")]
    public async void SaveMapGrid()
    {
        using (MapRegistry mapRegistry = Resources.Load<MapRegistry>(PATH))
        {
            if (mapRegistry == null || mapRegistry.MapGridConfigs == null)
                return;

            bool isModify = false;

            for (int i = 0; i < mapRegistry.MapGridConfigs.Length; i++)
            {
                var mapConfig = mapRegistry.MapGridConfigs[i];
                string scenePath = $"Assets/{mapConfig.SceneName}.unity";
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
                bool isSceneLoaded = scene.isLoaded;

                if (!isSceneLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath,
                    UnityEditor.SceneManagement.OpenSceneMode.Additive);

                    scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(mapConfig.SceneName);
                }

                if (scene.isLoaded)
                {
                    GameObject[] rootObjects = scene.GetRootGameObjects();

                    if (rootObjects.Length > 0)
                    {
                        Terrain terrain = rootObjects[0].GetComponent<Terrain>();

                        if (terrain != null)
                        {
                            int x = (int)(terrain.transform.position.x / IntDefine.MAP_UNIT_SIZE);
                            int y = (int)(terrain.transform.position.z / IntDefine.MAP_UNIT_SIZE);

                            mapConfig.Grid = new Vector2Int(x, y);
                            mapRegistry.MapGridConfigs[i] = mapConfig;

                            isModify = true;
                        }
                    }
                }

                if (!isSceneLoaded)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                }
            }

            if (isModify)
            {
                UnityEditor.EditorUtility.SetDirty(mapRegistry);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
    }
#endif
}
