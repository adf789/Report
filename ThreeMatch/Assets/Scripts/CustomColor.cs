using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomColor
{
    private CustomColor() { }

    public static Color Purple = new Color(68 / 255f, 0, 183 / 255f);
    public static Color Red = new Color(176 / 255f, 0, 0);
    public static Color Green = Color.green;
    public static Color Yellow = Color.yellow;
    public static Color Orange = new Color(1, 94 / 255f, 0);
    public static Color Blue = Color.blue;
    public static Color Black = Color.black;

    public static Color GetColor(Block.BlockColor blockColor)
    {
        switch (blockColor)
        {
            case Block.BlockColor.Red:
                return Red;
            case Block.BlockColor.Green:
                return Green;
            case Block.BlockColor.Yellow:
                return Yellow;
            case Block.BlockColor.Orange:
                return Orange;
            case Block.BlockColor.Purple:
                return Purple;
            case Block.BlockColor.Blue:
                return Blue;
            case Block.BlockColor.Black:
                return Black;
        }

        return Color.clear;
    }
}
