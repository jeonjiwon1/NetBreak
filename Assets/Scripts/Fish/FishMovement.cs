using UnityEngine;

[RequireComponent(typeof(FishController))]
public class FishMovement : MonoBehaviour
{
    [SerializeField] private float exitMargin = 0.5f;

    [Header("School Movement")]
    [SerializeField] private float schoolCorrectionSpeed = 2f;
    [SerializeField] private float waveAmplitude = 0.4f;
    [SerializeField] private float waveFrequency = 1.5f;

    private Camera mainCamera;
    private FishController fishController;

    private float schoolCenterY;
    private float personalOffsetY;
    private float wavePhase;

    private void Awake()
    {
        mainCamera = Camera.main;
        fishController = GetComponent<FishController>();
    }

    public void InitializeSchoolMovement(float centerY)
    {
        schoolCenterY = centerY;

        personalOffsetY = Random.Range(-0.8f, 0.8f);
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
                    * distanceFactor;

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
}