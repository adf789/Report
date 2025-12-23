using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Bubble : MonoBehaviour
{
    public BubbleColorType ColorType => bubbleColorType;
    public BubbleType Type => bubbleType;
    public HexCoordinate Coordinate
    {
        get => coordinate;
        set => coordinate = value;
    }
    public bool IsPlaced
    {
        get => isPlaced;
        set => isPlaced = value;
    }
    public bool IsBomb => Type == BubbleType.Bomb || Type == BubbleType.LargeBomb;

    [SerializeField] private BubbleColorType bubbleColorType;
    [SerializeField] private BubbleType bubbleType;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private new Collider2D collider;
    [SerializeField] private GameObject[] bubbleStates;

    private System.Action<Bubble> onEventReturnPool;
    private HexCoordinate coordinate;
    private bool isPlaced = false;

    private void Start()
    {
        UpdateVisual();
    }

    /// <summary>
    /// Update bubble visual based on type
    /// </summary>
    private void UpdateVisual()
    {
        UpdateBubbleColor();

        UpdateBubbleState();
    }

    private void UpdateBubbleColor()
    {
        if (spriteRenderer == null) return;

        // Temporary color mapping (will be replaced with sprites)
        Color color = bubbleColorType switch
        {
            BubbleColorType.Red => Color.red,
            BubbleColorType.Blue => Color.blue,
            BubbleColorType.Yellow => Color.yellow,
            _ => Color.white
        };

        spriteRenderer.color = color;
    }

    private void UpdateBubbleState()
    {
        for (BubbleType type = BubbleType.None;
        System.Enum.IsDefined(typeof(BubbleType), type);
        type++)
        {
            int index = (int)type;

            if (bubbleStates.Length <= index)
                break;

            bubbleStates[index].SetActive(type == bubbleType);
        }
    }

    /// <summary>
    /// Initialize bubble with type and coordinate
    /// </summary>
    public void Initialize(BubbleType type, BubbleColorType colorType, HexCoordinate coord)
    {
        SetType(type, colorType);
        Coordinate = coord;
        isPlaced = false;
    }

    /// <summary>
    /// Reset bubble to default state (for pooling)
    /// </summary>
    public void ResetBubble()
    {
        isPlaced = false;
        coordinate = new HexCoordinate(0, 0);
    }

    public void ReturnToPool()
    {
        if (onEventReturnPool != null)
        {
            ResetBubble();

            onEventReturnPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetType(BubbleType type, BubbleColorType colorType)
    {
        bubbleType = type;

        if (bubbleType == BubbleType.LargeBomb)
            bubbleColorType = BubbleColorType.Red;
        else
            bubbleColorType = colorType;

        UpdateVisual();
    }

    public void SetActiveCollider(bool isActive)
    {
        if (collider)
            collider.enabled = isActive;
    }

    public void SetEventReturnPool(System.Action<Bubble> onEvent)
    {
        onEventReturnPool = onEvent;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateVisual();
    }
#endif
}
