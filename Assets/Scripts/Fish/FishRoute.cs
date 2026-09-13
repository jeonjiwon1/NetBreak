using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class FishRoute : MonoBehaviour
{
    [Header("Route Points")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private List<Transform> waypoints = new();
    [SerializeField] private Transform destinationPoint;

    [Header("Movement")]
    [SerializeField] private float pointReachDistance = 0.4f;

    [Header("Route Width")]
    [SerializeField] private float spawnHalfWidth = 1.2f;
    [SerializeField] private float travelHalfWidth = 0.7f;
    [SerializeField] private float spawnForwardJitter = 0.3f;

    [Header("Route Preview")]
    [SerializeField] private bool showRoutePreview = true;
    [SerializeField] private bool showDuringFishing = true;
    [SerializeField] private float lineWidth = 0.08f;

    [Header("Route Labels")]
    [SerializeField] private bool showRouteLabels = true;
    [SerializeField] private bool showLabelsDuringFishing = true;
    [SerializeField] private string spawnLabel = "유입 지점";
    [SerializeField] private string destinationLabel = "이탈 지점";
    [SerializeField] private Vector2 labelSize = new Vector2(100f, 32f);
    [SerializeField] private float screenEdgeMargin = 12f;

    private LineRenderer lineRenderer;
    private Camera mainCamera;

    private GUIStyle markerStyle;

    public Vector3 SpawnPosition
    {
        get
        {
            if (spawnPoint == null)
            {
                return transform.position;
            }

            return spawnPoint.position;
        }
    }

    public Vector3 DestinationPosition
    {
        get
        {
            if (destinationPoint == null)
            {
                return transform.position;
            }

            return destinationPoint.position;
        }
    }

    public float PointReachDistance =>
        pointReachDistance;

    public int TargetPointCount
    {
        get
        {
            int waypointCount =
                waypoints != null
                    ? waypoints.Count
                    : 0;

            return waypointCount + 1;
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        lineRenderer =
            GetComponent<LineRenderer>();

        ConfigureLineRenderer();
        RefreshRoutePreview();
    }

    private void Start()
    {
        RefreshRoutePreview();
    }

    private void Update()
    {
        UpdatePreviewVisibility();
    }

    // =========================================================
    // ROUTE POINTS
    // =========================================================

    public Vector3 GetTargetPoint(
        int targetIndex)
    {
        if (targetIndex < 0)
        {
            targetIndex = 0;
        }

        if (waypoints != null &&
            targetIndex < waypoints.Count)
        {
            Transform waypoint =
                waypoints[targetIndex];

            if (waypoint != null)
            {
                return waypoint.position;
            }
        }

        return DestinationPosition;
    }

    public Vector3 GetTargetPointWithOffset(
        int targetIndex,
        float laneOffsetNormalized)
    {
        Vector3 targetPoint =
            GetTargetPoint(
                targetIndex
            );

        Vector2 direction =
            GetSegmentDirection(
                targetIndex
            );

        Vector2 perpendicular =
            new Vector2(
                -direction.y,
                direction.x
            );

        return targetPoint +
               (Vector3)(
                   perpendicular *
                   laneOffsetNormalized *
                   travelHalfWidth
               );
    }

    public Vector3 GetSpawnPosition(
        float laneOffsetNormalized)
    {
        Vector2 direction =
            GetFirstSegmentDirection();

        Vector2 perpendicular =
            new Vector2(
                -direction.y,
                direction.x
            );

        float forwardJitter =
            Random.Range(
                -spawnForwardJitter,
                spawnForwardJitter
            );

        Vector3 result =
            SpawnPosition
            +
            (Vector3)(
                perpendicular *
                laneOffsetNormalized *
                spawnHalfWidth
            )
            +
            (Vector3)(
                direction *
                forwardJitter
            );

        return result;
    }

    public bool IsDestinationIndex(
        int targetIndex)
    {
        return targetIndex >=
               TargetPointCount - 1;
    }

    public int GetClosestForwardTargetIndex(
        Vector3 fishPosition)
    {
        float closestDistance =
            float.MaxValue;

        int closestIndex = 0;

        for (int i = 0;
             i < TargetPointCount;
             i++)
        {
            Vector3 point =
                GetTargetPoint(i);

            float distance =
                Vector2.Distance(
                    fishPosition,
                    point
                );

            if (distance <
                closestDistance)
            {
                closestDistance =
                    distance;

                closestIndex = i;
            }
        }

        return closestIndex;
    }

    // =========================================================
    // SEGMENT DIRECTION
    // =========================================================

    private Vector2 GetFirstSegmentDirection()
    {
        Vector2 from =
            SpawnPosition;

        Vector2 to =
            GetTargetPoint(0);

        Vector2 direction =
            to - from;

        if (direction.sqrMagnitude <
            0.0001f)
        {
            return Vector2.right;
        }

        return direction.normalized;
    }

    private Vector2 GetSegmentDirection(
        int targetIndex)
    {
        Vector2 from;

        if (targetIndex <= 0)
        {
            from =
                SpawnPosition;
        }
        else
        {
            from =
                GetTargetPoint(
                    targetIndex - 1
                );
        }

        Vector2 to =
            GetTargetPoint(
                targetIndex
            );

        Vector2 direction =
            to - from;

        if (direction.sqrMagnitude <
            0.0001f)
        {
            return Vector2.right;
        }

        return direction.normalized;
    }

    // =========================================================
    // ROUTE PREVIEW
    // =========================================================

    public void RefreshRoutePreview()
    {
        if (lineRenderer == null)
        {
            lineRenderer =
                GetComponent<LineRenderer>();
        }

        if (lineRenderer == null)
        {
            return;
        }

        ConfigureLineRenderer();

        if (spawnPoint == null ||
            destinationPoint == null)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        List<Vector3> points =
            new();

        points.Add(
            spawnPoint.position
        );

        if (waypoints != null)
        {
            foreach (Transform waypoint
                     in waypoints)
            {
                if (waypoint != null)
                {
                    points.Add(
                        waypoint.position
                    );
                }
            }
        }

        points.Add(
            destinationPoint.position
        );

        lineRenderer.positionCount =
            points.Count;

        for (int i = 0;
             i < points.Count;
             i++)
        {
            lineRenderer.SetPosition(
                i,
                points[i]
            );
        }
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.useWorldSpace = true;

        lineRenderer.startWidth =
            lineWidth;

        lineRenderer.endWidth =
            lineWidth;

        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
    }

    private void UpdatePreviewVisibility()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.enabled =
            ShouldShowRoutePreview();
    }

    private bool ShouldShowRoutePreview()
    {
        if (!showRoutePreview)
        {
            return false;
        }

        if (showDuringFishing)
        {
            return true;
        }

        if (PrototypeGameFlowManager.Instance ==
            null)
        {
            return true;
        }

        return PrototypeGameFlowManager.Instance
            .IsPreparation;
    }

    // =========================================================
    // ROUTE LABELS
    // =========================================================

    private void OnGUI()
    {
        if (!ShouldShowRouteLabels())
        {
            return;
        }

        if (spawnPoint == null ||
            destinationPoint == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }

        EnsureMarkerStyle();

        DrawWorldMarker(
            spawnPoint.position,
            spawnLabel
        );

        DrawWorldMarker(
            destinationPoint.position,
            destinationLabel
        );
    }

    private bool ShouldShowRouteLabels()
    {
        if (!showRouteLabels)
        {
            return false;
        }

        if (showLabelsDuringFishing)
        {
            return true;
        }

        if (PrototypeGameFlowManager.Instance ==
            null)
        {
            return true;
        }

        return PrototypeGameFlowManager.Instance
            .IsPreparation;
    }

    private void DrawWorldMarker(
        Vector3 worldPosition,
        string text)
    {
        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(
                worldPosition
            );

        if (screenPosition.z < 0f)
        {
            return;
        }

        float guiX =
            screenPosition.x -
            labelSize.x * 0.5f;

        float guiY =
            Screen.height -
            screenPosition.y -
            labelSize.y * 0.5f;

        // Spawn / Destination이 카메라 밖에 있어도
        // 플레이어가 화면 가장자리에서 위치를 알 수 있게 한다.
        guiX =
            Mathf.Clamp(
                guiX,
                screenEdgeMargin,
                Screen.width -
                labelSize.x -
                screenEdgeMargin
            );

        guiY =
            Mathf.Clamp(
                guiY,
                screenEdgeMargin,
                Screen.height -
                labelSize.y -
                screenEdgeMargin
            );

        GUI.Box(
            new Rect(
                guiX,
                guiY,
                labelSize.x,
                labelSize.y
            ),
            text,
            markerStyle
        );
    }

    private void EnsureMarkerStyle()
    {
        if (markerStyle != null)
        {
            return;
        }

        markerStyle =
            new GUIStyle(
                GUI.skin.box
            );

        markerStyle.alignment =
            TextAnchor.MiddleCenter;

        markerStyle.fontSize = 14;

        markerStyle.fontStyle =
            FontStyle.Bold;

        markerStyle.normal.textColor =
            Color.white;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void OnValidate()
    {
        pointReachDistance =
            Mathf.Max(
                0.05f,
                pointReachDistance
            );

        lineWidth =
            Mathf.Max(
                0.01f,
                lineWidth
            );

        spawnHalfWidth =
            Mathf.Max(
                0f,
                spawnHalfWidth
            );

        travelHalfWidth =
            Mathf.Max(
                0f,
                travelHalfWidth
            );

        spawnForwardJitter =
            Mathf.Max(
                0f,
                spawnForwardJitter
            );

        labelSize.x =
            Mathf.Max(
                50f,
                labelSize.x
            );

        labelSize.y =
            Mathf.Max(
                20f,
                labelSize.y
            );

        screenEdgeMargin =
            Mathf.Max(
                0f,
                screenEdgeMargin
            );

        if (!Application.isPlaying)
        {
            lineRenderer =
                GetComponent<LineRenderer>();

            RefreshRoutePreview();
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnPoint == null ||
            destinationPoint == null)
        {
            return;
        }

        Vector3 previousPoint =
            spawnPoint.position;

        if (waypoints != null)
        {
            foreach (Transform waypoint
                     in waypoints)
            {
                if (waypoint == null)
                {
                    continue;
                }

                Gizmos.DrawLine(
                    previousPoint,
                    waypoint.position
                );

                previousPoint =
                    waypoint.position;
            }
        }

        Gizmos.DrawLine(
            previousPoint,
            destinationPoint.position
        );
    }
}