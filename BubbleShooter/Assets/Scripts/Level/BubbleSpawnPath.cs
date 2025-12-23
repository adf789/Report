using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class BubbleSpawnPath
{
    [Header("Path Configuration")]
    [Tooltip("Initial grid position where the path starts")]
    public HexCoordinate startPosition;

    [Tooltip("Sequence of directions to follow from start position")]
    public HexCoordinate.Direction[] directions;

    /// <summary>
    /// Calculate all coordinates in this path
    /// </summary>
    public List<HexCoordinate> GetAllCoordinates()
    {
        List<HexCoordinate> coordinates = new List<HexCoordinate>();

        // Start with initial position
        HexCoordinate current = startPosition;
        coordinates.Add(current);

        // Apply each direction sequentially
        foreach (var direction in directions)
        {
            current = current.GetNeighbor(direction);
            coordinates.Add(current);
        }

        return coordinates;
    }

    /// <summary>
    /// Get coordinate at specific index
    /// </summary>
    public HexCoordinate GetCoordinateAt(int index)
    {
        if (index == 0)
            return startPosition;

        if (index < 0 || index > directions.Length)
        {
            Debug.LogError($"[BubbleSpawnPath] Index {index} out of range (0 to {directions.Length})");
            return startPosition;
        }

        HexCoordinate current = startPosition;
        for (int i = 0; i < index; i++)
        {
            current = current.GetNeighbor(directions[i]);
        }

        return current;
    }

    /// <summary>
    /// Get total number of bubbles in this path (start + directions)
    /// </summary>
    public int GetTotalBubbleCount()
    {
        return directions.Length + 1; // +1 for start position
    }
}
