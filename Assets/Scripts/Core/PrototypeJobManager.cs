using UnityEngine;

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
    [SerializeField] private int jobLevel = 4;

    [Header("References")]
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private FishingRodPlacementController rodPlacement;
    [SerializeField] private LandingNetController landingNet;

    private JobType currentJob =
        JobType.Beginner;

    private bool isChoosingJob;
    private bool jobSelectionTriggered;

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
        if (jobSelectionTriggered ||
            HasAdvanced ||
            isChoosingJob)
        {
            return;
        }

        if (RunManager.Instance == null)
        {
            return;
        }

        if (RunManager.Instance.CurrentLevel <
            jobLevel)
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

    private void ShowJobChoices()
    {
        jobSelectionTriggered = true;
        isChoosingJob = true;

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
        float height = 150f;
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
            "1Â÷ ÀüÁ÷À» ¼±ÅÃÇÏ¼¼¿ä"
        );

        if (GUI.Button(
            new Rect(
                startX,
                y,
                width,
                height
            ),
            "Åõ¸Á²Û\n\n" +
            "Åõ¸Á Áõ°­ ÃâÇö·ü Áõ°¡\n" +
            "Åõ¸Á ±âº» ¹üÀ§ Áõ°¡"
        ))
        {
            SelectJob(
                JobType.CastNetFisher
            );
        }

        if (GUI.Button(
            new Rect(
                startX +
                (width + spacing),
                y,
                width,
                height
            ),
            "±×¹°ÀâÀÌ\n\n" +
            "±×¹° Áõ°­ ÃâÇö·ü Áõ°¡\n" +
            "ÃÖ´ë ±×¹° +1"
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
            "³¬½Ã²Û\n\n" +
            "³¬½Ë´ë Áõ°­ ÃâÇö·ü Áõ°¡\n" +
            "ÃÖ´ë ³¬½Ë´ë +1"
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
            "¶ãÃ¤ÀâÀÌ\n\n" +
            "¶ãÃ¤ Áõ°­ ÃâÇö·ü Áõ°¡\n" +
            "µ¿½Ã Å¸°Ý ¼ö +2"
        ))
        {
            SelectJob(
                JobType.LandingNetFisher
            );
        }
    }

    private void SelectJob(
        JobType selectedJob)
    {
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
        if (PrototypeAugmentManager.Instance ==
            null)
        {
            return;
        }

        PrototypeAugmentManager.Instance
            .ResetCategoryWeights();

        switch (job)
        {
            case JobType.CastNetFisher:

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

                if (castNet != null)
                {
                    castNet
                        .IncreaseCaptureRadius(
                            0.15f
                        );
                }

                break;

            case JobType.NetFisher:

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

                if (netPlacement != null)
                {
                    netPlacement
                        .IncreaseMaxActiveNets(
                            1
                        );
                }

                break;

            case JobType.Angler:

                PrototypeAugmentManager.Instance
                    .SetCategoryWeight(
                        AugmentCategory.FishingRod,
                        3f
                    );

                if (rodPlacement != null)
                {
                    rodPlacement
                        .IncreaseMaxActiveRods(
                            1
                        );
                }

                break;

            case JobType.LandingNetFisher:

                PrototypeAugmentManager.Instance
                    .SetCategoryWeight(
                        AugmentCategory.LandingNet,
                        3f
                    );

                if (landingNet != null)
                {
                    landingNet
                        .IncreaseMaxTargets(
                            2
                        );
                }

                break;
        }
    }

    private void DrawCurrentJob()
    {
        string jobName =
            GetJobName(
                currentJob
            );

        GUI.Label(
            new Rect(
                Screen.width - 220f,
                20f,
                200f,
                40f
            ),
            $"ÀüÁ÷: {jobName}"
        );
    }

    private string GetJobName(
        JobType job)
    {
        switch (job)
        {
            case JobType.CastNetFisher:
                return "Åõ¸Á²Û";

            case JobType.NetFisher:
                return "±×¹°ÀâÀÌ";

            case JobType.Angler:
                return "³¬½Ã²Û";

            case JobType.LandingNetFisher:
                return "¶ãÃ¤ÀâÀÌ";

            default:
                return "ÃÊº¸ ¾îºÎ";
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