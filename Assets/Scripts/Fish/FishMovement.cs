using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishController))]
public class FishMovement : MonoBehaviour
{
    [SerializeField] private float exitMargin = 0.5f;

    [Header("School Movement")]
    [SerializeField] private float schoolCorrectionSpeed = 1.5f;
    [SerializeField] private float waveAmplitude = 0.4f;
    [SerializeField] private float waveFrequency = 1.5f;

    private Camera mainCamera;
    private FishController fishController;

    private float schoolCenterY;
    private float personalOffsetY;
    private float wavePhase;

    private float netSpeedMultiplier = 1f;

    private readonly Dictionary<NetController, float>
        activeNets = new();

    private void Awake()
    {
        mainCamera = Camera.main;
        fishController = GetComponent<FishController>();
    }

    private void OnDisable()
    {
        activeNets.Clear();
        netSpeedMultiplier = 1f;
    }

    public void InitializeSchoolMovement(float centerY)
    {
        schoolCenterY = centerY;

        personalOffsetY =
            Random.Range(-1.6f, 1.6f);

        wavePhase =
            Random.Range(
                0f,
                Mathf.PI * 2f
            );

        activeNets.Clear();
        netSpeedMultiplier = 1f;
    }

    private void Update()
    {
        if (fishController.Data == null)
        {
            return;
        }

        MoveFish();
        CheckExit();
    }

    private void MoveFish()
    {
        float targetY =
            schoolCenterY
            + personalOffsetY
            + Mathf.Sin(
                Time.time * waveFrequency +
                wavePhase
            ) * waveAmplitude;

        float verticalDifference =
            targetY -
            transform.position.y;

        float schoolStrength =
            fishController.Data.SchoolStrength;

        Vector2 schoolDirection =
            new Vector2(
                1f,
                verticalDifference
                * schoolCorrectionSpeed
                * schoolStrength
            ).normalized;

        Vector2 finalDirection =
            schoolDirection;

        BaitController bait =
            BaitController.Instance;

        if (bait != null &&
            bait.IsActive)
        {
            Vector2 toBait =
                bait.Position -
                (Vector2)transform.position;

            float distance =
                toBait.magnitude;

            if (distance <=
                bait.AttractionRadius)
            {
                float distanceFactor =
                    1f -
                    distance /
                    bait.AttractionRadius;

                float baitStrength =
                    fishController.Data
                        .BaitAttraction
                    * distanceFactor
                    * 1.5f;

                baitStrength =
                    Mathf.Clamp01(
                        baitStrength
                    );

                Vector2 baitDirection =
                    toBait.normalized;

                finalDirection =
                    Vector2.Lerp(
                        schoolDirection,
                        baitDirection,
                        baitStrength
                    ).normalized;
            }
        }

        transform.position +=
            (Vector3)(
                finalDirection
                * fishController.Data.MoveSpeed
                * netSpeedMultiplier
                * Time.deltaTime
            );
    }

    private void CheckExit()
    {
        float cameraRight =
            mainCamera.transform.position.x
            + mainCamera.orthographicSize
            * mainCamera.aspect;

        if (transform.position.x >=
            cameraRight + exitMargin)
        {
            gameObject.SetActive(false);
        }
    }

    public void EnterNet(
        NetController net,
        float slowMultiplier)
    {
        if (net == null)
        {
            return;
        }

        activeNets[net] =
            Mathf.Clamp01(
                slowMultiplier
            );

        RecalculateNetSpeed();
    }

    public void ExitNet(
        NetController net)
    {
        if (net == null)
        {
            return;
        }

        activeNets.Remove(net);

        RecalculateNetSpeed();
    }

    private void RecalculateNetSpeed()
    {
        netSpeedMultiplier = 1f;

        foreach (
            KeyValuePair<
                NetController,
                float
            > pair in activeNets)
        {
            if (pair.Key == null)
            {
                continue;
            }

            netSpeedMultiplier =
                Mathf.Min(
                    netSpeedMultiplier,
                    pair.Value
                );
        }
    }
}