using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-70)]
public sealed class ItemEffectManager : MonoBehaviour
{
    [Header("Shared Targeting / Hit Counting")]
    [Tooltip("Camera pixel viewport 안에서 아이템이 대상으로 삼는 정규화된 실제 조업 영역입니다.")]
    [SerializeField] private Rect gameplayViewport = new(0f, 0.12f, 1f, 0.72f);
    [Min(0.01f)] [SerializeField] private float continuousHitCountInterval = 0.5f;

    [Header("Single-Element Synergy / 단일 속성 시너지")]
    [Tooltip("모든 단일 속성이 공유하는 누적 활성 문턱입니다.")]
    [SerializeField] private SingleElementSynergyThresholds singleElementThresholds = new();

    [Header("Synergy Crowd Control / 시너지 군중제어")]
    [Tooltip("시너지로 추가된 군중제어에만 적용합니다. Resistance 피해에는 적용하지 않습니다.")]
    [SerializeField] private SynergyCrowdControlPolicy synergyCrowdControl = new();

    [Header("Electric Synergy / 전기 시너지")]
    [Tooltip("전기 속성 레벨로 해금되는 독립 패시브의 유일한 효과 설정입니다.")]
    [SerializeField] private ElectricSynergySettings electricSynergy = new();

    [Header("Sword Synergy / 검 시너지")]
    [Tooltip("검 속성 레벨로 해금되는 독립 패시브의 유일한 효과 설정입니다.")]
    [SerializeField] private SwordSynergySettings swordSynergy = new();

    [Header("Ice Synergy / 얼음 시너지")]
    [Tooltip("얼음 속성 레벨로 해금되는 독립 패시브의 유일한 효과 설정입니다.")]
    [SerializeField] private IceSynergySettings iceSynergy = new();

    [Header("Combined Synergy / 복합 시너지")]
    [Tooltip("세 복합 시너지 전투 효과의 유일한 반복 튜닝 설정입니다.")]
    [SerializeField] private CombinedSynergyCombatSettings combinedSynergyCombat = new();

    [Header("Combat VFX / 전투 시각 효과")]
    [Tooltip("전투 판정과 독립적인 임시 VFX 색상, 크기와 재사용 상한입니다.")]
    [SerializeField] private CombatVfxSettings combatVfx = new();

    [Header("Storm Orb / 폭풍 구슬")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int stormOrbMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool stormOrbUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 피해에 레벨마다 더하는 비율입니다. 0.2는 레벨당 기준 피해의 20%입니다.")]
    [InspectorName("Damage Bonus Per Level")]
    [Min(0f)] [SerializeField] private float stormOrbDamageBonusPerLevel = 0.2f;
    [Min(0.1f)] [SerializeField] private float stormOrbAttackInterval = 8f;
    [Min(0f)] [SerializeField] private float stormOrbResistanceDamage = 12f;
    [Min(0f)] [SerializeField] private float stormOrbVisualDuration = 0.35f;

    [Header("Capacitor Coil / 축전 코일")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int capacitorMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool capacitorUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 피해에 레벨마다 더하는 비율입니다. 0.2는 레벨당 기준 피해의 20%입니다.")]
    [InspectorName("Damage Bonus Per Level")]
    [Min(0f)] [SerializeField] private float capacitorDamageBonusPerLevel = 0.2f;
    [Min(1)] [SerializeField] private int capacitorRequiredHits = 5;
    [Min(0f)] [SerializeField] private float capacitorChainRadius = 3f;
    [Min(1)] [SerializeField] private int capacitorMaximumTargets = 2;
    [Min(0f)] [SerializeField] private float capacitorResistanceDamage = 8f;
    [Min(0f)] [SerializeField] private float capacitorVisualDuration = 0.4f;

    [Header("Spectral Scabbard / 유령 검집")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int scabbardMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool scabbardUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 피해에 레벨마다 더하는 비율입니다. 0.25는 레벨당 기준 피해의 25%입니다.")]
    [InspectorName("Damage Bonus Per Level")]
    [Min(0f)] [SerializeField] private float scabbardDamageBonusPerLevel = 0.25f;
    [Min(1)] [SerializeField] private int scabbardRequiredHits = 4;
    [Min(0f)] [SerializeField] private float scabbardResistanceDamage = 16f;
    [Min(0f)] [SerializeField] private float scabbardVisualDuration = 0.35f;

    [Header("Autonomous Sword Array / 자동 검진")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int swordArrayMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool swordArrayUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 피해에 레벨마다 더하는 비율입니다. 0.2는 레벨당 기준 피해의 20%입니다.")]
    [InspectorName("Damage Bonus Per Level")]
    [Min(0f)] [SerializeField] private float swordArrayDamageBonusPerLevel = 0.2f;
    [Min(0.1f)] [SerializeField] private float swordArrayAttackInterval = 10f;
    [Min(0f)] [SerializeField] private float swordArrayResistanceDamage = 18f;
    [Min(0f)] [SerializeField] private float swordArrayVisualDuration = 0.45f;

    [Header("Frost Sigil / 서리 인장")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int frostSigilMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool frostSigilUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 둔화 지속시간에 레벨마다 더하는 초입니다.")]
    [InspectorName("Duration Per Level")]
    [Min(0f)] [SerializeField] private float frostSigilDurationPerLevel = 0.4f;
    [Min(1)] [SerializeField] private int frostSigilRequiredHits = 3;
    [Range(0f, 1f)] [SerializeField] private float frostSigilSlowPercentage = 0.4f;
    [Min(0f)] [SerializeField] private float frostSigilSlowDuration = 2f;

    [Header("Frost Crystal / 빙결 결정")]
    [Tooltip("이 아이템의 유한 최대 레벨입니다. 무제한 옵션이 켜지면 무시됩니다.")]
    [InspectorName("Maximum Level")]
    [Min(1)] [SerializeField] private int frostCrystalMaximumLevel = ItemLevelLimit.DefaultMaximumLevel;
    [Tooltip("켜면 유한 최대 레벨을 무시합니다. int 범위를 넘는 업그레이드는 항상 차단됩니다.")]
    [InspectorName("Unlimited Maximum Level")]
    [SerializeField] private bool frostCrystalUnlimitedMaximumLevel;
    [Tooltip("Lv1 기준 둔화 지속시간에 레벨마다 더하는 초입니다.")]
    [InspectorName("Duration Per Level")]
    [Min(0f)] [SerializeField] private float frostCrystalDurationPerLevel = 0.4f;
    [Min(0.1f)] [SerializeField] private float frostCrystalAttackInterval = 12f;
    [Min(1)] [SerializeField] private int frostCrystalMaximumTargets = 2;
    [Min(0f)] [SerializeField] private float frostCrystalTargetRadius = 3f;
    [Min(0f)] [SerializeField] private float frostCrystalResistanceDamage = 5f;
    [Range(0f, 1f)] [SerializeField] private float frostCrystalSlowPercentage = 0.3f;
    [Min(0f)] [SerializeField] private float frostCrystalSlowDuration = 3f;
    [Min(0f)] [SerializeField] private float frostCrystalVisualDuration = 0.5f;

    private readonly struct ContinuousHitKey : IEquatable<ContinuousHitKey>
    {
        public ContinuousHitKey(int fishId, int sourceId, string attackId)
        {
            FishId = fishId;
            SourceId = sourceId;
            AttackId = attackId ?? string.Empty;
        }

        public int FishId { get; }
        public int SourceId { get; }
        public string AttackId { get; }

        public bool Equals(ContinuousHitKey other) =>
            FishId == other.FishId &&
            SourceId == other.SourceId &&
            string.Equals(AttackId, other.AttackId, StringComparison.Ordinal);

        public override bool Equals(object obj) =>
            obj is ContinuousHitKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = FishId;
                hash = hash * 397 ^ SourceId;
                hash = hash * 397 ^ AttackId.GetHashCode();
                return hash;
            }
        }
    }

    private sealed class SynergyStatusVisual
    {
        public FishController Fish;
        public int LifecycleVersion;
        public LineRenderer Line;
    }

    private enum SwordSynergyVisualStyle
    {
        SoulSlash,
        AdditionalSlash,
        SwordRain
    }

    public static ItemEffectManager Instance { get; private set; }

    private const string ElectricChainDamageId = "electric_synergy.chain_discharge";
    private const string ElectricThunderstormDamageId = "electric_synergy.thunderstorm";
    private const string ElectricStunModifierId = "electric_synergy.stun";
    private const string SwordSoulSlashDamageId = "sword_synergy.soul_slash";
    private const string SwordAdditionalSlashDamageId = "sword_synergy.additional_slash";
    private const string SwordRainDamageId = "sword_synergy.sword_rain";
    private const string IceFrostBurstDamageId = "ice_synergy.frost_burst";
    private const string IceSlowModifierId = "ice_synergy.slow";
    private const string IceFreezeModifierId = "ice_synergy.freeze";
    private const string ThunderSwordPrimaryDamageId = "combined_synergy.thunder_sword.primary";
    private const string ThunderSwordSplashDamageId = "combined_synergy.thunder_sword.splash";
    private const string SuperconductivityDamageId = "combined_synergy.superconductivity";
    private const string FrostSwordDamageId = "combined_synergy.frost_sword";
    private const string FrostSwordSlowModifierId = "combined_synergy.frost_sword.slow";

    private readonly Dictionary<FishController, int> scabbardHitCounts = new();
    private readonly Dictionary<FishController, int> frostSigilHitCounts = new();
    private readonly HashSet<FishController> frostSigilSlowedFish = new();
    private readonly HashSet<FishController> frostCrystalSlowedFish = new();
    private readonly Dictionary<ContinuousHitKey, float> continuousHitTimes = new();
    private readonly HashSet<string> knownOwnedItems = new(StringComparer.Ordinal);
    private readonly List<CombatDamageResult> pendingDamageResults = new();
    private readonly Dictionary<FishController, SynergyStatusVisual>
        electricStunVisuals = new();
    private readonly Dictionary<FishController, SynergyStatusVisual>
        iceFreezeVisuals = new();
    private readonly List<FishController> staleStatusVisuals = new();
    private readonly ElectricSynergyRuntimeState electricSynergyRuntime = new();
    private readonly SwordSynergyRuntimeState swordSynergyRuntime = new();
    private readonly IceSynergyRuntimeState iceSynergyRuntime = new();
    private readonly CombinedSynergyCombatRuntime combinedSynergyRuntime = new();

    private RunItemInventory boundInventory;
    private CombatVfxPool combatVfxPool;
    private bool runWasActive;
    private int capacitorHitCount;
    private float stormOrbRemaining;
    private float swordArrayRemaining;
    private float frostCrystalRemaining;
    private CombinedSynergyId lastObservedCombinedSynergy;

    public int ActiveCombatVfxCount =>
        combatVfxPool?.ActiveCount ?? 0;

    public int CreatedCombatVfxCount =>
        combatVfxPool?.CreatedCount ?? 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        combatVfxPool = new CombatVfxPool(
            transform,
            GetCombatVfxSettings());
    }

    private void Update()
    {
        BindInventoryIfNeeded();
        combatVfxPool?.Tick(Time.deltaTime);

        bool isRunActive = IsRunActive();
        if (!isRunActive)
        {
            if (runWasActive ||
                ActiveCombatVfxCount > 0 ||
                electricStunVisuals.Count > 0 ||
                iceFreezeVisuals.Count > 0 ||
                pendingDamageResults.Count > 0)
            {
                ClearRuntimeState(false);
            }

            runWasActive = false;
            return;
        }

        if (!runWasActive)
        {
            ClearRuntimeState(true);
            RefreshOwnedItems();
            runWasActive = true;
        }

        UpdateSynergyStatusVisuals();
        UpdatePeriodicEffects();
    }

    private void LateUpdate()
    {
        int index = 0;
        while (index < pendingDamageResults.Count)
        {
            CombatDamageResult result = pendingDamageResults[index++];
            HandleCombatDamage(result);
            if (result.CapturedByHit &&
                result.Target != null &&
                result.Target.LifecycleVersion == result.TargetLifecycleVersion)
            {
                NotifyFishUnavailable(result.Target);
            }
        }

        pendingDamageResults.Clear();
    }

    public void QueueCombatDamage(CombatDamageResult result)
    {
        pendingDamageResults.Add(result);
    }

    public void ShowSquidInkAttack(
        Vector2 origin,
        Vector2 target)
    {
        CombatVfxSettings settings =
            GetCombatVfxSettings();

        CreateLightning(
            origin,
            target,
            settings.SquidTrajectoryDuration,
            settings.SquidTrajectoryColor,
            0.13f,
            "SquidInkTrajectoryVisual");

        CreateImpactMarker(
            target,
            settings.SquidImpactColor,
            0.45f,
            0.11f,
            settings.SquidImpactDuration,
            "SquidInkImpactVisual");
    }

    public bool CanUpgradeItem(string itemId, out ItemUpgradeResult result)
    {
        RunItemInventory inventory = GetCurrentInventory();
        return CanUpgradeItem(itemId, inventory, out result);
    }

    public bool CanUpgradeItem(
        string itemId,
        RunItemInventory inventory,
        out ItemUpgradeResult result)
    {
        if (inventory == null || !TryGetLevelLimit(itemId, out ItemLevelLimit limit))
        {
            result = ItemCatalog.TryGet(itemId, out _)
                ? ItemUpgradeResult.ConfigurationUnavailable
                : ItemUpgradeResult.InvalidItemId;
            return false;
        }

        return inventory.CanUpgrade(itemId, limit, out result);
    }

    public bool TryUpgradeItem(string itemId, out ItemUpgradeResult result)
    {
        RunItemInventory inventory = GetCurrentInventory();
        return TryUpgradeItem(itemId, inventory, out result);
    }

    public bool TryUpgradeItem(
        string itemId,
        RunItemInventory inventory,
        out ItemUpgradeResult result)
    {
        if (inventory == null || !TryGetLevelLimit(itemId, out ItemLevelLimit limit))
        {
            result = ItemCatalog.TryGet(itemId, out _)
                ? ItemUpgradeResult.ConfigurationUnavailable
                : ItemUpgradeResult.InvalidItemId;
            return false;
        }

        return inventory.TryUpgrade(itemId, limit, out result);
    }

    public bool TryGetLevelLimit(string itemId, out ItemLevelLimit limit)
    {
        switch (itemId)
        {
            case ItemCatalog.StormOrbId:
                limit = new ItemLevelLimit(
                    stormOrbMaximumLevel,
                    stormOrbUnlimitedMaximumLevel);
                return true;
            case ItemCatalog.CapacitorCoilId:
                limit = new ItemLevelLimit(
                    capacitorMaximumLevel,
                    capacitorUnlimitedMaximumLevel);
                return true;
            case ItemCatalog.SpectralScabbardId:
                limit = new ItemLevelLimit(
                    scabbardMaximumLevel,
                    scabbardUnlimitedMaximumLevel);
                return true;
            case ItemCatalog.AutonomousSwordArrayId:
                limit = new ItemLevelLimit(
                    swordArrayMaximumLevel,
                    swordArrayUnlimitedMaximumLevel);
                return true;
            case ItemCatalog.FrostSigilId:
                limit = new ItemLevelLimit(
                    frostSigilMaximumLevel,
                    frostSigilUnlimitedMaximumLevel);
                return true;
            case ItemCatalog.FrostCrystalId:
                limit = new ItemLevelLimit(
                    frostCrystalMaximumLevel,
                    frostCrystalUnlimitedMaximumLevel);
                return true;
            default:
                limit = default;
                return false;
        }
    }

    public float GetEffectivePrimaryValue(
        string itemId,
        RunItemInventory inventory)
    {
        int level = GetOwnedLevel(inventory, itemId);
        if (level == 0)
        {
            return 0f;
        }

        return GetEffectivePrimaryValue(itemId, level);
    }

    public float GetEffectivePrimaryValue(string itemId, int itemLevel)
    {
        if (itemLevel < 1)
        {
            return 0f;
        }

        return itemId switch
        {
            ItemCatalog.StormOrbId => ItemLevelScaling.CalculateAdditiveDamage(
                stormOrbResistanceDamage, itemLevel, stormOrbDamageBonusPerLevel),
            ItemCatalog.CapacitorCoilId => ItemLevelScaling.CalculateAdditiveDamage(
                capacitorResistanceDamage, itemLevel, capacitorDamageBonusPerLevel),
            ItemCatalog.SpectralScabbardId => ItemLevelScaling.CalculateAdditiveDamage(
                scabbardResistanceDamage, itemLevel, scabbardDamageBonusPerLevel),
            ItemCatalog.AutonomousSwordArrayId => ItemLevelScaling.CalculateAdditiveDamage(
                swordArrayResistanceDamage, itemLevel, swordArrayDamageBonusPerLevel),
            ItemCatalog.FrostSigilId => ItemLevelScaling.CalculateAdditiveDuration(
                frostSigilSlowDuration, itemLevel, frostSigilDurationPerLevel),
            ItemCatalog.FrostCrystalId => ItemLevelScaling.CalculateAdditiveDuration(
                frostCrystalSlowDuration, itemLevel, frostCrystalDurationPerLevel),
            _ => 0f
        };
    }

    public SingleElementSynergyState GetSingleElementSynergyState(
        ItemElement element,
        RunItemInventory inventory) =>
        GetSingleElementThresholds().Evaluate(inventory, element);

    public float GetSynergyCrowdControlDuration(
        float baseDuration,
        FishSpecialType specialType) =>
        GetCrowdControlPolicy().GetAdjustedDuration(baseDuration, specialType);

    public int GetElectricChainTargetCount(SingleElementSynergyState state) =>
        GetElectricSynergySettings().GetEffectiveChainTargetCount(state);

    public float GetElectricChainDamage(SingleElementSynergyState state) =>
        GetElectricSynergySettings().GetEffectiveDamage(
            GetElectricSynergySettings().ChainDamage,
            state);

    public float GetElectricThunderstormDamage(SingleElementSynergyState state) =>
        GetElectricSynergySettings().GetEffectiveDamage(
            GetElectricSynergySettings().ThunderstormDamage,
            state);

    public float GetElectricDischargeStunDuration(
        SingleElementSynergyState state,
        FishSpecialType specialType) =>
        state.IsLevel6Active
            ? GetSynergyCrowdControlDuration(
                GetElectricSynergySettings().StunDuration,
                specialType)
            : 0f;

    public int GetSwordSoulSlashRequiredHits(SingleElementSynergyState state) =>
        GetSwordSynergySettings().GetEffectiveSoulSlashRequiredHits(state);

    public float GetSwordSoulSlashDamage(SingleElementSynergyState state) =>
        GetSwordSynergySettings().GetEffectiveDamage(
            GetSwordSynergySettings().SoulSlashDamage,
            state);

    public float GetSwordAdditionalSlashDamage(SingleElementSynergyState state) =>
        state.IsLevel6Active
            ? GetSwordSynergySettings().GetEffectiveDamage(
                GetSwordSynergySettings().AdditionalSwordDamage,
                state)
            : 0f;

    public float GetSwordRainDamage(SingleElementSynergyState state) =>
        state.IsLevel10Active
            ? GetSwordSynergySettings().GetEffectiveDamage(
                GetSwordSynergySettings().SwordRainDamage,
                state)
            : 0f;

    public int GetIceColdWaveTargetCount(SingleElementSynergyState state) =>
        GetIceSynergySettings().GetEffectiveColdWaveTargetCount(state);

    public float GetIceSynergySlowDuration(
        SingleElementSynergyState state,
        bool frostBurst) =>
        GetIceSynergySettings().GetEffectiveSlowDuration(
            frostBurst
                ? GetIceSynergySettings().FrostBurstSlowDuration
                : GetIceSynergySettings().ColdWaveSlowDuration,
            state);

    public float GetIceSynergySlowPercentage(
        bool frostBurst,
        FishSpecialType specialType) =>
        GetCrowdControlPolicy().GetAdjustedAdditionalSlowPercentage(
            frostBurst
                ? GetIceSynergySettings().FrostBurstSlowPercentage
                : GetIceSynergySettings().ColdWaveSlowPercentage,
            specialType);

    public float GetIceFreezeDuration(
        SingleElementSynergyState state,
        FishSpecialType specialType) =>
        state.IsLevel6Active
            ? GetSynergyCrowdControlDuration(
                GetIceSynergySettings().FreezeDuration,
                specialType)
            : 0f;

    public float GetIceFrostBurstDamage(SingleElementSynergyState state) =>
        state.IsLevel10Active
            ? GetIceSynergySettings().FrostBurstDamage
            : 0f;

    public bool CanProcessCombatTrigger(
        CombatDamageResult result,
        float scaledTime)
    {
        if (float.IsNaN(scaledTime) ||
            !SingleElementSynergyTriggerPolicy.IsValidToolDamage(result))
        {
            return false;
        }

        return !result.Context.IsContinuous ||
            CanCountContinuousHit(result, scaledTime);
    }

    public string BuildElementSynergyTooltipText(
        ItemElement element,
        RunItemInventory inventory)
    {
        SingleElementSynergyState state = GetSingleElementSynergyState(
            element,
            inventory);
        StringBuilder builder = new();
        builder.Append("<size=22><b>");
        builder.Append(GetElementDisplayName(element));
        builder.Append(" 시너지 · 현재 Lv.");
        builder.Append(state.ElementLevel);
        builder.Append("</b></size>\n\n");

        if (element == ItemElement.Electric)
        {
            ElectricSynergySettings settings = GetElectricSynergySettings();
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level2,
                "연쇄 방전",
                $"유효한 도구 피해 적중 시 {settings.ChainCooldown:0.##}초마다 적중 위치 반경 {settings.ChainRadius:0.##} 안의 최대 {settings.ChainTargetCount}마리에게 저항력 피해 {settings.ChainDamage:0.##}.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level4,
                "전도 확장",
                $"연쇄 방전 최대 대상 +{settings.ConductiveAdditionalTargets} (총 {Math.Min((long)settings.ChainTargetCount + settings.ConductiveAdditionalTargets, int.MaxValue)}마리).",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level6,
                "감전 방전",
                $"연쇄 방전 적중 대상을 {settings.StunDuration:0.##}초 기절. 종료 후 {settings.RestunLockout:0.##}초 재기절 제한. MiniBoss ×{GetCrowdControlPolicy().MiniBossMultiplier:0.##}, Boss ×{GetCrowdControlPolicy().BossMultiplier:0.##}.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level8,
                "과충전",
                $"전기 시너지 피해 ×{settings.OverchargeDamageMultiplier:0.##}. 연쇄 방전 {ElectricSynergyMath.CalculateDamage(settings.ChainDamage, settings.OverchargeDamageMultiplier):0.##}, 천둥 폭풍 {ElectricSynergyMath.CalculateDamage(settings.ThunderstormDamage, settings.OverchargeDamageMultiplier):0.##}.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level10,
                "천둥 폭풍",
                $"{settings.ThunderstormCooldown:0.##}초마다 다음 유효 도구 피해에서 화면 안 최대 {settings.ThunderstormTargetCount}마리에게 기본 저항력 피해 {settings.ThunderstormDamage:0.##}.",
                true);
        }
        else if (element == ItemElement.Sword)
        {
            SwordSynergySettings settings = GetSwordSynergySettings();
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level2,
                "영혼 참격",
                $"유효 도구 적중 {settings.SoulSlashRequiredHits}회마다 마지막 유효 대상 우선으로 저항력 피해 {settings.SoulSlashDamage:0.##}의 영혼 검을 소환. 대체 탐색 반경 {settings.FallbackRadius:0.##}.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level4,
                "예리한 영혼",
                $"영혼 참격 필요 적중 -{settings.SharpSoulHitReduction} (최소 1, 기본 설정 기준 {Math.Max(1, settings.SoulSlashRequiredHits - settings.SharpSoulHitReduction)}회).",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level6,
                "쌍검 소환",
                $"같은 발동에서 다른 유효 대상에게 저항력 피해 {settings.AdditionalSwordDamage:0.##}의 추가 검 1개를 소환. 대상이 없으면 생략.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level8,
                "검기 증폭",
                $"검 시너지 피해 ×{settings.AmplificationDamageMultiplier:0.##}. 개별 검 아이템 피해에는 미적용.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level10,
                "검의 비",
                $"성공한 원본 영혼 참격 {settings.SwordRainRequiredActivations}회마다 화면 안 최대 {settings.SwordRainTargetCount}마리에게 각각 기본 저항력 피해 {settings.SwordRainDamage:0.##}.",
                true);
        }
        else
        {
            IceSynergySettings settings = GetIceSynergySettings();
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level2,
                "냉기 파동",
                $"도구 피해로 포획 시 반경 {settings.ColdWaveRadius:0.##} 안의 다른 물고기 최대 {settings.ColdWaveTargetCount}마리를 {settings.ColdWaveSlowDuration:0.##}초간 {settings.ColdWaveSlowPercentage * 100f:0.##}% 둔화.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level4,
                "냉기 확산",
                $"냉기 파동 최대 대상 +{settings.ColdSpreadAdditionalTargets} (총 {Math.Min((long)settings.ColdWaveTargetCount + settings.ColdSpreadAdditionalTargets, int.MaxValue)}마리).",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level6,
                "순간 빙결",
                $"얼음 시너지 대상을 {settings.FreezeDuration:0.##}초 빙결. 종료 후 {settings.RefreezeLockout:0.##}초 재빙결 제한. MiniBoss ×{GetCrowdControlPolicy().MiniBossMultiplier:0.##}, Boss ×{GetCrowdControlPolicy().BossMultiplier:0.##}.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level8,
                "오래가는 한기",
                $"얼음 시너지 둔화 지속시간 +{settings.LingeringChillAdditionalDuration:0.##}초. 빙결 지속시간에는 미적용.",
                true);
            AppendSynergyTier(builder, state, SingleElementSynergyTier.Level10,
                "서리 폭발",
                $"도구 포획 {settings.FrostBurstRequiredCaptures}회마다 냉기 파동을 대체. 반경 {settings.FrostBurstRadius:0.##}, 최대 {settings.FrostBurstTargetCount}마리, 저항력 피해 {settings.FrostBurstDamage:0.##}, {settings.FrostBurstSlowDuration:0.##}초간 {settings.FrostBurstSlowPercentage * 100f:0.##}% 둔화.",
                true);
        }

        return builder.ToString().TrimEnd();
    }

    public void HandleCombatDamage(CombatDamageResult result)
    {
        if (!IsRunActive() || boundInventory == null ||
            !SingleElementSynergyTriggerPolicy.IsValidToolDamage(result))
        {
            return;
        }

        bool recognizedToolHit =
            !result.Context.IsContinuous ||
            CanCountContinuousHit(result, Time.time);
        if (recognizedToolHit)
        {
            HandleRecognizedToolHitForItems(result);
            ActivateIndependentElectricSynergy(result);
            ActivateIndependentSwordSynergy(result);
        }

        // Capture-triggered Ice is an actual capture event, not an additional
        // continuous hit count. Its own lifecycle guard prevents duplicate callbacks.
        if (result.CapturedByHit)
        {
            ActivateIndependentIceSynergy(result);
        }
    }

    private void HandleRecognizedToolHitForItems(CombatDamageResult result)
    {
        bool ownsCoil = Owns(ItemCatalog.CapacitorCoilId);
        bool ownsScabbard = Owns(ItemCatalog.SpectralScabbardId);
        bool ownsSigil = Owns(ItemCatalog.FrostSigilId);
        bool targetStillEligible =
            !result.CapturedByHit &&
            result.Target.LifecycleVersion == result.TargetLifecycleVersion &&
            IsEligibleFish(result.Target);
        bool triggerCoil = false;
        bool triggerScabbard = false;
        bool triggerSigil = false;

        if (ownsCoil)
        {
            capacitorHitCount++;
            if (capacitorHitCount >= capacitorRequiredHits)
            {
                capacitorHitCount = 0;
                triggerCoil = true;
            }
        }

        if (targetStillEligible && ownsScabbard)
        {
            triggerScabbard = IncrementPerFishCounter(
                scabbardHitCounts,
                result.Target,
                scabbardRequiredHits);
        }

        if (targetStillEligible && ownsSigil)
        {
            triggerSigil = IncrementPerFishCounter(
                frostSigilHitCounts,
                result.Target,
                frostSigilRequiredHits);
        }

        if (triggerCoil)
        {
            ActivateCapacitorCoil(result.HitPosition, result.Target);
        }

        if (triggerScabbard && IsEligibleFish(result.Target))
        {
            ActivateSpectralScabbard(result.Target, result.HitPosition);
        }

        if (triggerSigil && IsEligibleFish(result.Target))
        {
            ApplySlow(
                result.Target,
                ItemCatalog.FrostSigilId,
                frostSigilSlowPercentage,
                GetEffectiveSlowDuration(ItemCatalog.FrostSigilId));
            CreateFrostMarker(
                result.Target.transform.position,
                0.65f,
                GetEffectiveSlowDuration(ItemCatalog.FrostSigilId));
        }
    }

    private void AppendSynergyTier(
        StringBuilder builder,
        SingleElementSynergyState state,
        SingleElementSynergyTier tier,
        string koreanName,
        string description,
        bool gameplayImplemented)
    {
        bool requirementMet = state.IsActive(tier);
        string marker;
        string status;
        string color;
        if (gameplayImplemented && requirementMet)
        {
            marker = "✓";
            status = "활성";
            color = "#F5FBFF";
        }
        else if (!gameplayImplemented && requirementMet)
        {
            marker = "◇";
            status = "조건 충족 · 구현 예정";
            color = "#F2C879";
        }
        else
        {
            marker = "○";
            status = gameplayImplemented ? "미해금" : "미해금 · 구현 예정";
            color = "#8C98A3";
        }

        builder.Append("<color=");
        builder.Append(color);
        builder.Append("><b>");
        builder.Append(marker);
        builder.Append(" Lv.");
        builder.Append(GetSingleElementThresholds().GetThreshold(tier));
        builder.Append(" — ");
        builder.Append(koreanName);
        builder.Append(" [");
        builder.Append(status);
        builder.Append("]</b>\n");
        builder.Append(description);
        builder.Append("</color>\n\n");
    }

    private static string GetElementDisplayName(ItemElement element) => element switch
    {
        ItemElement.Electric => "전기",
        ItemElement.Sword => "검",
        ItemElement.Ice => "얼음",
        _ => "알 수 없는 속성"
    };

    public void NotifyFishUnavailable(FishController fish)
    {
        if (fish == null)
        {
            return;
        }

        scabbardHitCounts.Remove(fish);
        frostSigilHitCounts.Remove(fish);
        frostSigilSlowedFish.Remove(fish);
        frostCrystalSlowedFish.Remove(fish);
        electricSynergyRuntime.ClearTarget(fish.GetInstanceID());
        iceSynergyRuntime.ClearTarget(fish.GetInstanceID());
        combinedSynergyRuntime.ClearTarget(fish.GetInstanceID());
        FishMovement movement = fish.GetComponent<FishMovement>();
        movement?.RemoveTimedSpeedModifier(ElectricStunModifierId);
        movement?.RemoveTimedSpeedModifier(IceSlowModifierId);
        movement?.RemoveTimedSpeedModifier(IceFreezeModifierId);
        movement?.RemoveTimedSpeedModifier(FrostSwordSlowModifierId);
        RemoveStatusVisual(electricStunVisuals, fish);
        RemoveStatusVisual(iceFreezeVisuals, fish);

        int fishId = fish.GetInstanceID();
        List<ContinuousHitKey> staleKeys = null;
        foreach (ContinuousHitKey key in continuousHitTimes.Keys)
        {
            if (key.FishId != fishId)
            {
                continue;
            }

            staleKeys ??= new List<ContinuousHitKey>();
            staleKeys.Add(key);
        }

        if (staleKeys == null)
        {
            return;
        }

        for (int i = 0; i < staleKeys.Count; i++)
        {
            continuousHitTimes.Remove(staleKeys[i]);
        }
    }

    public string GetStatusText(string itemId)
    {
        if (!Owns(itemId))
        {
            return string.Empty;
        }

        string runtimeStatus = GetRuntimeStatusText(itemId);

        float primaryValue = GetEffectivePrimaryValue(itemId, boundInventory);
        string primaryStatus = itemId switch
        {
            ItemCatalog.StormOrbId or
            ItemCatalog.CapacitorCoilId or
            ItemCatalog.SpectralScabbardId or
            ItemCatalog.AutonomousSwordArrayId =>
                $"현재 저항력 피해: {primaryValue:0.##}",
            ItemCatalog.FrostSigilId or ItemCatalog.FrostCrystalId =>
                $"현재 둔화 지속시간: {primaryValue:0.##}초",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(runtimeStatus)
            ? primaryStatus
            : $"{primaryStatus}\n{runtimeStatus}";
    }

    public string BuildItemTooltipText(
        RunItemInstance owned,
        RunItemInventory inventory)
    {
        if (owned == null || inventory == null ||
            !ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition))
        {
            return string.Empty;
        }

        int level = Math.Max(1, owned.Level);
        float currentValue = GetEffectivePrimaryValue(owned.ItemId, level);
        StringBuilder builder = new();
        builder.Append("<size=22><b>");
        builder.Append(definition.DisplayName);
        builder.Append("</b></size>\n");
        builder.Append("속성: ");
        builder.Append(definition.ElementDisplayName);
        builder.Append(" · Lv.");
        builder.Append(level);
        builder.Append("\n\n");
        builder.Append(BuildCurrentItemEffectDescription(
            owned.ItemId,
            currentValue));
        builder.Append("\n\n<color=#9DDEF2>");
        builder.Append("현재 ");
        builder.Append(definition.PrimaryEffectDisplayName);
        builder.Append(": ");
        builder.Append(currentValue.ToString("0.##"));
        builder.Append(definition.PrimaryValueSuffix);
        builder.Append("</color>");

        if (TryGetLevelLimit(owned.ItemId, out ItemLevelLimit limit) &&
            inventory.CanUpgrade(owned.ItemId, limit, out _))
        {
            float nextValue = GetEffectivePrimaryValue(owned.ItemId, level + 1);
            builder.Append("\n다음 Lv.");
            builder.Append(level + 1);
            builder.Append(": ");
            builder.Append(nextValue.ToString("0.##"));
            builder.Append(definition.PrimaryValueSuffix);
            builder.Append(" (+");
            builder.Append((nextValue - currentValue).ToString("0.##"));
            builder.Append(definition.PrimaryValueSuffix);
            builder.Append(')');
        }
        else
        {
            builder.Append("\n<b>최대 레벨</b>");
        }

        string runtimeStatus = Owns(owned.ItemId)
            ? GetRuntimeStatusText(owned.ItemId)
            : string.Empty;
        if (!string.IsNullOrEmpty(runtimeStatus))
        {
            builder.Append("\n\n");
            builder.Append(runtimeStatus);
        }

        return builder.ToString();
    }

    private string GetRuntimeStatusText(string itemId) => itemId switch
    {
        ItemCatalog.StormOrbId =>
            $"다음 자동 공격: {Mathf.Max(0f, stormOrbRemaining):0.0}초",
        ItemCatalog.CapacitorCoilId =>
            $"도구 적중: {capacitorHitCount}/{capacitorRequiredHits}",
        ItemCatalog.SpectralScabbardId =>
            $"대상별 도구 적중 {scabbardRequiredHits}회마다 발동",
        ItemCatalog.AutonomousSwordArrayId =>
            $"다음 자동 공격: {Mathf.Max(0f, swordArrayRemaining):0.0}초",
        ItemCatalog.FrostSigilId =>
            $"대상별 적중 {frostSigilRequiredHits}회 · 둔화 중 {CountActiveSlows(ItemCatalog.FrostSigilId)}마리",
        ItemCatalog.FrostCrystalId =>
            $"다음 자동 공격: {Mathf.Max(0f, frostCrystalRemaining):0.0}초 · 둔화 중 {CountActiveSlows(ItemCatalog.FrostCrystalId)}마리",
        _ => string.Empty
    };

    private string BuildCurrentItemEffectDescription(
        string itemId,
        float primaryValue) => itemId switch
    {
        ItemCatalog.StormOrbId =>
            $"{stormOrbAttackInterval:0.##}초마다 화면 안에서 커서에 가장 가까운 물고기에게 저항력 피해 {primaryValue:0.##}.",
        ItemCatalog.CapacitorCoilId =>
            $"유효한 도구 적중 {capacitorRequiredHits}회마다 반경 {capacitorChainRadius:0.##} 안의 다른 물고기 최대 {capacitorMaximumTargets}마리에게 각각 저항력 피해 {primaryValue:0.##}.",
        ItemCatalog.SpectralScabbardId =>
            $"같은 물고기에 유효한 도구 적중 {scabbardRequiredHits}회마다 저항력 피해 {primaryValue:0.##}.",
        ItemCatalog.AutonomousSwordArrayId =>
            $"{swordArrayAttackInterval:0.##}초마다 화면 안에서 남은 저항력이 가장 높은 물고기에게 저항력 피해 {primaryValue:0.##}.",
        ItemCatalog.FrostSigilId =>
            $"같은 물고기에 유효한 도구 적중 {frostSigilRequiredHits}회마다 {primaryValue:0.##}초 동안 이동 속도를 {frostSigilSlowPercentage * 100f:0.##}% 낮춥니다.",
        ItemCatalog.FrostCrystalId =>
            $"{frostCrystalAttackInterval:0.##}초마다 커서 반경 {frostCrystalTargetRadius:0.##} 안의 가까운 물고기 최대 {frostCrystalMaximumTargets}마리에게 저항력 피해 {frostCrystalResistanceDamage:0.##}와 {primaryValue:0.##}초간 {frostCrystalSlowPercentage * 100f:0.##}% 둔화를 줍니다.",
        _ => string.Empty
    };

    private void UpdatePeriodicEffects()
    {
        if (Owns(ItemCatalog.StormOrbId))
        {
            stormOrbRemaining -= Time.deltaTime;
            if (stormOrbRemaining <= 0f)
            {
                stormOrbRemaining = stormOrbAttackInterval;
                ActivateStormOrb();
            }
        }

        if (Owns(ItemCatalog.AutonomousSwordArrayId))
        {
            swordArrayRemaining -= Time.deltaTime;
            if (swordArrayRemaining <= 0f)
            {
                swordArrayRemaining = swordArrayAttackInterval;
                ActivateSwordArray();
            }
        }

        if (Owns(ItemCatalog.FrostCrystalId))
        {
            frostCrystalRemaining -= Time.deltaTime;
            if (frostCrystalRemaining <= 0f)
            {
                frostCrystalRemaining = frostCrystalAttackInterval;
                ActivateFrostCrystal();
            }
        }
    }

    private void ActivateStormOrb()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector2 cursor = GetCursorWorldPosition(camera);
        List<FishController> fish = GetEligibleFish(camera);
        fish.Sort((a, b) => CompareByDistanceThenId(a, b, cursor));
        if (fish.Count == 0)
        {
            return;
        }

        FishController target = fish[0];
        Vector2 position = target.transform.position;
        float effectiveDamage = GetEffectiveDamage(
            ItemCatalog.StormOrbId,
            stormOrbResistanceDamage,
            stormOrbDamageBonusPerLevel);
        DealItemDamage(
            target,
            ItemCatalog.StormOrbId,
            effectiveDamage);
        CreateLightning(
            new[] { position + Vector2.up * 1.8f, position },
            stormOrbVisualDuration);
    }

    private void ActivateCapacitorCoil(Vector2 origin, FishController triggeringFish)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        List<FishController> allFish = GetEligibleFish(camera);
        List<FishController> fish = new(allFish);
        fish.RemoveAll(candidate =>
            candidate == triggeringFish ||
            Vector2.Distance(origin, candidate.transform.position) > capacitorChainRadius);
        fish.Sort((a, b) => CompareByDistanceThenId(a, b, origin));

        int targetCount = Mathf.Min(capacitorMaximumTargets, fish.Count);
        float effectiveDamage = GetEffectiveDamage(
            ItemCatalog.CapacitorCoilId,
            capacitorResistanceDamage,
            capacitorDamageBonusPerLevel);

        if (targetCount == 0)
        {
            CreateLightning(
                new[] { origin, origin + Vector2.up * 0.45f },
                capacitorVisualDuration);
        }
        else
        {
            Vector2[] points = new Vector2[targetCount + 1];
            points[0] = origin;
            for (int i = 0; i < targetCount; i++)
            {
                FishController target = fish[i];
                points[i + 1] = target.transform.position;
                DealItemDamage(
                    target,
                    ItemCatalog.CapacitorCoilId,
                    effectiveDamage);
            }

            CreateLightning(points, capacitorVisualDuration);
        }

    }

    private void ActivateIndependentElectricSynergy(CombatDamageResult result)
    {
        SingleElementSynergyState state = GetElectricSynergyState();
        if (!state.IsLevel2Active)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        ElectricSynergySettings settings = GetElectricSynergySettings();
        List<FishController> candidates = GetEligibleFish(camera);
        List<FishController> chainTargets =
            SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                candidates,
                result.HitPosition,
                settings.ChainRadius,
                settings.GetEffectiveChainTargetCount(state));
        List<FishController> thunderstormTargets = state.IsLevel10Active
            ? SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                candidates,
                result.HitPosition,
                float.PositiveInfinity,
                settings.ThunderstormTargetCount)
            : new List<FishController>();

        ElectricSynergyActivation activation =
            electricSynergyRuntime.TryBeginActivation(
                state,
                Time.time,
                settings.ChainCooldown,
                settings.ThunderstormCooldown,
                chainTargets.Count > 0,
                thunderstormTargets.Count > 0);
        if (activation == ElectricSynergyActivation.Thunderstorm)
        {
            ActivateElectricThunderstorm(thunderstormTargets, state, settings);
        }
        else if (activation == ElectricSynergyActivation.ChainDischarge)
        {
            ActivateElectricChainDischarge(
                result.HitPosition,
                chainTargets,
                state,
                settings);
        }
    }

    private void ActivateElectricChainDischarge(
        Vector2 origin,
        IReadOnlyList<FishController> targets,
        SingleElementSynergyState state,
        ElectricSynergySettings settings)
    {
        float damage = settings.GetEffectiveDamage(settings.ChainDamage, state);
        List<Vector2> points = new(targets.Count + 1)
        {
            origin
        };
        CombatVfxSettings vfx = GetCombatVfxSettings();
        for (int i = 0; i < targets.Count; i++)
        {
            FishController target = targets[i];
            Vector2 targetPosition = target.transform.position;
            bool hadIceSynergySlow = HasIceSynergySlow(target);
            bool chainHit = DealSynergyDamage(target, ElectricChainDamageId, damage);
            if (chainHit)
            {
                points.Add(targetPosition);
                HandleCombinedElectricChainHit(
                    target,
                    targetPosition,
                    hadIceSynergySlow);
                CreateImpactMarker(
                    targetPosition,
                    vfx.ElectricHitColor,
                    0.32f,
                    0.08f,
                    vfx.HitEmphasisDuration,
                    "ElectricChainHitVisual");
            }
            if (state.IsLevel6Active && IsEligibleFish(target))
            {
                ApplyElectricDischargeStun(target, state, settings);
            }
        }

        CreateLightning(
            points,
            settings.ChainVisualDuration,
            vfx.ElectricChainColor,
            0.15f,
            "ElectricChainDischargeVisual");
    }

    private void ActivateElectricThunderstorm(
        IReadOnlyList<FishController> targets,
        SingleElementSynergyState state,
        ElectricSynergySettings settings)
    {
        float damage = settings.GetEffectiveDamage(settings.ThunderstormDamage, state);
        CombatVfxSettings vfx = GetCombatVfxSettings();
        for (int i = 0; i < targets.Count; i++)
        {
            FishController target = targets[i];
            Vector2 position = target.transform.position;
            if (!DealSynergyDamage(
                    target,
                    ElectricThunderstormDamageId,
                    damage))
            {
                continue;
            }

            CreateLightning(
                position + Vector2.up * 3.2f,
                position,
                settings.ThunderstormVisualDuration,
                vfx.ThunderstormColor,
                0.24f,
                "ElectricThunderstormVisual");
            CreateImpactMarker(
                position,
                vfx.ThunderstormColor,
                0.52f,
                0.12f,
                vfx.HitEmphasisDuration,
                "ElectricThunderstormHitVisual");
        }
    }

    private void ApplyElectricDischargeStun(
        FishController fish,
        SingleElementSynergyState state,
        ElectricSynergySettings settings)
    {
        if (!IsEligibleFish(fish) || fish.Data == null)
        {
            return;
        }

        float duration = GetElectricDischargeStunDuration(
            state,
            fish.Data.SpecialType);
        SynergyTargetKey target = new(
            fish.GetInstanceID(),
            fish.LifecycleVersion);
        if (!electricSynergyRuntime.TryBeginStun(
                target,
                Time.time,
                duration,
                settings.RestunLockout))
        {
            return;
        }

        FishMovement movement = fish.GetComponent<FishMovement>();
        if (movement == null)
        {
            electricSynergyRuntime.ClearTarget(fish.GetInstanceID());
            return;
        }

        movement.ApplyTimedSpeedModifier(
            ElectricStunModifierId,
            0f,
            duration);
        ShowStatusVisual(
            electricStunVisuals,
            fish,
            GetCombatVfxSettings().ElectricStunColor,
            "ElectricStunStatusVisual");
    }

    private void ActivateIndependentSwordSynergy(CombatDamageResult result)
    {
        SingleElementSynergyState state = GetSwordSynergyState();
        SwordSynergySettings settings = GetSwordSynergySettings();
        swordSynergyRuntime.RefreshTierState(state);
        if (!swordSynergyRuntime.RecordValidToolHit(
                state,
                settings.GetEffectiveSoulSlashRequiredHits(state)))
        {
            return;
        }

        Camera camera = Camera.main;
        FishController primaryTarget =
            !result.CapturedByHit &&
            result.Target != null &&
            result.Target.LifecycleVersion == result.TargetLifecycleVersion &&
            IsEligibleFish(result.Target)
                ? result.Target
                : null;
        List<FishController> candidates = camera != null
            ? GetEligibleFish(camera)
            : new List<FishController>();
        if (primaryTarget == null)
        {
            List<FishController> fallbackTargets =
                SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                    candidates,
                    result.HitPosition,
                    settings.FallbackRadius,
                    1);
            if (fallbackTargets.Count == 0)
            {
                // The completed hit threshold is consumed; no attack is queued.
                return;
            }

            primaryTarget = fallbackTargets[0];
        }

        Vector2 primaryPosition = primaryTarget.transform.position;
        bool hadIceSynergyControl = HasIceSynergyControl(primaryTarget);
        bool primaryExecuted = DealSynergyDamage(
            primaryTarget,
            SwordSoulSlashDamageId,
            settings.GetEffectiveDamage(settings.SoulSlashDamage, state));
        if (!primaryExecuted)
        {
            return;
        }

        HandleCombinedSoulSlashHit(
            primaryTarget,
            primaryPosition,
            hadIceSynergyControl);

        CreateSwordSynergyMarker(
            primaryPosition,
            settings.SoulSlashVisualDuration,
            SwordSynergyVisualStyle.SoulSlash);

        if (state.IsLevel6Active && camera != null)
        {
            candidates = GetEligibleFish(camera);
            HashSet<FishController> excluded = new() { primaryTarget };
            List<FishController> additionalTargets =
                SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                    candidates,
                    result.HitPosition,
                    settings.FallbackRadius,
                    1,
                    excluded);
            if (additionalTargets.Count > 0)
            {
                FishController additionalTarget = additionalTargets[0];
                Vector2 additionalPosition = additionalTarget.transform.position;
                if (DealSynergyDamage(
                        additionalTarget,
                        SwordAdditionalSlashDamageId,
                        settings.GetEffectiveDamage(
                            settings.AdditionalSwordDamage,
                            state)))
                {
                    CreateSwordSynergyMarker(
                        additionalPosition,
                        settings.SoulSlashVisualDuration,
                        SwordSynergyVisualStyle.AdditionalSlash);
                }
            }
        }

        if (!swordSynergyRuntime.RecordSuccessfulSoulSlash(
                state,
                settings.SwordRainRequiredActivations) ||
            camera == null)
        {
            return;
        }

        List<FishController> rainTargets =
            SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                GetEligibleFish(camera),
                result.HitPosition,
                float.PositiveInfinity,
                settings.SwordRainTargetCount);
        float rainDamage = settings.GetEffectiveDamage(settings.SwordRainDamage, state);
        for (int i = 0; i < rainTargets.Count; i++)
        {
            FishController target = rainTargets[i];
            Vector2 position = target.transform.position;
            if (DealSynergyDamage(target, SwordRainDamageId, rainDamage))
            {
                CreateSwordSynergyMarker(
                    position,
                    settings.SwordRainVisualDuration,
                    SwordSynergyVisualStyle.SwordRain);
            }
        }
    }

    private void ActivateIndependentIceSynergy(CombatDamageResult result)
    {
        SingleElementSynergyState state = GetIceSynergyState();
        IceSynergySettings settings = GetIceSynergySettings();
        IceSynergyActivation activation = iceSynergyRuntime.RecordToolCapture(
            result,
            state,
            settings.FrostBurstRequiredCaptures);
        if (activation == IceSynergyActivation.None)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        bool frostBurst = activation == IceSynergyActivation.FrostBurst;
        float radius = frostBurst
            ? settings.FrostBurstRadius
            : settings.ColdWaveRadius;
        int targetCount = frostBurst
            ? settings.FrostBurstTargetCount
            : settings.GetEffectiveColdWaveTargetCount(state);
        HashSet<FishController> excluded = new();
        if (result.Target != null)
        {
            excluded.Add(result.Target);
        }

        List<FishController> targets =
            SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
                GetEligibleFish(camera),
                result.HitPosition,
                radius,
                targetCount,
                excluded);
        float slowDuration = settings.GetEffectiveSlowDuration(
            frostBurst
                ? settings.FrostBurstSlowDuration
                : settings.ColdWaveSlowDuration,
            state);
        bool affectedAnyTarget = false;
        for (int i = 0; i < targets.Count; i++)
        {
            FishController target = targets[i];
            if (frostBurst)
            {
                bool damageApplied = DealSynergyDamage(
                    target,
                    IceFrostBurstDamageId,
                    GetIceFrostBurstDamage(state));
                affectedAnyTarget |= damageApplied;
                if (!IsEligibleFish(target))
                {
                    continue;
                }
            }

            affectedAnyTarget |=
                ApplyIceSynergySlow(
                    target,
                    frostBurst,
                    slowDuration);
            if (state.IsLevel6Active)
            {
                affectedAnyTarget |=
                    ApplyIceSynergyFreeze(
                        target,
                        state,
                        settings);
            }
        }

        if (affectedAnyTarget)
        {
            CreateIceSynergyMarker(
                result.HitPosition,
                radius,
                frostBurst
                    ? settings.FrostBurstVisualDuration
                    : settings.ColdWaveVisualDuration,
                frostBurst);
        }
    }

    private bool ApplyIceSynergySlow(
        FishController fish,
        bool frostBurst,
        float duration)
    {
        if (!IsEligibleFish(fish) || fish.Data == null)
        {
            return false;
        }

        ApplySlow(
            fish,
            IceSlowModifierId,
            GetIceSynergySlowPercentage(frostBurst, fish.Data.SpecialType),
            duration);

        FishMovement movement = fish.GetComponent<FishMovement>();
        return movement != null &&
            movement.HasTimedSpeedModifier(IceSlowModifierId);
    }

    private bool ApplyIceSynergyFreeze(
        FishController fish,
        SingleElementSynergyState state,
        IceSynergySettings settings)
    {
        if (!IsEligibleFish(fish) || fish.Data == null)
        {
            return false;
        }

        float duration = GetIceFreezeDuration(state, fish.Data.SpecialType);
        SynergyTargetKey target = new(fish.GetInstanceID(), fish.LifecycleVersion);
        if (!iceSynergyRuntime.TryBeginFreeze(
                target,
                Time.time,
                duration,
                settings.RefreezeLockout))
        {
            return false;
        }

        FishMovement movement = fish.GetComponent<FishMovement>();
        if (movement == null)
        {
            iceSynergyRuntime.ClearTarget(fish.GetInstanceID());
            return false;
        }

        movement.ApplyTimedSpeedModifier(IceFreezeModifierId, 0f, duration);
        ShowStatusVisual(
            iceFreezeVisuals,
            fish,
            GetCombatVfxSettings().FreezeColor,
            "IceFreezeStatusVisual");
        return movement.HasTimedSpeedModifier(IceFreezeModifierId);
    }

    private void ActivateSpectralScabbard(FishController target, Vector2 position)
    {
        if (IsEligibleFish(target))
        {
            DealItemDamage(
                target,
                ItemCatalog.SpectralScabbardId,
                GetEffectiveDamage(
                    ItemCatalog.SpectralScabbardId,
                    scabbardResistanceDamage,
                    scabbardDamageBonusPerLevel));
        }
        CreateSwordMarker(position, scabbardVisualDuration, false);
    }

    private void ActivateSwordArray()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        List<FishController> fish = GetEligibleFish(camera);
        fish.Sort((a, b) =>
        {
            int resistance = b.CurrentResistance.CompareTo(a.CurrentResistance);
            return resistance != 0
                ? resistance
                : a.GetInstanceID().CompareTo(b.GetInstanceID());
        });
        if (fish.Count == 0)
        {
            return;
        }

        FishController target = fish[0];
        Vector2 position = target.transform.position;
        if (IsEligibleFish(target))
        {
            DealItemDamage(
                target,
                ItemCatalog.AutonomousSwordArrayId,
                GetEffectiveDamage(
                    ItemCatalog.AutonomousSwordArrayId,
                    swordArrayResistanceDamage,
                    swordArrayDamageBonusPerLevel));
        }
        CreateSwordMarker(position, swordArrayVisualDuration, true);
    }

    private void ActivateFrostCrystal()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        Vector2 center = GetCursorWorldPosition(camera);
        List<FishController> fish = GetEligibleFish(camera);
        fish.RemoveAll(candidate =>
            Vector2.Distance(center, candidate.transform.position) > frostCrystalTargetRadius);
        fish.Sort((a, b) => CompareByDistanceThenId(a, b, center));
        if (fish.Count == 0)
        {
            return;
        }

        int targetCount = Mathf.Min(frostCrystalMaximumTargets, fish.Count);
        List<Vector2> affectedPositions = new(targetCount);
        for (int i = 0; i < targetCount; i++)
        {
            FishController target = fish[i];
            Vector2 position = target.transform.position;
            affectedPositions.Add(position);
            DealItemDamage(
                target,
                ItemCatalog.FrostCrystalId,
                frostCrystalResistanceDamage);
            if (!IsEligibleFish(target))
            {
                continue;
            }

            ApplySlow(
                target,
                ItemCatalog.FrostCrystalId,
                frostCrystalSlowPercentage,
                GetEffectiveSlowDuration(ItemCatalog.FrostCrystalId));
        }

        CreateFrostMarker(
            center,
            frostCrystalTargetRadius,
            frostCrystalVisualDuration);
        for (int i = 0; i < affectedPositions.Count; i++)
        {
            CreateFrostMarker(
                affectedPositions[i],
                0.55f,
                Mathf.Max(
                    frostCrystalVisualDuration,
                    GetEffectiveSlowDuration(ItemCatalog.FrostCrystalId)));
        }
    }

    private void DealItemDamage(FishController fish, string itemId, float damage)
    {
        if (!IsEligibleFish(fish) || damage <= 0f)
        {
            return;
        }

        fish.TakeCaptureDamage(
            damage,
            CombatDamageContext.Item(itemId, this));
    }

    private bool DealSynergyDamage(
        FishController fish,
        string synergyId,
        float damage)
    {
        if (!IsEligibleFish(fish) || damage <= 0f)
        {
            return false;
        }

        fish.TakeCaptureDamage(
            damage,
            CombatDamageContext.ItemSynergy(synergyId, this));
        return true;
    }

    private void HandleCombinedElectricChainHit(
        FishController target,
        Vector2 hitPosition,
        bool hadIceSynergySlow)
    {
        CombinedSynergyId active = ObserveActiveCombinedSynergy();
        CombinedSynergyCombatSettings settings = GetCombinedSynergyCombatSettings();
        if (active == CombinedSynergyId.ThunderSwordResonance && IsEligibleFish(target))
        {
            combinedSynergyRuntime.ApplyConductiveMark(
                new SynergyTargetKey(target.GetInstanceID(), target.LifecycleVersion),
                Time.time,
                settings.ConductiveMarkDuration);
            return;
        }

        if (active != CombinedSynergyId.Superconductivity || !hadIceSynergySlow ||
            !combinedSynergyRuntime.TryBeginSuperconductivity(
                Time.time,
                settings.SuperconductivityCooldown))
        {
            return;
        }

        if (target != null && target.Data != null)
        {
            target.GetComponent<FishMovement>()?.ExtendTimedSpeedModifier(
                IceSlowModifierId,
                GetCrowdControlPolicy().GetAdjustedDuration(
                    settings.SuperconductivitySlowExtension,
                    target.Data.SpecialType));
        }

        DamageAdditionalCombinedTargets(
            hitPosition,
            target,
            GetElectricSynergySettings().ChainRadius,
            settings.SuperconductivityAdditionalTargets,
            settings.SuperconductivityDamage,
            SuperconductivityDamageId,
            false);
    }

    private void HandleCombinedSoulSlashHit(
        FishController target,
        Vector2 hitPosition,
        bool hadIceSynergyControl)
    {
        CombinedSynergyId active = ObserveActiveCombinedSynergy();
        CombinedSynergyCombatSettings settings = GetCombinedSynergyCombatSettings();
        if (active == CombinedSynergyId.ThunderSwordResonance)
        {
            SynergyTargetKey key = target != null
                ? new SynergyTargetKey(target.GetInstanceID(), target.LifecycleVersion)
                : default;
            if (!combinedSynergyRuntime.TryConsumeConductiveMark(
                    key,
                    Time.time,
                    settings.ThunderSwordCooldown))
            {
                return;
            }

            DealSynergyDamage(target, ThunderSwordPrimaryDamageId, settings.ThunderSwordPrimaryDamage);
            DamageAdditionalCombinedTargets(
                hitPosition,
                target,
                GetSwordSynergySettings().FallbackRadius,
                settings.ThunderSwordAdditionalTargets,
                settings.ThunderSwordAdditionalDamage,
                ThunderSwordSplashDamageId,
                false);
            return;
        }

        if (active != CombinedSynergyId.FrostSwordResonance || !hadIceSynergyControl ||
            !combinedSynergyRuntime.TryBeginFrostSword(Time.time, settings.FrostSwordCooldown))
        {
            return;
        }

        DamageAdditionalCombinedTargets(
            hitPosition,
            target,
            GetSwordSynergySettings().FallbackRadius,
            settings.FrostSwordAdditionalTargets,
            settings.FrostSwordDamage,
            FrostSwordDamageId,
            true);
    }

    private void DamageAdditionalCombinedTargets(
        Vector2 origin,
        FishController source,
        float radius,
        int maximumTargets,
        float damage,
        string damageId,
        bool applyFrostSwordSlow)
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        HashSet<FishController> excluded = new();
        if (source != null) excluded.Add(source);
        List<FishController> targets = SingleElementSynergyTargeting.SelectDistinctEligibleTargets(
            GetEligibleFish(camera), origin, radius, maximumTargets, excluded);
        CombinedSynergyCombatSettings settings = GetCombinedSynergyCombatSettings();
        for (int i = 0; i < targets.Count; i++)
        {
            FishController target = targets[i];
            if (!DealSynergyDamage(target, damageId, damage) ||
                !applyFrostSwordSlow || !IsEligibleFish(target) || target.Data == null)
            {
                continue;
            }

            ApplySlow(
                target,
                FrostSwordSlowModifierId,
                GetCrowdControlPolicy().GetAdjustedAdditionalSlowPercentage(
                    settings.FrostSwordSlowPercentage,
                    target.Data.SpecialType),
                GetCrowdControlPolicy().GetAdjustedDuration(
                    settings.FrostSwordSlowDuration,
                    target.Data.SpecialType));
        }
    }

    private static bool HasIceSynergySlow(FishController fish)
    {
        FishMovement movement = fish != null ? fish.GetComponent<FishMovement>() : null;
        return movement != null && movement.HasTimedSpeedModifier(IceSlowModifierId);
    }

    private static bool HasIceSynergyControl(FishController fish)
    {
        FishMovement movement = fish != null ? fish.GetComponent<FishMovement>() : null;
        return movement != null &&
            (movement.HasTimedSpeedModifier(IceSlowModifierId) ||
             movement.HasTimedSpeedModifier(IceFreezeModifierId));
    }

    private static CombinedSynergyId GetActiveCombinedSynergy() =>
        RunManager.Instance?.GrowthState?.CombinedSynergy.ActiveId ?? CombinedSynergyId.None;

    private CombinedSynergyId ObserveActiveCombinedSynergy()
    {
        CombinedSynergyId active = GetActiveCombinedSynergy();
        if (active == lastObservedCombinedSynergy)
        {
            return active;
        }

        combinedSynergyRuntime.Reset();
        lastObservedCombinedSynergy = active;
        return active;
    }

    private CombinedSynergyCombatSettings GetCombinedSynergyCombatSettings()
    {
        combinedSynergyCombat ??= new CombinedSynergyCombatSettings();
        return combinedSynergyCombat;
    }

    private SingleElementSynergyState GetElectricSynergyState() =>
        GetSingleElementSynergyState(ItemElement.Electric, boundInventory);

    private ElectricSynergySettings GetElectricSynergySettings()
    {
        electricSynergy ??= new ElectricSynergySettings();
        return electricSynergy;
    }

    private SingleElementSynergyState GetSwordSynergyState() =>
        GetSingleElementSynergyState(ItemElement.Sword, boundInventory);

    private SwordSynergySettings GetSwordSynergySettings()
    {
        swordSynergy ??= new SwordSynergySettings();
        return swordSynergy;
    }

    private SingleElementSynergyState GetIceSynergyState() =>
        GetSingleElementSynergyState(ItemElement.Ice, boundInventory);

    private IceSynergySettings GetIceSynergySettings()
    {
        iceSynergy ??= new IceSynergySettings();
        return iceSynergy;
    }

    private SingleElementSynergyThresholds GetSingleElementThresholds()
    {
        singleElementThresholds ??= new SingleElementSynergyThresholds();
        return singleElementThresholds;
    }

    private SynergyCrowdControlPolicy GetCrowdControlPolicy()
    {
        synergyCrowdControl ??= new SynergyCrowdControlPolicy();
        return synergyCrowdControl;
    }

    private CombatVfxSettings GetCombatVfxSettings()
    {
        combatVfx ??= new CombatVfxSettings();
        return combatVfx;
    }

    private float GetEffectiveDamage(
        string itemId,
        float baseline,
        float bonusPerLevel)
    {
        int level = GetOwnedLevel(boundInventory, itemId);
        return ItemLevelScaling.CalculateAdditiveDamage(
            baseline,
            level == 0 ? 1 : level,
            bonusPerLevel);
    }

    private float GetEffectiveSlowDuration(string itemId)
    {
        int level = GetOwnedLevel(boundInventory, itemId);
        return itemId switch
        {
            ItemCatalog.FrostSigilId => ItemLevelScaling.CalculateAdditiveDuration(
                frostSigilSlowDuration,
                level == 0 ? 1 : level,
                frostSigilDurationPerLevel),
            ItemCatalog.FrostCrystalId => ItemLevelScaling.CalculateAdditiveDuration(
                frostCrystalSlowDuration,
                level == 0 ? 1 : level,
                frostCrystalDurationPerLevel),
            _ => 0f
        };
    }

    private static int GetOwnedLevel(
        RunItemInventory inventory,
        string itemId)
    {
        RunItemInstance owned = inventory?.GetOwned(itemId);
        return owned != null && owned.Level > 0 ? owned.Level : 0;
    }

    private void ApplySlow(
        FishController fish,
        string modifierId,
        float slowPercentage,
        float duration)
    {
        if (!IsEligibleFish(fish))
        {
            return;
        }

        FishMovement movement = fish.GetComponent<FishMovement>();
        movement?.ApplyTimedSpeedModifier(
            modifierId,
            1f - Mathf.Clamp01(slowPercentage),
            duration);
        if (movement == null)
        {
            return;
        }

        if (string.Equals(modifierId, ItemCatalog.FrostSigilId, StringComparison.Ordinal))
        {
            frostSigilSlowedFish.Add(fish);
        }
        else if (string.Equals(modifierId, ItemCatalog.FrostCrystalId, StringComparison.Ordinal))
        {
            frostCrystalSlowedFish.Add(fish);
        }
    }

    private bool CanCountContinuousHit(
        CombatDamageResult result,
        float scaledTime)
    {
        if (result.Target.LifecycleVersion != result.TargetLifecycleVersion)
        {
            return true;
        }

        ContinuousHitKey key = new(
            result.Target.GetInstanceID(),
            result.Context.SourceInstanceId,
            result.Context.AttackId);
        if (continuousHitTimes.TryGetValue(key, out float lastCountedAt) &&
            scaledTime - lastCountedAt < continuousHitCountInterval)
        {
            return false;
        }

        continuousHitTimes[key] = scaledTime;
        return true;
    }

    private static bool IncrementPerFishCounter(
        Dictionary<FishController, int> counters,
        FishController fish,
        int requiredHits)
    {
        counters.TryGetValue(fish, out int count);
        count++;
        if (count < requiredHits)
        {
            counters[fish] = count;
            return false;
        }

        counters.Remove(fish);
        return true;
    }

    private List<FishController> GetEligibleFish(Camera camera)
    {
        FishController[] allFish = FindObjectsByType<FishController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        List<FishController> eligible = new(allFish.Length);
        Rect pixelBounds = GetGameplayPixelBounds(camera);
        for (int i = 0; i < allFish.Length; i++)
        {
            FishController fish = allFish[i];
            if (!IsEligibleFish(fish))
            {
                continue;
            }

            Vector3 screen = camera.WorldToScreenPoint(fish.transform.position);
            if (screen.z >= 0f && pixelBounds.Contains(screen))
            {
                eligible.Add(fish);
            }
        }

        return eligible;
    }

    private static bool IsEligibleFish(FishController fish) =>
        fish != null &&
        fish.gameObject.activeInHierarchy &&
        fish.Data != null &&
        !fish.IsCaptured;

    private Vector2 GetCursorWorldPosition(Camera camera)
    {
        Rect bounds = GetGameplayPixelBounds(camera);
        Vector2 screen = Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : bounds.center;
        screen.x = Mathf.Clamp(screen.x, bounds.xMin, bounds.xMax);
        screen.y = Mathf.Clamp(screen.y, bounds.yMin, bounds.yMax);
        Vector3 world = camera.ScreenToWorldPoint(screen);
        return new Vector2(world.x, world.y);
    }

    private Rect GetGameplayPixelBounds(Camera camera)
    {
        Rect pixel = camera.pixelRect;
        return Rect.MinMaxRect(
            pixel.xMin + pixel.width * gameplayViewport.xMin,
            pixel.yMin + pixel.height * gameplayViewport.yMin,
            pixel.xMin + pixel.width * gameplayViewport.xMax,
            pixel.yMin + pixel.height * gameplayViewport.yMax);
    }

    private static int CompareByDistanceThenId(
        FishController a,
        FishController b,
        Vector2 origin)
    {
        float aDistance = ((Vector2)a.transform.position - origin).sqrMagnitude;
        float bDistance = ((Vector2)b.transform.position - origin).sqrMagnitude;
        int distance = aDistance.CompareTo(bDistance);
        return distance != 0
            ? distance
            : a.GetInstanceID().CompareTo(b.GetInstanceID());
    }

    [ContextMenu("Development/Upgrade First Eligible Owned Item")]
    private void DevelopmentUpgradeFirstEligibleOwnedItem()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Application.isPlaying || !IsRunActive())
        {
            Debug.LogWarning("아이템 업그레이드 테스트는 Play Mode의 진행 중인 Run에서만 사용할 수 있습니다.");
            return;
        }

        if (ItemRewardManager.IsSelectionPendingOrActive)
        {
            Debug.LogWarning("필수 아이템 보상이 진행 중일 때는 직접 업그레이드할 수 없습니다.");
            return;
        }

        RunItemInventory inventory = GetCurrentInventory();
        if (inventory == null || inventory.Count == 0)
        {
            Debug.LogWarning("업그레이드 테스트에는 실제로 소유한 아이템이 필요합니다.");
            return;
        }

        IReadOnlyList<RunItemInstance> items = inventory.OwnedItems;
        ItemUpgradeResult lastResult = ItemUpgradeResult.ItemNotOwned;
        for (int i = 0; i < items.Count; i++)
        {
            if (!CanUpgradeItem(items[i].ItemId, out lastResult))
            {
                continue;
            }

            string itemId = items[i].ItemId;
            if (TryUpgradeItem(itemId, out ItemUpgradeResult result))
            {
                RunItemInstance upgraded = inventory.GetOwned(itemId);
                Debug.Log($"아이템 업그레이드 테스트 완료: {itemId} Lv.{upgraded?.Level}");
                return;
            }

            lastResult = result;
        }

        Debug.LogWarning($"업그레이드 가능한 소유 아이템이 없습니다: {GetUpgradeDiagnostic(lastResult)}");
#else
        Debug.LogWarning("아이템 업그레이드 테스트는 Editor 또는 Development Build에서만 사용할 수 있습니다.");
#endif
    }

    [ContextMenu("Development/Upgrade Owned Electric Items To Level 5")]
    private void DevelopmentUpgradeOwnedElectricItemsToLevelFive()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!Application.isPlaying || !IsRunActive())
        {
            Debug.LogWarning("전기 레벨 10 테스트는 Play Mode의 진행 중인 Run에서만 사용할 수 있습니다.");
            return;
        }

        if (ItemRewardManager.IsSelectionPendingOrActive)
        {
            Debug.LogWarning("필수 아이템 보상이 진행 중일 때는 전기 아이템을 직접 업그레이드할 수 없습니다.");
            return;
        }

        RunItemInventory inventory = GetCurrentInventory();
        if (inventory == null)
        {
            Debug.LogWarning("진행 중인 Run의 아이템 인벤토리를 찾을 수 없습니다.");
            return;
        }

        int upgradeCount = 0;
        IReadOnlyList<RunItemInstance> items = inventory.OwnedItems;
        for (int i = 0; i < items.Count; i++)
        {
            RunItemInstance owned = items[i];
            if (!ItemCatalog.TryGet(owned.ItemId, out ItemDefinition definition) ||
                definition.Element != ItemElement.Electric)
            {
                continue;
            }

            while (owned.Level < ItemLevelLimit.DefaultMaximumLevel &&
                   CanUpgradeItem(owned.ItemId, inventory, out _) &&
                   TryUpgradeItem(owned.ItemId, inventory, out _))
            {
                upgradeCount++;
            }
        }

        Debug.Log(
            $"전기 시너지 테스트 업그레이드 완료: {upgradeCount}회, " +
            $"전기 속성 Lv.{inventory.GetElementLevel(ItemElement.Electric)}. " +
            "레벨 10에는 실제로 소유한 폭풍 구슬과 축전 코일이 모두 Lv.5여야 합니다.");
#else
        Debug.LogWarning("전기 레벨 10 테스트는 Editor 또는 Development Build에서만 사용할 수 있습니다.");
#endif
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

    private RunItemInventory GetCurrentInventory() =>
        boundInventory ??
        (RunManager.Instance != null && RunManager.Instance.GrowthState != null
            ? RunManager.Instance.GrowthState.ItemInventory
            : null);

    private void BindInventoryIfNeeded()
    {
        RunItemInventory current =
            RunManager.Instance != null && RunManager.Instance.GrowthState != null
                ? RunManager.Instance.GrowthState.ItemInventory
                : null;
        if (ReferenceEquals(current, boundInventory))
        {
            return;
        }

        if (boundInventory != null)
        {
            boundInventory.Changed -= OnInventoryChanged;
        }

        boundInventory = current;
        knownOwnedItems.Clear();
        ClearRuntimeState(false);
        if (boundInventory != null)
        {
            boundInventory.Changed += OnInventoryChanged;
            RefreshOwnedItems();
        }
    }

    private void OnInventoryChanged()
    {
        RefreshOwnedItems();
    }

    private void RefreshOwnedItems()
    {
        if (boundInventory == null)
        {
            return;
        }

        IReadOnlyList<RunItemInstance> items = boundInventory.OwnedItems;
        for (int i = 0; i < items.Count; i++)
        {
            string itemId = items[i].ItemId;
            if (!knownOwnedItems.Add(itemId))
            {
                continue;
            }

            switch (itemId)
            {
                case ItemCatalog.StormOrbId:
                    stormOrbRemaining = stormOrbAttackInterval;
                    break;
                case ItemCatalog.AutonomousSwordArrayId:
                    swordArrayRemaining = swordArrayAttackInterval;
                    break;
                case ItemCatalog.FrostCrystalId:
                    frostCrystalRemaining = frostCrystalAttackInterval;
                    break;
            }
        }
    }

    private bool Owns(string itemId) =>
        boundInventory != null && boundInventory.Owns(itemId);

    private static bool IsRunActive()
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        return flow != null && flow.IsFishingStarted && !flow.IsGameEnded;
    }

    private void ShowStatusVisual(
        Dictionary<FishController, SynergyStatusVisual> visuals,
        FishController fish,
        Color color,
        string objectName)
    {
        if (fish == null || combatVfxPool == null)
        {
            return;
        }

        if (visuals.TryGetValue(
                fish,
                out SynergyStatusVisual existing))
        {
            if (existing.LifecycleVersion == fish.LifecycleVersion &&
                existing.Line != null)
            {
                return;
            }

            combatVfxPool.Release(existing.Line);
            visuals.Remove(fish);
        }

        LineRenderer line = combatVfxPool.AcquirePersistent(
            objectName,
            color,
            0.08f,
            9,
            30);
        if (line == null)
        {
            return;
        }

        visuals.Add(
            fish,
            new SynergyStatusVisual
            {
                Fish = fish,
                LifecycleVersion = fish.LifecycleVersion,
                Line = line
            });
    }

    private void UpdateSynergyStatusVisuals()
    {
        UpdateStatusVisuals(
            electricStunVisuals,
            ElectricStunModifierId,
            false);
        UpdateStatusVisuals(
            iceFreezeVisuals,
            IceFreezeModifierId,
            true);
    }

    private void UpdateStatusVisuals(
        Dictionary<FishController, SynergyStatusVisual> visuals,
        string modifierId,
        bool freezeStyle)
    {
        if (visuals.Count == 0)
        {
            return;
        }

        staleStatusVisuals.Clear();
        foreach (KeyValuePair<FishController, SynergyStatusVisual> pair in visuals)
        {
            FishController fish = pair.Key;
            SynergyStatusVisual visual = pair.Value;
            FishMovement movement = fish != null
                ? fish.GetComponent<FishMovement>()
                : null;
            if (!IsEligibleFish(fish) ||
                fish.LifecycleVersion != visual.LifecycleVersion ||
                movement == null ||
                !movement.HasTimedSpeedModifier(modifierId) ||
                visual.Line == null)
            {
                staleStatusVisuals.Add(fish);
                continue;
            }

            if (freezeStyle)
            {
                SetFreezeStatusGeometry(
                    visual.Line,
                    fish.transform.position);
            }
            else
            {
                SetStunStatusGeometry(
                    visual.Line,
                    fish.transform.position);
            }
        }

        for (int i = 0; i < staleStatusVisuals.Count; i++)
        {
            RemoveStatusVisual(
                visuals,
                staleStatusVisuals[i]);
        }
        staleStatusVisuals.Clear();
    }

    private void SetStunStatusGeometry(
        LineRenderer line,
        Vector2 position)
    {
        float size =
            0.42f *
            GetCombatVfxSettings().EffectSizeMultiplier;
        Vector2 center = position + Vector2.up * size * 1.25f;
        for (int i = 0; i < 9; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            float radius = i % 2 == 0 ? size : size * 0.62f;
            line.SetPosition(
                i,
                center + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius));
        }
    }

    private void SetFreezeStatusGeometry(
        LineRenderer line,
        Vector2 position)
    {
        float size =
            0.5f *
            GetCombatVfxSettings().EffectSizeMultiplier;
        Vector2 center = position;
        line.SetPosition(0, center + Vector2.up * size);
        line.SetPosition(1, center + new Vector2(size * 0.5f, size * 0.5f));
        line.SetPosition(2, center + Vector2.right * size);
        line.SetPosition(3, center + new Vector2(size * 0.5f, -size * 0.5f));
        line.SetPosition(4, center + Vector2.down * size);
        line.SetPosition(5, center + new Vector2(-size * 0.5f, -size * 0.5f));
        line.SetPosition(6, center + Vector2.left * size);
        line.SetPosition(7, center + new Vector2(-size * 0.5f, size * 0.5f));
        line.SetPosition(8, center + Vector2.up * size);
    }

    private void RemoveStatusVisual(
        Dictionary<FishController, SynergyStatusVisual> visuals,
        FishController fish)
    {
        if (!visuals.TryGetValue(
                fish,
                out SynergyStatusVisual visual))
        {
            return;
        }

        combatVfxPool?.Release(visual.Line);
        visuals.Remove(fish);
    }

    private void ClearRuntimeState(bool resetKnownOwnership)
    {
        capacitorHitCount = 0;
        scabbardHitCounts.Clear();
        frostSigilHitCounts.Clear();
        frostSigilSlowedFish.Clear();
        frostCrystalSlowedFish.Clear();
        continuousHitTimes.Clear();
        pendingDamageResults.Clear();
        electricSynergyRuntime.Reset();
        swordSynergyRuntime.Reset();
        iceSynergyRuntime.Reset();
        combinedSynergyRuntime.Reset();
        lastObservedCombinedSynergy = CombinedSynergyId.None;
        stormOrbRemaining = stormOrbAttackInterval;
        swordArrayRemaining = swordArrayAttackInterval;
        frostCrystalRemaining = frostCrystalAttackInterval;

        combatVfxPool?.Clear();
        electricStunVisuals.Clear();
        iceFreezeVisuals.Clear();
        staleStatusVisuals.Clear();

        FishMovement[] movements = FindObjectsByType<FishMovement>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (int i = 0; i < movements.Length; i++)
        {
            if (movements[i] == null)
            {
                continue;
            }

            movements[i].RemoveTimedSpeedModifier(ItemCatalog.FrostSigilId);
            movements[i].RemoveTimedSpeedModifier(ItemCatalog.FrostCrystalId);
            movements[i].RemoveTimedSpeedModifier(ElectricStunModifierId);
            movements[i].RemoveTimedSpeedModifier(IceSlowModifierId);
            movements[i].RemoveTimedSpeedModifier(IceFreezeModifierId);
            movements[i].RemoveTimedSpeedModifier(FrostSwordSlowModifierId);
        }

        if (resetKnownOwnership)
        {
            knownOwnedItems.Clear();
        }
    }

    private int CountActiveSlows(string modifierId)
    {
        HashSet<FishController> slowedFish =
            string.Equals(modifierId, ItemCatalog.FrostSigilId, StringComparison.Ordinal)
                ? frostSigilSlowedFish
                : frostCrystalSlowedFish;
        List<FishController> expired = null;
        int count = 0;
        foreach (FishController fish in slowedFish)
        {
            FishMovement movement = fish != null
                ? fish.GetComponent<FishMovement>()
                : null;
            if (IsEligibleFish(fish) &&
                movement != null &&
                movement.HasTimedSpeedModifier(modifierId))
            {
                count++;
                continue;
            }

            expired ??= new List<FishController>();
            expired.Add(fish);
        }

        if (expired != null)
        {
            for (int i = 0; i < expired.Count; i++)
            {
                slowedFish.Remove(expired[i]);
            }
        }

        return count;
    }

    private void CreateLightning(IReadOnlyList<Vector2> points, float duration)
    {
        CreateLightning(
            points,
            duration,
            new Color(0.35f, 0.85f, 1f, 0.95f),
            0.11f,
            "ItemElectricVisual");
    }

    private void CreateLightning(
        IReadOnlyList<Vector2> points,
        float duration,
        Color color,
        float width,
        string objectName)
    {
        if (points == null || points.Count < 2)
        {
            return;
        }

        LineRenderer line = AcquireLineVisual(
            objectName,
            color,
            width,
            points.Count,
            duration);
        if (line == null)
        {
            return;
        }

        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }

    private void CreateLightning(
        Vector2 start,
        Vector2 end,
        float duration,
        Color color,
        float width,
        string objectName)
    {
        LineRenderer line = AcquireLineVisual(
            objectName,
            color,
            width,
            2,
            duration);
        if (line == null)
        {
            return;
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void CreateSwordMarker(Vector2 position, float duration, bool arrayStyle)
    {
        LineRenderer line = AcquireLineVisual(
            arrayStyle ? "ItemSwordArrayVisual" : "ItemSpectralSwordVisual",
            arrayStyle
                ? new Color(1f, 0.45f, 0.25f, 0.95f)
                : new Color(0.85f, 0.75f, 1f, 0.95f),
            0.12f,
            arrayStyle ? 5 : 3,
            duration);
        if (line == null)
        {
            return;
        }

        line.transform.position = position;
        line.useWorldSpace = false;
        if (arrayStyle)
        {
            line.SetPosition(0, new Vector3(-0.8f, 0.7f));
            line.SetPosition(1, new Vector3(0.8f, -0.7f));
            line.SetPosition(2, new Vector3(0f, 0f));
            line.SetPosition(3, new Vector3(0.75f, 0.75f));
            line.SetPosition(4, new Vector3(-0.75f, -0.75f));
        }
        else
        {
            line.SetPosition(0, new Vector3(-0.55f, 0.8f));
            line.SetPosition(1, new Vector3(0.5f, -0.65f));
            line.SetPosition(2, new Vector3(0.18f, -0.45f));
        }
    }

    private void CreateSwordSynergyMarker(
        Vector2 position,
        float duration,
        SwordSynergyVisualStyle style)
    {
        CombatVfxSettings vfx = GetCombatVfxSettings();
        bool rainStyle = style == SwordSynergyVisualStyle.SwordRain;
        bool additionalStyle = style == SwordSynergyVisualStyle.AdditionalSlash;
        LineRenderer line = AcquireLineVisual(
            rainStyle
                ? "SwordRainVisual"
                : additionalStyle
                    ? "AdditionalSoulSlashVisual"
                    : "SoulSlashVisual",
            rainStyle
                ? vfx.SwordRainColor
                : additionalStyle
                    ? vfx.AdditionalSlashColor
                    : vfx.SoulSlashColor,
            rainStyle ? 0.16f : additionalStyle ? 0.1f : 0.13f,
            rainStyle ? 7 : 3,
            duration);
        if (line == null)
        {
            return;
        }

        line.transform.position = position;
        line.useWorldSpace = false;
        if (rainStyle)
        {
            line.SetPosition(0, new Vector3(-0.8f, 1f));
            line.SetPosition(1, new Vector3(0.35f, -0.85f));
            line.SetPosition(2, new Vector3(0.05f, -0.45f));
            line.SetPosition(3, new Vector3(0.75f, 1f));
            line.SetPosition(4, new Vector3(-0.3f, -0.9f));
            line.SetPosition(5, new Vector3(-0.05f, -0.5f));
            line.SetPosition(6, new Vector3(0f, 1.1f));
        }
        else if (additionalStyle)
        {
            line.SetPosition(0, new Vector3(0.45f, 0.72f));
            line.SetPosition(1, new Vector3(-0.48f, -0.62f));
            line.SetPosition(2, new Vector3(-0.12f, -0.42f));
        }
        else
        {
            line.SetPosition(0, new Vector3(-0.5f, 0.85f));
            line.SetPosition(1, new Vector3(0.5f, -0.7f));
            line.SetPosition(2, new Vector3(0.15f, -0.48f));
        }
    }

    private void CreateFrostMarker(Vector2 position, float radius, float duration)
    {
        const int segments = 32;
        LineRenderer line = AcquireLineVisual(
            "ItemFrostVisual",
            new Color(0.55f, 0.9f, 1f, 0.9f),
            0.08f,
            segments + 1,
            duration);
        if (line == null)
        {
            return;
        }

        line.loop = false;
        line.transform.position = position;
        line.useWorldSpace = false;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            line.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f));
        }
    }

    private void CreateIceSynergyMarker(
        Vector2 position,
        float radius,
        float duration,
        bool frostBurst)
    {
        const int segments = 32;
        CombatVfxSettings vfx = GetCombatVfxSettings();
        LineRenderer line = AcquireLineVisual(
            frostBurst ? "FrostBurstVisual" : "ColdWaveVisual",
            frostBurst
                ? vfx.FrostBurstColor
                : vfx.ColdWaveColor,
            frostBurst ? 0.14f : 0.1f,
            segments + 1,
            duration);
        if (line == null)
        {
            return;
        }

        line.loop = false;
        line.transform.position = position;
        line.useWorldSpace = false;
        float sizeMultiplier = vfx.EffectSizeMultiplier;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float styledRadius = radius * sizeMultiplier;
            if (frostBurst && i % 2 != 0)
            {
                styledRadius *= 0.78f;
            }

            line.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * styledRadius,
                Mathf.Sin(angle) * styledRadius,
                0f));
        }
    }

    private void CreateImpactMarker(
        Vector2 position,
        Color color,
        float radius,
        float width,
        float duration,
        string objectName)
    {
        const int segments = 12;
        LineRenderer line = AcquireLineVisual(
            objectName,
            color,
            width,
            segments + 1,
            duration);
        if (line == null)
        {
            return;
        }

        line.transform.position = position;
        line.useWorldSpace = false;
        float scaledRadius =
            radius *
            GetCombatVfxSettings().EffectSizeMultiplier;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float styledRadius = i % 2 == 0
                ? scaledRadius
                : scaledRadius * 0.72f;
            line.SetPosition(
                i,
                new Vector3(
                    Mathf.Cos(angle) * styledRadius,
                    Mathf.Sin(angle) * styledRadius,
                    0f));
        }
    }

    private LineRenderer AcquireLineVisual(
        string objectName,
        Color color,
        float width,
        int positionCount,
        float duration)
    {
        combatVfxPool ??= new CombatVfxPool(
            transform,
            GetCombatVfxSettings());
        return combatVfxPool.AcquireTransient(
            objectName,
            color,
            width,
            positionCount,
            duration,
            28);
    }

    private void OnDisable()
    {
        if (boundInventory != null)
        {
            boundInventory.Changed -= OnInventoryChanged;
            boundInventory = null;
        }

        ClearRuntimeState(false);
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        combatVfxPool?.Dispose();
        combatVfxPool = null;
    }

    private void OnValidate()
    {
        gameplayViewport.x = Mathf.Clamp01(gameplayViewport.x);
        gameplayViewport.y = Mathf.Clamp01(gameplayViewport.y);
        gameplayViewport.width = Mathf.Clamp(
            gameplayViewport.width, 0.01f, 1f - gameplayViewport.x);
        gameplayViewport.height = Mathf.Clamp(
            gameplayViewport.height, 0.01f, 1f - gameplayViewport.y);
        GetSingleElementThresholds().Normalize();
        GetCrowdControlPolicy().Normalize();
        GetElectricSynergySettings().Normalize();
        GetSwordSynergySettings().Normalize();
        GetIceSynergySettings().Normalize();
        GetCombinedSynergyCombatSettings().Normalize();
        GetCombatVfxSettings().Normalize();
        stormOrbMaximumLevel = Mathf.Max(1, stormOrbMaximumLevel);
        capacitorMaximumLevel = Mathf.Max(1, capacitorMaximumLevel);
        scabbardMaximumLevel = Mathf.Max(1, scabbardMaximumLevel);
        swordArrayMaximumLevel = Mathf.Max(1, swordArrayMaximumLevel);
        frostSigilMaximumLevel = Mathf.Max(1, frostSigilMaximumLevel);
        frostCrystalMaximumLevel = Mathf.Max(1, frostCrystalMaximumLevel);
        stormOrbDamageBonusPerLevel = Mathf.Max(0f, stormOrbDamageBonusPerLevel);
        capacitorDamageBonusPerLevel = Mathf.Max(0f, capacitorDamageBonusPerLevel);
        scabbardDamageBonusPerLevel = Mathf.Max(0f, scabbardDamageBonusPerLevel);
        swordArrayDamageBonusPerLevel = Mathf.Max(0f, swordArrayDamageBonusPerLevel);
        frostSigilDurationPerLevel = Mathf.Max(0f, frostSigilDurationPerLevel);
        frostCrystalDurationPerLevel = Mathf.Max(0f, frostCrystalDurationPerLevel);
        capacitorRequiredHits = Mathf.Max(1, capacitorRequiredHits);
        capacitorMaximumTargets = Mathf.Max(1, capacitorMaximumTargets);
        scabbardRequiredHits = Mathf.Max(1, scabbardRequiredHits);
        frostSigilRequiredHits = Mathf.Max(1, frostSigilRequiredHits);
        frostCrystalMaximumTargets = Mathf.Max(1, frostCrystalMaximumTargets);
    }
}
