using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ToolAcquisitionManager : MonoBehaviour
{
    private static readonly ToolId[] DefaultAvailableTools =
    {
        ToolId.Bait,
        ToolId.Net,
        ToolId.CastNet,
        ToolId.FishingRod
    };

    private readonly List<ToolId> availableTools = new(DefaultAvailableTools);
    private readonly ToolId[] currentChoices = new ToolId[3];
    private bool requestPending;
    private bool isChoosingTool;
    private bool canSelect;
    private float selectionUnlockTime;

    public static ToolAcquisitionManager Instance { get; private set; }
    public bool IsChoosingTool => isChoosingTool;
    public bool CanSelect => canSelect;
    public int ChoiceCount => currentChoices.Length;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isChoosingTool)
        {
            UpdateSelectionLock();
            return;
        }

        if (!requestPending || IsAnotherSelectionOpen())
        {
            return;
        }

        ShowChoices();
    }

    public bool RequestToolAcquisition()
    {
        if (requestPending || isChoosingTool || !HasAvailableChoice())
        {
            return false;
        }

        requestPending = true;
        return true;
    }

    public ToolId GetChoice(int index)
    {
        return index >= 0 && index < currentChoices.Length
            ? currentChoices[index]
            : ToolId.None;
    }

    public string GetChoiceName(int index) => GetToolName(GetChoice(index));

    public string GetChoiceDescription(int index)
    {
        ToolId tool = GetChoice(index);
        return tool == ToolId.None
            ? ""
            : "이 도구를 획득합니다.\n첫 번째 빈 액티브 슬롯에 장착됩니다.";
    }

    public void SelectChoiceFromUI(int index)
    {
        if (!isChoosingTool || !canSelect || RunManager.Instance == null)
        {
            return;
        }

        ToolId tool = GetChoice(index);
        if (tool == ToolId.None || !RunManager.Instance.ToolSlots.TryAcquireTool(tool))
        {
            return;
        }

        FinishSelection();
    }

    private void ShowChoices()
    {
        requestPending = false;
        GenerateChoices();
        isChoosingTool = true;
        canSelect = false;
        selectionUnlockTime = Time.realtimeSinceStartup + 0.25f;
        Time.timeScale = 0f;
    }

    private void GenerateChoices()
    {
        RunToolLoadout loadout = RunManager.Instance.ToolSlots;
        int choiceIndex = 0;
        for (int i = 0; i < availableTools.Count && choiceIndex < currentChoices.Length; i++)
        {
            ToolId tool = availableTools[i];
            if (!loadout.OwnsTool(tool))
            {
                currentChoices[choiceIndex++] = tool;
            }
        }

        while (choiceIndex < currentChoices.Length)
        {
            currentChoices[choiceIndex++] = ToolId.None;
        }
    }

    private bool HasAvailableChoice()
    {
        if (RunManager.Instance == null || RunManager.Instance.ToolSlots.FindFirstEmptySlot() < 0)
        {
            return false;
        }

        for (int i = 0; i < availableTools.Count; i++)
        {
            if (!RunManager.Instance.ToolSlots.OwnsTool(availableTools[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAnotherSelectionOpen()
    {
        return (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
            (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob);
    }

    private void UpdateSelectionLock()
    {
        if (canSelect || Time.realtimeSinceStartup < selectionUnlockTime)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return;
        }

        canSelect = true;
    }

    private void FinishSelection()
    {
        isChoosingTool = false;
        canSelect = false;
        Time.timeScale = 1f;
    }

    private static string GetToolName(ToolId tool)
    {
        switch (tool)
        {
            case ToolId.Bait: return "미끼";
            case ToolId.Net: return "그물";
            case ToolId.CastNet: return "투망";
            case ToolId.FishingRod: return "낚싯대";
            case ToolId.LandingNet: return "뜰채";
            default: return "";
        }
    }

    private void OnDisable()
    {
        if (isChoosingTool)
        {
            Time.timeScale = 1f;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
