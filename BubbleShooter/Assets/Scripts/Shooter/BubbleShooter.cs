using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BubbleShooter : MonoBehaviour
{
    public int RemainShotCount { get; private set; }

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AimGuide aimGuide;
    [SerializeField] private PreviewFrame previewFrame;
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private BubbleReadyPool bubbleReadyPool;
    [SerializeField] private SacrificeBubble sacrificeBubble;

    [Header("UI")]
    [SerializeField] private TextMeshPro remainBubbleText;

    [Header("Touch Area")]
    [SerializeField] private BoxCollider2D shootArea = null;
    [SerializeField] private BoxCollider2D rotateArea = null;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform shooterTransform;
    [Range(1f, 100f)]
    [SerializeField] private float minAimDistance = 1f;
    [Range(1f, 100f)]
    [SerializeField] private float shootSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private bool isShooting;
    private bool isAiming;
    private bool isInput;
    private bool isReadyRotate;
    private bool isLock;
    private Vector2 aimDirection;
    private System.Func<Bubble, IEnumerator> afterShootCoroutine;
    private TrajectoryResult currentTrajectory;

    public void Initialize(int shotCount)
    {
        SetRemainShotCount(shotCount);
        SetLock(false);

        bubbleReadyPool.SetEventGetBubble(() => SetRemainShotCount(RemainShotCount - 1));
        bubbleReadyPool.Reload();
    }

    public void SetEventAfterShootCoroutine(System.Func<Bubble, IEnumerator> coroutine)
    {
        afterShootCoroutine = coroutine;
    }

    public void SetEventSacrificeBubble(System.Action onEvent)
    {
        sacrificeBubble.SetEventSacrificeBubble(onEvent);
    }

    public void SetRemainShotCount(int remainShotCount)
    {
        RemainShotCount = Mathf.Max(0, remainShotCount);

        remainBubbleText.text = RemainShotCount.ToString();
    }

    public void SetLock(bool isLock)
    {
        this.isLock = isLock;
    }

    private void Awake()
    {
        previewFrame.SetGrid(bubbleGrid);
    }

    private void Update()
    {
        if (isLock)
            return;

        if (HandleInput())
        {
            var touchPos = GetTouchPosition();
            bool checkTouchInShoot = CheckTouchInArea(in touchPos, shootArea);
            bool checkTouchInRotate = CheckTouchInArea(in touchPos, rotateArea);

            // 클릭의 경우
            if (!isInput)
            {
                HandleAimingPointDown(checkTouchInShoot);
                HandleRotatePointDown(checkTouchInRotate);
            }
            // 지속 업데이트의 경우
            else
            {
                HandleAimingPointMove(checkTouchInShoot);
                HandleRotatePointMove(checkTouchInRotate);
            }

            isInput = true;
        }
        else if (isInput)
        {
            var touchPos = GetTouchPosition();
            bool checkTouchInShoot = CheckTouchInArea(in touchPos, shootArea);
            bool checkTouchInRotate = CheckTouchInArea(in touchPos, rotateArea);

            HandleAimingPointUp(checkTouchInShoot);
            HandleRotatePointUp(checkTouchInRotate);

            isInput = false;
        }
    }

    private bool HandleInput()
    {
        // Don't allow aiming while shooting
        if (isShooting)
            return false;

        if (bubbleReadyPool.IsReloading)
            return false;

        if (GameManager.Instance.LevelManager.IsSpawning)
            return false;

        // Check if left mouse button is pressed
        bool isMousePressed = Mouse.current.leftButton.isPressed;

        return isMousePressed;
    }

    private void HandleAimingPointDown(bool touchInArea)
    {
        if (touchInArea)
        {
            if (!isAiming)
                StartAiming();
        }
        else
        {
            if (isAiming)
                StopAiming();
        }
    }

    private void HandleAimingPointMove(bool touchInArea)
    {
        if (!isAiming)
            return;

        if (touchInArea)
        {
            UpdateAiming();
        }
        else
        {
            if (isAiming)
                StopAiming();

            HideAimVisuals();
        }
    }

    private void HandleAimingPointUp(bool touchInArea)
    {
        if (!isAiming)
            return;

        if (touchInArea)
            Shoot();

        StopAiming();
    }

    private void HandleRotatePointDown(bool touchInArea)
    {
        if (touchInArea)
            isReadyRotate = true;
    }

    private void HandleRotatePointMove(bool touchInArea)
    {
        if (!touchInArea)
            isReadyRotate = false;
    }

    private void HandleRotatePointUp(bool touchInArea)
    {
        if (isReadyRotate && touchInArea)
        {
            bubbleReadyPool.Rotate();
            bubbleReadyPool.StartReplace();
        }

        isReadyRotate = false;
    }

    /// <summary>
    /// Start aiming mode
    /// </summary>
    private void StartAiming()
    {
        isAiming = true;
    }

    /// <summary>
    /// Update aiming direction and trajectory
    /// </summary>
    private void UpdateAiming()
    {
        if (bubbleReadyPool.Current() == null || aimGuide == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        Vector2 currentBubblePos = bubbleReadyPool.Current().transform.position;

        // Calculate aim direction
        aimDirection = (worldPos - currentBubblePos).normalized;

        // Check minimum distance
        float sqrMagnitude = Vector2.SqrMagnitude(worldPos - currentBubblePos);
        if (sqrMagnitude < minAimDistance)
        {
            HideAimVisuals();
            return;
        }

        // Update aim guide
        currentTrajectory = aimGuide.CalculateTrajectory(currentBubblePos, aimDirection);

        // Update tranjectory
        aimGuide.UpdateTrajectory(in currentTrajectory);

        // Update preview frame
        BubbleType bubbleType = bubbleReadyPool.Current().Type;
        UpdatePreviewFrame(bubbleType, in currentTrajectory);
    }

    /// <summary>
    /// Update hexagonal preview frame position
    /// </summary>
    private void UpdatePreviewFrame(BubbleType previewType, in TrajectoryResult trajectory)
    {
        if (previewFrame == null || bubbleGrid == null)
            return;

        if (!trajectory.HitBubble)
        {
            previewFrame.Hide();
            return;
        }

        // Calculate placement coordinate
        HexCoordinate targetCoord = CalculatePlacementCoordinate(in trajectory);

        previewFrame.SetPreviewType(previewType);
        previewFrame.ShowAtCoordinate(targetCoord, bubbleGrid.HexSize);
    }

    /// <summary>
    /// Calculate placement coordinate based on collision point and trajectory direction
    /// </summary>
    private HexCoordinate CalculatePlacementCoordinate(in TrajectoryResult trajectory)
    {
        Vector2 collisionPoint = trajectory.FinalPosition;
        Vector2 hitBubbleCenter = trajectory.HitInfo.transform.position;

        // Calculate direction from hit bubble to collision point
        Vector2 impactDirection = (collisionPoint - hitBubbleCenter).normalized;

        // Move slightly in the direction of impact to find placement position
        // This ensures we find the empty neighbor closest to the impact point
        float offset = bubbleGrid.HexSize * 0.6f; // 60% of hex size
        Vector2 placementSearchPos = collisionPoint + impactDirection * offset;

        // Get hex coordinate at the search position
        HexCoordinate targetCoord = bubbleGrid.GetHexCoordinate(placementSearchPos);

        return targetCoord;
    }

    /// <summary>
    /// Execute shoot action
    /// </summary>
    private void Shoot()
    {
        Debug.Log("[BubbleShooter] Shoot!");
        if (currentTrajectory.Points == null || currentTrajectory.Points.Length < 2)
        {
            Debug.Log("Invalid trajectory, cannot shoot");
            return;
        }

        StartCoroutine(ShootBubbleCoroutine());
    }

    /// <summary>
    /// Shoot bubble with animation along trajectory
    /// </summary>
    private IEnumerator ShootBubbleCoroutine()
    {
        if (afterShootCoroutine == null)
        {
            Debug.LogError("After shoot coroutine is null.");
            yield break;
        }

        if (RemainShotCount <= 0)
            yield break;

        isShooting = true;

        // Get bubble from pool
        Bubble bubble = bubbleReadyPool?.Get();
        if (bubble == null)
        {
            Debug.LogError("Failed to get bubble from pool!");
            isShooting = false;
            yield break;
        }

        // Check if shooting LargeBomb - reset save controller
        if (bubble.Type == BubbleType.LargeBomb)
        {
            if (sacrificeBubble != null)
            {
                sacrificeBubble.ResetValues();
                Debug.Log("[BubbleShooter] LargeBomb shot - BubbleSaveController reset");
            }
        }

        // Lock sacrifice bubble
        sacrificeBubble.SetLock(true);

        // Animate along trajectory
        yield return LaunchBubbleAlongPath(bubble, currentTrajectory.Points);

        // Calculate placement position using same logic as preview
        HexCoordinate placementCoord = CalculatePlacementCoordinate(in currentTrajectory);

        if (bubbleReadyPool)
            bubbleReadyPool.Reload();

        // Check if valid placement position was found
        if (default(HexCoordinate).Equals(placementCoord))
        {
            Debug.LogWarning("Grid is full - no valid placement position!");
            bubble.ReturnToPool();
            sacrificeBubble.SetLock(false);
            isShooting = false;
            yield break;
        }

        // Place bubble on grid
        bubbleGrid.PlaceBubble(placementCoord, bubble);

        // Process game logic
        yield return afterShootCoroutine(bubble);

        sacrificeBubble.SetLock(false);
        isShooting = false;
    }

    /// <summary>
    /// Launch bubble animation along path
    /// </summary>
    private IEnumerator LaunchBubbleAlongPath(Bubble bubble, Vector2[] pathPoints)
    {
        int pointCount = pathPoints.Length;

        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector2 start = pathPoints[i];
            Vector2 end = pathPoints[i + 1];
            float distance = Vector2.Distance(start, end);
            float duration = distance / shootSpeed;
            float inverseDuration = shootSpeed / distance;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed * inverseDuration;
                bubble.transform.position = Vector2.Lerp(start, end, t);
                yield return null;
            }
        }

        // Snap to final position
        bubble.transform.position = pathPoints[pathPoints.Length - 1];
    }

    /// <summary>
    /// Stop aiming mode
    /// </summary>
    private void StopAiming()
    {
        isAiming = false;
        HideAimVisuals();
    }

    /// <summary>
    /// Hide all aim visuals
    /// </summary>
    private void HideAimVisuals()
    {
        if (aimGuide != null)
            aimGuide.Hide();

        if (previewFrame != null)
            previewFrame.Hide();
    }

    private Vector2 GetTouchPosition()
    {
        var position = Mouse.current.position.ReadValue();

        return mainCamera.ScreenToWorldPoint(position);
    }

    private bool CheckTouchInArea(in Vector2 touchPos, BoxCollider2D area)
    {
        if (!area)
            return false;

        var bounds = area.bounds;

        return bounds.min.x <= touchPos.x
        && bounds.max.x >= touchPos.x
        && bounds.min.y <= touchPos.y
        && bounds.max.y >= touchPos.y;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !isAiming)
            return;

        // Draw trajectory
        if (aimGuide != null && currentTrajectory.Points != null)
        {
            aimGuide.DrawTrajectoryGizmos(currentTrajectory, Color.cyan);
        }

        // Draw shooter position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(shooterTransform.position, 0.3f);

        // Draw aim direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(shooterTransform.position, aimDirection * 2f);
    }
#endif
}
