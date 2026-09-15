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

    [Header("Input")]
    [SerializeField] private float selectionInputDelay = 0.25f;

    [Header("References")]
    [SerializeField] private CastNetController castNet;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private FishingRodPlacementController rodPlacement;
    [SerializeField] private LandingNetController landingNet;

    private JobType currentJob =
        JobType.Beginner;

    private bool isChoosingJob;
    private bool jobSelectionTriggered;
    private bool jobSelectionRequested;

    private bool canSelect;
    private float selectionUnlockTime;

    public JobType CurrentJob =>
        currentJob;

    public string CurrentJobName =>
        GetJobName(
            currentJob
        );

    public bool IsChoosingJob =>
        isChoosingJob;

    public bool CanSelect =>
        canSelect;

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
            HasAdvanced ||
            !jobSelectionRequested)
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

    // =========================================================
    // REQUEST
    // =========================================================

    public void RequestJobSelection()
    {
        if (jobSelectionTriggered ||
            HasAdvanced)
        {
            return;
        }

        jobSelectionRequested = true;
    }

    private void ShowJobChoices()
    {
        jobSelectionRequested = false;
        jobSelectionTriggered = true;

        isChoosingJob = true;

        LockSelectionInput();

        Time.timeScale = 0f;
    }

    // =========================================================
    // INPUT LOCK
    // =========================================================

    private void LockSelectionInput()
    {
        canSelect = false;

        selectionUnlockTime =
            Time.realtimeSinceStartup +
            selectionInputDelay;
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

    // =========================================================
    // CANVAS INPUT
    // =========================================================

    public void SelectJobFromUI(
        JobType selectedJob)
    {
        if (!isChoosingJob ||
            !canSelect)
        {
            return;
        }

        if (selectedJob ==
            JobType.Beginner)
        {
            return;
        }

        currentJob =
            selectedJob;

        ApplyJobEffect(
            selectedJob
        );

        isChoosingJob = false;
        canSelect = false;

        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance
                .ResumeGameplayTimeScale();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    // =========================================================
    // EFFECT
    // =========================================================

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
                    castNet
                        .EnableCastNetFisherJob();
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
                    netPlacement
                        .EnableNetFisherJob();
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
                    rodPlacement
                        .EnableAnglerJob();
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
                    landingNet
                        .EnableLandingNetFisherJob();
                }

                break;
        }
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

        if (Instance == this)
        {
            Instance = null;
        }
    }
}