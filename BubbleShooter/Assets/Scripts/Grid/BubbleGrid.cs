using System.Collections.Generic;
using UnityEngine;
using System;

public class BubbleGrid : MonoBehaviour
{
    public Vector2 GridOffset => gridOrigin;

    [SerializeField] private float hexSize = 0.5f;
    [SerializeField] private Vector2 gridOrigin = Vector2.zero;
    [SerializeField] private Vector2Int maxGridSize = Vector2Int.one;

    private readonly Dictionary<HexCoordinate, Bubble> grid = new Dictionary<HexCoordinate, Bubble>();

    public float HexSize => hexSize;

    void Awake()
    {
        gridOrigin += new Vector2(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// Place bubble at coordinate
    /// </summary>
    public void PlaceBubble(HexCoordinate coord, Bubble bubble)
    {
        if (bubble == null)
        {
            Debug.LogWarning("[BubbleGrid] Attempted to place null bubble!");
            return;
        }

        if (grid.ContainsKey(coord))
        {
            Debug.LogWarning($"[BubbleGrid] Overwriting bubble at {coord}! Old: {grid[coord].ColorType}, New: {bubble.ColorType}");
        }

        grid[coord] = bubble;
        bubble.Coordinate = coord;
        bubble.IsPlaced = true;
        bubble.transform.position = GetWorldPosition(coord);
        bubble.SetActiveCollider(true);

        Debug.Log($"Test [BubbleGrid] Placed {bubble.name} at {coord}, Type: {bubble.ColorType}, Total bubbles: {grid.Count}");
    }

    /// <summary>
    /// Get bubble at coordinate
    /// </summary>
    public Bubble GetBubble(HexCoordinate coord)
    {
        return grid.TryGetValue(coord, out Bubble bubble) ? bubble : null;
    }

    /// <summary>
    /// Remove bubble from grid
    /// </summary>
    public void RemoveBubble(HexCoordinate coord)
    {
        if (grid.ContainsKey(coord))
        {
            var bubble = grid[coord];
            grid.Remove(coord);

            if (bubble != null)
            {
                bubble.IsPlaced = false;
                bubble.Coordinate = default;
                bubble.SetActiveCollider(false);
            }
        }
    }

    /// <summary>
    /// Get all neighbors of a coordinate by depth
    /// </summary>
    public IEnumerable<Bubble> GetNeighbors(HexCoordinate currentCoord, int depth = 1)
    {
        if (depth <= 0)
            yield break;

        HashSet<HexCoordinate> checkSet = new() { currentCoord };

        foreach (var bubble in GetNeighborsWithSelf(currentCoord, depth, checkSet))
        {
            yield return bubble;
        }
    }

    /// <summary>
    /// Get all neighbors of a coordinate recursive
    /// </summary>
    public IEnumerable<Bubble> GetNeighborsWithSelf(HexCoordinate currentCoord, int depth, HashSet<HexCoordinate> checkSet)
    {
        foreach (var coord in GetNeighborCoordinatesWithSelf(currentCoord, depth, checkSet))
        {
            var bubble = GetBubble(coord);

            if (bubble != null)
                yield return bubble;
        }
    }

    /// <summary>
    /// Get all neighbors of a coordinate recursive
    /// </summary>
    public IEnumerable<HexCoordinate> GetNeighborCoordinatesWithSelf(HexCoordinate currentCoord, int depth, HashSet<HexCoordinate> checkSet)
    {
        if (depth <= 0)
            yield break;

        // Return self
        if (!checkSet.Contains(currentCoord))
        {
            checkSet.Add(currentCoord);

            if (CheckInGrid(in currentCoord))
                yield return currentCoord;
        }

        // Return neighbors
        for (HexCoordinate.Direction dir = HexCoordinate.Direction.TopRight;
        Enum.IsDefined(typeof(HexCoordinate.Direction), dir);
        dir++)
        {
            HexCoordinate neighborCoord = currentCoord.GetNeighbor(dir);

            if (!checkSet.Contains(neighborCoord))
            {
                checkSet.Add(neighborCoord);

                if (CheckInGrid(in neighborCoord))
                    yield return neighborCoord;
            }
        }

        if (depth <= 1)
            yield break;

        // Return neighbor children
        for (HexCoordinate.Direction dir = HexCoordinate.Direction.TopRight;
        Enum.IsDefined(typeof(HexCoordinate.Direction), dir);
        dir++)
        {
            HexCoordinate neighborCoord = currentCoord.GetNeighbor(dir);
            foreach (var coord in GetNeighborCoordinatesWithSelf(neighborCoord, depth - 1, checkSet))
            {
                yield return coord;
            }
        }
    }

    private bool CheckInGrid(in HexCoordinate coord)
    {
        return Mathf.Abs(coord.q) <= maxGridSize.x && Mathf.Abs(coord.r) <= maxGridSize.y;
    }

    /// <summary>
    /// Get all bubbles in a specific row
    /// </summary>
    public IEnumerable<Bubble> GetBubblesInRow(int row)
    {
        foreach (var kvp in grid)
        {
            if (kvp.Key.r == row)
            {
                yield return kvp.Value;
            }
        }
    }

    /// <summary>
    /// Get all bubbles in grid
    /// </summary>
    public IEnumerable<Bubble> GetAllBubbles()
    {
        return grid.Values;
    }

    /// <summary>
    /// Check if coordinate is occupied
    /// </summary>
    public bool IsOccupied(HexCoordinate coord)
    {
        return grid.ContainsKey(coord);
    }

    /// <summary>
    /// Clear all bubbles from grid
    /// </summary>
    public void ClearAll()
    {
        foreach (var bubble in grid.Values)
        {
            if (bubble != null)
            {
                DestroyImmediate(bubble.gameObject);
            }
        }
        grid.Clear();
    }

    /// <summary>
    /// Convert hex coordinate to world position
    /// </summary>
    public Vector2 GetWorldPosition(HexCoordinate coord)
    {
        return gridOrigin + coord.ToWorldPosition(hexSize);
    }

    /// <summary>
    /// Convert world position to hex coordinate
    /// </summary>
    public HexCoordinate GetHexCoordinate(Vector2 worldPos)
    {
        Vector2 localPos = worldPos - gridOrigin;
        return HexCoordinate.FromWorldPosition(localPos, hexSize);
    }

    /// <summary>
    /// Get total bubble count
    /// </summary>
    public int GetBubbleCount()
    {
        return grid.Count;
    }

#if UNITY_EDITOR
    [SerializeField] private bool isDrawGrids = true;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (isDrawGrids)
                OnDrawEditorGrid();
            return;
        }

        Gizmos.color = Color.yellow;

        // Draw grid positions and coordinates
        foreach (var kvp in grid)
        {
            Vector2 pos = GetWorldPosition(kvp.Key);
            DrawHexagon(pos, hexSize);

            // Draw coordinate text
            UnityEditor.Handles.Label(
                new Vector3(pos.x, pos.y, 0f),
                kvp.Key.ToString(),
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white },
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }

        // Draw grid origin
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gridOrigin, 0.2f);
    }

    private void OnDrawEditorGrid()
    {
        Gizmos.color = Color.yellow;

        // Draw grid positions and coordinates
        for (int q = -maxGridSize.x; q <= maxGridSize.x; q++)
        {
            for (int r = -maxGridSize.y; r <= maxGridSize.y; r++)
            {
                HexCoordinate coord = new HexCoordinate(q, r);
                Vector2 pos = GetWorldPosition(coord);
                DrawHexagon(pos, hexSize);

                // Draw coordinate text
                UnityEditor.Handles.Label(
                    new Vector3(pos.x, pos.y, 0f),
                    coord.ToString(),
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter
                    }
                );
            }
        }
    }

    private void DrawHexagon(Vector2 center, float size)
    {
        // Pointy-top hexagon: start at 30 degrees
        for (int i = 0; i < 6; i++)
        {
            float angle1 = (60f * i + 30f) * Mathf.Deg2Rad;
            float angle2 = (60f * (i + 1) + 30f) * Mathf.Deg2Rad;

            Vector2 p1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * size;
            Vector2 p2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * size;

            Gizmos.DrawLine(p1, p2);
        }
    }
#endif
}
