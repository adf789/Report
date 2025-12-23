using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Boomerang : SpecialBlock
{
    protected new Vector3 originScale = new Vector3(0.3f, 0.3f, 1f);

    public override void Reset()
    {
        base.Reset();
        transform.localScale = originScale;
        transform.localRotation = Quaternion.identity;
    }

    public override bool Break()
    {
        base.Break();
        Slot someSlot = PuzzleSearch.Instance.SelectSomeSlot();
        StartCoroutine(MoveTo(someSlot));
        return true;
    }

    IEnumerator MoveTo(Slot slot)
    {
        Vector3 targetPos = slot.transform.position;
        while (transform.position != targetPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 0.01f * Time.deltaTime);
            transform.Rotate(Vector3.forward, 60 * Time.deltaTime);

            yield return null;
        };

        PuzzleBreaker.Instance.StartBreakBlockWithOneBlock(slot);
        BlockPooling.Instance.ReturnBlock(this);
        base.Check();
    }
}
