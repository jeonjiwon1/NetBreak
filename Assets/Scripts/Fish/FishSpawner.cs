using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [Header("Fish")]
    [SerializeField] private FishController fishPrefab;
    [SerializeField] private FishData[] fishTypes;

    [Header("Pool")]
    [SerializeField] private int poolSize = 100;

    [Header("School Spawn")]
    [SerializeField] private float schoolSpawnInterval = 3f;
    [SerializeField] private int minSchoolSize = 6;
    [SerializeField] private int maxSchoolSize = 12;

    [Header("Spawn Area")]
    [SerializeField] private float spawnMargin = 0.5f;
    [SerializeField] private float verticalPadding = 1f;
    [SerializeField] private float schoolSpreadX = 1.5f;
    [SerializeField] private float schoolSpreadY = 1f;

    private readonly List<FishController> fishPool = new();

    private Camera mainCamera;
    private float spawnTimer;

    private void Awake()
    {
        mainCamera = Camera.main;

        CreatePool();
    }

    private void Start()
    {
        SpawnSchool();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= schoolSpawnInterval)
        {
            spawnTimer = 0f;

            SpawnSchool();
        }
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            FishController fish =
                Instantiate(fishPrefab, transform);

            fish.gameObject.SetActive(false);

            fishPool.Add(fish);
        }
    }

    private void SpawnSchool()
    {
        if (fishTypes == null || fishTypes.Length == 0)
        {
            return;
        }

        FishData selectedData =
            fishTypes[Random.Range(0, fishTypes.Length)];

        int schoolSize =
            Random.Range(
                minSchoolSize,
                maxSchoolSize + 1
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
            Random.Range(cameraBottom, cameraTop);

        for (int i = 0; i < schoolSize; i++)
        {
            FishController fish = GetInactiveFish();

            if (fish == null)
            {
                return;
            }

            float offsetX =
                Random.Range(
                    -schoolSpreadX,
                    0f
                );

            float offsetY =
                Random.Range(
                    -schoolSpreadY,
                    schoolSpreadY
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

            fish.transform.position = spawnPosition;

            fish.Initialize(selectedData);

            FishMovement movement =
                fish.GetComponent<FishMovement>();

            movement.InitializeSchoolMovement(
                schoolCenterY
            );

            fish.gameObject.SetActive(true);
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