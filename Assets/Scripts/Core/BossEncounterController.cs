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

    [Header("Resistance Recovery")]
    [Range(0f, 1f)]
    [SerializeField] private float firstEscapeRecoveryRate = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float secondEscapeRecoveryRate = 0.20f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private FishSpawner fishSpawner;
    private FishData bossData;
    private FishRoute bossRoute;

    private FishController activeBoss;
    private FishMovement activeBossMovement;

    private Coroutine encounterCoroutine;

    private int currentPass;
    private float savedResistance;

    private bool isRunning;
    private bool isBetweenPasses;

    private bool passResolved;
    private bool bossCapturedThisPass;

    // 마지막 도주/회복 정보를 HUD에 보여주기 위한 값.
    private float lastResistanceBeforeRecovery;
    private float lastResistanceAfterRecovery;
    private float lastRecoveryAmount;

    // 회유 사이 메시지를 표시할 실제 시간.
    private float betweenPassStartTime;

    public bool IsRunning =>
        isRunning;

    public bool IsBetweenPasses =>
        isBetweenPasses;

    public int CurrentPass =>
        currentPass;

    public int MaxPasses =>
        maxPasses;

    public FishController ActiveBoss =>
        activeBoss;

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

    // =========================================================
    // ENCOUNTER START
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

        if (route == null)
        {
            Debug.LogError(
                "BossEncounterController: Boss Route가 없습니다."
            );

            return;
        }

        fishSpawner = spawner;
        bossData = data;
        bossRoute = route;

        savedResistance =
            bossData.MaxResistance;

        currentPass = 1;

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

            bool registerSpawn =
                currentPass == 1;

            activeBoss =
                fishSpawner.SpawnBossPass(
                    bossData,
                    bossRoute,
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

            activeBoss.Captured +=
                HandleBossCaptured;

            if (activeBossMovement != null)
            {
                activeBossMovement
                    .DestinationReached +=
                    HandleBossDestinationReached;
            }

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[BOSS] {currentPass}/{maxPasses} 회유 시작 | " +
                    $"Resistance: {savedResistance:F0} / " +
                    $"{bossData.MaxResistance:F0}"
                );
            }

            yield return new WaitUntil(
                () => passResolved
            );

            DetachBossEvents();

            // -------------------------
            // 포획 성공
            // -------------------------

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

            // -------------------------
            // 마지막 회유 실패
            // -------------------------

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

            // -------------------------
            // Resistance 회복
            // -------------------------

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
                    $"{lastResistanceAfterRecovery:F0} 회복"
                );
            }

            // 다음 회유 번호로 먼저 증가시킨다.
            currentPass++;

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
        DetachBossEvents();

        activeBoss = null;
        activeBossMovement = null;

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
        DetachBossEvents();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}