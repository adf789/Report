using TMPro;
using UnityEngine;

public class ResultPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI result;
    [SerializeField] private TextMeshProUGUI score;

    private System.Action onEventResetGame = null;

    private const string SCORE_FORMAT = "Score: {0}";

    public void SetEventResetGame(System.Action onEvent)
    {
        onEventResetGame = onEvent;
    }

    public void SetScore(int scoreValue)
    {
        score.text = string.Format(SCORE_FORMAT, scoreValue);
    }

    public void SetResult(bool isWin)
    {
        if (isWin)
        {
            result.text = "WIN";
            result.color = Color.blue;
        }
        else
        {
            result.text = "DEFEAT";
            result.color = Color.red;
        }
    }

    public void OnClickIntro()
    {
        onEventResetGame?.Invoke();
        onEventResetGame = null;
    }
}
