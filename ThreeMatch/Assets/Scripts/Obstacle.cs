using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : Block
{
    protected new Vector3 originScale = new Vector3(1.8f, 1.8f, 1.8f);
    [SerializeField] private int hp;
    [SerializeField] private SpriteRenderer numberRenderer;
    [SerializeField] private SpriteRenderer backRenderer;
    [SerializeField] private Transform spinTransform;
    private bool isDisappearing;
    private const int bonusScore = 200;

    private void Update()
    {
        if (hp < 2) Spin();
    }

    public override void Reset()
    {
        base.Reset();
        transform.localScale = originScale;
        hp = 2;
        spinTransform.localRotation = Quaternion.identity;
        isDisappearing = false;

        Color color = backRenderer.color;
        color.a = 1.0f;
        backRenderer.color = color;
    }

    public override bool Break()
    {
        base.Break();
        hp--;
        if (hp < 1)
        {
            AddBonusScore();
            StartCoroutine(Disappear());
            return true;
        }
        else return false;
    }

    protected override void InitColor()
    {
        return;
    }

    private void Spin()
    {
        spinTransform.Rotate(Vector3.forward, 180 * Time.deltaTime);
    }

    private void AddBonusScore()
    {
        ScoreText scoreText = ExtraPooling.Instance.GetUnUseScoreText();
        scoreText.transform.position = transform.position;
        scoreText.Show(bonusScore, Block.BlockColor.Black);
        ScreenUI.Instance.AddScore(bonusScore);
    }

    IEnumerator Disappear()
    {
        if (isDisappearing) yield break;
        isDisappearing = true;

        Vector3 maxScale = transform.localScale * 2.0f;
        while (transform.localScale != maxScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, maxScale, 10 * Time.deltaTime);
            yield return null;
        }

        Vector3 destination = PuzzleSearch.Instance.DestoryPoint;
        float speed = 10f;
        while(transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, 5 * Time.deltaTime);
            speed += 1.0f;
            yield return null;
        }

        ScreenUI.Instance.MinusObstacleCount();
        if(!PuzzleBreaker.Instance.IsBreaking) ScreenUI.Instance.CheckEndForStage();
        isDisappearing = false;
        BlockPooling.Instance.ReturnBlock(this);
    }
}
