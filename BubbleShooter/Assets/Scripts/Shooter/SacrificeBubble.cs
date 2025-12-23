using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SacrificeBubble : MonoBehaviour
{
    [SerializeField] private Transform gauge;
    [SerializeField] private Collider2D touchArea;
    [Range(1, 100)]
    [SerializeField] private int maxSaveCount;
    [SerializeField] private BubbleReadyPool bubbleReadyPool;
    [SerializeField] private float moveAnimationDuration = 0.5f;

    private System.Action onEventSacrificeBubble = null;
    private int saveCount = 0;
    private bool isPointDown = false;
    private bool isLock = false;
    private bool isProcess = false;
    private bool isCreated = false;

    public void SetLock(bool isLock)
    {
        this.isLock = isLock;
    }

    public void SetCreateFlag(bool isCreated)
    {
        this.isCreated = isCreated;
    }

    public void ResetValues()
    {
        saveCount = 0;
        SetCreateFlag(false);

        SetRate();
    }

    public void AddCount(int count)
    {
        if (isLock)
            return;

        saveCount = Mathf.Clamp(saveCount + count, 0, maxSaveCount);

        SetRate();
    }

    public void SetEventSacrificeBubble(System.Action onEvent)
    {
        onEventSacrificeBubble = onEvent;
    }

    void OnDisable()
    {
        isPointDown = false;
    }

    void Start()
    {
        ResetValues();
    }

    void Update()
    {
        OnPointEvent();
    }

    private void OnPointEvent()
    {
        if (GameManager.Instance.LevelManager.IsSpawning)
            return;

        if (CheckPressed())
        {
            if (CheckTouchInArea())
                isPointDown = true;
        }
        else if (isPointDown)
        {
            if (CheckTouchInArea())
                OnClick();

            isPointDown = false;
        }
    }

    private Vector2 GetTouchPosition()
    {
        var position = Mouse.current.position.ReadValue();

        return Camera.main.ScreenToWorldPoint(position);
    }

    private bool CheckPressed()
    {
        // Check if left mouse button is pressed
        bool isMousePressed = Mouse.current.leftButton.isPressed;

        return isMousePressed;
    }

    private bool CheckTouchInArea()
    {
        if (!touchArea)
            return false;

        Vector2 touchPos = GetTouchPosition();
        var bounds = touchArea.bounds;

        return bounds.min.x <= touchPos.x
        && bounds.max.x >= touchPos.x
        && bounds.min.y <= touchPos.y
        && bounds.max.y >= touchPos.y;
    }

    private void OnClick()
    {
        if (isLock || isProcess || isCreated)
            return;

        if (bubbleReadyPool == null)
        {
            Debug.LogError("[BubbleSaveController] BubbleReadyPool is not assigned!");
            return;
        }

        if (bubbleReadyPool.IsReloading)
            return;

        StartCoroutine(OnClickCoroutine());
    }

    private IEnumerator OnClickCoroutine()
    {
        // Step 1: Remove first bubble from ready pool
        Bubble bubble = bubbleReadyPool.Get();
        if (bubble == null)
        {
            Debug.LogWarning("[BubbleSaveController] No bubble to remove from ready pool!");
            yield break;
        }

        isProcess = true;

        // Step 2: Animate bubble to this controller's position
        Vector3 startPosition = bubble.transform.position;
        Vector3 targetPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;

            // Ease-out curve for smooth deceleration
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            bubble.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        // Ensure final position
        bubble.transform.position = targetPosition;

        // Return bubble to pool (it's been saved)
        bubble.ReturnToPool();

        // Reload ready bubble
        bubbleReadyPool.Reload();

        // Step 3: Add count after animation
        AddCount(1);

        // Step 4: Check if max save count reached
        if (saveCount >= maxSaveCount)
        {
            SetCreateFlag(true);

            yield return new WaitUntil(() => !bubbleReadyPool.IsReloading);

            // Create LargeBomb at first position in ready pool
            bubbleReadyPool.CreateLargeBomb();
        }

        onEventSacrificeBubble?.Invoke();

        isProcess = false;
    }

    private void SetRate()
    {
        float rate = (float)saveCount / maxSaveCount;
        rate = Mathf.Clamp01(rate);

        var scale = gauge.localScale;
        scale.x = rate;
        scale.y = rate;
        gauge.localScale = scale;
    }
}
