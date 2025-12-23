using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraPooling : Singleton<ExtraPooling>
{
    [SerializeField] private Queue<ScoreText> scoreTexts = new Queue<ScoreText>();
    [SerializeField] private GameObject scoreTextObj;


    private void Start()
    {
        InstantiateDamageText();
    }

    private void InstantiateDamageText()
    {
        for(int i = 0; i < 5; i++)
        {
            GameObject obj = Instantiate(scoreTextObj, transform);
            scoreTexts.Enqueue(obj.GetComponent<ScoreText>());
        }
    }

    public void ReturnObj(ScoreText scoreText)
    {
        scoreTexts.Enqueue(scoreText);
    }

    public ScoreText GetUnUseScoreText()
    {
        if(scoreTexts.Count == 0)
        {
            InstantiateDamageText();
        }

        return scoreTexts.Dequeue();
    }
}
