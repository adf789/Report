using TMPro;
using UnityEngine;

public class IntroUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI touchText = null;

    [Header("Glow Animation Settings")]
    [SerializeField] private float glowSpeed = 2f; // Speed of glow pulse
    [SerializeField] private float minAlpha = 0.3f; // Minimum alpha (darkest)
    [SerializeField] private float maxAlpha = 1.0f; // Maximum alpha (brightest)

    private System.Action onEventLoadScene = null;
    private float glowTime = 0f;

    void Update()
    {
        // Glowing effect for touch text
        if (touchText != null)
        {
            // Calculate pulsing alpha using sine wave
            glowTime += Time.deltaTime * glowSpeed;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(glowTime) + 1f) * 0.5f);

            // Apply alpha to text color
            Color currentColor = touchText.color;
            currentColor.a = alpha;
            touchText.color = currentColor;
        }
    }

    public void SetEventLoadScene(System.Action onEvent)
    {
        onEventLoadScene = onEvent;
    }

    public void OnClickStartGame()
    {
        onEventLoadScene?.Invoke();

        UnityEngine.SceneManagement.SceneManager.LoadScene($"Scenes/MainScene");
    }
}
