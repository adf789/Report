using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block_Rocket : SpecialBlock
{
    protected new Vector3 originScale = new Vector3(0.3f, 0.3f, 1f);

    public override void Reset()
    {
        base.Reset();
        transform.localScale = originScale;
    }

    public override bool Break()
    {
        base.Break();
        StartCoroutine(Disappear());
        return true;
    }

    IEnumerator Disappear()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            Vector3 scale = originScale;
            while (renderer.color.a > 0.01f)
            {
                color.a = Mathf.Lerp(color.a, 0, 5 * Time.deltaTime);
                scale.x += 2f * Time.deltaTime;
                scale.y += 2f * Time.deltaTime;
                transform.localScale = scale;
                renderer.color = color;
                yield return null;
            }
        };

        transform.localScale = originScale;
        BlockPooling.Instance.ReturnBlock(this);
    }
}
