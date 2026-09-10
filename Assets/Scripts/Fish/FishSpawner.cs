using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishController fishPrefab;
    [SerializeField] private FishData[] fishTypes;

    [Header("Pool")]
    [SerializeField] private int poolSize = 200;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMargin = 0.5f;
    [SerializeField] private float verticalPadding = 1f;
    [SerializeField] private float schoolSpreadX = 1.5f;

    [Header("Encounter Timing")]
    [SerializeField] private float shortInterval = 1.2f;
    [SerializeField] private float normalInterval = 2f;
    [SerializeField] private float largeSchoolWarningTime = 2f;

    private const int CoastEncounterCount = 16;

    private readonly List<FishController>
        fishPool = new();

    private Camera mainCamera;

    private bool hasStarted;
    private bool spawningFinished;

    private int spawnedEncounterCount;

    private string currentPhaseName =
        "조업 준비";

    private string announcementText =
        "";

    private float announcementEndTime;

    public bool HasStarted =>
        hasStarted;

    public bool SpawningFinished =>
        spawningFinished;

    public int SpawnedEncounterCount =>
        spawnedEncounterCount;

    public int TotalEncounterCount =>
        CoastEncounterCount;

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

    // 기존 코드와의 임시 호환용.
    public int SpawnedSchoolCount =>
        spawnedEncounterCount;

    public int TotalSchoolCount =>
        CoastEncounterCount;

    private void Awake()
    {
        mainCamera = Camera.main;

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
            RunCoastEncounterSequence()
        );
    }

    private IEnumerator
        RunCoastEncounterSequence()
    {
        FishData lowValueFish =
            GetFishByValueRank(0);

        FishData midValueFish =
            GetFishByValueRank(1);

        FishData highValueFish =
            GetFishByValueRank(2);

        // -------------------------
        // PHASE 1
        // 잔잔한 초반 조업
        // -------------------------

        currentPhaseName =
            "초반 조업";

        BeginEncounter();

        SpawnLooseFish(
            lowValueFish,
            2
        );

        yield return new WaitForSeconds(
            shortInterval
        );

        BeginEncounter();

        SpawnLooseFish(
            midValueFish,
            1
        );

        yield return new WaitForSeconds(
            shortInterval
        );

        BeginEncounter();

        SpawnLooseFish(
            lowValueFish,
            3
        );

        yield return new WaitForSeconds(
            shortInterval
        );

        BeginEncounter();

        SpawnLooseFish(
            midValueFish,
            2
        );

        yield return new WaitForSeconds(
            shortInterval
        );

        BeginEncounter();

        SpawnSchool(
            lowValueFish,
            6,
            0.6f,
            0.8f
        );

        yield return new WaitForSeconds(
            normalInterval
        );

        // -------------------------
        // PHASE 2
        // 첫 대형 어군
        // -------------------------

        currentPhaseName =
            "첫 대형 어군";

        ShowAnnouncement(
            "대규모 어군이 접근 중입니다.",
            largeSchoolWarningTime
        );

        yield return new WaitForSeconds(
            largeSchoolWarningTime
        );

        BeginEncounter();

        List<FishController>
            firstLargeSchool =
                SpawnSchool(
                    lowValueFish,
                    20,
                    1.2f,
                    1.1f
                );

        // 첫 대형 어군은 어느 정도 정리될 때까지
        // 다음 단계로 넘어가지 않는다.
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

        // -------------------------
        // PHASE 3
        // 전직 이후 / 빌드 성장
        // -------------------------

        currentPhaseName =
            "빌드 성장";

        BeginEncounter();

        SpawnLooseFish(
            highValueFish,
            1
        );

        yield return new WaitForSeconds(
            1.5f
        );

        BeginEncounter();

        SpawnSchool(
            midValueFish,
            4,
            0.5f,
            0.8f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        BeginEncounter();

        SpawnLooseFish(
            highValueFish,
            2
        );

        yield return new WaitForSeconds(
            2f
        );

        BeginEncounter();

        SpawnSchool(
            midValueFish,
            12,
            0.9f,
            1f
        );

        yield return new WaitForSeconds(
            3f
        );

        // -------------------------
        // PHASE 4
        // 대형어 이벤트
        // 실제 미니보스가 들어오면 교체할 자리
        // -------------------------

        currentPhaseName =
            "대형어 출현";

        ShowAnnouncement(
            "대형 개체들이 감지되었습니다.",
            2f
        );

        yield return new WaitForSeconds(
            2f
        );

        BeginEncounter();

        SpawnLooseFish(
            highValueFish,
            3
        );

        yield return new WaitForSeconds(
            3f
        );

        // -------------------------
        // PHASE 5
        // 어군 러시
        // -------------------------

        currentPhaseName =
            "어군 러시";

        BeginEncounter();

        SpawnSchool(
            lowValueFish,
            8,
            0.7f,
            0.9f
        );

        yield return new WaitForSeconds(
            2f
        );

        ShowAnnouncement(
            "대규모 어군이 연속으로 접근합니다.",
            1.5f
        );

        yield return new WaitForSeconds(
            1.5f
        );

        BeginEncounter();

        SpawnSchool(
            lowValueFish,
            24,
            1.3f,
            1.2f
        );

        yield return new WaitForSeconds(
            3f
        );

        BeginEncounter();

        SpawnSchool(
            midValueFish,
            18,
            1.2f,
            1.15f
        );

        yield return new WaitForSeconds(
            2f
        );

        BeginEncounter();

        SpawnLooseFish(
            highValueFish,
            2
        );

        yield return new WaitForSeconds(
            2f
        );

        // -------------------------
        // PHASE 6
        // 마지막 혼합 어군
        // -------------------------

        currentPhaseName =
            "마감 조업";

        ShowAnnouncement(
            "마지막 대규모 어군이 접근 중입니다.",
            2.5f
        );

        yield return new WaitForSeconds(
            2.5f
        );

        BeginEncounter();

        SpawnMixedFinalSchool(
            lowValueFish,
            midValueFish,
            highValueFish
        );

        spawningFinished = true;

        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance
                .BeginFinalFishing();
        }
    }

    private void BeginEncounter()
    {
        spawnedEncounterCount++;
    }

    private void ShowAnnouncement(
        string text,
        float duration)
    {
        announcementText = text;

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

    private FishData GetFishByValueRank(
        int rank)
    {
        List<FishData> validTypes =
            new List<FishData>();

        foreach (FishData data
                 in fishTypes)
        {
            if (data != null)
            {
                validTypes.Add(data);
            }
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

    private List<FishController>
        SpawnLooseFish(
            FishData data,
            int count)
    {
        List<FishController> spawned =
            new List<FishController>();

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
                spawned.Add(fish);
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
            new List<FishController>();

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
                spawned.Add(fish);
            }
        }
    }

    private void SpawnMixedFinalSchool(
        FishData lowValueFish,
        FishData midValueFish,
        FishData highValueFish)
    {
        float centerY =
            Random.Range(
                GetCameraBottom(),
                GetCameraTop()
            );

        List<FishController> unusedList =
            new List<FishController>();

        SpawnSchoolMembers(
            lowValueFish,
            28,
            centerY,
            1.5f,
            1.3f,
            unusedList
        );

        SpawnSchoolMembers(
            midValueFish,
            14,
            centerY,
            1.4f,
            1.25f,
            unusedList
        );

        SpawnSchoolMembers(
            highValueFish,
            3,
            centerY,
            1.1f,
            1.1f,
            unusedList
        );
    }

    private FishController SpawnFish(
        FishData data,
        Vector3 position,
        float movementCenterY)
    {
        FishController fish =
            GetInactiveFish();

        if (fish == null)
        {
            return null;
        }

        fish.transform.position =
            position;

        fish.Initialize(data);

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
            movement
                .InitializeSchoolMovement(
                    movementCenterY
                );
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
            bool anyActive = false;

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

            timer += Time.deltaTime;

            yield return null;
        }
    }

    private float GetCameraLeft()
    {
        return
            mainCamera.transform.position.x
            - mainCamera.orthographicSize
            * mainCamera.aspect;
    }

    private float GetCameraBottom()
    {
        return
            mainCamera.transform.position.y
            - mainCamera.orthographicSize
            + verticalPadding;
    }

    private float GetCameraTop()
    {
        return
            mainCamera.transform.position.y
            + mainCamera.orthographicSize
            - verticalPadding;
    }

    private FishController GetInactiveFish()
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