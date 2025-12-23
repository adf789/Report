using UnityEngine;


/// <summary>
/// Axial coordinate system for hexagonal grid
/// q = column, r = row
/// </summary>
[System.Serializable]
public struct HexCoordinate
{
    public enum Direction
    {
        TopRight,
        Right,
        BottomRight,
        BottomLeft,
        Left,
        TopLeft
    }

    public int q; // Column
    public int r; // Row

    private static float SQRT_VALUE = Mathf.Sqrt(3f);
    private static float DEVIDE_THREE = 1 / 3f;

    public HexCoordinate(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    // 6 neighbor directions for pointy-top hexagonal grid
    public static readonly HexCoordinate[] Directions = new HexCoordinate[]
    {
            new HexCoordinate(0, 1),   // 0: Top-Right
            new HexCoordinate(1, 0),    // 1: Right
            new HexCoordinate(1, -1),    // 2: Bottom-Right
            new HexCoordinate(0, -1),   // 3: Bottom-Left
            new HexCoordinate(-1, 0),   // 4: Left
            new HexCoordinate(-1, 1)    // 5: Top-Left
    };

    /// <summary>
    /// Get neighbor coordinate in specified direction (0-5)
    /// </summary>
    public HexCoordinate GetNeighbor(Direction direction)
    {
        HexCoordinate d = Directions[(int)direction];
        return new HexCoordinate(q + d.q, r + d.r);
    }

    /// <summary>
    /// Convert hex coordinate to world position (Pointy-Top)
    /// </summary>
    public Vector2 ToWorldPosition(float hexSize)
    {
        // float x = hexSize * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
        // float y = hexSize * (3f / 2f * r);
        float x = hexSize * (SQRT_VALUE * q + SQRT_VALUE * 0.5f * r);
        float y = hexSize * 1.5f * r;
        return new Vector2(x, y);
    }

    /// <summary>
    /// Convert world position to nearest hex coordinate
    /// Inverse of ToWorldPosition for Pointy-Top hexagons
    /// </summary>
    public static HexCoordinate FromWorldPosition(Vector2 worldPos, float hexSize)
    {
        // Inverse formulas for pointy-top hexagon
        // Forward:  x = hexSize * (√3*q + √3/2 * r)
        //           y = hexSize * (3/2 * r)
        //
        // Inverse:  r = (2/3 * y) / hexSize
        //           x/hexSize = √3*q + √3/2 * r
        //           x/hexSize - √3/2 * r = √3*q
        //           q = (x/hexSize - √3/2 * r) / √3
        //           q = x/(hexSize*√3) - r/2
        //           q = (√3/3 * x)/hexSize - r/2

        // float r = 2f / 3f * worldPos.y / hexSize;
        // float q = Mathf.Sqrt(3f) / 3f * worldPos.x / hexSize - r / 2f;
        float inverseHexSize = 1 / hexSize;
        float r = 1.5f * worldPos.y * inverseHexSize;
        float q = SQRT_VALUE * DEVIDE_THREE * worldPos.x * inverseHexSize - r * 0.5f;

        return HexRound(q, r);
    }

    // Round fractional hex coordinates to nearest integer coordinates
    private static HexCoordinate HexRound(float q, float r)
    {
        float s = -q - r;

        int qi = Mathf.RoundToInt(q);
        int ri = Mathf.RoundToInt(r);
        int si = Mathf.RoundToInt(s);

        float qDiff = Mathf.Abs(qi - q);
        float rDiff = Mathf.Abs(ri - r);
        float sDiff = Mathf.Abs(si - s);

        if (qDiff > rDiff && qDiff > sDiff)
        {
            qi = -ri - si;
        }
        else if (rDiff > sDiff)
        {
            ri = -qi - si;
        }

        return new HexCoordinate(qi, ri);
    }

    // Equality operators
    public override bool Equals(object obj)
    {
        if (!(obj is HexCoordinate)) return false;
        HexCoordinate other = (HexCoordinate)obj;
        return q == other.q && r == other.r;
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(q, r);
    }

    public static bool operator ==(HexCoordinate a, HexCoordinate b)
    {
        return a.q == b.q && a.r == b.r;
    }

    public static bool operator !=(HexCoordinate a, HexCoordinate b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"Hex({q}, {r})";
    }
}
