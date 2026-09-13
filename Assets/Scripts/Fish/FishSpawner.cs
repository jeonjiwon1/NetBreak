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

    [Header("Pool")]
    [SerializeField] private int poolSize = 200;

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

    private const int CoastStageCount = 7;

    private readonly List<FishController> fishPool =
        new();

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

    // Temporary compatibility with older prototype UI/code.
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

        StartCoroutine(
            RunCoastSequence()
        );
    }

    private IEnumerator RunCoastSequence()
    {
        FishData lowValueFish =
            GetStandardFishByValueRank(
                0
            );

        FishData midValueFish =
            GetStandardFishByValueRank(
                1
            );

        FishData highValueFish =
            GetStandardFishByValueRank(
                2
            );

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

        yield return RunEarlyPhase(
            lowValueFish,
            midValueFish
        );

        yield return RunFirstLargeSchoolPhase(
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
            squid
        );
    }

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

        yield return
            WaitForEncounterResolved(
                firstLargeSchool,
                18f
            );

        ShowAnnouncement(
            "1차 전직이 가능합니다.",
            1.2f
        );

        yield return new WaitForSeconds(
            1.2f
        );

        if (PrototypeJobManager.Instance != null)
        {
            PrototypeJobManager.Instance
                .RequestJobSelection();

            yield return new WaitUntil(
                () =>
                    PrototypeJobManager.Instance == null
                    ||
                    PrototypeJobManager.Instance
                        .HasAdvanced
            );
        }

        yield return new WaitForSeconds(
            1f
        );
    }

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

        SpawnMixedSchool(
            midValueFish,
            4,
            miniBoss,
            1,
            0.8f,
            0.9f
        );

        yield return RunAmbientWindow(
            miniBossPhaseDuration,
            AmbientIntensity.MiniBossSupport,
            lowValueFish,
            midValueFish,
            highValueFish,
            null,
            null
        );
    }

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

    private IEnumerator RunFinalPhase(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish,
        FishData pufferfish,
        FishData squid)
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
            "마감 조업";

        ShowAnnouncement(
            "마지막 대규모 어군이 접근 중입니다.",
            largeSchoolWarningTime
        );

        yield return new WaitForSeconds(
            largeSchoolWarningTime
        );

        SpawnMixedFinalSchool(
            lowValueFish,
            midValueFish,
            highValueFish,
            pufferfish,
            squid
        );

        spawningFinished = true;

        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance
                .BeginFinalFishing();
        }
    }

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
                return new Vector2(
                    6f,
                    8f
                );

            case AmbientIntensity.Growth:
                return new Vector2(
                    6f,
                    8f
                );

            case AmbientIntensity.Special:
                return new Vector2(
                    6f,
                    8f
                );

            case AmbientIntensity.MiniBossSupport:
                return new Vector2(
                    7f,
                    9f
                );

            case AmbientIntensity.Rush:
                return new Vector2(
                    6f,
                    8f
                );

            case AmbientIntensity.Final:
                return new Vector2(
                    5f,
                    7f
                );

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
            Random.Range(
                0,
                100
            );

        if (roll < 45)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(
                    1,
                    4
                )
            );

            return;
        }

        if (roll < 75)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    1,
                    3
                )
            );

            return;
        }

        SpawnSchool(
            lowValueFish,
            Random.Range(
                4,
                7
            ),
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
            Random.Range(
                0,
                100
            );

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
                Random.Range(
                    2,
                    5
                )
            );

            return;
        }

        if (roll < 70)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    1,
                    4
                )
            );

            return;
        }

        if (roll < 88)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(
                    5,
                    9
                ),
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
            Random.Range(
                0,
                100
            );

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
                Random.Range(
                    2,
                    5
                )
            );

            return;
        }

        if (roll < 60)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    2,
                    5
                )
            );

            return;
        }

        if (roll < 78)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(
                    6,
                    10
                ),
                0.8f,
                0.95f
            );

            return;
        }

        if (roll < 90)
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
            Random.Range(
                0,
                100
            );

        if (roll < 50)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(
                    1,
                    4
                )
            );

            return;
        }

        if (roll < 80)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    1,
                    3
                )
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
            Random.Range(
                0,
                100
            );

        if (roll < 20)
        {
            SpawnLooseFish(
                highValueFish,
                1
            );

            return;
        }

        if (roll < 45)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(
                    3,
                    6
                )
            );

            return;
        }

        if (roll < 65)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    2,
                    5
                )
            );

            return;
        }

        if (roll < 82)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(
                    7,
                    11
                ),
                0.9f,
                1f
            );

            return;
        }

        if (roll < 91)
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
            Random.Range(
                0,
                100
            );

        if (roll < 15)
        {
            SpawnLooseFish(
                highValueFish,
                Random.Range(
                    1,
                    3
                )
            );

            return;
        }

        if (roll < 35)
        {
            SpawnLooseFish(
                lowValueFish,
                Random.Range(
                    4,
                    7
                )
            );

            return;
        }

        if (roll < 55)
        {
            SpawnLooseFish(
                midValueFish,
                Random.Range(
                    3,
                    6
                )
            );

            return;
        }

        if (roll < 75)
        {
            SpawnSchool(
                lowValueFish,
                Random.Range(
                    8,
                    13
                ),
                1f,
                1.05f
            );

            return;
        }

        if (roll < 87)
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

    private void ShowAnnouncement(
        string text,
        float duration)
    {
        announcementText =
            text;

        announcementEndTime =
            Time.time +
            duration;
    }

    private void CreatePool()
    {
        for (int i = 0;
             i < poolSize;
             i++)
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

    private FishData
        GetStandardFishByValueRank(
            int rank)
    {
        List<FishData> validTypes =
            new List<FishData>();

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
                    cameraLeft
                    - spawnMargin
                    + offsetX,
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
                    -schoolSpreadX
                    * spreadXMultiplier,
                    0f
                );

            float offsetY =
                Random.Range(
                    -data.SchoolSpawnSpreadY
                    * spreadYMultiplier,
                    data.SchoolSpawnSpreadY
                    * spreadYMultiplier
                );

            Vector3 spawnPosition =
                new Vector3(
                    cameraLeft
                    - spawnMargin
                    + offsetX,
                    centerY
                    + offsetY,
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

    private void SpawnMixedFinalSchool(
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
            Debug.LogWarning(
                "FishSpawner: 비활성 Fish가 부족합니다."
            );

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
                    anyActive =
                        true;

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

    private float GetCameraLeft()
    {
        return
            mainCamera.transform.position.x
            -
            mainCamera.orthographicSize
            * mainCamera.aspect;
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

        return null;
    }
}