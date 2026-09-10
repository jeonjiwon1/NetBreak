using UnityEngine;
using UnityEngine.InputSystem;

public enum JobType
{
    Beginner,
    CastNetFisher,
    NetFisher,
    Angler,
    LandingNetFisher
}

public class PrototypeJobManager : MonoBehaviour
{
    public static PrototypeJobManager Instance
    {
        get;
        private set;
    }

    [Header("Job Advancement")]
    [SerializeField] private int jobAfterSchoolCount = 3;

    [Header("Input")]
    [SerializeField] private float selectionInputDelay = 0.25f;

    [Header("References")]
    [SerializeField] private FishSpawner fishSpawner;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private FishingRodPlacementController rodPlacement;
    [SerializeField] private LandingNetController landingNet;

    private JobType currentJob =
        JobType.Beginner;

    private bool isChoosingJob;
    private bool jobSelectionTriggered;

    private bool canSelect;
    private float selectionUnlockTime;

    public JobType CurrentJob =>
        currentJob;

    public bool IsChoosingJob =>
        isChoosingJob;

    public bool HasAdvanced =>
        currentJob != JobType.Beginner;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isChoosingJob)
        {
            UpdateSelectionLock();
            return;
        }

        if (jobSelectionTriggered ||
            HasAdvanced)
        {
            return;
        }

        if (fishSpawner == null)
        {
            return;
        }

        if (fishSpawner.SpawnedSchoolCount <
            jobAfterSchoolCount)
        {
            return;
        }

        if (PrototypeAugmentManager.Instance != null &&
            PrototypeAugmentManager.Instance.IsChoosingAugment)
        {
            return;
        }

        ShowJobChoices();
    }

    private void UpdateSelectionLock()
    {
        if (canSelect)
        {
            return;
        }

        if (Time.realtimeSinceStartup <
            selectionUnlockTime)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed)
        {
            return;
        }

        canSelect = true;
    }

    private void ShowJobChoices()
    {
        jobSelectionTriggered = true;
        isChoosingJob = true;

        canSelect = false;

        selectionUnlockTime =
            Time.realtimeSinceStartup +
            selectionInputDelay;

        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (isChoosingJob)
        {
            DrawJobSelection();
            return;
        }

        DrawCurrentJob();
    }

    private void DrawJobSelection()
    {
        float width = 220f;
        float height = 180f;
        float spacing = 15f;

        float totalWidth =
            width * 4f +
            spacing * 3f;

        float startX =
            (Screen.width - totalWidth) *
            0.5f;

        float y =
            Screen.height * 0.5f -
            height * 0.5f;

        GUI.Box(
            new Rect(
                Screen.width * 0.5f - 180f,
                y - 80f,
                360f,
                50f
            ),
            "1차 전직을 선택하세요"
        );

        GUI.enabled = canSelect;

        if (GUI.Button(
            new Rect(
                startX,
                y,
                width,
                height
            ),
            "투망꾼\n\n" +
            "투망 최대 3스택\n" +
            "투망 증강 출현률 증가"
        ))
        {
            SelectJob(
                JobType.CastNetFisher
            );
        }

        if (GUI.Button(
            new Rect(
                startX + width + spacing,
                y,
                width,
                height
            ),
            "그물잡이\n\n" +
            "최대 그물 10개\n" +
            "그물 포획력 대폭 증가\n" +
            "설치 비용 감소"
        ))
        {
            SelectJob(
                JobType.NetFisher
            );
        }

        if (GUI.Button(
            new Rect(
                startX +
                (width + spacing) * 2f,
                y,
                width,
                height
            ),
            "낚시꾼\n\n" +
            "최대 낚싯대 12개\n" +
            "설치 비용 감소\n" +
            "낚싯대 증강 출현률 증가"
        ))
        {
            SelectJob(
                JobType.Angler
            );
        }

        if (GUI.Button(
            new Rect(
                startX +
                (width + spacing) * 3f,
                y,
                width,
                height
            ),
            "뜰채잡이\n\n" +
            "뜰채 범위 2배\n" +
            "최대 8마리 타격\n" +
            "쿨타임마다 자동 사용"
        ))
        {
            SelectJob(
                JobType.LandingNetFisher
            );
        }

        GUI.enabled = true;
    }

    private void SelectJob(
        JobType selectedJob)
    {
        if (!canSelect)
        {
            return;
        }

        currentJob = selectedJob;

        ApplyJobEffect(
            selectedJob
        );

        isChoosingJob = false;

        Time.timeScale = 1f;
    }

    private void ApplyJobEffect(
        JobType job)
    {
        if (PrototypeAugmentManager.Instance != null)
        {
            PrototypeAugmentManager.Instance
                .ResetCategoryWeights();
        }

        switch (job)
        {
            case JobType.CastNetFisher:

                if (PrototypeAugmentManager.Instance != null)
                {
                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.CastNet,
                            3f
                        );

                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.Bait,
                            1.4f
                        );
                }

                if (castNet != null)
                {
                    castNet.EnableCastNetFisherJob();
                }

                break;

            case JobType.NetFisher:

                if (PrototypeAugmentManager.Instance != null)
                {
                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.Net,
                            3f
                        );

                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.Bait,
                            1.4f
                        );
                }

                if (netPlacement != null)
                {
                    netPlacement.EnableNetFisherJob();
                }

                break;

            case JobType.Angler:

                if (PrototypeAugmentManager.Instance != null)
                {
                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.FishingRod,
                            3f
                        );
                }

                if (rodPlacement != null)
                {
                    rodPlacement.EnableAnglerJob();
                }

                break;

            case JobType.LandingNetFisher:

                if (PrototypeAugmentManager.Instance != null)
                {
                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.LandingNet,
                            3f
                        );

                    PrototypeAugmentManager.Instance
                        .SetCategoryWeight(
                            AugmentCategory.Bait,
                            1.25f
                        );
                }

                if (landingNet != null)
                {
                    landingNet.EnableLandingNetFisherJob();
                }

                break;
        }
    }

    private void DrawCurrentJob()
    {
        GUI.Label(
            new Rect(
                Screen.width - 220f,
                20f,
                200f,
                40f
            ),
            $"전직: {GetJobName(currentJob)}"
        );
    }

    private string GetJobName(
        JobType job)
    {
        switch (job)
        {
            case JobType.CastNetFisher:
                return "투망꾼";

            case JobType.NetFisher:
                return "그물잡이";

            case JobType.Angler:
                return "낚시꾼";

            case JobType.LandingNetFisher:
                return "뜰채잡이";

            default:
                return "초보 어부";
        }
    }

    private void OnDisable()
    {
        if (isChoosingJob)
        {
            Time.timeScale = 1f;
        }
    }
}