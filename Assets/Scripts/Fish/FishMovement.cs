using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishController))]
public class FishMovement : MonoBehaviour
{
    [Header("Legacy Movement")]
    [SerializeField] private float exitMargin = 0.5f;

    [Header("School Movement")]
    [SerializeField] private float schoolCorrectionSpeed = 1.5f;
    [SerializeField] private float waveAmplitude = 0.4f;
    [SerializeField] private float waveFrequency = 1.5f;

    [Header("Route Movement")]
    [SerializeField] private float routeWaveStrength = 0.35f;

    private Camera mainCamera;
    private FishController fishController;

    // -------------------------
    // Legacy movement
    // -------------------------

    private float schoolCenterY;
    private float personalOffsetY;

    // -------------------------
    // Shared movement
    // -------------------------

    private float wavePhase;

    // -------------------------
    // Route
    // -------------------------

    private FishRoute activeRoute;
    private int routeTargetIndex;

    private float routeLaneOffset;

    private bool wasAttractedByBait;

    // -------------------------
    // Net slow
    // -------------------------

    private readonly Dictionary<NetController, float>
        activeNets = new();

    private float netSpeedMultiplier = 1f;

    // 구버전 NetController 호환용.
    private int legacyNetContactCount;
    private float legacyNetSpeedMultiplier = 1f;

    // -------------------------
    // Special fish
    // -------------------------

    private float specialSpeedMultiplier = 1f;

    public FishRoute ActiveRoute =>
        activeRoute;

    public int RouteTargetIndex =>
        routeTargetIndex;

    public event System.Action<FishMovement>
        DestinationReached;

    private void Awake()
    {
        mainCamera = Camera.main;

        fishController =
            GetComponent<FishController>();
    }

    // =========================================================
    // INITIALIZE
    // =========================================================

    public void InitializeRouteMovement(
        FishRoute route,
        float laneOffset)
    {
        activeRoute =
            route;

        routeTargetIndex = 0;

        routeLaneOffset =
            laneOffset;

        wavePhase =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        wasAttractedByBait = false;

        ResetMovementModifiers();
    }

    // 기존 코드와의 호환을 위해
    // Legacy 이동도 당분간 유지한다.
    public void InitializeSchoolMovement(
        float centerY)
    {
        activeRoute = null;

        schoolCenterY =
            centerY;

        personalOffsetY =
            Random.Range(
                -1.6f,
                1.6f
            );

        wavePhase =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        wasAttractedByBait = false;

        ResetMovementModifiers();
    }

    private void ResetMovementModifiers()
    {
        activeNets.Clear();

        legacyNetContactCount = 0;
        legacyNetSpeedMultiplier = 1f;

        netSpeedMultiplier = 1f;

        specialSpeedMultiplier = 1f;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (fishController.Data == null)
        {
            return;
        }

        if (activeRoute != null)
        {
            MoveAlongRoute();
        }
        else
        {
            MoveLegacy();
            CheckLegacyExit();
        }
    }

    // =========================================================
    // ROUTE MOVEMENT
    // =========================================================

    private void MoveAlongRoute()
    {
        if (activeRoute == null)
        {
            return;
        }

        Vector2 currentPosition =
            transform.position;

        Vector2 targetPosition =
            activeRoute.GetTargetPointWithOffset(
                routeTargetIndex,
                routeLaneOffset
            );

        float distanceToTarget =
            Vector2.Distance(
                currentPosition,
                targetPosition
            );

        if (distanceToTarget <=
            activeRoute.PointReachDistance)
        {
            if (activeRoute.IsDestinationIndex(
                routeTargetIndex))
            {
                ReachDestination();
                return;
            }

            routeTargetIndex++;

            targetPosition =
                activeRoute.GetTargetPointWithOffset(
                    routeTargetIndex,
                    routeLaneOffset
                );
        }

        Vector2 toTarget =
            targetPosition -
            currentPosition;

        if (toTarget.sqrMagnitude <=
            0.0001f)
        {
            return;
        }

        Vector2 routeDirection =
            toTarget.normalized;

        Vector2 perpendicular =
            new Vector2(
                -routeDirection.y,
                routeDirection.x
            );

        float wave =
            Mathf.Sin(
                Time.time *
                waveFrequency +
                wavePhase
            )
            *
            waveAmplitude;

        float lateralAmount =
            wave
            *
            fishController.Data
                .SchoolStrength
            *
            routeWaveStrength;

        Vector2 schoolDirection =
            (
                routeDirection +
                perpendicular *
                lateralAmount
            ).normalized;

        Vector2 finalDirection =
            ApplyBait(
                schoolDirection
            );

        MoveInDirection(
            finalDirection
        );
    }

    private void ReachDestination()
    {
        // 보스 시스템에서 회유 종료를 감지할 수 있도록
        // 비활성화 전에 먼저 이벤트를 발생시킨다.
        DestinationReached?.Invoke(
            this
        );

        // 포획되지 않은 채 Destination에 도착한 물고기는 Miss.
        gameObject.SetActive(
            false
        );
    }

    // =========================================================
    // BAIT
    // =========================================================

    private Vector2 ApplyBait(
        Vector2 baseDirection)
    {
        BaitController bait =
            BaitController.Instance;

        if (bait == null ||
            !bait.IsActive)
        {
            RecoverRouteAfterBait();

            return baseDirection;
        }

        Vector2 toBait =
            bait.Position -
            (Vector2)transform.position;

        float distance =
            toBait.magnitude;

        if (distance >
            bait.AttractionRadius)
        {
            RecoverRouteAfterBait();

            return baseDirection;
        }

        float distanceFactor =
            1f -
            distance /
            bait.AttractionRadius;

        float baitStrength =
            fishController.Data
                .BaitAttraction
            *
            distanceFactor
            *
            1.5f;

        baitStrength =
            Mathf.Clamp01(
                baitStrength
            );

        Vector2 baitDirection =
            toBait.normalized;

        wasAttractedByBait = true;

        return Vector2.Lerp(
            baseDirection,
            baitDirection,
            baitStrength
        ).normalized;
    }

    private void RecoverRouteAfterBait()
    {
        if (!wasAttractedByBait ||
            activeRoute == null)
        {
            return;
        }

        int closestForwardIndex =
            activeRoute
                .GetClosestForwardTargetIndex(
                    transform.position
                );

        if (closestForwardIndex >
            routeTargetIndex)
        {
            routeTargetIndex =
                closestForwardIndex;
        }

        wasAttractedByBait = false;
    }

    // =========================================================
    // LEGACY MOVEMENT
    // =========================================================

    private void MoveLegacy()
    {
        float targetY =
            schoolCenterY
            +
            personalOffsetY
            +
            Mathf.Sin(
                Time.time *
                waveFrequency +
                wavePhase
            )
            *
            waveAmplitude;

        float verticalDifference =
            targetY -
            transform.position.y;

        float schoolStrength =
            fishController.Data
                .SchoolStrength;

        Vector2 schoolDirection =
            new Vector2(
                1f,
                verticalDifference *
                schoolCorrectionSpeed *
                schoolStrength
            ).normalized;

        Vector2 finalDirection =
            ApplyBait(
                schoolDirection
            );

        MoveInDirection(
            finalDirection
        );
    }

    private void CheckLegacyExit()
    {
        if (mainCamera == null)
        {
            return;
        }

        float cameraRight =
            mainCamera.transform.position.x
            +
            mainCamera.orthographicSize
            *
            mainCamera.aspect;

        if (transform.position.x >=
            cameraRight +
            exitMargin)
        {
            gameObject.SetActive(
                false
            );
        }
    }

    // =========================================================
    // MOVEMENT SPEED
    // =========================================================

    private void MoveInDirection(
        Vector2 direction)
    {
        float moveSpeed =
            fishController.Data.MoveSpeed
            *
            netSpeedMultiplier
            *
            specialSpeedMultiplier;

        transform.position +=
            (Vector3)(
                direction *
                moveSpeed *
                Time.deltaTime
            );
    }

    public void SetSpecialSpeedMultiplier(
        float multiplier)
    {
        specialSpeedMultiplier =
            Mathf.Max(
                0f,
                multiplier
            );
    }

    // =========================================================
    // NET
    // =========================================================

    public void EnterNet(
        NetController source,
        float slowMultiplier)
    {
        if (source == null)
        {
            return;
        }

        activeNets[source] =
            Mathf.Clamp(
                slowMultiplier,
                0f,
                1f
            );

        RecalculateNetSpeed();
    }

    public void ExitNet(
        NetController source)
    {
        if (source == null)
        {
            return;
        }

        activeNets.Remove(
            source
        );

        RecalculateNetSpeed();
    }

    // 이전 NetController 버전과의
    // 컴파일 호환을 위해 유지한다.
    public void EnterNet(
        float slowMultiplier)
    {
        legacyNetContactCount++;

        legacyNetSpeedMultiplier =
            Mathf.Min(
                legacyNetSpeedMultiplier,
                Mathf.Clamp(
                    slowMultiplier,
                    0f,
                    1f
                )
            );

        RecalculateNetSpeed();
    }

    public void ExitNet()
    {
        legacyNetContactCount =
            Mathf.Max(
                0,
                legacyNetContactCount - 1
            );

        if (legacyNetContactCount == 0)
        {
            legacyNetSpeedMultiplier = 1f;
        }

        RecalculateNetSpeed();
    }

    private void RecalculateNetSpeed()
    {
        float strongestSlow =
            1f;

        foreach (
            KeyValuePair<NetController, float>
            pair in activeNets)
        {
            if (pair.Key == null)
            {
                continue;
            }

            strongestSlow =
                Mathf.Min(
                    strongestSlow,
                    pair.Value
                );
        }

        if (legacyNetContactCount > 0)
        {
            strongestSlow =
                Mathf.Min(
                    strongestSlow,
                    legacyNetSpeedMultiplier
                );
        }

        netSpeedMultiplier =
            strongestSlow;
    }

    // =========================================================
    // POOL RESET
    // =========================================================

    private void OnDisable()
    {
        activeNets.Clear();

        legacyNetContactCount = 0;
        legacyNetSpeedMultiplier = 1f;

        netSpeedMultiplier = 1f;
        specialSpeedMultiplier = 1f;

        activeRoute = null;
        routeTargetIndex = 0;

        wasAttractedByBait = false;

        DestinationReached = null;
    }
}