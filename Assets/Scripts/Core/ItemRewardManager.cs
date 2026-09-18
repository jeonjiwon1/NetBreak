using System;
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

public enum ItemUpgradeConfirmationResult
{
    Success = 0,
    RewardNotPending = 1,
    ConfirmationInProgress = 2,
    ItemNotOffered = 3,
    RewardContextUnavailable = 4,
    ItemNoLongerEligible = 5,
    UpgradeFailed = 6
}

public readonly struct ItemUpgradePreview
{
    public ItemUpgradePreview(
        ItemDefinition definition,
        int currentLevel,
        float currentPrimaryValue,
        float nextPrimaryValue)
    {
        Definition = definition;
        CurrentLevel = currentLevel;
        NextLevel = currentLevel < int.MaxValue ? currentLevel + 1 : int.MaxValue;
        CurrentPrimaryValue = currentPrimaryValue;
        NextPrimaryValue = nextPrimaryValue;
    }

    public ItemDefinition Definition { get; }
    public int CurrentLevel { get; }
    public int NextLevel { get; }
    public float CurrentPrimaryValue { get; }
    public float NextPrimaryValue { get; }
}

public static class ItemUpgradeRewardLogic
{
    public static List<ItemDefinition> GenerateCandidates(
        RunItemInventory inventory,
        ItemEffectManager itemEffects,
        int candidateCount,
        System.Random random = null)
    {
        List<ItemDefinition> eligible = new();
        if (inventory == null || itemEffects == null || candidateCount <= 0)
        {
            return eligible;
        }

        IReadOnlyList<RunItemInstance> ownedItems = inventory.OwnedItems;
        for (int i = 0; i < ownedItems.Count; i++)
        {
            string itemId = ownedItems[i]?.ItemId;
            if (!IsEligibleCandidate(inventory, itemEffects, itemId) ||
                !ItemCatalog.TryGet(itemId, out ItemDefinition definition))
            {
                continue;
            }

            eligible.Add(definition);
        }

        random ??= new System.Random(Guid.NewGuid().GetHashCode());
        for (int i = eligible.Count - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (eligible[i], eligible[swapIndex]) = (eligible[swapIndex], eligible[i]);
        }

        int resultCount = Math.Min(Math.Max(1, candidateCount), eligible.Count);
        if (resultCount < eligible.Count)
        {
            eligible.RemoveRange(resultCount, eligible.Count - resultCount);
        }

        return eligible;
    }

    public static bool IsEligibleCandidate(
        RunItemInventory inventory,
        ItemEffectManager itemEffects,
        string itemId)
    {
        return inventory != null &&
            itemEffects != null &&
            ItemCatalog.TryGet(itemId, out _) &&
            inventory.Owns(itemId) &&
            itemEffects.CanUpgradeItem(itemId, inventory, out _);
    }
}

public sealed class ItemUpgradeRewardState
{
    private readonly List<string> offeredItemIds = new();
    private bool confirmationInProgress;

    public bool IsPending { get; private set; }
    public bool IsConsumed { get; private set; }
    public int OfferedCount => offeredItemIds.Count;

    public bool TryBegin(IReadOnlyList<ItemDefinition> candidates)
    {
        if (IsPending || candidates == null || candidates.Count == 0)
        {
            return false;
        }

        offeredItemIds.Clear();
        for (int i = 0; i < candidates.Count; i++)
        {
            ItemDefinition candidate = candidates[i];
            if (candidate == null ||
                !ItemCatalog.TryGet(candidate.ItemId, out _) ||
                Contains(candidate.ItemId))
            {
                continue;
            }

            offeredItemIds.Add(candidate.ItemId);
        }

        if (offeredItemIds.Count == 0)
        {
            return false;
        }

        confirmationInProgress = false;
        IsConsumed = false;
        IsPending = true;
        return true;
    }

    public bool Contains(string itemId)
    {
        for (int i = 0; i < offeredItemIds.Count; i++)
        {
            if (string.Equals(offeredItemIds[i], itemId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public ItemUpgradeConfirmationResult TryConfirm(
        string itemId,
        RunItemInventory inventory,
        ItemEffectManager itemEffects,
        out ItemUpgradeResult upgradeResult)
    {
        upgradeResult = ItemUpgradeResult.ConfigurationUnavailable;
        if (!IsPending || IsConsumed)
        {
            return ItemUpgradeConfirmationResult.RewardNotPending;
        }

        if (confirmationInProgress)
        {
            return ItemUpgradeConfirmationResult.ConfirmationInProgress;
        }

        if (!Contains(itemId))
        {
            return ItemUpgradeConfirmationResult.ItemNotOffered;
        }

        if (inventory == null || itemEffects == null)
        {
            return ItemUpgradeConfirmationResult.RewardContextUnavailable;
        }

        confirmationInProgress = true;
        try
        {
            if (!itemEffects.CanUpgradeItem(itemId, inventory, out upgradeResult))
            {
                return ItemUpgradeConfirmationResult.ItemNoLongerEligible;
            }

            if (!itemEffects.TryUpgradeItem(itemId, inventory, out upgradeResult))
            {
                return ItemUpgradeConfirmationResult.UpgradeFailed;
            }

            IsConsumed = true;
            IsPending = false;
            offeredItemIds.Clear();
            return ItemUpgradeConfirmationResult.Success;
        }
        finally
        {
            confirmationInProgress = false;
        }
    }

    public void Reset()
    {
        offeredItemIds.Clear();
        confirmationInProgress = false;
        IsPending = false;
        IsConsumed = false;
    }
}

public sealed class ItemRewardManager : MonoBehaviour
{
    public static ItemRewardManager Instance { get; private set; }
    public static bool IsSelectionPendingOrActive =>
        Instance != null && Instance.selectionPending;

    [Header("Item Acquisition")]
    [Min(1)]
    [SerializeField] private int acquisitionCandidateCount = 3;

    [Header("Item Upgrade")]
    [Min(1)]
    [SerializeField] private int upgradeCandidateCount = 2;

    private readonly List<ItemDefinition> choices = new();
    private readonly ItemUpgradeRewardState upgradeReward = new();
    private bool selectionPending;
    private bool selectionCommitted;
    private ItemRewardRequestType activeRequestType;
    private RunItemInventory rewardInventory;

    public bool IsChoosing => selectionPending;
    public bool CanSelect => selectionPending && !selectionCommitted;
    public int ChoiceCount => choices.Count;
    public int AcquisitionCandidateCount => acquisitionCandidateCount;
    public int UpgradeCandidateCount => upgradeCandidateCount;
    public ItemRewardRequestType ActiveRequestType => activeRequestType;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (selectionPending && !ReferenceEquals(rewardInventory, GetInventory()))
        {
            Debug.LogWarning("새 Run 상태가 감지되어 진행 중이던 아이템 보상을 초기화했습니다.");
            ClearPendingReward(true);
            return;
        }

        if (selectionPending && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }
    }

    public ItemRewardRequestResult RequestReward(ItemRewardRequestType requestType) =>
        requestType switch
        {
            ItemRewardRequestType.Acquisition => RequestAcquisition(),
            ItemRewardRequestType.Upgrade => RequestUpgrade(),
            _ => ItemRewardRequestResult.UnsupportedRequestType
        };

    public ItemRewardRequestResult RequestAcquisition()
    {
        ItemRewardRequestResult validation = ValidateRequest(out RunItemInventory inventory);
        if (validation != ItemRewardRequestResult.Opened)
        {
            return validation;
        }

        if (inventory.IsFull)
        {
            Debug.LogWarning("아이템 슬롯이 가득 찼습니다. 획득 요청은 업그레이드 요청으로 자동 전환되지 않습니다.");
            return ItemRewardRequestResult.FullInventoryUpgradeDeferred;
        }

        GenerateAcquisitionChoices(inventory);
        if (choices.Count == 0)
        {
            Debug.LogWarning("획득 가능한 미보유 아이템이 없습니다.");
            return ItemRewardRequestResult.NoEligibleCandidates;
        }

        return OpenReward(ItemRewardRequestType.Acquisition, inventory);
    }

    public ItemRewardRequestResult RequestUpgrade()
    {
        ItemRewardRequestResult validation = ValidateRequest(out RunItemInventory inventory);
        if (validation != ItemRewardRequestResult.Opened)
        {
            return validation;
        }

        ItemEffectManager itemEffects = GetItemEffects();
        if (itemEffects == null)
        {
            return ItemRewardRequestResult.RunUnavailable;
        }

        choices.Clear();
        choices.AddRange(ItemUpgradeRewardLogic.GenerateCandidates(
            inventory,
            itemEffects,
            upgradeCandidateCount));
        if (choices.Count == 0)
        {
            Debug.LogWarning("업그레이드 가능한 보유 아이템이 없습니다. 최대 레벨과 아이템 설정을 확인하세요.");
            return ItemRewardRequestResult.NoEligibleCandidates;
        }

        if (!upgradeReward.TryBegin(choices))
        {
            choices.Clear();
            return ItemRewardRequestResult.NoEligibleCandidates;
        }

        return OpenReward(ItemRewardRequestType.Upgrade, inventory);
    }

    public ItemDefinition GetChoice(int index) =>
        index >= 0 && index < choices.Count ? choices[index] : null;

    public bool TryGetUpgradePreview(int index, out ItemUpgradePreview preview)
    {
        preview = default;
        if (!selectionPending || activeRequestType != ItemRewardRequestType.Upgrade ||
            !ReferenceEquals(rewardInventory, GetInventory()))
        {
            return false;
        }

        ItemDefinition definition = GetChoice(index);
        RunItemInstance owned = definition != null
            ? rewardInventory.GetOwned(definition.ItemId)
            : null;
        ItemEffectManager itemEffects = GetItemEffects();
        if (definition == null || owned == null || owned.Level < 1 ||
            owned.Level == int.MaxValue || itemEffects == null)
        {
            return false;
        }

        preview = new ItemUpgradePreview(
            definition,
            owned.Level,
            itemEffects.GetEffectivePrimaryValue(definition.ItemId, owned.Level),
            itemEffects.GetEffectivePrimaryValue(definition.ItemId, owned.Level + 1));
        return true;
    }

    public bool SelectChoice(int index)
    {
        if (!CanSelect)
        {
            return false;
        }

        ItemDefinition definition = GetChoice(index);
        RunItemInventory inventory = GetInventory();
        if (definition == null || inventory == null ||
            !ReferenceEquals(inventory, rewardInventory))
        {
            return false;
        }

        selectionCommitted = true;
        bool succeeded;
        if (activeRequestType == ItemRewardRequestType.Upgrade)
        {
            ItemUpgradeConfirmationResult confirmation = upgradeReward.TryConfirm(
                definition.ItemId,
                inventory,
                GetItemEffects(),
                out ItemUpgradeResult upgradeResult);
            succeeded = confirmation == ItemUpgradeConfirmationResult.Success;
            if (!succeeded)
            {
                Debug.LogWarning(
                    $"아이템 업그레이드 선택을 완료하지 못했습니다: " +
                    $"{GetUpgradeConfirmationDiagnostic(confirmation, upgradeResult)}");
            }
        }
        else
        {
            succeeded = inventory.TryAcquire(definition);
        }

        if (!succeeded)
        {
            selectionCommitted = false;
            return false;
        }

        ClearPendingReward(true);
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
            Debug.LogWarning($"아이템 획득 테스트를 열 수 없습니다: {GetDiagnostic(result, false)}");
        }
#else
        Debug.LogWarning("아이템 획득 테스트는 Editor 또는 Development Build에서만 사용할 수 있습니다.");
#endif
    }

    [ContextMenu("Development/Open Item Upgrade Reward")]
    private void DevelopmentOpenUpgradeReward()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Application.isPlaying)
        {
            Debug.LogWarning("아이템 업그레이드 보상 테스트는 Play Mode의 진행 중인 Run에서만 사용할 수 있습니다.");
            return;
        }

        ItemRewardRequestResult result = RequestUpgrade();
        if (result != ItemRewardRequestResult.Opened)
        {
            Debug.LogWarning($"아이템 업그레이드 보상 테스트를 열 수 없습니다: {GetDiagnostic(result, true)}");
        }
#else
        Debug.LogWarning("아이템 업그레이드 보상 테스트는 Editor 또는 Development Build에서만 사용할 수 있습니다.");
#endif
    }

    private ItemRewardRequestResult ValidateRequest(out RunItemInventory inventory)
    {
        inventory = null;
        if (selectionPending)
        {
            return ItemRewardRequestResult.AlreadyPending;
        }

        inventory = GetInventory();
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        if (inventory == null || flow == null || !flow.IsFishingStarted || flow.IsGameEnded)
        {
            return ItemRewardRequestResult.RunUnavailable;
        }

        return IsOtherModalOpen()
            ? ItemRewardRequestResult.AnotherModalIsOpen
            : ItemRewardRequestResult.Opened;
    }

    private ItemRewardRequestResult OpenReward(
        ItemRewardRequestType requestType,
        RunItemInventory inventory)
    {
        activeRequestType = requestType;
        rewardInventory = inventory;
        selectionCommitted = false;
        selectionPending = true;
        Time.timeScale = 0f;
        return ItemRewardRequestResult.Opened;
    }

    private void GenerateAcquisitionChoices(RunItemInventory inventory)
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
            int swapIndex = UnityEngine.Random.Range(0, i + 1);
            (eligible[i], eligible[swapIndex]) = (eligible[swapIndex], eligible[i]);
        }

        int count = Mathf.Min(Mathf.Max(1, acquisitionCandidateCount), eligible.Count);
        for (int i = 0; i < count; i++)
        {
            choices.Add(eligible[i]);
        }
    }

    private void ClearPendingReward(bool resumeGameplay)
    {
        bool wasPending = selectionPending;
        selectionPending = false;
        selectionCommitted = false;
        choices.Clear();
        rewardInventory = null;
        upgradeReward.Reset();
        if (wasPending && resumeGameplay)
        {
            ResumeGameplaySpeed();
        }
    }

    private static RunItemInventory GetInventory() =>
        RunManager.Instance != null && RunManager.Instance.GrowthState != null
            ? RunManager.Instance.GrowthState.ItemInventory
            : null;

    private static ItemEffectManager GetItemEffects() =>
        (RunManager.Instance != null ? RunManager.Instance.ItemEffects : null) ??
        ItemEffectManager.Instance;

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

    private static string GetDiagnostic(ItemRewardRequestResult result, bool isUpgrade) => result switch
    {
        ItemRewardRequestResult.RunUnavailable => "진행 중인 Run이 아닙니다.",
        ItemRewardRequestResult.AnotherModalIsOpen => "다른 필수 선택 또는 모달이 열려 있습니다.",
        ItemRewardRequestResult.AlreadyPending => "아이템 선택이 이미 진행 중입니다.",
        ItemRewardRequestResult.FullInventoryUpgradeDeferred =>
            "아이템 슬롯이 가득 찼으며 획득 요청은 자동 전환되지 않습니다.",
        ItemRewardRequestResult.NoEligibleCandidates => isUpgrade
            ? "업그레이드 가능한 보유 아이템이 없습니다."
            : "획득 가능한 미보유 아이템이 없습니다.",
        _ => "지원하지 않는 보상 요청입니다."
    };

    private static string GetUpgradeConfirmationDiagnostic(
        ItemUpgradeConfirmationResult confirmation,
        ItemUpgradeResult upgradeResult)
    {
        return confirmation switch
        {
            ItemUpgradeConfirmationResult.RewardNotPending => "이미 소비되었거나 대기 중인 보상이 아닙니다.",
            ItemUpgradeConfirmationResult.ConfirmationInProgress => "선택 처리가 이미 진행 중입니다.",
            ItemUpgradeConfirmationResult.ItemNotOffered => "현재 보상에서 제시된 아이템이 아닙니다.",
            ItemUpgradeConfirmationResult.RewardContextUnavailable => "현재 Run의 아이템 상태를 찾을 수 없습니다.",
            ItemUpgradeConfirmationResult.ItemNoLongerEligible or
            ItemUpgradeConfirmationResult.UpgradeFailed => GetUpgradeDiagnostic(upgradeResult),
            _ => "알 수 없는 선택 오류입니다."
        };
    }

    private static string GetUpgradeDiagnostic(ItemUpgradeResult result) => result switch
    {
        ItemUpgradeResult.InvalidItemId => "알 수 없는 아이템 ID입니다.",
        ItemUpgradeResult.ItemNotOwned => "소유하지 않은 아이템입니다.",
        ItemUpgradeResult.InvalidCurrentLevel => "현재 아이템 레벨이 유효하지 않습니다.",
        ItemUpgradeResult.MaximumLevelReached => "설정된 최대 레벨에 도달했습니다.",
        ItemUpgradeResult.LevelOverflow => "레벨 정수 범위를 넘을 수 없습니다.",
        ItemUpgradeResult.ConfigurationUnavailable => "아이템 레벨 설정을 찾을 수 없습니다.",
        _ => "업그레이드할 수 없습니다."
    };

    private void OnDisable()
    {
        if (Instance != this)
        {
            return;
        }

        bool wasPending = selectionPending;
        ClearPendingReward(false);
        Instance = null;
        if (wasPending)
        {
            ResumeGameplaySpeed();
        }
    }

    private void OnValidate()
    {
        acquisitionCandidateCount = Mathf.Max(1, acquisitionCandidateCount);
        upgradeCandidateCount = Mathf.Max(1, upgradeCandidateCount);
    }
}
