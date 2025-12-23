using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bossHpText;
    [SerializeField] private Image bossHpBarInnerground;
    [SerializeField] private Image bossHpBar;

    [Header("Settings")]
    [SerializeField] private float hpBarAnimationSpeed = 0.5f;

    // Boss HP bar animation
    private Coroutine hpBarAnimationCoroutine;
    private System.Action onEventWin = null;
    private float bossHpRatio = 1f;

    // Format
    private const string BOSS_HP_FORMAT = "{0} / {1} ({2:F1}%)";
    private const string SCORE_FORMAT = "Score: {0}";

    public void SetBossHp(in BossHp bossHp)
    {
        bossHpRatio = bossHp.Rate;

        bossHpText.text = string.Format(BOSS_HP_FORMAT, bossHp.CurrentHp, bossHp.MaxHp, bossHp.Rate * 100);
        bossHpBar.transform.localScale = new Vector3(bossHpRatio, 1f, 1f);
    }

    public void SetEventWin(System.Action onEvent)
    {
        onEventWin = onEvent;
    }

    public void ResetGame()
    {
        bossHpRatio = 1f;

        UpdateScore(0);
        UpdateBossHp(new BossHp(1));
    }

    public void UpdateScore(int score)
    {
        scoreText.text = string.Format(SCORE_FORMAT, score);
    }

    public void UpdateBossHp(BossHp bossHp)
    {
        SetBossHp(in bossHp);

        if (hpBarAnimationCoroutine != null)
            return;

        hpBarAnimationCoroutine = StartCoroutine(AnimateBossHpBar());
    }

    private System.Collections.IEnumerator AnimateBossHpBar()
    {
        Debug.Log("[GameUI] Start animate boss hp bar");
        if (bossHpBar == null || bossHpBarInnerground == null)
        {
            Debug.LogWarning("[GameUI] Boss HP bar references are null!");
            hpBarAnimationCoroutine = null;
            yield break;
        }

        Vector3 targetRatio = new Vector3(bossHpRatio, 1f, 1f);
        Vector3 currentRatio = bossHpBarInnerground.transform.localScale;

        while (Mathf.Abs(currentRatio.x - bossHpRatio) > 0.001f)
        {
            currentRatio.x = Mathf.Lerp(
                currentRatio.x,
                bossHpRatio,
                Time.deltaTime / hpBarAnimationSpeed
            );

            bossHpBarInnerground.transform.localScale = currentRatio;

            yield return null;
        }

        bossHpBarInnerground.transform.localScale = new Vector3(bossHpRatio, 1f, 1f);
        hpBarAnimationCoroutine = null;

        if (bossHpRatio <= float.Epsilon)
            onEventWin?.Invoke();

        Debug.Log("[GameUI] End animate boss hp bar");
    }
}
