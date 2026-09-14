using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrototypeSelectionCanvas : MonoBehaviour
{
    [Header("Job Selection")]
    [SerializeField] private GameObject jobSelectionPanel;

    [SerializeField] private Button castNetJobButton;
    [SerializeField] private Button netJobButton;
    [SerializeField] private Button anglerJobButton;
    [SerializeField] private Button landingNetJobButton;

    [Header("Augment Selection")]
    [SerializeField] private GameObject augmentSelectionPanel;

    [SerializeField] private Button augmentButton1;
    [SerializeField] private Button augmentButton2;
    [SerializeField] private Button augmentButton3;

    [SerializeField] private TMP_Text augmentText1;
    [SerializeField] private TMP_Text augmentText2;
    [SerializeField] private TMP_Text augmentText3;

    [Header("Reroll")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text rerollText;

    private TMP_Text selectionTitleText;
    private string defaultSelectionTitle;

    private void Awake()
    {
        FindSelectionTitle();

        if (castNetJobButton != null)
        {
            castNetJobButton.onClick.AddListener(
                () =>
                    SelectJob(
                        JobType.CastNetFisher
                    )
            );
        }

        if (netJobButton != null)
        {
            netJobButton.onClick.AddListener(
                () =>
                    SelectJob(
                        JobType.NetFisher
                    )
            );
        }

        if (anglerJobButton != null)
        {
            anglerJobButton.onClick.AddListener(
                () =>
                    SelectJob(
                        JobType.Angler
                    )
            );
        }

        if (landingNetJobButton != null)
        {
            landingNetJobButton.onClick.AddListener(
                () =>
                    SelectJob(
                        JobType.LandingNetFisher
                    )
            );
        }

        if (augmentButton1 != null)
        {
            augmentButton1.onClick.AddListener(
                () =>
                    SelectPrimaryChoice(
                        0
                    )
            );
        }

        if (augmentButton2 != null)
        {
            augmentButton2.onClick.AddListener(
                () =>
                    SelectPrimaryChoice(
                        1
                    )
            );
        }

        if (augmentButton3 != null)
        {
            augmentButton3.onClick.AddListener(
                () =>
                    SelectPrimaryChoice(
                        2
                    )
            );
        }

        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(
                HandleReroll
            );
        }
    }

    private void Start()
    {
        if (jobSelectionPanel != null)
        {
            jobSelectionPanel.SetActive(
                false
            );
        }

        if (augmentSelectionPanel != null)
        {
            augmentSelectionPanel.SetActive(
                false
            );
        }
    }

    private void Update()
    {
        UpdateJobSelection();
        UpdateAugmentSelection();
    }

    // =========================================================
    // JOB
    // =========================================================

    private void UpdateJobSelection()
    {
        PrototypeJobManager manager =
            PrototypeJobManager.Instance;

        bool shouldShow =
            manager != null &&
            manager.IsChoosingJob;

        if (jobSelectionPanel != null &&
            jobSelectionPanel.activeSelf !=
            shouldShow)
        {
            jobSelectionPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow ||
            manager == null)
        {
            return;
        }

        bool interactable =
            manager.CanSelect;

        SetButtonInteractable(
            castNetJobButton,
            interactable
        );

        SetButtonInteractable(
            netJobButton,
            interactable
        );

        SetButtonInteractable(
            anglerJobButton,
            interactable
        );

        SetButtonInteractable(
            landingNetJobButton,
            interactable
        );
    }

    private void SelectJob(
        JobType job)
    {
        PrototypeJobManager manager =
            PrototypeJobManager.Instance;

        if (manager == null)
        {
            return;
        }

        manager.SelectJobFromUI(
            job
        );
    }

    // =========================================================
    // AUGMENT
    // =========================================================

    private void UpdateAugmentSelection()
    {
        PrototypeAugmentManager manager =
            PrototypeAugmentManager.Instance;

        ToolAcquisitionManager acquisition =
            ToolAcquisitionManager.Instance;

        bool showingTools =
            acquisition != null &&
            acquisition.IsChoosingTool;

        bool shouldShow =
            showingTools ||
            (manager != null &&
             manager.IsShowingChoices);

        if (augmentSelectionPanel != null &&
            augmentSelectionPanel.activeSelf !=
            shouldShow)
        {
            augmentSelectionPanel.SetActive(
                shouldShow
            );
        }

        if (!shouldShow)
        {
            return;
        }

        if (showingTools)
        {
            if (selectionTitleText != null)
            {
                selectionTitleText.text = "도구를 선택하세요";
            }

            UpdateToolChoice(acquisition, 0, augmentButton1, augmentText1);
            UpdateToolChoice(acquisition, 1, augmentButton2, augmentText2);
            UpdateToolChoice(acquisition, 2, augmentButton3, augmentText3);

            if (rerollButton != null)
            {
                rerollButton.gameObject.SetActive(false);
            }

            return;
        }

        if (manager == null)
        {
            return;
        }

        if (selectionTitleText != null)
        {
            selectionTitleText.text = defaultSelectionTitle;
        }

        if (rerollButton != null &&
            !rerollButton.gameObject.activeSelf)
        {
            rerollButton.gameObject.SetActive(true);
        }

        UpdateAugmentChoice(
            manager,
            0,
            augmentButton1,
            augmentText1
        );

        UpdateAugmentChoice(
            manager,
            1,
            augmentButton2,
            augmentText2
        );

        UpdateAugmentChoice(
            manager,
            2,
            augmentButton3,
            augmentText3
        );

        if (rerollText != null)
        {
            rerollText.text =
                $"리롤 {manager.CurrentRerollCost}G";
        }

        if (rerollButton != null)
        {
            rerollButton.interactable =
                manager.CanReroll;
        }
    }

    private void UpdateToolChoice(
        ToolAcquisitionManager manager,
        int index,
        Button button,
        TMP_Text text)
    {
        string name =
            manager.GetChoiceName(index);

        string description =
            manager.GetChoiceDescription(index);

        bool hasChoice =
            !string.IsNullOrEmpty(name);

        if (button != null)
        {
            button.interactable =
                hasChoice &&
                manager.CanSelect;
        }

        if (text != null)
        {
            text.text =
                hasChoice
                    ? $"{name}\n\n{description}"
                    : "";
        }
    }

    private void UpdateAugmentChoice(
        PrototypeAugmentManager manager,
        int index,
        Button button,
        TMP_Text text)
    {
        string name =
            manager.GetChoiceName(
                index
            );

        string description =
            manager.GetChoiceDescription(
                index
            );

        bool hasChoice =
            !string.IsNullOrEmpty(
                name
            );

        if (button != null)
        {
            button.interactable =
                hasChoice &&
                manager.CanSelect;
        }

        if (text != null)
        {
            if (!hasChoice)
            {
                text.text = "";
                return;
            }

            text.text =
                $"{name}\n\n{description}";
        }
    }

    private void SelectPrimaryChoice(
        int index)
    {
        ToolAcquisitionManager acquisition =
            ToolAcquisitionManager.Instance;

        if (acquisition != null &&
            acquisition.IsChoosingTool)
        {
            acquisition.SelectChoiceFromUI(
                index
            );
            return;
        }

        SelectAugment(
            index
        );
    }

    private void SelectAugment(
        int index)
    {
        PrototypeAugmentManager manager =
            PrototypeAugmentManager.Instance;

        if (manager == null)
        {
            return;
        }

        manager.SelectChoiceFromUI(
            index
        );
    }

    private void HandleReroll()
    {
        PrototypeAugmentManager manager =
            PrototypeAugmentManager.Instance;

        if (manager == null)
        {
            return;
        }

        manager.TryRerollFromUI();
    }

    // =========================================================
    // UTILITY
    // =========================================================

    private void FindSelectionTitle()
    {
        if (augmentSelectionPanel == null)
        {
            return;
        }

        TMP_Text[] texts =
            augmentSelectionPanel.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text != augmentText1 &&
                text != augmentText2 &&
                text != augmentText3 &&
                text != rerollText)
            {
                selectionTitleText = text;
                defaultSelectionTitle = text.text;
                return;
            }
        }
    }

    private void SetButtonInteractable(
        Button button,
        bool interactable)
    {
        if (button == null)
        {
            return;
        }

        button.interactable =
            interactable;
    }
}