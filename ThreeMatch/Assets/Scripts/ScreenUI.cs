using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenUI : Singleton<ScreenUI>
{
    public GameObject pauseScreen;
    public GameObject resultScreen;

    [SerializeField] private Text leftObstacleCountText;
    [SerializeField] private Text LeftMoveCountText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject victoryText;
    [SerializeField] private GameObject failedText;
    [SerializeField] private TextMeshProUGUI resultScoreText;

    private int leftObstacleCount = 10;
    private int score = 0;
    private int leftMoveCount = 20;

    public int LeftObstacleCount { get => leftObstacleCount; }
    public int Score { get => score; }
    public int LeftMoveCount { get => leftMoveCount; }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        leftObstacleCountText.text = leftObstacleCount.ToString();
        LeftMoveCountText.text = leftMoveCount.ToString();
        scoreText.text = score.ToString();
        pauseScreen.SetActive(false);
        resultScreen.SetActive(false);
    }

    public void MinusObstacleCount()
    {
        leftObstacleCount--;
        if (leftObstacleCount < 0) leftObstacleCount = 0;
        leftObstacleCountText.text = leftObstacleCount.ToString();
    }

    public void MinusMoveCount()
    {
        leftMoveCount--;
        if (leftMoveCount < 0) leftMoveCount = 0;
        LeftMoveCountText.text = leftMoveCount.ToString();
    }

    public void AddScore(int addScore)
    {
        score += addScore;
        scoreText.text = score.ToString();
    }

    public void OnOffClickPause()
    {
        pauseScreen.SetActive(!pauseScreen.activeSelf);
        if (Time.timeScale == 0f) Time.timeScale = 1.0f;
        else Time.timeScale = 0f;
    }

    public void OnClickExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene("StageScene");
        Time.timeScale = 1.0f;
    }

    public void CheckEndForStage()
    {
        if (leftObstacleCount != 0 && leftMoveCount != 0) return;

        resultScreen.SetActive(true);
        if (leftObstacleCount == 0)
        {
            victoryText.SetActive(true);
            failedText.SetActive(false);
            resultScoreText.gameObject.SetActive(true);
            resultScoreText.text = string.Format("Score: {0}", score);
        }
        else if (leftMoveCount == 0)
        {
            victoryText.SetActive(false);
            failedText.SetActive(true);
            resultScoreText.gameObject.SetActive(false);
        }
    }
}
