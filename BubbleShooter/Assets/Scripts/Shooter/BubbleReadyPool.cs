using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleReadyPool : MonoBehaviour
{
    public bool IsReloading { get; private set; }
    private Queue<Bubble> readyBubbles = new Queue<Bubble>();
    private System.Action onEventGetBubble = null;
    private readonly int CIRCLE_RADIUS = 1;

    public void SetEventGetBubble(System.Action onEvent)
    {
        onEventGetBubble = onEvent;
    }

    public void Reload()
    {
        if (!BubblePoolManager.Instance)
            return;

        int currentCount = readyBubbles.Count;
        for (int num = currentCount; num < IntDefine.MAX_READY_POOL_SIZE; num++)
        {
            Bubble bubble = BubblePoolManager.Instance.GetBubble();

            // Initialize bubble with random color (temp)
            BubbleColorType randomType = (BubbleColorType)Random.Range(0, IntDefine.MAX_BUBBLE_COLOR_COUNT);
            BubbleType bubbleType = BubbleType.None;
            HexCoordinate coordinate = new HexCoordinate(0, 0);
            bubble.Initialize(bubbleType, randomType, coordinate);
            bubble.SetActiveCollider(false);
            bubble.transform.position = transform.position;
            bubble.gameObject.SetActive(true);

            readyBubbles.Enqueue(bubble);

            Debug.Log($"Test [BubbleReadyPool] Reload {bubble.name} at {currentCount}, Type: {bubble.ColorType}");
        }

        StartReplace();
    }

    public void Rotate()
    {
        if (readyBubbles.Count == 0)
            return;

        readyBubbles.Enqueue(readyBubbles.Dequeue());
    }

    public void StartReplace()
    {
        StartCoroutine(Replace());
    }

    public IEnumerator Replace()
    {
        if (readyBubbles == null || readyBubbles.Count == 0)
            yield break;

        if (IsReloading)
            yield break;

        IsReloading = true;
        Bubble[] rotateBubbles = new Bubble[readyBubbles.Count];
        float angleStep = 360f / readyBubbles.Count;
        float animationDuration = 1f; // 1초 동안 애니메이션

        // 회전 애니메이션을 진행할 버블 배열 생성 및 시작/목표 위치 저장
        Vector3[] startPositions = new Vector3[rotateBubbles.Length];
        Vector3[] targetPositions = new Vector3[rotateBubbles.Length];

        int index = 0;
        foreach (var bubble in readyBubbles)
        {
            rotateBubbles[index] = bubble;

            // 시작 위치: 현재 위치 (90도 수직 위)
            startPositions[index] = bubble.transform.position;

            // 목표 각도: 90도에서 반시계방향으로 회전 (angleStep만큼 빼기)
            float targetAngle = 90f - (angleStep * index);
            float radians = targetAngle * Mathf.Deg2Rad;

            // 목표 위치 계산
            float x = transform.position.x + CIRCLE_RADIUS * Mathf.Cos(radians);
            float y = transform.position.y + CIRCLE_RADIUS * Mathf.Sin(radians);
            targetPositions[index] = new Vector3(x, y, 0f);

            index++;
        }

        // 애니메이션 실행
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Ease-out 곡선 적용 (부드러운 감속)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // 모든 버블 동시에 이동
            for (int i = 0; i < rotateBubbles.Length; i++)
            {
                if (rotateBubbles[i] != null)
                {
                    rotateBubbles[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothT);
                }
            }

            yield return null;
        }

        // 최종 위치 보정
        for (int i = 0; i < rotateBubbles.Length; i++)
        {
            if (rotateBubbles[i] != null)
            {
                rotateBubbles[i].transform.position = targetPositions[i];
            }
        }

        IsReloading = false;
    }

    /// <summary>
    /// Bring bubble from pooling
    /// </summary>
    public Bubble Get()
    {
        if (readyBubbles == null || readyBubbles.Count == 0)
            return null;

        Bubble bubble = readyBubbles.Dequeue();
        bubble.SetActiveCollider(true);

        onEventGetBubble?.Invoke();

        return bubble;
    }

    /// <summary>
    /// Get current bubble
    /// </summary>
    public Bubble Current()
    {
        if (readyBubbles == null || readyBubbles.Count == 0)
            return null;

        return readyBubbles.Peek();
    }

    /// <summary>
    /// Create LargeBomb at first position with spawn animation
    /// </summary>
    public void CreateLargeBomb(System.Action onComplete = null)
    {
        if (!BubblePoolManager.Instance)
            return;

        StartCoroutine(CreateLargeBombCoroutine(onComplete));
    }

    private IEnumerator CreateLargeBombCoroutine(System.Action onComplete)
    {
        // Create LargeBomb bubble
        Bubble bubble = BubblePoolManager.Instance.GetBubble();

        BubbleColorType randomColor = BubbleColorType.Red;
        BubbleType bubbleType = BubbleType.LargeBomb;
        HexCoordinate coordinate = new HexCoordinate(0, 0);
        bubble.Initialize(bubbleType, randomColor, coordinate);
        bubble.SetActiveCollider(false);

        // Start from center with scale 0
        bubble.transform.position = transform.position;
        bubble.gameObject.SetActive(true);

        // Spawn animation: scale up
        StartCoroutine(StartScaleUpAnimation(bubble));

        // Add to front of queue by creating temporary list
        int count = readyBubbles.Count;
        readyBubbles.Enqueue(bubble);
        for (int i = 0; i < count; i++)
        {
            readyBubbles.Enqueue(readyBubbles.Dequeue());
        }

        yield return Replace();

        onComplete?.Invoke();
    }

    private IEnumerator StartScaleUpAnimation(Bubble bubble)
    {
        float animationDuration = 0.5f;
        float elapsed = 0f;
        bubble.transform.localScale = Vector3.zero;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;

            // Ease-out curve for smooth animation
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            bubble.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smoothT);

            yield return null;
        }

        // Ensure final state
        bubble.transform.localScale = Vector3.one;
    }
}
