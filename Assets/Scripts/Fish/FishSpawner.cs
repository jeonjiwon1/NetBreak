using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishController fishPrefab;
    [SerializeField] private FishData[] fishTypes;

    [Header("Pool")]
    [SerializeField] private int poolSize = 200;

    [Header("School Spawn")]
    [SerializeField] private int totalSchoolCount = 6;
    [SerializeField] private float schoolSpawnInterval = 4f;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMargin = 0.5f;
    [SerializeField] private float verticalPadding = 1f;
    [SerializeField] private float schoolSpreadX = 1.5f;

    private readonly List<FishController> fishPool = new();

    private Camera mainCamera;
    private float spawnTimer;

    private int spawnedSchoolCount;
    private bool spawningFinished;
    private bool hasStarted;

    public int SpawnedSchoolCount => spawnedSchoolCount;
    public int TotalSchoolCount => totalSchoolCount;
    public bool SpawningFinished => spawningFinished;
    public bool HasStarted => hasStarted;

    private void Awake()
    {
        mainCamera = Camera.main;

        CreatePool();
    }

    private void Update()
    {
        if (!hasStarted || spawningFinished)
        {
            return;
        }

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= schoolSpawnInterval)
        {
            spawnTimer = 0f;

            SpawnSchool();
        }
    }

    public void StartSpawning()
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;
        spawnTimer = 0f;

        SpawnSchool();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            FishController fish =
                Instantiate(
                    fishPrefab,
                    transform
                );

            fish.gameObject.SetActive(false);

            fishPool.Add(fish);
        }
    }

    private void SpawnSchool()
    {
        if (fishTypes == null ||
            fishTypes.Length == 0 ||
            spawningFinished)
        {
            return;
        }

        FishData selectedData =
            fishTypes[
                Random.Range(
                    0,
                    fishTypes.Length
                )
            ];

        int schoolSize =
            Random.Range(
                selectedData.MinSchoolSize,
                selectedData.MaxSchoolSize + 1
            );

        float cameraLeft =
            mainCamera.transform.position.x
            - mainCamera.orthographicSize
            * mainCamera.aspect;

        float cameraBottom =
            mainCamera.transform.position.y
            - mainCamera.orthographicSize
            + verticalPadding;

        float cameraTop =
            mainCamera.transform.position.y
            + mainCamera.orthographicSize
            - verticalPadding;

        float schoolCenterY =
            Random.Range(
                cameraBottom,
                cameraTop
            );

        for (int i = 0; i < schoolSize; i++)
        {
            FishController fish =
                GetInactiveFish();

            if (fish == null)
            {
                break;
            }

            float offsetX =
                Random.Range(
                    -schoolSpreadX,
                    0f
                );

            float offsetY =
                Random.Range(
                    -selectedData.SchoolSpawnSpreadY,
                    selectedData.SchoolSpawnSpreadY
                );

            Vector3 spawnPosition =
                new Vector3(
                    cameraLeft
                    - spawnMargin
                    + offsetX,
                    schoolCenterY
                    + offsetY,
                    0f
                );

            fish.transform.position =
                spawnPosition;

            fish.Initialize(selectedData);

            if (RunManager.Instance != null)
            {
                RunManager.Instance.RegisterFishSpawned(
                    selectedData
                );
            }

            FishMovement movement =
                fish.GetComponent<FishMovement>();

            movement.InitializeSchoolMovement(
                schoolCenterY
            );

            fish.gameObject.SetActive(true);
        }

        spawnedSchoolCount++;

        if (spawnedSchoolCount >= totalSchoolCount)
        {
            spawningFinished = true;

            if (PrototypeGameFlowManager.Instance != null)
            {
                PrototypeGameFlowManager.Instance
                    .BeginFinalFishing();
            }
        }
    }

    private FishController GetInactiveFish()
    {
        foreach (FishController fish in fishPool)
        {
            if (!fish.gameObject.activeSelf)
            {
                return fish;
            }
        }

        return null;
    }
}