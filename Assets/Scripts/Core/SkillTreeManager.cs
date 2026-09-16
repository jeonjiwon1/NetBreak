using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SkillTreeManager : MonoBehaviour
{
    private static readonly ToolId[] CorePool =
    {
        ToolId.FishingRod,
        ToolId.Net,
        ToolId.CastNet
    };

    private static readonly ToolId[] PartnerPool =
    {
        ToolId.Bait,
        ToolId.FishingRod,
        ToolId.Net,
        ToolId.CastNet
    };

    [Header("Tree Definitions")]
    [Tooltip("같은 Tool/Role의 에셋이 있으면 내장 VS 기본 트리 대신 사용합니다.")]
    [SerializeField] private SkillTreeDefinition[] treeDefinitions =
        Array.Empty<SkillTreeDefinition>();

    [Header("Acquisition")]
    [Min(1)] [SerializeField] private int acquisitionCandidateCount = 3;
    [Min(1)] [SerializeField] private int acquisitionMasteryCost = 1;
    [Min(0f)] [SerializeField] private float inputUnlockDelay = 0.25f;

    [Header("Effect References (Optional)")]
    [SerializeField] private FishingRodPlacementController fishingRodPlacement;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private BaitController bait;

    private readonly List<SkillTreeDefinition> fallbackDefinitions = new();
    private readonly List<ToolId> acquisitionChoices = new();
    private GrowthToolRole mandatoryRole;
    private bool isMandatoryAcquisition;
    private bool isOpen;
    private bool canInteract;
    private float unlockTime;
    private string notice = string.Empty;

    public static SkillTreeManager Instance { get; private set; }
    public bool IsOpen => isOpen;
    public bool IsMandatoryAcquisition => isMandatoryAcquisition;
    public GrowthToolRole MandatoryRole => mandatoryRole;
    public bool CanInteract => isOpen && canInteract;
    public int AcquisitionChoiceCount => acquisitionChoices.Count;
    public int AcquisitionMasteryCost => acquisitionMasteryCost;
    public string Notice => notice;
    public event Action<bool> OpenStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        fallbackDefinitions.AddRange(VerticalSliceSkillTreeDefaults.CreateAll());
        ResolveEffectReferences();
    }

    private void Update()
    {
        UpdateInteractionLock();

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isOpen)
            {
                TryClose();
            }
            else
            {
                OpenForBrowsing();
            }
        }
        else if (isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TryClose();
        }
    }

    public bool RequestMandatoryAcquisition(GrowthToolRole role)
    {
        RunGrowthState growth = GetGrowthState();
        if (growth == null || isMandatoryAcquisition ||
            growth.GetTree(role).Tool != ToolId.None)
        {
            return false;
        }

        mandatoryRole = role;
        isMandatoryAcquisition = true;
        GenerateAcquisitionChoices(role);
        notice = role == GrowthToolRole.Core
            ? "주력 도구를 선택해야 조업을 계속할 수 있습니다."
            : "보조 도구를 선택해야 조업을 계속할 수 있습니다.";
        OpenInternal();
        return acquisitionChoices.Count > 0;
    }

    public void OpenForBrowsing()
    {
        if (isOpen || IsOtherModalOpen())
        {
            return;
        }

        notice = string.Empty;
        OpenInternal();
    }

    public void ToggleFromUI()
    {
        if (isOpen)
        {
            TryClose();
        }
        else
        {
            OpenForBrowsing();
        }
    }

    public bool TryClose()
    {
        if (!isOpen || isMandatoryAcquisition)
        {
            return false;
        }

        isOpen = false;
        canInteract = false;
        OpenStateChanged?.Invoke(false);
        RestoreGameplaySpeed();
        return true;
    }

    public ToolId GetAcquisitionChoice(int index) =>
        index >= 0 && index < acquisitionChoices.Count
            ? acquisitionChoices[index]
            : ToolId.None;

    public bool SelectAcquisitionChoice(int index)
    {
        if (!isMandatoryAcquisition || !CanInteract ||
            RunManager.Instance == null)
        {
            return false;
        }

        ToolId tool = GetAcquisitionChoice(index);
        RunGrowthState growth = RunManager.Instance.GrowthState;
        RunToolLoadout loadout = RunManager.Instance.ToolSlots;
        int slot = mandatoryRole == GrowthToolRole.Core ? 0 : 1;

        if (tool == ToolId.None ||
            !growth.CanAcquireTool(mandatoryRole, tool, acquisitionMasteryCost) ||
            !loadout.CanAcquireToolAt(slot, tool))
        {
            return false;
        }

        // Both operations were fully validated above and execute in one frame.
        if (!growth.TryAcquireTool(mandatoryRole, tool, acquisitionMasteryCost) ||
            !loadout.TryAcquireToolAt(slot, tool))
        {
            Debug.LogError("Growth tool acquisition failed after validation.");
            return false;
        }

        isMandatoryAcquisition = false;
        acquisitionChoices.Clear();
        notice = $"{GetToolName(tool)} 획득 완료";
        RunManager.Instance.ResolveLevelUp();
        return true;
    }

    public void NotifyMasteryPointGranted(int amount)
    {
        if (amount > 0)
        {
            notice = $"레벨 업! 숙련 포인트 +{amount}";
        }
    }

    public void FocusAcquisitionRoot(GrowthToolRole role)
    {
        if (!isMandatoryAcquisition || role != mandatoryRole)
        {
            return;
        }

        notice = role == GrowthToolRole.Core
            ? "주력 도구 후보 중 하나를 선택하세요."
            : "보조 도구 후보 중 하나를 선택하세요.";
    }

    public SkillTreeDefinition GetDefinition(GrowthToolRole role)
    {
        RunGrowthState growth = GetGrowthState();
        return growth == null
            ? null
            : GetDefinition(role, growth.GetTree(role).Tool);
    }

    public SkillTreeDefinition GetDefinition(GrowthToolRole role, ToolId tool)
    {
        SkillTreeDefinition configured = FindDefinition(treeDefinitions, role, tool);
        return configured != null
            ? configured
            : FindDefinition(fallbackDefinitions, role, tool);
    }

    public bool TryPurchaseNode(GrowthToolRole role, string nodeId)
    {
        if (!CanInteract || isMandatoryAcquisition || RunManager.Instance == null)
        {
            return false;
        }

        SkillTreeDefinition definition = GetDefinition(role);
        RunSkillTreeProgress progress = RunManager.Instance.GrowthState.GetTree(role);
        if (definition == null || !definition.TryGetNode(nodeId, out SkillTreeNodeDefinition node))
        {
            return false;
        }

        int previousRank = progress.GetNodeRank(nodeId);
        SkillTreeRankDefinition purchasedRank = node.GetRank(previousRank);
        ResolveEffectReferences();
        if (!CanApplyEffects(purchasedRank))
        {
            notice = "효과 대상을 찾을 수 없어 구매하지 않았습니다.";
            return false;
        }

        if (!RunManager.Instance.GrowthState.TryPurchaseNodeRank(
                role,
                definition,
                nodeId,
                GetProgressionContext()))
        {
            return false;
        }

        ApplyEffects(purchasedRank);
        notice = $"{node.DisplayName} {previousRank + 1}랭크 구매 완료";
        return true;
    }

    public string GetNodeLockReason(
        GrowthToolRole role,
        SkillTreeDefinition definition,
        SkillTreeNodeDefinition node)
    {
        RunGrowthState growth = GetGrowthState();
        if (growth == null || definition == null || node == null)
        {
            return "트리 정보를 확인할 수 없습니다.";
        }

        RunSkillTreeProgress progress = growth.GetTree(role);
        int rank = progress.GetNodeRank(node.NodeId);
        if (rank >= node.MaxRank)
        {
            return "최대 랭크 달성";
        }

        SkillTreeRankDefinition next = node.GetRank(rank);
        SkillTreeProgressionContext context = GetProgressionContext();
        int rankLimit = node.GetCurrentRankLimit(context);
        if (rank >= rankLimit)
        {
            SkillTreeRankUnlockCondition condition = next.UnlockCondition;
            if (context.PlayerLevel < condition.MinimumPlayerLevel)
                return $"플레이어 Lv{condition.MinimumPlayerLevel} 필요";
            if (context.CurrentArea < condition.MinimumArea)
                return $"해역 {condition.MinimumArea} 도달 필요";
            if (condition.RequiresMiniBossClear && !context.HasClearedMiniBoss)
                return "미니보스 포획 필요";
            if (condition.RequiresBossClear && !context.HasClearedBoss)
                return "보스 포획 필요";
            return "현재 진행도에서 랭크가 잠겨 있습니다.";
        }

        IReadOnlyList<SkillTreeNodePrerequisite> prerequisites = node.Prerequisites;
        for (int i = 0; i < prerequisites.Count; i++)
        {
            SkillTreeNodePrerequisite prerequisite = prerequisites[i];
            if (prerequisite == null ||
                progress.GetNodeRank(prerequisite.NodeId) < prerequisite.RequiredRank)
            {
                return prerequisite == null
                    ? "선행 노드 필요"
                    : definition.TryGetNode(
                        prerequisite.NodeId,
                        out SkillTreeNodeDefinition prerequisiteNode)
                        ? $"{prerequisiteNode.DisplayName} {prerequisite.RequiredRank}랭크 필요"
                        : "선행 노드 필요";
            }
        }

        if (!growth.CanSpendMasteryPoints(next.MasteryPointCost))
        {
            return $"숙련 포인트 {next.MasteryPointCost} 필요";
        }

        return string.Empty;
    }

    public bool CanPurchaseNode(
        GrowthToolRole role,
        SkillTreeDefinition definition,
        SkillTreeNodeDefinition node) =>
        CanInteract && !isMandatoryAcquisition &&
        string.IsNullOrEmpty(GetNodeLockReason(role, definition, node));

    public static string GetToolName(ToolId tool) => tool switch
    {
        ToolId.FishingRod => "낚싯대",
        ToolId.Net => "그물",
        ToolId.CastNet => "투망",
        ToolId.Bait => "미끼",
        ToolId.LandingNet => "뜰채",
        _ => "?"
    };

    public static string GetEffectDescription(SkillTreeRankDefinition rank)
    {
        if (rank == null || rank.BalanceEffects.Count == 0)
        {
            return string.Empty;
        }

        List<string> descriptions = new();
        for (int i = 0; i < rank.BalanceEffects.Count; i++)
        {
            SkillTreeBalanceEffect effect = rank.BalanceEffects[i];
            if (effect == null)
            {
                continue;
            }

            string label = effect.EffectId switch
            {
                SkillTreeEffectIds.FishingRodPower => "낚싯대 포획력",
                SkillTreeEffectIds.FishingRodRange => "낚싯대 범위",
                SkillTreeEffectIds.NetLength => "그물 최대 길이",
                SkillTreeEffectIds.NetCount => "그물 설치 상한",
                SkillTreeEffectIds.CastNetPower => "투망 포획력",
                SkillTreeEffectIds.CastNetRadius => "투망 반경",
                SkillTreeEffectIds.BaitRadius => "미끼 유인 반경",
                SkillTreeEffectIds.BaitDuration => "미끼 지속시간",
                _ => "효과"
            };
            descriptions.Add($"{label} +{effect.Value:0.##}");
        }

        return string.Join(", ", descriptions);
    }

    public static string GetRankProgressionDescription(SkillTreeNodeDefinition node)
    {
        if (node == null)
        {
            return string.Empty;
        }

        List<string> ranks = new();
        for (int i = 0; i < node.MaxRank; i++)
        {
            SkillTreeRankDefinition rank = node.GetRank(i);
            if (rank == null)
            {
                continue;
            }

            SkillTreeRankUnlockCondition condition = rank.UnlockCondition;
            List<string> gates = new();
            if (condition.MinimumPlayerLevel > 0)
                gates.Add($"Lv{condition.MinimumPlayerLevel}");
            if (condition.MinimumArea > 1)
                gates.Add($"해역 {condition.MinimumArea}");
            if (condition.RequiresMiniBossClear)
                gates.Add("미니보스 포획");
            if (condition.RequiresBossClear)
                gates.Add("보스 포획");

            string gateText = gates.Count > 0
                ? $", 조건: {string.Join("/", gates)}"
                : string.Empty;
            ranks.Add(
                $"R{i + 1}: {rank.MasteryPointCost}P, {GetEffectDescription(rank)}{gateText}");
        }

        return string.Join("\n", ranks);
    }

    private void OpenInternal()
    {
        bool wasOpen = isOpen;
        isOpen = true;
        canInteract = false;
        unlockTime = Time.realtimeSinceStartup + inputUnlockDelay;
        Time.timeScale = 0f;
        if (!wasOpen)
        {
            OpenStateChanged?.Invoke(true);
        }
    }

    private void UpdateInteractionLock()
    {
        if (!isOpen || canInteract || Time.realtimeSinceStartup < unlockTime)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return;
        }

        canInteract = true;
    }

    private void GenerateAcquisitionChoices(GrowthToolRole role)
    {
        acquisitionChoices.Clear();
        RunGrowthState growth = GetGrowthState();
        ToolId[] pool = role == GrowthToolRole.Core ? CorePool : PartnerPool;
        ToolId excluded = role == GrowthToolRole.Core
            ? growth.SelectedPartnerTool
            : growth.SelectedCoreTool;

        List<ToolId> eligible = new();
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i] != excluded)
            {
                eligible.Add(pool[i]);
            }
        }

        int count = Mathf.Min(acquisitionCandidateCount, eligible.Count);
        while (acquisitionChoices.Count < count)
        {
            int index = UnityEngine.Random.Range(0, eligible.Count);
            acquisitionChoices.Add(eligible[index]);
            eligible.RemoveAt(index);
        }
    }

    private void ApplyEffects(SkillTreeRankDefinition rank)
    {
        if (rank == null)
        {
            return;
        }

        ResolveEffectReferences();
        IReadOnlyList<SkillTreeBalanceEffect> effects = rank.BalanceEffects;
        for (int i = 0; i < effects.Count; i++)
        {
            SkillTreeBalanceEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            switch (effect.EffectId)
            {
                case SkillTreeEffectIds.FishingRodPower:
                    fishingRodPlacement?.IncreaseRodCapturePower(effect.Value);
                    break;
                case SkillTreeEffectIds.FishingRodRange:
                    fishingRodPlacement?.IncreaseRodRange(effect.Value);
                    break;
                case SkillTreeEffectIds.NetLength:
                    netPlacement?.IncreaseMaxLength(effect.Value);
                    break;
                case SkillTreeEffectIds.NetCount:
                    netPlacement?.IncreaseMaxActiveNets(Mathf.RoundToInt(effect.Value));
                    break;
                case SkillTreeEffectIds.CastNetPower:
                    castNet?.IncreaseCapturePower(effect.Value);
                    break;
                case SkillTreeEffectIds.CastNetRadius:
                    castNet?.IncreaseCaptureRadius(effect.Value);
                    break;
                case SkillTreeEffectIds.BaitRadius:
                    bait?.IncreaseAttractionRadius(effect.Value);
                    break;
                case SkillTreeEffectIds.BaitDuration:
                    bait?.IncreaseDuration(effect.Value);
                    break;
                default:
                    Debug.LogWarning($"Unknown skill tree effect: {effect.EffectId}");
                    break;
            }
        }
    }

    private bool CanApplyEffects(SkillTreeRankDefinition rank)
    {
        if (rank == null)
        {
            return false;
        }

        IReadOnlyList<SkillTreeBalanceEffect> effects = rank.BalanceEffects;
        if (effects.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < effects.Count; i++)
        {
            SkillTreeBalanceEffect effect = effects[i];
            if (effect == null)
            {
                return false;
            }

            bool available = effect.EffectId switch
            {
                SkillTreeEffectIds.FishingRodPower => fishingRodPlacement != null,
                SkillTreeEffectIds.FishingRodRange => fishingRodPlacement != null,
                SkillTreeEffectIds.NetLength => netPlacement != null,
                SkillTreeEffectIds.NetCount => netPlacement != null,
                SkillTreeEffectIds.CastNetPower => castNet != null,
                SkillTreeEffectIds.CastNetRadius => castNet != null,
                SkillTreeEffectIds.BaitRadius => bait != null,
                SkillTreeEffectIds.BaitDuration => bait != null,
                _ => false
            };

            if (!available)
            {
                return false;
            }
        }

        return true;
    }

    private void ResolveEffectReferences()
    {
        if (fishingRodPlacement == null)
            fishingRodPlacement = FindFirstObjectByType<FishingRodPlacementController>();
        if (netPlacement == null)
            netPlacement = FindFirstObjectByType<NetPlacementController>();
        if (castNet == null)
            castNet = FindFirstObjectByType<CastNetController>();
        if (bait == null)
            bait = FindFirstObjectByType<BaitController>();
    }

    private SkillTreeProgressionContext GetProgressionContext()
    {
        int level = RunManager.Instance != null ? RunManager.Instance.CurrentLevel : 0;
        bool miniBoss = PrototypeJobManager.Instance != null &&
            PrototypeJobManager.Instance.HasAdvanced;
        bool boss = PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsGameEnded &&
            PrototypeGameFlowManager.Instance.IsSuccess;
        return new SkillTreeProgressionContext(level, 1, miniBoss, boss);
    }

    private static SkillTreeDefinition FindDefinition(
        IEnumerable<SkillTreeDefinition> definitions,
        GrowthToolRole role,
        ToolId tool)
    {
        if (definitions == null)
        {
            return null;
        }

        foreach (SkillTreeDefinition definition in definitions)
        {
            if (definition != null && definition.Role == role && definition.Tool == tool)
            {
                return definition;
            }
        }

        return null;
    }

    private static RunGrowthState GetGrowthState() =>
        RunManager.Instance != null ? RunManager.Instance.GrowthState : null;

    private static bool IsOtherModalOpen() =>
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsChoosingTool) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        (PrototypeGameFlowManager.Instance != null && PrototypeGameFlowManager.Instance.IsGameEnded);

    private static void RestoreGameplaySpeed()
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

    private void OnDisable()
    {
        if (Instance == this)
        {
            if (isOpen)
            {
                isOpen = false;
                OpenStateChanged?.Invoke(false);
                RestoreGameplaySpeed();
            }

            Instance = null;
        }
    }
}
