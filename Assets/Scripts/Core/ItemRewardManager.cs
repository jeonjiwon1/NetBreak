using System.Collections.Generic;
using UnityEngine;

public enum ItemRewardRequestType
{
    Acquisition = 0,
    Upgrade = 1
}

public enum ItemRewardRequestResult
{
    Opened = 0,
    RunUnavailable = 1,
    AnotherModalIsOpen = 2,
    AlreadyPending = 3,
    FullInventoryUpgradeDeferred = 4,
    NoEligibleCandidates = 5,
    UnsupportedRequestType = 6
}

public sealed class ItemRewardManager : MonoBehaviour
{
    public static ItemRewardManager Instance { get; private set; }
    public static bool IsSelectionPendingOrActive =>
        Instance != null && Instance.selectionPending;

    [Header("Item Acquisition")]
    [Min(1)]
    [SerializeField] private int acquisitionCandidateCount = 3;

    private readonly List<ItemDefinition> choices = new();
    private bool selectionPending;
    private bool selectionCommitted;

    public bool IsChoosing => selectionPending;
    public bool CanSelect => selectionPending && !selectionCommitted;
    public int ChoiceCount => choices.Count;
    public int AcquisitionCandidateCount => acquisitionCandidateCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public ItemRewardRequestResult RequestReward(ItemRewardRequestType requestType)
    {
        if (requestType != ItemRewardRequestType.Acquisition)
        {
            return ItemRewardRequestResult.UnsupportedRequestType;
        }

        return RequestAcquisition();
    }

    public ItemRewardRequestResult RequestAcquisition()
    {
        if (selectionPending)
        {
            return ItemRewardRequestResult.AlreadyPending;
        }

        RunItemInventory inventory = GetInventory();
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        if (inventory == null || flow == null || !flow.IsFishingStarted || flow.IsGameEnded)
        {
            return ItemRewardRequestResult.RunUnavailable;
        }

        if (IsOtherModalOpen())
        {
            return ItemRewardRequestResult.AnotherModalIsOpen;
        }

        if (inventory.IsFull)
        {
            Debug.LogWarning("아이템 슬롯이 가득 찼습니다. 아이템 업그레이드 보상은 G6에서 구현됩니다.");
            return ItemRewardRequestResult.FullInventoryUpgradeDeferred;
        }

        GenerateChoices(inventory);
        if (choices.Count == 0)
        {
            Debug.LogWarning("획득 가능한 미보유 아이템이 없습니다.");
            return ItemRewardRequestResult.NoEligibleCandidates;
        }

        selectionCommitted = false;
        selectionPending = true;
        Time.timeScale = 0f;
        return ItemRewardRequestResult.Opened;
    }

    public ItemDefinition GetChoice(int index) =>
        index >= 0 && index < choices.Count ? choices[index] : null;

    public bool SelectChoice(int index)
    {
        if (!CanSelect)
        {
            return false;
        }

        ItemDefinition definition = GetChoice(index);
        RunItemInventory inventory = GetInventory();
        if (definition == null || inventory == null)
        {
            return false;
        }

        selectionCommitted = true;
        if (!inventory.TryAcquire(definition))
        {
            selectionCommitted = false;
            return false;
        }

        choices.Clear();
        selectionPending = false;
        ResumeGameplaySpeed();
        return true;
    }

    [ContextMenu("Development/Open Item Acquisition Reward")]
    private void DevelopmentOpenAcquisitionReward()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Application.isPlaying)
        {
            Debug.LogWarning("아이템 획득 테스트는 Play Mode의 진행 중인 Run에서만 사용할 수 있습니다.");
            return;
        }

        ItemRewardRequestResult result = RequestAcquisition();
        if (result != ItemRewardRequestResult.Opened)
        {
            Debug.LogWarning($"아이템 획득 테스트를 열 수 없습니다: {GetDiagnostic(result)}");
        }
#else
        Debug.LogWarning("아이템 획득 테스트는 Editor 또는 Development Build에서만 사용할 수 있습니다.");
#endif
    }

    private void GenerateChoices(RunItemInventory inventory)
    {
        choices.Clear();
        List<ItemDefinition> eligible = new();
        IReadOnlyList<ItemDefinition> catalog = ItemCatalog.All;
        for (int i = 0; i < catalog.Count; i++)
        {
            if (!inventory.Owns(catalog[i].ItemId))
            {
                eligible.Add(catalog[i]);
            }
        }

        for (int i = eligible.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (eligible[i], eligible[swapIndex]) = (eligible[swapIndex], eligible[i]);
        }

        int count = Mathf.Min(Mathf.Max(1, acquisitionCandidateCount), eligible.Count);
        for (int i = 0; i < count; i++)
        {
            choices.Add(eligible[i]);
        }
    }

    private static RunItemInventory GetInventory() =>
        RunManager.Instance != null && RunManager.Instance.GrowthState != null
            ? RunManager.Instance.GrowthState.ItemInventory
            : null;

    private static bool IsOtherModalOpen() =>
        (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen) ||
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsAcquisitionPending) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        TacticalSkillManager.IsSelectionPendingOrActive ||
        TacticalSkillManager.IsTargeting ||
        SignatureSkillManager.IsTargeting ||
        (PrototypeGameFlowManager.Instance != null &&
         (PrototypeGameFlowManager.Instance.IsGameEnded ||
          PrototypeGameFlowManager.Instance.IsBossRewardPending));

    private static void ResumeGameplaySpeed()
    {
        if (PrototypeGameFlowManager.Instance != null)
        {
            PrototypeGameFlowManager.Instance.ResumeGameplayTimeScale();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private static string GetDiagnostic(ItemRewardRequestResult result) => result switch
    {
        ItemRewardRequestResult.RunUnavailable => "진행 중인 Run이 아닙니다.",
        ItemRewardRequestResult.AnotherModalIsOpen => "다른 필수 선택 또는 모달이 열려 있습니다.",
        ItemRewardRequestResult.AlreadyPending => "아이템 선택이 이미 진행 중입니다.",
        ItemRewardRequestResult.FullInventoryUpgradeDeferred =>
            "아이템 슬롯이 가득 찼습니다. 아이템 업그레이드 보상은 G6에서 구현됩니다.",
        ItemRewardRequestResult.NoEligibleCandidates => "획득 가능한 미보유 아이템이 없습니다.",
        _ => "지원하지 않는 보상 요청입니다."
    };

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        bool wasPending = selectionPending;
        selectionPending = false;
        selectionCommitted = false;
        choices.Clear();
        Instance = null;
        if (wasPending)
        {
            ResumeGameplaySpeed();
        }
    }

    private void OnValidate()
    {
        acquisitionCandidateCount = Mathf.Max(1, acquisitionCandidateCount);
    }
}
