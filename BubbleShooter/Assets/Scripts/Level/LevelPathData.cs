using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "LevelPathData", menuName = "BubblePuzzle/Level Path Data", order = 1)]
public class LevelPathData : ScriptableObject
{
    public BubbleSpawnPath[] SpawnPaths => spawnPaths;
    public int MaxSpawnCount => maxSpawnCount;
    public int MinSpawnCount => minSpawnCount;
    public int BubbleShotCount => bubbleShotCount;
    public int BossHpValue => bossHpValue;
    public float MoveSpeed => moveSpeed;
    public HexCoordinate CenterPosition => centerPosition;

    [Header("Spawn Paths")]
    [Tooltip("Array of spawn paths")]
    [SerializeField] private BubbleSpawnPath[] spawnPaths;

    [Header("Spawn count Settings")]
    [Tooltip("Spawn max and min count")]
    [SerializeField] private int maxSpawnCount;
    [SerializeField] private int minSpawnCount;
    [Range(1, 100)]
    [SerializeField] private int bubbleShotCount;

    [Header("HP Settings")]
    [Tooltip("Victory condition")]
    [SerializeField] private int bossHpValue;

    [Header("Animation Settings")]
    [Tooltip("Time in seconds for each bubble movement step")]
    [Range(0.1f, 1.0f)]
    [SerializeField] private float moveSpeed = 0.3f;

    [Header("Grid Reference")]
    [Tooltip("Center position reference (usually 0,0)")]
    [SerializeField] private HexCoordinate centerPosition = new HexCoordinate(0, 0);

    private List<HexCoordinate>[] paths = null;

    public bool CheckHasPath(int pathIndex)
    {
        return spawnPaths != null && pathIndex >= 0 && pathIndex < spawnPaths.Length;
    }

    /// <summary>
    /// Get all coordinates for a specific path
    /// </summary>
    public IReadOnlyList<HexCoordinate> GetCoordinatesForPath(int pathIndex)
    {
        if (!CheckHasPath(pathIndex))
        {
            Debug.LogError($"[LevelPathData] Invalid path index {pathIndex} (total paths: {spawnPaths?.Length ?? 0})");
            return new List<HexCoordinate>();
        }

        if (paths == null)
            paths = new List<HexCoordinate>[spawnPaths.Length];

        if (paths[pathIndex] == null)
            paths[pathIndex] = spawnPaths[pathIndex].GetAllCoordinates();

        return paths[pathIndex];
    }

    /// <summary>
    /// Get coordinate at specific index in a path
    /// </summary>
    public HexCoordinate GetCoordinateAt(int pathIndex, int coordinateIndex)
    {
        if (spawnPaths == null || pathIndex < 0 || pathIndex >= spawnPaths.Length)
        {
            Debug.LogError($"[LevelPathData] Invalid path index {pathIndex}");
            return new HexCoordinate(0, 0);
        }

        return spawnPaths[pathIndex].GetCoordinateAt(coordinateIndex);
    }

    /// <summary>
    /// Get total number of paths
    /// </summary>
    public int GetPathCount()
    {
        return spawnPaths?.Length ?? 0;
    }

    /// <summary>
    /// Get total bubbles in a specific path
    /// </summary>
    public int GetBubbleCountForPath(int pathIndex)
    {
        if (spawnPaths == null || pathIndex < 0 || pathIndex >= spawnPaths.Length)
        {
            return 0;
        }

        return spawnPaths[pathIndex].GetTotalBubbleCount();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        InitializeSpawnCount();
    }

    private void InitializeSpawnCount()
    {
        if (spawnPaths == null || spawnPaths.Length == 0)
            return;

        maxSpawnCount = Mathf.Max(1, maxSpawnCount);
        minSpawnCount = Mathf.Clamp(minSpawnCount, 1, maxSpawnCount);

        for (int i = 0; i < spawnPaths.Length; i++)
        {
            if (spawnPaths[i].directions.Length == maxSpawnCount)
                continue;

            var newDirections = new HexCoordinate.Direction[maxSpawnCount - 1];
            int minCount = Mathf.Min(spawnPaths[i].directions.Length, newDirections.Length);

            System.Array.Copy(spawnPaths[i].directions, newDirections, minCount);

            spawnPaths[i].directions = newDirections;
        }
    }
#endif
}
