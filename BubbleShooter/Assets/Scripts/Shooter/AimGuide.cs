using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AimGuide : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color reflectionColor = Color.yellow;
    [SerializeField] private Material lineMaterial;

    [Header("Calculate Trajectory Settings")]
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask bubbleLayer;

    private readonly List<Vector2> calculatePoints = new List<Vector2>();

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        SetupLineRenderer();
    }

    private void SetupLineRenderer()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;
        lineRenderer.positionCount = 0;
        lineRenderer.sortingOrder = 10; // Draw on top
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;

        // Hide initially
        Hide();
    }

    /// <summary>
    /// Calculate trajectory with max 1 reflection
    /// </summary>
    public TrajectoryResult CalculateTrajectory(Vector2 origin, Vector2 direction)
    {
        TrajectoryResult result = new TrajectoryResult
        {
            HasReflection = false,
            HitBubble = false
        };

        calculatePoints.Clear();
        calculatePoints.Add(origin);

        // First raycast - check for wall or bubble
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallLayer | bubbleLayer);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                // Hit wall - calculate reflection
                calculatePoints.Add(hit.point);
                result.HasReflection = true;

                // Reflect direction (angle of incidence = angle of reflection)
                Vector2 reflectedDir = Vector2.Reflect(direction, hit.normal).normalized;

                // Second raycast from reflection point (NO MORE REFLECTIONS)
                RaycastHit2D secondHit = Physics2D.Raycast(
                    hit.point + reflectedDir * 0.01f, // Small offset to avoid self-collision
                    reflectedDir,
                    maxDistance,
                    wallLayer | bubbleLayer
                );

                if (secondHit.collider != null)
                {
                    calculatePoints.Add(secondHit.point);
                    result.FinalPosition = secondHit.point;
                    result.HitBubble = secondHit.collider.gameObject.layer == LayerMask.NameToLayer("Bubble");
                    result.HitInfo = secondHit;
                }
                else
                {
                    // No second hit, extend to max distance
                    Vector2 endPoint = hit.point + reflectedDir * maxDistance;
                    calculatePoints.Add(endPoint);
                    result.FinalPosition = endPoint;
                }
            }
            else
            {
                // Hit bubble directly (no reflection)
                calculatePoints.Add(hit.point);
                result.FinalPosition = hit.point;
                result.HitBubble = true;
                result.HitInfo = hit;
            }
        }
        else
        {
            // No hit - extend to max distance
            Vector2 endPoint = origin + direction * maxDistance;
            calculatePoints.Add(endPoint);
            result.FinalPosition = endPoint;
        }

        result.Points = calculatePoints.ToArray();
        return result;
    }


    /// <summary>
    /// Update guide line with trajectory points
    /// </summary>
    public void UpdateTrajectory(in TrajectoryResult trajectory)
    {
        if (trajectory.Points == null || trajectory.Points.Length < 2)
        {
            Hide();
            return;
        }

        lineRenderer.positionCount = trajectory.Points.Length;

        for (int i = 0; i < trajectory.Points.Length; i++)
        {
            lineRenderer.SetPosition(i, trajectory.Points[i]);
        }

        // Change color if reflection occurred
        if (trajectory.HasReflection)
        {
            lineRenderer.startColor = reflectionColor;
            lineRenderer.endColor = reflectionColor;
        }
        else
        {
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
        }

        Show();
    }

    /// <summary>
    /// Show guide line
    /// </summary>
    public void Show()
    {
        lineRenderer.enabled = true;
    }

    /// <summary>
    /// Hide guide line
    /// </summary>
    public void Hide()
    {
        lineRenderer.enabled = false;
    }

    /// <summary>
    /// Clear guide line
    /// </summary>
    public void Clear()
    {
        lineRenderer.positionCount = 0;
        Hide();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
        }
    }

    /// <summary>
    /// Visualize trajectory in Scene view (for debugging)
    /// </summary>
    public void DrawTrajectoryGizmos(TrajectoryResult result, Color color)
    {
        if (result.Points == null || result.Points.Length < 2)
            return;

        Gizmos.color = color;

        for (int i = 0; i < result.Points.Length - 1; i++)
        {
            Gizmos.DrawLine(result.Points[i], result.Points[i + 1]);
        }

        // Draw final position
        Gizmos.color = result.HitBubble ? Color.green : Color.red;
        Gizmos.DrawWireSphere(result.FinalPosition, 0.2f);

        // Draw reflection point if exists
        if (result.HasReflection && result.Points.Length > 1)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(result.Points[1], 0.15f);
        }
    }
#endif
}
