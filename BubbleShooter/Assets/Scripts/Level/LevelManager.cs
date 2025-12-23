using System.Collections;
using UnityEngine;
using System.Collections.Generic;


public class LevelManager : MonoBehaviour
{
    public bool IsSpawning => isSpawning;
    public BossHp BossHp => bossHp;
    public int BubbleShotCount => levelPathData != null ? levelPathData.BubbleShotCount : 0;

    [Header("Path Data")]
    [SerializeField] private LevelPathData levelPathData;

    // Precomputed coordinates from path data
    private IReadOnlyList<HexCoordinate>[] coordinates = null;
    // Bubble chains for animation
    private LinkedList<Bubble>[] bubbleChains = null;
    private BubbleGrid bubbleGrid = null;
    private BossHp bossHp;
    private bool isSpawning = false;

    public void SetBubbleGrid(BubbleGrid bubbleGrid)
    {
        this.bubbleGrid = bubbleGrid;
    }

    /// <summary>
    /// Start dynamic level generation
    /// </summary>
    public void StartGeneration()
    {
        if (isSpawning)
        {
            Debug.LogWarning("[DynamicLevelSpawner] Already spawning!");
            return;
        }

        if (levelPathData == null)
        {
            Debug.LogError("[DynamicLevelSpawner] LevelPathData is null! Assign a LevelPathData asset.");
            return;
        }

        // Init boss hp
        bossHp = new BossHp(levelPathData.BossHpValue);

        // Clear existing bubbles
        bubbleGrid?.ClearAll();

        // Load coordinates from path data
        int pathCount = levelPathData.SpawnPaths.Length;
        coordinates = new IReadOnlyList<HexCoordinate>[pathCount];
        bubbleChains = new LinkedList<Bubble>[pathCount];

        for (int i = 0; i < pathCount; i++)
        {
            coordinates[i] = levelPathData.GetCoordinatesForPath(i);
            bubbleChains[i] = new LinkedList<Bubble>();
        }

        // Start generation coroutine
        StartCoroutine(GenerationCoroutine(levelPathData.MaxSpawnCount));
    }

    /// <summary>
    /// Check if all required bubbles exist in grid and regenerate only missing segments
    /// Generates bubbles only for continuous empty segments along each path
    /// </summary>
    public IEnumerator RegenerateIfNeeded()
    {
        if (levelPathData == null)
        {
            Debug.LogError("[DynamicLevelSpawner] LevelPathData is null!");
            yield break;
        }

        if (isSpawning)
        {
            Debug.LogWarning("[DynamicLevelSpawner] Already spawning, cannot regenerate!");
            yield break;
        }

        // Run all regeneration coroutines in parallel
        List<Coroutine> coroutines = new List<Coroutine>();

        // Start all coroutines in parallel
        foreach (var pathIndex in GetNeedRegeneratePathIndices())
        {
            var coroutine = StartCoroutine(RegenerateMissingSegments(pathIndex));

            coroutines.Add(coroutine);
        }

        // Wait for all coroutines to complete
        for (int i = 0; i < coroutines.Count; i++)
        {
            yield return coroutines[i];
        }
    }

    /// <summary>
    /// Main generation coroutine - Chain push animation
    /// </summary>
    private IEnumerator GenerationCoroutine(int totalPhases)
    {
        isSpawning = true;

        for (int phase = 0; phase < totalPhases; phase++)
        {
            // Step 1: Move all existing bubbles in chains simultaneously (if any exist)
            bool hasAnyBubbles = false;
            for (int i = 0; i < bubbleChains.Length; i++)
            {
                if (bubbleChains[i].Count > 0)
                {
                    hasAnyBubbles = true;
                    break;
                }
            }

            if (hasAnyBubbles)
            {
                yield return AnimateAllChains();
            }

            // Step 2: After movement complete, create new bubbles at start positions
            CreateNewBubblesAtStart();
        }

        // Final step: Place all bubbles in grid
        PlaceAllBubblesInGrid();

        isSpawning = false;
    }

    /// <summary>
    /// Animate all chains simultaneously - all bubbles move one step forward
    /// </summary>
    private IEnumerator AnimateAllChains()
    {
        // Start all animations in parallel
        Coroutine[] animations = new Coroutine[bubbleChains.Length];
        for (int i = 0; i < bubbleChains.Length; i++)
        {
            animations[i] = StartCoroutine(AnimateChain(bubbleChains[i], coordinates[i]));
        }

        // Wait for all to complete
        for (int i = 0; i < animations.Length; i++)
        {
            yield return animations[i];
        }
    }

    /// <summary>
    /// Animate a single chain
    /// </summary>
    private IEnumerator AnimateChain(ICollection<Bubble> chain, IReadOnlyList<HexCoordinate> coords, int startIndex = 0)
    {
        if (chain.Count == 0)
            yield break;

        // Set start and target positions
        Vector3[] startPositions = new Vector3[chain.Count];
        Vector3[] targetPositions = new Vector3[chain.Count];
        int index = 0;
        foreach (var bubble in chain)
        {
            // Next coordinate index
            int targetIndex = startIndex + index + 1;

            // Set start position
            startPositions[index] = bubble.transform.position;

            // Set target position
            targetPositions[index] = targetIndex < coords.Count ?
            bubbleGrid.GetWorldPosition(coords[targetIndex])
            : startPositions[index];

            index++;
        }

        // Animate movement - all bubbles move simultaneously
        float elapsed = 0f;
        float moveSpeed = levelPathData.MoveSpeed;

        while (elapsed < moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveSpeed;

            // Ease-out curve for smooth animation
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);

            // Move all bubbles simultaneously
            index = 0;
            foreach (var bubble in chain)
            {
                bubble.transform.position = Vector3.Lerp(startPositions[index], targetPositions[index], smoothT);

                index++;
            }

            yield return null;
        }

        // Snap to final positions
        index = 0;
        foreach (var bubble in chain)
        {
            bubble.transform.position = targetPositions[index++];
        }
    }

    /// <summary>
    /// Place all bubbles in grid after animation complete
    /// </summary>
    private void PlaceAllBubblesInGrid()
    {
        for (int chainIndex = 0; chainIndex < bubbleChains.Length; chainIndex++)
        {
            // Convert LinkedList to array for indexed access
            Bubble[] bubbles = new Bubble[bubbleChains[chainIndex].Count];
            int index = 0;
            foreach (var bubble in bubbleChains[chainIndex])
            {
                bubbles[index++] = bubble;
            }

            // Place bubbles in grid
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i] != null && i < coordinates[chainIndex].Count)
                {
                    bubbleGrid.PlaceBubble(coordinates[chainIndex][i], bubbles[i]);
                }
            }
        }
    }

    private IEnumerable<int> GetNeedRegeneratePathIndices()
    {
        for (int pathIndex = 0; pathIndex < levelPathData.SpawnPaths.Length; pathIndex++)
        {
            var pathCoords = levelPathData.GetCoordinatesForPath(pathIndex);

            // Find first missing bubble in path
            for (int i = 0; i < pathCoords.Count; i++)
            {
                if (!bubbleGrid.IsOccupied(pathCoords[i]))
                {
                    yield return pathIndex;
                }
            }
        }
    }

    /// <summary>
    /// Regenerate only missing continuous segments for each path
    /// </summary>
    private IEnumerator RegenerateMissingSegments(int pathIndex)
    {
        if (!levelPathData.CheckHasPath(pathIndex))
            yield break;

        // Process each path independently
        var pathCoords = levelPathData.GetCoordinatesForPath(pathIndex);

        // Find missing bubbles
        int firstMissingIndex = -1;
        int lastMissingIndex = -1;
        bool findMissing = false;

        // Determine the size of the path based on the boss's health
        int pathCount = (int)Mathf.Lerp(levelPathData.MinSpawnCount, levelPathData.MaxSpawnCount, BossHp.Rate);
        for (int i = 0; i < pathCount; i++)
        {
            // Find missing bubble
            if (!findMissing)
            {
                if (!bubbleGrid.IsOccupied(pathCoords[i]))
                {
                    firstMissingIndex = i;
                    lastMissingIndex = i;
                    findMissing = true;
                }
            }
            else
            {
                // After find missing bubble, find the bubble
                if (bubbleGrid.IsOccupied(pathCoords[i]))
                    break;

                lastMissingIndex = i;
            }
        }

        // Not find missing bubble
        if (!findMissing) yield break;

        // Build chain: collect existing bubbles from start to firstMissingIndex-1
        LinkedList<Bubble> chain = new();

        for (int i = 0; i < firstMissingIndex; i++)
        {
            Bubble existingBubble = bubbleGrid.GetBubble(pathCoords[i]);
            if (existingBubble != null)
            {
                chain.AddLast(existingBubble);

                // Remove from grid temporarily for animation
                bubbleGrid.RemoveBubble(pathCoords[i]);
            }
        }

        int segmentLength = lastMissingIndex - firstMissingIndex + 1;

        for (int phase = 0; phase < segmentLength; phase++)
        {
            // Step 1: Move all bubbles in chain (existing + newly created) forward
            if (chain.Count > 0)
            {
                yield return AnimateChain(chain, pathCoords, 0);
            }

            // Step 2: Create new bubble at path start position (index 0)
            CreateNewBubbleInList(ref chain, pathIndex);
        }

        // Place bubbles back in grid
        // Only place up to lastMissingIndex (fill the missing segment completely)
        int index = 0;
        int count = Mathf.Min(lastMissingIndex + 1, chain.Count);

        foreach (var bubble in chain)
        {
            if (index >= count)
                break;

            if (bubble != null)
            {
                if (!bubbleGrid.IsOccupied(pathCoords[index]))
                {
                    bubbleGrid.PlaceBubble(pathCoords[index], bubble);
                }
                else
                {
                    bubble.ReturnToPool();
                }
            }

            index++;
        }

        chain.Clear();
    }

    /// <summary>
    /// Create new bubbles at start positions
    /// </summary>
    private void CreateNewBubblesAtStart()
    {
        for (int i = 0; i < coordinates.Length; i++)
        {
            // Get start position (always first coordinate in path)
            HexCoordinate startCoord = coordinates[i][0];
            Vector2 startWorldPos = bubbleGrid.GetWorldPosition(startCoord);

            // Create bubble at start position
            Bubble bubble = CreateBubble(startCoord);
            if (bubble != null)
            {
                bubble.transform.position = startWorldPos;
                bubbleChains[i].AddFirst(bubble); // Insert at beginning of chain
            }
        }
    }

    /// <summary>
    /// Create new bubble at start position
    /// </summary>
    private void CreateNewBubbleInList(ref LinkedList<Bubble> chain, int pathIndex)
    {
        var pathCoords = levelPathData.GetCoordinatesForPath(pathIndex);

        if (pathCoords.Count == 0)
        {
            Debug.LogError($"[LevelManager]: Failed to create new bubble. Path{pathIndex} size is zero.");
            return;
        }

        HexCoordinate startCoord = pathCoords[0];
        Vector2 startWorldPos = bubbleGrid.GetWorldPosition(startCoord);

        Bubble newBubble = CreateBubble(startCoord);

        newBubble.transform.position = startWorldPos;

        if (chain == null)
            chain = new();

        chain.AddFirst(newBubble);
    }

    /// <summary>
    /// Create a bubble with random color
    /// </summary>
    private Bubble CreateBubble(HexCoordinate coord)
    {
        if (!BubblePoolManager.Instance)
        {
            Debug.LogError("[DynamicLevelSpawner] PoolManager is null!");
            return null;
        }

        Bubble bubble = BubblePoolManager.Instance.GetBubble();
        if (bubble != null)
        {
            // Random color
            BubbleColorType randomColorType = (BubbleColorType)Random.Range(0, IntDefine.MAX_BUBBLE_COLOR_COUNT);
            int randomValue = Random.Range(0, 100);
            BubbleType randomType = randomValue switch
            {
                < 60 => BubbleType.None,
                >= 60 and < 80 => BubbleType.Fairy,
                >= 80 and <= 99 => BubbleType.Bomb,
                _ => BubbleType.None,
            };

            bubble.Initialize(randomType, randomColorType, coord);
            bubble.SetActiveCollider(true);
            bubble.gameObject.SetActive(true);
        }

        return bubble;
    }

    /// <summary>
    /// Stop generation if in progress
    /// </summary>
    public void StopGeneration()
    {
        if (isSpawning)
        {
            StopAllCoroutines();
            isSpawning = false;
            Debug.Log("[DynamicLevelSpawner] Generation stopped");
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (bubbleGrid == null || levelPathData == null)
            return;

        // Draw center position
        Gizmos.color = Color.cyan;
        Vector2 centerWorld = bubbleGrid.GetWorldPosition(levelPathData.CenterPosition);
        Gizmos.DrawWireSphere(centerWorld, 0.3f);

        // Draw path preview
        if (levelPathData.SpawnPaths != null)
        {
            for (int pathIndex = 0; pathIndex < levelPathData.SpawnPaths.Length; pathIndex++)
            {
                var coords = levelPathData.GetCoordinatesForPath(pathIndex);

                // Different colors for different paths
                Gizmos.color = pathIndex == 0 ? Color.yellow : Color.green;

                // Draw path as connected spheres
                for (int i = 0; i < coords.Count; i++)
                {
                    Vector2 worldPos = bubbleGrid.GetWorldPosition(coords[i]);
                    Gizmos.DrawWireSphere(worldPos, 0.2f);

                    // Draw connection line to next position
                    if (i < coords.Count - 1)
                    {
                        Vector2 nextWorldPos = bubbleGrid.GetWorldPosition(coords[i + 1]);
                        Gizmos.DrawLine(worldPos, nextWorldPos);
                    }
                }
            }
        }
    }
#endif
}
