using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionHandler : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float destructionDelay = 0.1f;
    [SerializeField] private float destructionDuration = 0.3f;
    [SerializeField] private AnimationCurve destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Fall Settings")]
    [SerializeField] private float fallDuration = 1.0f;
    [SerializeField] private float fallGravity = 10f;
    [SerializeField] private float fallRotationSpeed = 360f;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // Prefab for flying projectile
    [SerializeField] private Transform projectileTarget; // Target position (e.g., boss position)
    [SerializeField] private float projectileInitialSpeed = 5f;
    [SerializeField] private float projectileAcceleration = 10f;
    [SerializeField] private float projectileMaxSpeed = 30f;
    [SerializeField] private float projectileRotationSpeed = 720f;

    [Header("References")]
    [SerializeField] private BubbleGrid grid;

    private System.Action onEventDamagedBoss = null;

    public void SetEventDamagedBoss(System.Action onEvent)
    {
        onEventDamagedBoss = onEvent;
    }

    /// <summary>
    /// Destroy bubbles with animation
    /// </summary>
    public IEnumerator DestroyBubbles(ICollection<Bubble> bubbles)
    {
        if (bubbles == null || bubbles.Count == 0)
        {
            Debug.Log("[DestructionHandler] No bubbles to destroy");
            yield break;
        }

        // Remove from grid first
        int damageCount = 0;
        foreach (Bubble bubble in bubbles)
        {
            if (bubble == null)
            {
                Debug.LogWarning("[DestructionHandler] Null bubble in destruction list!");
                continue;
            }

            grid?.RemoveBubble(bubble.Coordinate);

            if (bubble.Type == BubbleType.Fairy)
            {
                damageCount++;
                SpawnProjectile(bubble.transform.position, onEventDamagedBoss);
            }

            StartCoroutine(DestructionAnimation(bubble));
        }

        // Wait for delay between bubbles
        yield return new WaitForSeconds(destructionDelay * bubbles.Count);
    }

    /// <summary>
    /// Spawn a projectile at bubble position that flies to target
    /// </summary>
    private void SpawnProjectile(Vector3 startPosition, System.Action onEventFinish = null)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[DestructionHandler] Projectile prefab is null!");
            return;
        }

        if (projectileTarget == null)
        {
            Debug.LogWarning("[DestructionHandler] Projectile target is null!");
            return;
        }

        // Instantiate projectile
        GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);

        // Start flying animation
        StartCoroutine(ProjectileFlightAnimation(projectile, startPosition, onEventFinish));
    }

    /// <summary>
    /// Projectile flight animation with acceleration like a missile
    /// </summary>
    private IEnumerator ProjectileFlightAnimation(GameObject projectile, Vector3 startPosition, System.Action onEventFinish = null)
    {
        if (projectile == null) yield break;

        float currentSpeed = projectileInitialSpeed;
        Vector3 currentPosition = startPosition;

        while (projectile != null && projectileTarget != null)
        {
            // Calculate direction to target
            Vector3 direction = (projectileTarget.position - currentPosition).normalized;

            // Accelerate
            currentSpeed += projectileAcceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, projectileMaxSpeed);

            // Move towards target
            currentPosition += direction * currentSpeed * Time.deltaTime;
            projectile.transform.position = currentPosition;

            // Rotate towards direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

            // Add spinning effect
            projectile.transform.Rotate(0, 0, projectileRotationSpeed * Time.deltaTime);

            // Check if reached target
            float distanceToTarget = Vector3.Distance(currentPosition, projectileTarget.position);
            if (distanceToTarget < 0.5f)
            {
                // Reached target - apply effect here if needed
                Debug.Log("[DestructionHandler] Projectile reached target!");

                // Destroy projectile
                DestroyImmediate(projectile);
                onEventFinish?.Invoke();
                yield break;
            }

            yield return null;
        }

        // Cleanup if something went wrong
        if (projectile != null)
        {
            DestroyImmediate(projectile);
            onEventFinish?.Invoke();
        }
    }

    /// <summary>
    /// Single bubble destruction animation
    /// </summary>
    private IEnumerator DestructionAnimation(Bubble bubble)
    {
        if (bubble == null) yield break;

        Vector3 startScale = bubble.transform.localScale;
        float elapsed = 0f;

        while (elapsed < destructionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / destructionDuration;
            float scale = destructionCurve.Evaluate(t);

            bubble.transform.localScale = startScale * scale;

            yield return null;
        }

        bubble.ReturnToPool();
        bubble.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Make bubbles fall with animation
    /// </summary>
    public IEnumerator MakeBubblesFall(List<Bubble> bubbles, System.Action onEventFinished = null)
    {
        if (bubbles == null || bubbles.Count == 0)
        {
            Debug.Log("[DestructionHandler] No bubbles to make fall");
            yield break;
        }

        // Remove from grid
        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null && grid != null)
            {
                grid.RemoveBubble(bubble.Coordinate);
            }
            else if (bubble == null)
            {
                Debug.LogWarning("[DestructionHandler] Null bubble in fall list!");
            }
        }

        // Start fall animations
        List<Coroutine> animations = new List<Coroutine>();

        foreach (Bubble bubble in bubbles)
        {
            if (bubble != null)
            {
                animations.Add(StartCoroutine(FallAnimation(bubble)));
            }
        }

        // Wait for fall to complete
        yield return new WaitForSeconds(fallDuration);

        onEventFinished?.Invoke();
    }

    /// <summary>
    /// Single bubble fall animation
    /// </summary>
    private IEnumerator FallAnimation(Bubble bubble)
    {
        if (bubble == null) yield break;

        Vector3 startPos = bubble.transform.position;
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            // Accelerating fall (gravity)
            float fallDistance = 0.5f * fallGravity * elapsed * elapsed;
            bubble.transform.position = startPos + Vector3.down * fallDistance;

            // Rotate while falling
            bubble.transform.Rotate(0, 0, fallRotationSpeed * Time.deltaTime);

            // Check if off-screen
            if (bubble.transform.position.y < -15f)
            {
                break;
            }

            yield return null;
        }

        bubble.ReturnToPool();
    }
}
