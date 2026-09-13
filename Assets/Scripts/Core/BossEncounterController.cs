using System.Collections;
using UnityEngine;

public class BossEncounterController : MonoBehaviour
{
    public static BossEncounterController Instance
    {
        get;
        private set;
    }

    [Header("Boss Passes")]
    [SerializeField] private int maxPasses = 3;
    [SerializeField] private float betweenPassDelay = 3f;

    [Header("Pass Routes")]
    [SerializeField] private FishRoute firstPassRoute;
    [SerializeField] private FishRoute secondPassRoute;
    [SerializeField] private FishRoute thirdPassRoute;

    [Header("Boss Phases")]
    [Range(0f, 1f)]
    [SerializeField] private float phase2StartRatio = 0.65f;

    [Range(0f, 1f)]
    [SerializeField] private float phase3StartRatio = 0.30f;

    [Header("Resistance Recovery")]
    [Range(0f, 1f)]
    [SerializeField] private float firstEscapeRecoveryRate = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float secondEscapeRecoveryRate = 0.20f;

    [Header("Support Fish")]
    [SerializeField] private bool spawnSupportFish = true;
    [SerializeField] private float supportFirstDelay = 3f;
    [SerializeField] private float phase1SupportInterval = 8f;
    [SerializeField] private float phase2SupportInterval = 6.5f;
    [SerializeField] private float phase3SupportInterval = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private FishSpawner fishSpawner;
    private FishData bossData;

    // 일반 Coast Route.
    // 전용 Boss Route가 비어 있을 때 fallback으로만 사용.
    private FishRoute fallbackRoute;

    private FishRoute currentPassRoute;

    private FishController activeBoss;
    private FishMovement activeBossMovement;
    private BossBehaviorController activeBossBehavior;

    private Coroutine encounterCoroutine;
    private Coroutine supportCoroutine;

    private int currentPass;
    private int currentPhase;

    private float savedResistance;

    private bool isRunning;
    private bool isBetweenPasses;

    private bool passResolved;
    private bool bossCapturedThisPass;

    private float lastResistanceBeforeRecovery;
    private float lastResistanceAfterRecovery;
    private float lastRecoveryAmount;

    private float betweenPassStartTime;

    public bool IsRunning =>
        isRunning;

    public bool IsBetweenPasses =>
        isBetweenPasses;

    public int CurrentPass =>
        currentPass;

    public int MaxPasses =>
        maxPasses;

    public int CurrentPhase =>
        currentPhase;

    public int MaxPhases =>
        3;

    public FishController ActiveBoss =>
        activeBoss;

    public BossBehaviorController ActiveBossBehavior =>
        activeBossBehavior;

    public FishRoute CurrentPassRoute =>
        currentPassRoute;

    public FishRoute NextPassRoute
    {
        get
        {
            if (!isRunning ||
                currentPass >= maxPasses)
            {
                return null;
            }

            return GetRouteForPass(
                currentPass + 1
            );
        }
    }

    public float BetweenPassDelay =>
        betweenPassDelay;

    public float BetweenPassElapsed
    {
        get
        {
            if (!isBetweenPasses)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                Time.time -
                betweenPassStartTime
            );
        }
    }

    public float BetweenPassRemaining
    {
        get
        {
            if (!isBetweenPasses)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                betweenPassDelay -
                BetweenPassElapsed
            );
        }
    }

    public float LastResistanceBeforeRecovery =>
        lastResistanceBeforeRecovery;

    public float LastResistanceAfterRecovery =>
        lastResistanceAfterRecovery;

    public float LastRecoveryAmount =>
        lastRecoveryAmount;

    public bool IsFinalPass =>
        currentPass >= maxPasses;

    public string BossName
    {
        get
        {
            if (bossData == null)
            {
                return "최종 보스";
            }

            return bossData.FishName;
        }
    }

    public float CurrentResistance
    {
        get
        {
            if (activeBoss != null &&
                activeBoss.gameObject.activeInHierarchy)
            {
                return activeBoss.CurrentResistance;
            }

            return savedResistance;
        }
    }

    public float MaxResistance
    {
        get
        {
            if (bossData == null)
            {
                return 0f;
            }

            return bossData.MaxResistance;
        }
    }

    public float ResistanceRatio
    {
        get
        {
            if (MaxResistance <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                CurrentResistance /
                MaxResistance
            );
        }
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // 정상 플레이 중에는 Boss Route 3개를 숨긴다.
        HideAllDedicatedBossRoutes();
    }

    private void Update()
    {
        if (!isRunning ||
            isBetweenPasses ||
            passResolved ||
            activeBoss == null ||
            !activeBoss.gameObject.activeInHierarchy)
        {
            return;
        }

        UpdateBossPhase();
    }

    // =========================================================
    // START
    // =========================================================

    public void BeginEncounter(
        FishSpawner spawner,
        FishData data,
        FishRoute route)
    {
        if (isRunning)
        {
            return;
        }

        if (spawner == null)
        {
            Debug.LogError(
                "BossEncounterController: FishSpawner가 없습니다."
            );

            return;
        }

        if (data == null)
        {
            Debug.LogError(
                "BossEncounterController: Boss FishData가 없습니다."
            );

            return;
        }

        if (data.SpecialType !=
            FishSpecialType.Boss)
        {
            Debug.LogError(
                "BossEncounterController: Boss 타입이 아닌 FishData가 전달되었습니다."
            );

            return;
        }

        fishSpawner =
            spawner;

        bossData =
            data;

        fallbackRoute =
            route;

        if (GetRouteForPass(1) == null)
        {
            Debug.LogError(
                "BossEncounterController: 1차 회유에 사용할 Route가 없습니다."
            );

            return;
        }

        savedResistance =
            bossData.MaxResistance;

        currentPass = 1;
        currentPhase = 1;

        currentPassRoute = null;

        isRunning = true;
        isBetweenPasses = false;

        passResolved = false;
        bossCapturedThisPass = false;

        lastResistanceBeforeRecovery =
            savedResistance;

        lastResistanceAfterRecovery =
            savedResistance;

        lastRecoveryAmount = 0f;

        betweenPassStartTime = 0f;

        ShowOnlyDedicatedBossRoute(
            GetRouteForPass(1)
        );

        if (encounterCoroutine != null)
        {
            StopCoroutine(
                encounterCoroutine
            );
        }

        encounterCoroutine =
            StartCoroutine(
                RunEncounter()
            );
    }

    // =========================================================
    // MAIN LOOP
    // =========================================================

    private IEnumerator RunEncounter()
    {
        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance
                .BeginBossEncounter();
        }

        while (currentPass <= maxPasses)
        {
            isBetweenPasses = false;

            passResolved = false;
            bossCapturedThisPass = false;

            currentPassRoute =
                GetRouteForPass(
                    currentPass
                );

            if (currentPassRoute == null)
            {
                Debug.LogError(
                    $"BossEncounterController: " +
                    $"{currentPass}차 회유 Route가 없습니다."
                );

                FinishEncounter(
                    false
                );

                yield break;
            }

            // 회유 시작 시 현재 경로만 표시.
            ShowOnlyDedicatedBossRoute(
                currentPassRoute
            );

            bool registerSpawn =
                currentPass == 1;

            activeBoss =
                fishSpawner.SpawnBossPass(
                    bossData,
                    currentPassRoute,
                    savedResistance,
                    registerSpawn
                );

            if (activeBoss == null)
            {
                Debug.LogError(
                    "BossEncounterController: 보스 생성에 실패했습니다."
                );

                FinishEncounter(
                    false
                );

                yield break;
            }

            activeBossMovement =
                activeBoss.GetComponent<
                    FishMovement
                >();

            activeBossBehavior =
                activeBoss.GetComponent<
                    BossBehaviorController
                >();

            if (activeBossBehavior != null)
            {
                activeBossBehavior.SetPhase(
                    currentPhase
                );
            }

            activeBoss.Captured +=
                HandleBossCaptured;

            if (activeBossMovement != null)
            {
                activeBossMovement
                    .DestinationReached +=
                    HandleBossDestinationReached;
            }

            StartSupportFishLoop();

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[BOSS] {currentPass}/{maxPasses} 회유 시작 | " +
                    $"Route: {currentPassRoute.name} | " +
                    $"Phase {currentPhase} | " +
                    $"Resistance: {savedResistance:F0} / " +
                    $"{bossData.MaxResistance:F0}"
                );
            }

            yield return new WaitUntil(
                () => passResolved
            );

            StopSupportFishLoop();

            DetachBossEvents();

            activeBoss = null;
            activeBossMovement = null;
            activeBossBehavior = null;

            // =========================================
            // CAPTURE
            // =========================================

            if (bossCapturedThisPass)
            {
                if (showDebugLogs)
                {
                    Debug.Log(
                        $"[BOSS] {bossData.FishName} 포획 성공!"
                    );
                }

                FinishEncounter(
                    true
                );

                yield break;
            }

            // =========================================
            // FINAL PASS FAILURE
            // =========================================

            if (currentPass >= maxPasses)
            {
                if (showDebugLogs)
                {
                    Debug.Log(
                        $"[BOSS] {maxPasses}차 회유 실패. Run 실패."
                    );
                }

                FinishEncounter(
                    false
                );

                yield break;
            }

            // =========================================
            // RESISTANCE RECOVERY
            // =========================================

            float recoveryRate =
                GetRecoveryRate(
                    currentPass
                );

            lastResistanceBeforeRecovery =
                savedResistance;

            savedResistance =
                CalculateRecoveredResistance(
                    savedResistance,
                    recoveryRate
                );

            lastResistanceAfterRecovery =
                savedResistance;

            lastRecoveryAmount =
                Mathf.Max(
                    0f,
                    lastResistanceAfterRecovery -
                    lastResistanceBeforeRecovery
                );

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[BOSS] {currentPass}차 회유 종료 | " +
                    $"{lastResistanceBeforeRecovery:F0} → " +
                    $"{lastResistanceAfterRecovery:F0} 회복 | " +
                    $"Phase {currentPhase} 유지"
                );
            }

            currentPass++;

            FishRoute nextRoute =
                GetRouteForPass(
                    currentPass
                );

            // 도주 직후에는 현재 Route를 없애고
            // 다음 Route만 보여준다.
            ShowOnlyDedicatedBossRoute(
                nextRoute
            );

            if (showDebugLogs &&
                nextRoute != null)
            {
                Debug.Log(
                    $"[BOSS] 다음 회유 경로 예고: " +
                    $"{nextRoute.name}"
                );
            }

            isBetweenPasses = true;

            betweenPassStartTime =
                Time.time;

            yield return new WaitForSeconds(
                betweenPassDelay
            );

            isBetweenPasses = false;
        }
    }

    // =========================================================
    // SUPPORT FISH
    // =========================================================

    private void StartSupportFishLoop()
    {
        StopSupportFishLoop();

        if (!spawnSupportFish ||
            fishSpawner == null ||
            currentPassRoute == null)
        {
            return;
        }

        supportCoroutine =
            StartCoroutine(
                RunSupportFishLoop()
            );
    }

    private void StopSupportFishLoop()
    {
        if (supportCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            supportCoroutine
        );

        supportCoroutine = null;
    }

    private IEnumerator RunSupportFishLoop()
    {
        yield return new WaitForSeconds(
            supportFirstDelay
        );

        while (
            isRunning &&
            !isBetweenPasses &&
            !passResolved &&
            activeBoss != null &&
            activeBoss.gameObject.activeInHierarchy)
        {
            fishSpawner
                .SpawnBossSupportEvent(
                    currentPassRoute,
                    currentPhase
                );

            float interval =
                GetCurrentSupportInterval();

            yield return new WaitForSeconds(
                interval
            );
        }

        supportCoroutine = null;
    }

    private float GetCurrentSupportInterval()
    {
        if (currentPhase >= 3)
        {
            return phase3SupportInterval;
        }

        if (currentPhase == 2)
        {
            return phase2SupportInterval;
        }

        return phase1SupportInterval;
    }

    // =========================================================
    // ROUTE PREVIEW
    // =========================================================

    private void ShowOnlyDedicatedBossRoute(
        FishRoute routeToShow)
    {
        HideAllDedicatedBossRoutes();

        if (!IsDedicatedBossRoute(
            routeToShow
        ))
        {
            return;
        }

        routeToShow.gameObject.SetActive(
            true
        );
    }

    private void HideAllDedicatedBossRoutes()
    {
        SetDedicatedBossRouteActive(
            firstPassRoute,
            false
        );

        SetDedicatedBossRouteActive(
            secondPassRoute,
            false
        );

        SetDedicatedBossRouteActive(
            thirdPassRoute,
            false
        );
    }

    private void SetDedicatedBossRouteActive(
        FishRoute route,
        bool active)
    {
        if (route == null)
        {
            return;
        }

        route.gameObject.SetActive(
            active
        );
    }

    private bool IsDedicatedBossRoute(
        FishRoute route)
    {
        if (route == null)
        {
            return false;
        }

        return
            route == firstPassRoute ||
            route == secondPassRoute ||
            route == thirdPassRoute;
    }

    // =========================================================
    // ROUTE SELECTION
    // =========================================================

    private FishRoute GetRouteForPass(
        int pass)
    {
        switch (pass)
        {
            case 1:
                if (firstPassRoute != null)
                {
                    return firstPassRoute;
                }

                return fallbackRoute;

            case 2:
                if (secondPassRoute != null)
                {
                    return secondPassRoute;
                }

                if (firstPassRoute != null)
                {
                    return firstPassRoute;
                }

                return fallbackRoute;

            case 3:
                if (thirdPassRoute != null)
                {
                    return thirdPassRoute;
                }

                if (secondPassRoute != null)
                {
                    return secondPassRoute;
                }

                if (firstPassRoute != null)
                {
                    return firstPassRoute;
                }

                return fallbackRoute;

            default:
                return fallbackRoute;
        }
    }

    // =========================================================
    // PHASE
    // =========================================================

    private void UpdateBossPhase()
    {
        float ratio =
            activeBoss.ResistanceRatio;

        int targetPhase = 1;

        if (ratio <= phase3StartRatio)
        {
            targetPhase = 3;
        }
        else if (ratio <= phase2StartRatio)
        {
            targetPhase = 2;
        }

        // 한 번 진입한 Phase는 역행하지 않는다.
        if (targetPhase <= currentPhase)
        {
            return;
        }

        currentPhase =
            targetPhase;

        if (activeBossBehavior != null)
        {
            activeBossBehavior.SetPhase(
                currentPhase
            );
        }

        if (showDebugLogs)
        {
            Debug.Log(
                $"[BOSS] Phase {currentPhase} 진입!"
            );
        }
    }

    // =========================================================
    // PASS RESULT
    // =========================================================

    private void HandleBossCaptured(
        FishController fish)
    {
        if (!isRunning ||
            passResolved)
        {
            return;
        }

        savedResistance = 0f;

        bossCapturedThisPass = true;
        passResolved = true;
    }

    private void HandleBossDestinationReached(
        FishMovement movement)
    {
        if (!isRunning ||
            passResolved)
        {
            return;
        }

        if (activeBoss != null)
        {
            savedResistance =
                activeBoss.CurrentResistance;
        }

        bossCapturedThisPass = false;
        passResolved = true;
    }

    // =========================================================
    // RECOVERY
    // =========================================================

    private float GetRecoveryRate(
        int completedPass)
    {
        if (completedPass == 1)
        {
            return firstEscapeRecoveryRate;
        }

        if (completedPass == 2)
        {
            return secondEscapeRecoveryRate;
        }

        return 0f;
    }

    private float CalculateRecoveredResistance(
        float currentResistance,
        float recoveryRate)
    {
        if (bossData == null)
        {
            return currentResistance;
        }

        float maxResistance =
            bossData.MaxResistance;

        float lostResistance =
            Mathf.Max(
                0f,
                maxResistance -
                currentResistance
            );

        float recoveryAmount =
            lostResistance *
            Mathf.Clamp01(
                recoveryRate
            );

        return Mathf.Clamp(
            currentResistance +
            recoveryAmount,
            0f,
            maxResistance
        );
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void FinishEncounter(
        bool success)
    {
        StopSupportFishLoop();

        DetachBossEvents();

        HideAllDedicatedBossRoutes();

        activeBoss = null;
        activeBossMovement = null;
        activeBossBehavior = null;

        currentPassRoute = null;

        isRunning = false;
        isBetweenPasses = false;

        encounterCoroutine = null;

        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance
                .CompleteBossEncounter(
                    success
                );
        }
    }

    private void DetachBossEvents()
    {
        if (activeBoss != null)
        {
            activeBoss.Captured -=
                HandleBossCaptured;
        }

        if (activeBossMovement != null)
        {
            activeBossMovement
                .DestinationReached -=
                HandleBossDestinationReached;
        }
    }

    private void OnDisable()
    {
        StopSupportFishLoop();

        DetachBossEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        maxPasses =
            Mathf.Clamp(
                maxPasses,
                1,
                3
            );

        if (phase3StartRatio >
            phase2StartRatio)
        {
            phase3StartRatio =
                phase2StartRatio;
        }

        supportFirstDelay =
            Mathf.Max(
                0f,
                supportFirstDelay
            );

        phase1SupportInterval =
            Mathf.Max(
                1f,
                phase1SupportInterval
            );

        phase2SupportInterval =
            Mathf.Max(
                1f,
                phase2SupportInterval
            );

        phase3SupportInterval =
            Mathf.Max(
                1f,
                phase3SupportInterval
            );
    }
}