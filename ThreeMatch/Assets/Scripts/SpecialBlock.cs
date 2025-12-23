using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialBlock : Block
{
    public override bool Break()
    {
        PuzzleBreaker.Instance.waitingSpecialBlockCount++;
        return true;
    }

    protected void Check() { PuzzleBreaker.Instance.waitingSpecialBlockCount--; }
}
