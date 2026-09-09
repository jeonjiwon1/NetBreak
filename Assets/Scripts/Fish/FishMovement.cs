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
    private int netContactCount = 0;

    private void Awake()
    {
        mainCamera = Camera.main;
        fishController = GetComponent<FishController>();
    }

    public void InitializeSchoolMovement(float centerY)
    {
        schoolCenterY = centerY;

        personalOffsetY = Random.Range(-1.6f, 1.6f);
        wavePhase = Random.Range(0f, Mathf.PI * 2f);
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
                Time.time * waveFrequency + wavePhase
            ) * waveAmplitude;

        float verticalDifference =
            targetY - transform.position.y;

        float schoolStrength =
            fishController.Data.SchoolStrength;

        Vector2 schoolDirection = new Vector2(
            1f,
            verticalDifference
            * schoolCorrectionSpeed
            * schoolStrength
        ).normalized;

        Vector2 finalDirection = schoolDirection;

        BaitController bait = BaitController.Instance;

        if (bait != null && bait.IsActive)
        {
            Vector2 toBait =
                bait.Position
                - (Vector2)transform.position;

            float distance = toBait.magnitude;

            if (distance <= bait.AttractionRadius)
            {
                float distanceFactor =
                    1f - distance / bait.AttractionRadius;

                float baitStrength =
                    fishController.Data.BaitAttraction
                    * distanceFactor
                    * 1.5f;

                baitStrength = Mathf.Clamp01(baitStrength);

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

    public void EnterNet(float slowMultiplier)
    {
        netContactCount++;

        netSpeedMultiplier =
            Mathf.Min(netSpeedMultiplier, slowMultiplier);
    }

    public void ExitNet()
    {
        netContactCount =
            Mathf.Max(0, netContactCount - 1);

        if (netContactCount == 0)
        {
            netSpeedMultiplier = 1f;
        }
    }
}