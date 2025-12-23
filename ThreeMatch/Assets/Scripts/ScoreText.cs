using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] private TextMeshPro scoreText;

    private void OnDisable()
    {
        ExtraPooling.Instance.ReturnObj(this);
    }

    private void OnEnable()
    {
        Color tempColor = scoreText.color;
        tempColor.a = 1f;
        scoreText.color = tempColor;
    }

    public void Show(int score, Block.BlockColor color)
    {
        scoreText.text = string.Format("+{0}", score);
        scoreText.outlineColor = CustomColor.GetColor(color);
        gameObject.SetActive(true);
        StartCoroutine(StartAnim());
    }

    IEnumerator StartAnim()
    {
        Color tempColor = scoreText.color;
        Vector3 pos = transform.position;

        while (scoreText.color.a > 0.01f)
        {
            tempColor.a = Mathf.Lerp(tempColor.a, 0f, 2f * Time.deltaTime);
            scoreText.color = tempColor;

            pos.y += Time.deltaTime;
            transform.position = pos;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
