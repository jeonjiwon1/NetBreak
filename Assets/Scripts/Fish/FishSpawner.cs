using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    private enum AmbientIntensity
    {
        Early,
        Growth,
        Special,
        MiniBossSupport,
        Rush,
        Final
    }

    [Header("Fish")]
    [SerializeField] private FishController fishPrefab;
    [SerializeField] private FishData[] fishTypes;

    [Header("Route")]
    [SerializeField] private FishRoute activeRoute;

    [Header("Boss")]
    [SerializeField]
    private BossEncounterController bossEncounterController;

    [SerializeField]
    private bool bossTestMode = false;
    [SerializeField]
    private bool miniBossTestMode = false;

    [Header("Pool")]
    [SerializeField] private int poolSize = 200;
    [Min(1)] [SerializeField] private int maxPoolSize = 320;
    [Min(1)] [SerializeField] private int poolGrowthBatchSize = 20;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMargin = 0.5f;
    [SerializeField] private float verticalPadding = 1f;
    [SerializeField] private float schoolSpreadX = 1.5f;

    [Header("Phase Duration")]
    [SerializeField] private float earlyPhaseDuration = 75f;


    [SerializeField] private float growthPhaseDuration = 105f;
    [SerializeField] private float specialPhaseDuration = 90f;
    [SerializeField] private float miniBossPhaseDuration = 45f;
    [SerializeField] private float rushPhaseDuration = 180f;
    [SerializeField] private float finalBuildUpDuration = 70f;

    [Header("Warning")]
    [SerializeField] private float largeSchoolWarningTime = 2.5f;
    [SerializeField] private float specialFishWarningTime = 2.5f;
    [SerializeField] private float miniBossWarningTime = 2.5f;
    [SerializeField] private float bossWarningTime = 3f;
    [Header("Spawn Timing")]
    [SerializeField] private Vector2 earlyAmbientInterval = new Vector2(2f, 3f);
    [SerializeField] private Vector2 growthAmbientInterval = new Vector2(1.7f, 2.6f);
    [SerializeField] private Vector2 specialAmbientInterval = new Vector2(1.5f, 2.4f);
    [SerializeField] private Vector2 miniBossSupportAmbientInterval = new Vector2(2f, 3f);
    [SerializeField] private Vector2 rushAmbientInterval = new Vector2(0.9f, 1.6f);
    [SerializeField] private Vector2 finalAmbientInterval = new Vector2(1.1f, 1.8f);

    [Header("Special Fish")]
    [Range(0, 100)] [SerializeField] private int growthSpecialFishChancePercent = 20;
    [Range(0, 100)] [SerializeField] private int specialPhaseSpecialFishChancePercent = 35;
    [Range(0, 100)] [SerializeField] private int rushSpecialFishChancePercent = 35;
    [Range(0, 100)] [SerializeField] private int finalSpecialFishChancePercent = 40;

    private const int CoastStageCount = 8;

    private readonly List<FishController>
        fishPool = new();

    private int lastPoolExhaustionWarningFrame = -1;

    private Camera mainCamera;

    private bool hasStarted;
    private bool spawningFinished;

    private int currentStageIndex;

    private string currentPhaseName =
        "조업 준비";

    private string announcementText =
        "";

    private float announcementEndTime;

    public bool HasStarted =>
        hasStarted;

    public bool SpawningFinished =>
        spawningFinished;

    public int CurrentStageIndex =>
        currentStageIndex;

    public int TotalStageCount =>
        CoastStageCount;

    public string CurrentPhaseName =>
        currentPhaseName;

    public bool IsShowingAnnouncement =>
        !string.IsNullOrEmpty(
            announcementText
        )
        &&
        Time.time <
        announcementEndTime;

    public string AnnouncementText =>
        IsShowingAnnouncement
            ? announcementText
            : "";

    public int SpawnedEncounterCount =>
        currentStageIndex;

    public int TotalEncounterCount =>
        CoastStageCount;

    public int SpawnedSchoolCount =>
        currentStageIndex;

    public int TotalSchoolCount =>
        CoastStageCount;

    private void Awake()
    {
        mainCamera =
            Camera.main;

        if (bossEncounterController == null)
        {
            bossEncounterController =
                FindFirstObjectByType<
                    BossEncounterController
                >();
        }

        NormalizePoolSettings();
        CreatePool();
    }

    public void StartSpawning()
    {
        if (hasStarted)
        {
            return;
        }

        if (fishTypes == null ||
            fishTypes.Length == 0)
        {
            Debug.LogWarning(
                "FishSpawner: Fish Types가 비어 있습니다."
            );

            return;
        }

        hasStarted = true;

        if (bossTestMode)
        {
            StartCoroutine(
                RunBossTestSequence()
            );

            return;
        }

        if (miniBossTestMode)
        {
            StartCoroutine(
                RunMiniBossTestSequence()
            );

            return;
        }

        StartCoroutine(
            RunCoastSequence()
        );
    }

    // =========================================================
    // MAIN COAST SEQUENCE
    // =========================================================

    private IEnumerator RunCoastSequence()
    {
        FishData lowValueFish =
            GetStandardFishByValueRank(0);

        FishData midValueFish =
            GetStandardFishByValueRank(1);

        FishData highValueFish =
            GetStandardFishByValueRank(2);

        FishData pufferfish =
            GetFishBySpecialType(
                FishSpecialType.Pufferfish
            );

        FishData squid =
            GetFishBySpecialType(
                FishSpecialType.Squid
            );

        FishData miniBoss =
            GetFishBySpecialType(
                FishSpecialType.MiniBoss
            );

        FishData boss =
            GetFishBySpecialType(
                FishSpecialType.Boss
            );

        yield return RunEarlyPhase(
            lowValueFish,
            midValueFish
        );

        yield return
            RunFirstLargeSchoolPhase(
                lowValueFish
            );

        yield return RunGrowthPhase(
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish
        );

        yield return RunSpecialPhase(
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        yield return RunMiniBossPhase(
            lowValueFish,
            midValueFish,
            highValueFish,
            miniBoss
        );

        if (PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsGameEnded)
        {
            yield break;
        }

        yield return RunRushPhase(
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        yield return RunFinalPhase(
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid,
            boss
        );
    }

    // =========================================================
    // TEST MODE
    // =========================================================

    private IEnumerator RunBossTestSequence()
    {
        FishData boss =
            GetFishBySpecialType(
                FishSpecialType.Boss
            );

        if (boss == null)
        {
            Debug.LogError(
                "FishSpawner: Boss 타입 FishData가 Fish Types에 없습니다."
            );

            yield break;
        }

        SetStage(
            8,
            "최종 보스 테스트"
        );

        ShowAnnouncement(
            "최종 보스 테스트를 시작합니다.",
            1.5f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        spawningFinished = true;

        StartBossEncounter(
            boss
        );
    }

    private IEnumerator RunMiniBossTestSequence()
    {
        FishData miniBoss =
            GetFishBySpecialType(
                FishSpecialType.MiniBoss
            );

        if (miniBoss == null)
        {
            Debug.LogError(
                "FishSpawner: MiniBoss 타입 FishData가 Fish Types에 없습니다."
            );

            yield break;
        }

        SetStage(
            5,
            "미니보스 테스트"
        );

        ShowAnnouncement(
            "거대 참치 테스트를 시작합니다.",
            1.5f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        SpawnSchool(
            miniBoss,
            1,
            0.8f,
            0.9f
        );
    }

    // =========================================================
    // PHASE 1
    // =========================================================

    private IEnumerator RunEarlyPhase(
        FishData lowValueFish,
        FishData midValueFish)
    {
        SetStage(
            1,
            "초반 조업"
        );

        SpawnLooseFish(
            lowValueFish,
            2
        );

        yield return RunAmbientWindow(
            earlyPhaseDuration,
            AmbientIntensity.Early,
            lowValueFish,
            midValueFish,
            null,
            null,
            null
        );
    }

    private IEnumerator RequestToolAcquisition()
    {
        ToolAcquisitionManager acquisition =
            ToolAcquisitionManager.Instance;

        if (acquisition == null ||
            !acquisition.RequestToolAcquisition())
        {
            yield break;
        }

        yield return new WaitUntil(
            () =>
                (acquisition == null ||
                 !acquisition.IsAcquisitionPending) &&
                (PrototypeAugmentManager.Instance == null ||
                 !PrototypeAugmentManager.Instance.IsShowingChoices)
        );
    }

    // =========================================================
    // PHASE 2
    // =========================================================

    private IEnumerator
        RunFirstLargeSchoolPhase(
            FishData lowValueFish)
    {
        SetStage(
            2,
            "첫 대형 어군"
        );

        ShowAnnouncement(
            "대규모 어군이 접근 중입니다.",
            largeSchoolWarningTime
        );

        yield return new WaitForSeconds(
            largeSchoolWarningTime
        );

        List<FishController>
            firstLargeSchool =
                SpawnSchool(
                    lowValueFish,
                    20,
                    1.2f,
                    1.1f
                );

        yield return WaitForEncounterResolved(
            firstLargeSchool,
            18f
        );

        yield return new WaitForSeconds(
            1f
        );
    }

    // =========================================================
    // PHASE 3
    // =========================================================

    private IEnumerator RunGrowthPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish)
    {
        SetStage(
            3,
            "빌드 성장"
        );

        SpawnLooseFish(
            highValueFish,
            1
        );

        yield return new WaitForSeconds(
            4f
        );

        ShowAnnouncement(
            "복어 출현 - 그물 포획을 방해합니다.",
            specialFishWarningTime
        );

        yield return new WaitForSeconds(
            specialFishWarningTime
        );

        SpawnMixedSchool(
            midValueFish,
            4,
            pufferfish,
            1,
            0.7f,
            0.9f
        );

        yield return RunAmbientWindow(
            growthPhaseDuration,
            AmbientIntensity.Growth,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            null
        );
    }

    // =========================================================
    // PHASE 4
    // =========================================================

    private IEnumerator RunSpecialPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        SetStage(
            4,
            "특수어 조업"
        );

        ShowAnnouncement(
            "오징어 출현 - 주변 설치 어구를 먹물로 정지시킵니다.",
            specialFishWarningTime
        );

        yield return new WaitForSeconds(
            specialFishWarningTime
        );

        SpawnMixedSchool(
            midValueFish,
            7,
            squid,
            1,
            0.9f,
            1f
        );

        yield return RunAmbientWindow(
            specialPhaseDuration,
            AmbientIntensity.Special,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );
    }

    // =========================================================
    // PHASE 5
    // =========================================================

    private IEnumerator RunMiniBossPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData miniBoss)
    {
        SetStage(
            5,
            "대형 개체 출현"
        );

        ShowAnnouncement(
            "거대 참치가 접근합니다!",
            miniBossWarningTime
        );

        yield return new WaitForSeconds(
            miniBossWarningTime
        );

        List<FishController> miniBossSchool =
            SpawnMixedSchool(
                midValueFish,
                4,
                miniBoss,
                1,
                0.8f,
                0.9f
            );

        FishController spawnedMiniBoss =
            miniBossSchool.Find(
                fish =>
                    fish != null &&
                    fish.Data != null &&
                    fish.Data.SpecialType ==
                    FishSpecialType.MiniBoss
            );

        Coroutine supportSpawning =
            StartCoroutine(
                RunAmbientWindow(
                    miniBossPhaseDuration,
                    AmbientIntensity.MiniBossSupport,
                    lowValueFish,
                    midValueFish,
                    highValueFish,
                    null,
                    null
                )
            );

        float elapsed = 0f;

        while (spawnedMiniBoss != null &&
               spawnedMiniBoss.gameObject.activeSelf &&
               !spawnedMiniBoss.IsCaptured &&
               elapsed < miniBossPhaseDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }


        if (supportSpawning != null)
        {
            StopCoroutine(
                supportSpawning
            );
        }

        bool miniBossCaptured =
            spawnedMiniBoss != null &&
            spawnedMiniBoss.IsCaptured;

        if (!miniBossCaptured)
        {
            if (PrototypeGameFlowManager.Instance != null)
            {
                PrototypeGameFlowManager.Instance
                    .FailMiniBossEncounter();
            }

            yield break;
        }

        TacticalSkillManager tacticalSkills =
            RunManager.Instance != null
                ? RunManager.Instance.TacticalSkills
                : TacticalSkillManager.Instance;

        if (tacticalSkills == null || !tacticalSkills.RequestSelection())
        {
            Debug.LogError("MiniBoss reward could not open the required Tactical E selection.");
            yield break;
        }

        yield return new WaitUntil(
            () => tacticalSkills == null || tacticalSkills.HasEquippedSkill
        );
    }

    // =========================================================
    // PHASE 6
    // =========================================================

    private IEnumerator RunRushPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        SetStage(
            6,
            "어군 러시"
        );

        ShowAnnouncement(
            "대규모 어군이 연속으로 접근합니다.",
            2f
        );

        yield return new WaitForSeconds(
            2f
        );

        float sectionDuration =
            rushPhaseDuration /
            4f;

        SpawnMixedSchool(
            lowValueFish,
            18,
            pufferfish,
            1,
            1.2f,
            1.1f
        );

        yield return RunAmbientWindow(
            sectionDuration,
            AmbientIntensity.Rush,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        ShowAnnouncement(
            "두 번째 어군이 접근합니다.",
            1.5f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        SpawnMixedSchool(
            midValueFish,
            14,
            squid,
            1,
            1.15f,
            1.1f
        );

        SpawnLooseFish(
            highValueFish,
            1
        );

        yield return RunAmbientWindow(
            sectionDuration,
            AmbientIntensity.Rush,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        SpawnMixedSchool(
            lowValueFish,
            22,
            pufferfish,
            2,
            1.3f,
            1.2f
        );

        yield return RunAmbientWindow(
            sectionDuration,
            AmbientIntensity.Rush,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        ShowAnnouncement(
            "어군 밀도가 증가합니다.",
            1.5f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        SpawnMixedSchool(
            midValueFish,
            18,
            squid,
            1,
            1.25f,
            1.15f
        );

        SpawnLooseFish(
            highValueFish,
            2
        );

        yield return RunAmbientWindow(
            sectionDuration,
            AmbientIntensity.Rush,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );
    }

    // =========================================================
    // PHASE 7 + BOSS
    // =========================================================

    private IEnumerator RunFinalPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid,
        FishData boss)
    {
        SetStage(
            7,
            "마감 직전"
        );

        yield return RunAmbientWindow(
            finalBuildUpDuration,
            AmbientIntensity.Final,
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        currentPhaseName =
            "마지막 어군";

        ShowAnnouncement(
            "마지막 대규모 어군이 접근 중입니다.",
            largeSchoolWarningTime
        );

        yield return new WaitForSeconds(
            largeSchoolWarningTime
        );

        List<FishController> finalSchool =
            SpawnMixedFinalSchool(
                lowValueFish,
                midValueFish,
                highValueFish,
                pufferfish,
                squid
            );

        yield return WaitForEncounterResolved(
            finalSchool,
            25f
        );

        spawningFinished = true;

        SetStage(
            8,
            "최종 보스"
        );

        ShowAnnouncement(
            "거대한 개체가 접근합니다.",
            bossWarningTime
        );

        yield return new WaitForSeconds(
            bossWarningTime
        );

        StartBossEncounter(
            boss
        );
    }

    private void StartBossEncounter(
        FishData boss)
    {
        if (boss == null)
        {
            Debug.LogError(
                "FishSpawner: 최종 보스 FishData가 없습니다."
            );

            return;
        }

        if (bossEncounterController == null)
        {
            Debug.LogError(
                "FishSpawner: BossEncounterController가 없습니다."
            );

            return;
        }

        if (activeRoute == null)
        {
            Debug.LogError(
                "FishSpawner: Active Route가 없습니다."
            );

            return;
        }

        bossEncounterController.BeginEncounter(
            this,
            boss,
            activeRoute
        );
    }

    // =========================================================
    // AMBIENT
    // =========================================================

    private IEnumerator RunAmbientWindow(
        float duration,
        AmbientIntensity intensity,
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 delayRange =
                GetAmbientDelayRange(
                    intensity
                );

            float delay =
                Random.Range(
                    delayRange.x,
                    delayRange.y
                );

            float remaining =
                duration - elapsed;

            if (delay > remaining)
            {
                yield return new WaitForSeconds(
                    remaining
                );

                yield break;
            }

            yield return new WaitForSeconds(
                delay
            );

            elapsed += delay;

            SpawnAmbientEvent(
                intensity,
                lowValueFish,
                midValueFish,
                highValueFish,
                pufferfish,
                squid
            );
        }
    }

    private Vector2 GetAmbientDelayRange(
        AmbientIntensity intensity)
    {
        switch (intensity)
        {
            case AmbientIntensity.Early:
                return earlyAmbientInterval;

            case AmbientIntensity.Growth:
                return growthAmbientInterval;

            case AmbientIntensity.Special:
                return specialAmbientInterval;

            case AmbientIntensity.Rush:
                return rushAmbientInterval;

            case AmbientIntensity.MiniBossSupport:
                return miniBossSupportAmbientInterval;

            case AmbientIntensity.Final:
                return finalAmbientInterval;

            default:
                return new Vector2(
                    6f,
                    8f
                );
        }
    }
    private void SpawnAmbientEvent(
        AmbientIntensity intensity,
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        switch (intensity)
        {
            case AmbientIntensity.Early:
                SpawnEarlyAmbient(
                    lowValueFish,
                    midValueFish
                );
                break;

            case AmbientIntensity.Growth:
                SpawnGrowthAmbient(
                    lowValueFish,
                    midValueFish,
                    highValueFish,
                    pufferfish
                );
                break;

            case AmbientIntensity.Special:
                SpawnSpecialAmbient(
                    lowValueFish,
                    midValueFish,
                    highValueFish,
                    pufferfish,
                    squid
                );
                break;

            case AmbientIntensity.MiniBossSupport:
                SpawnMiniBossSupportAmbient(
                    lowValueFish,
                    midValueFish,
                    highValueFish
                );
                break;

            case AmbientIntensity.Rush:
                SpawnRushAmbient(
                    lowValueFish,
                    midValueFish,
                    highValueFish,
                    pufferfish,
                    squid
                );
                break;

            case AmbientIntensity.Final:
                SpawnFinalAmbient(
                    lowValueFish,
                    midValueFish,
                    highValueFish,
                    pufferfish,
                    squid
                );
                break;
        }
    }

    private void SpawnEarlyAmbient(
        FishData lowValueFish,
        FishData midValueFish)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 45)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(1, 4)
            );
            return;
        }

        if (roll < 75)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(1, 3)
            );
            return;
        }

        SpawnSchool(
            lowValueFish,
            Random.Range(4, 7),
            0.6f,
            0.8f
        );
    }

    private void SpawnGrowthAmbient(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 25)
        {
            SpawnLooseFish(
                highValueFish,
                1
            );
            return;
        }

        if (roll < 50)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(2, 5)
            );
            return;
        }

        if (roll < 70)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(1, 4)
            );
            return;
        }

        if (roll < 100 - growthSpecialFishChancePercent)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(5, 9),
                0.7f,
                0.9f
            );
            return;
        }

        SpawnMixedSchool(
            midValueFish,
            4,
            pufferfish,
            1,
            0.8f,
            0.9f
        );
    }

    private void SpawnSpecialAmbient(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 20)
        {
            SpawnLooseFish(
                highValueFish,
                1
            );
            return;
        }

        if (roll < 40)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(2, 5)
            );
            return;
        }

        if (roll < 60)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(2, 5)
            );
            return;
        }

        int specialStart = 100 - specialPhaseSpecialFishChancePercent;

        if (roll < specialStart)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(6, 10),
                0.8f,
                0.95f
            );
            return;
        }

        int squidChance = Mathf.RoundToInt(
            specialPhaseSpecialFishChancePercent *
            (18f / 35f)
        );

        if (roll < specialStart + squidChance)
        {
            SpawnMixedSchool(
                midValueFish,
                5,
                squid,
                1,
                0.9f,
                1f
            );
            return;
        }

        SpawnMixedSchool(
            lowValueFish,
            7,
            pufferfish,
            1,
            0.9f,
            1f
        );
    }

    private void SpawnMiniBossSupportAmbient(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 50)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(1, 4)
            );
            return;
        }

        if (roll < 80)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(1, 3)
            );
            return;
        }

        if (roll < 95)
        {
            SpawnLooseFish(
                highValueFish,
                1
            );
            return;
        }

        SpawnSchool(
            lowValueFish,
            5,
            0.6f,
            0.8f
        );
    }

    private void SpawnRushAmbient(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 15)
        {
            SpawnLooseFish(
                highValueFish,
                1
            );
            return;
        }

        if (roll < 35)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(3, 6)
            );
            return;
        }

        if (roll < 50)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(2, 5)
            );
            return;
        }

        int specialStart = 100 - rushSpecialFishChancePercent;

        if (roll < specialStart)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(7, 11),
                0.9f,
                1f
            );
            return;
        }

        int pufferfishChance = Mathf.RoundToInt(
            rushSpecialFishChancePercent *
            (17f / 35f)
        );

        if (roll < specialStart + pufferfishChance)
        {
            SpawnMixedSchool(
                lowValueFish,
                7,
                pufferfish,
                1,
                0.9f,
                1f
            );
            return;
        }

        SpawnMixedSchool(
            midValueFish,
            6,
            squid,
            1,
            0.9f,
            1f
        );
    }

    private void SpawnFinalAmbient(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
    {
        int roll =
            Random.Range(0, 100);

        if (roll < 15)
        {
            SpawnLooseFish(
                highValueFish,
                Random.Range(1, 3)
            );
            return;
        }

        if (roll < 30)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(4, 7)
            );
            return;
        }

        if (roll < 45)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(3, 6)
            );
            return;
        }

        int specialStart = 100 - finalSpecialFishChancePercent;

        if (roll < specialStart)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(8, 13),
                1f,
                1.05f
            );
            return;
        }

        int pufferfishChance = Mathf.RoundToInt(
            finalSpecialFishChancePercent * 0.5f
        );

        if (roll < specialStart + pufferfishChance)
        {
            SpawnMixedSchool(
                lowValueFish,
                10,
                pufferfish,
                1,
                1f,
                1.05f
            );
            return;
        }

        SpawnMixedSchool(
            midValueFish,
            8,
            squid,
            1,
            1f,
            1.05f
        );
    }

    // =========================================================
    // STATE / ANNOUNCEMENT
    // =========================================================

    private void SetStage(
        int stageIndex,
        string phaseName)
    {
        currentStageIndex =
            Mathf.Clamp(
                stageIndex,
                1,
                CoastStageCount
            );

        currentPhaseName =
            phaseName;
    }

    public void ShowAnnouncement(
        string text,
        float duration)
    {
        announcementText =
            text;

        announcementEndTime =
            Time.time +
            duration;
    }

    // =========================================================
    // POOL
    // =========================================================

    public void ClearAnnouncement()
    {
        announcementText = string.Empty;
        announcementEndTime = 0f;
    }

    private void NormalizePoolSettings()
    {
        poolSize = Mathf.Max(1, poolSize);
        maxPoolSize = Mathf.Max(poolSize, maxPoolSize);
        poolGrowthBatchSize = Mathf.Max(1, poolGrowthBatchSize);
    }

    private void CreatePool()
    {
        CreatePoolEntries(poolSize);
    }

    private void CreatePoolEntries(int count)
    {
        for (int i = 0; i < count; i++)
        {
            FishController fish =
                Instantiate(
                    fishPrefab,
                    transform
                );

            fish.gameObject.SetActive(
                false
            );

            fishPool.Add(
                fish
            );
        }
    }

    private FishController
        GetInactiveFish()
    {
        foreach (FishController fish
                 in fishPool)
        {
            if (!fish.gameObject.activeSelf)
            {
                return fish;
            }
        }

        int remainingCapacity =
            maxPoolSize - fishPool.Count;

        if (remainingCapacity <= 0)
        {
            return null;
        }

        int growthCount =
            Mathf.Min(
                poolGrowthBatchSize,
                remainingCapacity
            );

        int firstNewIndex = fishPool.Count;
        CreatePoolEntries(growthCount);
        return fishPool[firstNewIndex];
    }

    private void ReportPoolExhaustion()
    {
        if (lastPoolExhaustionWarningFrame == Time.frameCount)
        {
            return;
        }

        lastPoolExhaustionWarningFrame = Time.frameCount;
        Debug.LogWarning(
            $"FishSpawner: bounded pool exhausted " +
            $"({fishPool.Count}/{maxPoolSize}). " +
            "Remaining spawn requests in this frame are omitted."
        );
    }

    // =========================================================
    // FISH DATA LOOKUP
    // =========================================================

    private FishData
        GetStandardFishByValueRank(
            int rank)
    {
        List<FishData> validTypes =
            new();

        foreach (FishData data
                 in fishTypes)
        {
            if (data == null)
            {
                continue;
            }

            if (data.SpecialType !=
                FishSpecialType.None)
            {
                continue;
            }

            validTypes.Add(
                data
            );
        }

        if (validTypes.Count == 0)
        {
            return null;
        }

        validTypes.Sort(
            (a, b) =>
                a.CatchValue.CompareTo(
                    b.CatchValue
                )
        );

        rank =
            Mathf.Clamp(
                rank,
                0,
                validTypes.Count - 1
            );

        return validTypes[rank];
    }

    private FishData GetFishBySpecialType(
        FishSpecialType specialType)
    {
        foreach (FishData data
                 in fishTypes)
        {
            if (data != null &&
                data.SpecialType ==
                specialType)
            {
                return data;
            }
        }

        return null;
    }

    // =========================================================
    // NORMAL SPAWN
    // =========================================================

    private List<FishController>
        SpawnLooseFish(
            FishData data,
            int count)
    {
        List<FishController> spawned =
            new();

        if (data == null)
        {
            return spawned;
        }

        float cameraLeft =
            GetCameraLeft();

        float cameraBottom =
            GetCameraBottom();

        float cameraTop =
            GetCameraTop();

        for (int i = 0;
             i < count;
             i++)
        {
            float spawnY =
                Random.Range(
                    cameraBottom,
                    cameraTop
                );

            float offsetX =
                Random.Range(
                    -0.6f,
                    0f
                );

            Vector3 spawnPosition =
                new Vector3(
                    cameraLeft -
                    spawnMargin +
                    offsetX,
                    spawnY,
                    0f
                );

            FishController fish =
                SpawnFish(
                    data,
                    spawnPosition,
                    spawnY
                );

            if (fish != null)
            {
                spawned.Add(
                    fish
                );
            }
        }

        return spawned;
    }

    private List<FishController>
        SpawnSchool(
            FishData data,
            int count,
            float spreadXMultiplier,
            float spreadYMultiplier)
    {
        List<FishController> spawned =
            new();

        if (data == null)
        {
            return spawned;
        }

        float centerY =
            Random.Range(
                GetCameraBottom(),
                GetCameraTop()
            );

        SpawnSchoolMembers(
            data,
            count,
            centerY,
            spreadXMultiplier,
            spreadYMultiplier,
            spawned
        );

        return spawned;
    }

    private List<FishController>
        SpawnMixedSchool(
            FishData firstData,
            int firstCount,
            FishData secondData,
            int secondCount,
            float spreadXMultiplier,
            float spreadYMultiplier)
    {
        List<FishController> spawned =
            new();

        float centerY =
            Random.Range(
                GetCameraBottom(),
                GetCameraTop()
            );

        SpawnSchoolMembers(
            firstData,
            firstCount,
            centerY,
            spreadXMultiplier,
            spreadYMultiplier,
            spawned
        );

        SpawnSchoolMembers(
            secondData,
            secondCount,
            centerY,
            spreadXMultiplier,
            spreadYMultiplier,
            spawned
        );

        return spawned;
    }

    private void SpawnSchoolMembers(
        FishData data,
        int count,
        float centerY,
        float spreadXMultiplier,
        float spreadYMultiplier,
        List<FishController> spawned)
    {
        if (data == null)
        {
            return;
        }

        float cameraLeft =
            GetCameraLeft();

        for (int i = 0;
             i < count;
             i++)
        {
            float offsetX =
                Random.Range(
                    -schoolSpreadX *
                    spreadXMultiplier,
                    0f
                );

            float offsetY =
                Random.Range(
                    -data.SchoolSpawnSpreadY *
                    spreadYMultiplier,
                    data.SchoolSpawnSpreadY *
                    spreadYMultiplier
                );

            Vector3 spawnPosition =
                new Vector3(
                    cameraLeft -
                    spawnMargin +
                    offsetX,
                    centerY +
                    offsetY,
                    0f
                );

            FishController fish =
                SpawnFish(
                    data,
                    spawnPosition,
                    centerY
                );

            if (fish != null)
            {
                spawned.Add(
                    fish
                );
            }
        }
    }

    private List<FishController>
        SpawnMixedFinalSchool(
            FishData lowValueFish,
            FishData midValueFish,
            FishData highValueFish,
            FishData pufferfish,
            FishData squid)
    {
        float centerY =
            Random.Range(
                GetCameraBottom(),
                GetCameraTop()
            );

        List<FishController> spawned =
            new();

        SpawnSchoolMembers(
            lowValueFish,
            28,
            centerY,
            1.5f,
            1.3f,
            spawned
        );

        SpawnSchoolMembers(
            midValueFish,
            14,
            centerY,
            1.4f,
            1.25f,
            spawned
        );

        SpawnSchoolMembers(
            highValueFish,
            3,
            centerY,
            1.1f,
            1.1f,
            spawned
        );

        SpawnSchoolMembers(
            pufferfish,
            2,
            centerY,
            1.2f,
            1.2f,
            spawned
        );

        SpawnSchoolMembers(
            squid,
            2,
            centerY,
            1.2f,
            1.2f,
            spawned
        );

        return spawned;
    }

    private FishController SpawnFish(
        FishData data,
        Vector3 position,
        float movementCenterY)
    {
        if (data == null)
        {
            return null;
        }

        FishController fish =
            GetInactiveFish();

        if (fish == null)
        {
            ReportPoolExhaustion();
            return null;
        }

        float routeLaneOffset = 0f;

        if (activeRoute != null)
        {
            float speciesSpreadMultiplier =
                Mathf.Clamp(
                    data.SchoolSpawnSpreadY /
                    1.5f,
                    0.65f,
                    1.4f
                );

            routeLaneOffset =
                Random.Range(
                    -1f,
                    1f
                )
                *
                speciesSpreadMultiplier;

            position =
                activeRoute.GetSpawnPosition(
                    routeLaneOffset
                );
        }

        fish.transform.position =
            position;

        fish.Initialize(
            data
        );

        if (RunManager.Instance != null)
        {
            RunManager.Instance
                .RegisterFishSpawned(
                    data
                );
        }

        FishMovement movement =
            fish.GetComponent<
                FishMovement
            >();

        if (movement != null)
        {
            if (activeRoute != null)
            {
                movement
                    .InitializeRouteMovement(
                        activeRoute,
                        routeLaneOffset
                    );
            }
            else
            {
                movement
                    .InitializeSchoolMovement(
                        movementCenterY
                    );
            }
        }

        fish.gameObject.SetActive(
            true
        );

        return fish;
    }

    // =========================================================
    // BOSS SPAWN
    // =========================================================

    public FishController SpawnBossPass(
        FishData data,
        FishRoute route,
        float resistance,
        bool registerSpawn)
    {
        if (data == null ||
            route == null)
        {
            return null;
        }

        FishController fish =
            GetInactiveFish();

        if (fish == null)
        {
            ReportPoolExhaustion();
            return null;
        }

        fish.transform.position =
            route.GetSpawnPosition(
                0f
            );

        fish.Initialize(
            data
        );

        FishMovement movement =
            fish.GetComponent<
                FishMovement
            >();

        if (movement != null)
        {
            movement
                .InitializeRouteMovement(
                    route,
                    0f
                );
        }

        fish.gameObject.SetActive(
            true
        );

        fish.SetCurrentResistance(
            resistance
        );

        // 보스는 여러 번 회유하더라도
        // 한 마리로 취급한다.
        if (registerSpawn &&
            RunManager.Instance != null)
        {
            RunManager.Instance
                .RegisterFishSpawned(
                    data
                );
        }

        return fish;
    }

    // =========================================================
    // BOSS SUPPORT FISH
    // =========================================================

    public void SpawnBossSupportEvent(
        FishRoute route,
        int bossPhase)
    {
        if (route == null)
        {
            return;
        }

        FishData lowValueFish =
            GetStandardFishByValueRank(
                0
            );

        FishData midValueFish =
            GetStandardFishByValueRank(
                1
            );

        FishData pufferfish =
            GetFishBySpecialType(
                FishSpecialType.Pufferfish
            );

        FishData squid =
            GetFishBySpecialType(
                FishSpecialType.Squid
            );

        int phase =
            Mathf.Clamp(
                bossPhase,
                1,
                3
            );

        // Phase 1
        // 작은 물고기만 소량 유입.
        if (phase == 1)
        {
            SpawnBossSupportGroup(
                lowValueFish,
                2,
                route,
                0.8f
            );

            return;
        }

        // Phase 2
        // 어군 밀도가 조금 증가.
        if (phase == 2)
        {
            SpawnBossSupportGroup(
                lowValueFish,
                2,
                route,
                0.9f
            );

            SpawnBossSupportGroup(
                midValueFish,
                1,
                route,
                0.8f
            );

            return;
        }

        // Phase 3
        // 일반 어군 + 낮은 확률로 특수어 1마리.
        SpawnBossSupportGroup(
            lowValueFish,
            3,
            route,
            1f
        );

        SpawnBossSupportGroup(
            midValueFish,
            2,
            route,
            0.9f
        );

        if (Random.value <= 0.35f)
        {
            FishData specialFish =
                Random.value < 0.5f
                    ? pufferfish
                    : squid;

            if (specialFish != null)
            {
                SpawnBossSupportGroup(
                    specialFish,
                    1,
                    route,
                    0.7f
                );
            }
        }
    }

    private void SpawnBossSupportGroup(
        FishData data,
        int count,
        FishRoute route,
        float spreadMultiplier)
    {
        if (data == null ||
            route == null ||
            count <= 0)
        {
            return;
        }

        float speciesSpreadMultiplier =
            Mathf.Clamp(
                data.SchoolSpawnSpreadY /
                1.5f,
                0.65f,
                1.4f
            );

        for (int i = 0;
             i < count;
             i++)
        {
            float routeLaneOffset =
                Random.Range(
                    -1f,
                    1f
                )
                *
                speciesSpreadMultiplier
                *
                spreadMultiplier;

            SpawnFishOnSpecificRoute(
                data,
                route,
                routeLaneOffset
            );
        }
    }

    private FishController SpawnFishOnSpecificRoute(
        FishData data,
        FishRoute route,
        float laneOffset)
    {
        if (data == null ||
            route == null)
        {
            return null;
        }

        FishController fish =
            GetInactiveFish();

        if (fish == null)
        {
            ReportPoolExhaustion();
            return null;
        }

        fish.transform.position =
            route.GetSpawnPosition(
                laneOffset
            );

        fish.Initialize(
            data
        );

        FishMovement movement =
            fish.GetComponent<
                FishMovement
            >();

        if (movement != null)
        {
            movement
                .InitializeRouteMovement(
                    route,
                    laneOffset
                );
        }

        fish.gameObject.SetActive(
            true
        );

        if (RunManager.Instance != null)
        {
            RunManager.Instance
                .RegisterFishSpawned(
                    data
                );
        }

        return fish;
    }

    // =========================================================
    // ENCOUNTER WAIT
    // =========================================================

    private IEnumerator
        WaitForEncounterResolved(
            List<FishController> encounterFish,
            float maxWaitTime)
    {
        float timer = 0f;

        while (timer < maxWaitTime)
        {
            bool anyActive =
                false;

            foreach (FishController fish
                     in encounterFish)
            {
                if (fish != null &&
                    fish.gameObject.activeSelf)
                {
                    anyActive = true;
                    break;
                }
            }

            if (!anyActive)
            {
                yield break;
            }

            timer +=
                Time.deltaTime;

            yield return null;
        }
    }

    // =========================================================
    // CAMERA
    // =========================================================

    private float GetCameraLeft()
    {
        return
            mainCamera.transform.position.x
            -
            mainCamera.orthographicSize *
            mainCamera.aspect;
    }

    private float GetCameraBottom()
    {
        return
            mainCamera.transform.position.y
            -
            mainCamera.orthographicSize
            +
            verticalPadding;
    }

    private float GetCameraTop()
    {
        return
            mainCamera.transform.position.y
            +
            mainCamera.orthographicSize
            -
            verticalPadding;
    }
    private void OnValidate()
    {
        NormalizePoolSettings();
    }

}
