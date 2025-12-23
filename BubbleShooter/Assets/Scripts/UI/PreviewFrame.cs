using UnityEngine;
using System.Collections.Generic;

public class PreviewFrame : MonoBehaviour
{
    [SerializeField] private GameObject previewTarget;
    [SerializeField] private Transform bombRendererParent;
    [SerializeField] private Transform largeBombRendererParent;

    private BubbleGrid bubbleGrid;
    private BubbleType previewType;
    private bool isInitialized = false;

    private void Awake()
    {
        Hide();
    }

    private void Start()
    {
        // Pre-generate all renderers on start to avoid runtime lag
        InitializeRenderers();
    }

    public void SetGrid(BubbleGrid bubbleGrid)
    {
        this.bubbleGrid = bubbleGrid;
    }

    /// <summary>
    /// Show frame at hex coordinate
    /// </summary>
    public void ShowAtCoordinate(HexCoordinate coord, float hexSize)
    {
        if (!bubbleGrid)
        {
            Debug.LogError("[PreviewFrame]: Not found grid");
            return;
        }

        Vector2 worldPos = coord.ToWorldPosition(hexSize);

        worldPos.x += bubbleGrid.GridOffset.x;
        worldPos.y += bubbleGrid.GridOffset.y;
        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

        Show();
    }

    /// <summary>
    /// Show frame at world position
    /// </summary>
    public void ShowAtPosition(Vector2 worldPos)
    {
        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        Show();
    }

    /// <summary>
    /// Show frame
    /// </summary>
    public void Show()
    {
        previewTarget.SetActive(true);

        switch (previewType)
        {
            case BubbleType.Bomb:
                bombRendererParent.gameObject.SetActive(true);
                break;

            case BubbleType.LargeBomb:
                bombRendererParent.gameObject.SetActive(true);
                largeBombRendererParent.gameObject.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// Hide frame
    /// </summary>
    public void Hide()
    {
        previewTarget.SetActive(false);
        bombRendererParent.gameObject.SetActive(false);
        largeBombRendererParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Set preview type
    /// </summary>
    public void SetPreviewType(BubbleType type)
    {
        previewType = type;

        // Ensure renderers are initialized
        if (!isInitialized)
        {
            InitializeRenderers();
        }
    }

    /// <summary>
    /// Pre-generate all renderers once to avoid runtime lag
    /// </summary>
    private void InitializeRenderers()
    {
        if (isInitialized)
            return;

        // Create bomb range
        CreateRenderersAtDistance(1, bombRendererParent);

        // Create large bomb range
        CreateRenderersAtDistance(2, largeBombRendererParent);

        isInitialized = true;
    }

    /// <summary>
    /// Create renderer objects at specified hex grid distance
    /// </summary>
    private void CreateRenderersAtDistance(int distance, Transform parent)
    {
        if (previewTarget == null || parent == null)
        {
            Debug.LogError("[PreviewFrame] targetRenderer or parent is null!");
            return;
        }

        if (bubbleGrid == null)
        {
            Debug.LogError("[PreviewFrame] grid is null!");
            return;
        }

        // BFS to find all tiles at specified distance
        HexCoordinate center = new HexCoordinate(0, 0);
        HashSet<HexCoordinate> visited = new HashSet<HexCoordinate>();
        Queue<(HexCoordinate coord, int dist)> queue = new Queue<(HexCoordinate, int)>();

        queue.Enqueue((center, 0));
        visited.Add(center);

        while (queue.Count > 0)
        {
            var (currentCoord, currentDist) = queue.Dequeue();

            foreach (var coord in bubbleGrid.GetNeighborCoordinatesWithSelf(currentCoord, 1, visited))
            {
                int neighborDist = currentDist + 1;
                if (distance == neighborDist)
                {
                    Vector2 worldPos = coord.ToWorldPosition(bubbleGrid.HexSize);
                    GameObject rendererObj = Instantiate(previewTarget, parent);
                    rendererObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0f);
                    rendererObj.SetActive(true);
                }

                if (distance >= neighborDist)
                    queue.Enqueue((coord, neighborDist));
            }
        }
    }
}
