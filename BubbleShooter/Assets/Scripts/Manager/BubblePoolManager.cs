using System.Collections.Generic;
using UnityEngine;

public class BubblePoolManager : MonoBehaviour
{
    public static BubblePoolManager Instance { get; private set; }
    public int PoolCount => pool.Count;

    [Header("Pool Settings")]
    [SerializeField] private Bubble bubblePrefab;
    [SerializeField] private int initialPoolSize = 50;
    [SerializeField] private Transform poolParent;

    private Queue<Bubble> pool = new Queue<Bubble>();

#if UNITY_EDITOR
    private int createCount = 0;
#endif

    private void Awake()
    {
        // Singleton pattern
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Create initial pool
    /// </summary>
    public void InitializePool()
    {
        if (bubblePrefab == null)
        {
            Debug.LogError("Bubble prefab not assigned!");
            return;
        }

        if (poolParent == null)
        {
            GameObject poolObj = new GameObject("BubblePool");
            poolParent = poolObj.transform;
            poolParent.SetParent(transform);
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBubble();
        }

        Debug.Log($"Bubble pool initialized with {initialPoolSize} bubbles");
    }

    /// <summary>
    /// Create new bubble and add to pool
    /// </summary>
    private Bubble CreateNewBubble()
    {
        Bubble bubble = Instantiate(bubblePrefab, poolParent);
        bubble.SetEventReturnPool(ReturnBubble);
        bubble.gameObject.SetActive(false);

        pool.Enqueue(bubble);

#if UNITY_EDITOR
        bubble.gameObject.name = $"Bubble {++createCount}";
#endif

        return bubble;
    }

    /// <summary>
    /// Get bubble from pool
    /// </summary>
    public Bubble GetBubble()
    {
        Bubble bubble;

        if (PoolCount > 0)
        {
            bubble = pool.Dequeue();
        }
        else
        {
            Debug.LogWarning("Pool empty, creating new bubble");
            bubble = CreateNewBubble();
        }

        bubble.transform.position = Vector3.zero;
        bubble.transform.rotation = Quaternion.identity;
        bubble.transform.localScale = Vector3.one;

        return bubble;
    }

    /// <summary>
    /// Return bubble to pool
    /// </summary>
    public void ReturnBubble(Bubble bubble)
    {
        if (bubble == null) return;

        bubble.gameObject.SetActive(false);
        pool.Enqueue(bubble);
    }
}
