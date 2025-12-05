using TMPro;
using UnityEngine;

public class IntroView : BaseUnit<IntroViewModel>
{
    [SerializeField] private TextMeshProUGUI _touchText;

    [Header("Glow Animation Settings")]
    [SerializeField] private float glowSpeed = 2f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;

    private float glowTime = 0f;

    void Update()
    {
        OnUpdateGlowingText();
    }

    private void OnUpdateGlowingText()
    {
        if (!_touchText)
            return;

        glowTime += Time.deltaTime * glowSpeed;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(glowTime) + 1f) * 0.5f);

        Color currentColor = _touchText.color;
        currentColor.a = alpha;
        _touchText.color = currentColor;
    }
}
