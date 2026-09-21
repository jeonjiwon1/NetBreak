using System;
using System.Collections.Generic;
using UnityEngine;

public enum CombinedSynergyId
{
    None = 0,
    ThunderSwordResonance = 1,
    Superconductivity = 2,
    FrostSwordResonance = 3
}

public sealed class CombinedSynergyDefinition
{
    public CombinedSynergyDefinition(
        CombinedSynergyId id,
        string displayName,
        ItemElement firstElement,
        ItemElement secondElement,
        int requiredFirstLevel,
        int requiredSecondLevel,
        string shortDescription,
        string detailedDescription)
    {
        Id = id;
        DisplayName = displayName ?? string.Empty;
        FirstElement = firstElement;
        SecondElement = secondElement;
        RequiredFirstLevel = Math.Max(1, requiredFirstLevel);
        RequiredSecondLevel = Math.Max(1, requiredSecondLevel);
        ShortDescription = shortDescription ?? string.Empty;
        DetailedDescription = detailedDescription ?? string.Empty;
    }

    public CombinedSynergyId Id { get; }
    public string DisplayName { get; }
    public ItemElement FirstElement { get; }
    public ItemElement SecondElement { get; }
    public int RequiredFirstLevel { get; }
    public int RequiredSecondLevel { get; }
    public string ShortDescription { get; }
    public string DetailedDescription { get; }

    public bool IsEligible(
        RunItemInventory inventory,
        CombinedSynergyUnlockSettings settings = null)
    {
        int firstRequirement = settings != null
            ? settings.GetRequiredFirstLevel(this)
            : RequiredFirstLevel;
        int secondRequirement = settings != null
            ? settings.GetRequiredSecondLevel(this)
            : RequiredSecondLevel;
        return
        inventory != null &&
        inventory.GetElementLevel(FirstElement) >= firstRequirement &&
        inventory.GetElementLevel(SecondElement) >= secondRequirement;
    }
}

[Serializable]
public sealed class CombinedSynergyUnlockSettings
{
    [Tooltip("뇌검 공명의 전기/검 각각에 필요한 속성 레벨입니다.")]
    [Min(1)] [SerializeField] private int thunderSwordRequiredLevel = 2;
    [Tooltip("초전도의 전기/얼음 각각에 필요한 속성 레벨입니다.")]
    [Min(1)] [SerializeField] private int superconductivityRequiredLevel = 2;
    [Tooltip("빙검 공명의 검/얼음 각각에 필요한 속성 레벨입니다.")]
    [Min(1)] [SerializeField] private int frostSwordRequiredLevel = 2;

    public int GetRequiredFirstLevel(CombinedSynergyDefinition definition) =>
        GetRequiredLevel(definition);

    public int GetRequiredSecondLevel(CombinedSynergyDefinition definition) =>
        GetRequiredLevel(definition);

    private int GetRequiredLevel(CombinedSynergyDefinition definition)
    {
        if (definition == null)
        {
            return 1;
        }

        int configured = definition.Id switch
        {
            CombinedSynergyId.ThunderSwordResonance => thunderSwordRequiredLevel,
            CombinedSynergyId.Superconductivity => superconductivityRequiredLevel,
            CombinedSynergyId.FrostSwordResonance => frostSwordRequiredLevel,
            _ => 1
        };
        return Math.Max(1, configured);
    }
}

public static class CombinedSynergyCatalog
{
    public const bool CombatEffectsImplemented = true;

    private static readonly CombinedSynergyDefinition[] Definitions =
    {
        new(
            CombinedSynergyId.ThunderSwordResonance,
            "뇌검 공명",
            ItemElement.Electric,
            ItemElement.Sword,
            2,
            2,
            "연쇄 방전 표식을 영혼 참격으로 폭발시킵니다.",
            "연쇄 방전이 유효한 물고기에 8초 표식을 남깁니다. 영혼 참격이 표식을 소비하면 대상에게 추가 저항력 피해 18, 주변 최대 2마리에게 각각 8 피해를 줍니다. 내부 쿨다운 8초."),
        new(
            CombinedSynergyId.Superconductivity,
            "초전도",
            ItemElement.Electric,
            ItemElement.Ice,
            2,
            2,
            "얼음 둔화 대상의 연쇄 방전이 추가 번개를 전파합니다.",
            "얼음 시너지 둔화 중인 물고기에 연쇄 방전이 적중하면 주변 최대 2마리에게 각각 저항력 피해 8을 주고 둔화를 최대 0.5초 연장합니다. 내부 쿨다운 6초."),
        new(
            CombinedSynergyId.FrostSwordResonance,
            "빙검 공명",
            ItemElement.Sword,
            ItemElement.Ice,
            2,
            2,
            "얼음 제어 대상의 영혼 참격이 추가 범위 참격을 만듭니다.",
            "얼음 시너지 둔화 또는 빙결 중인 물고기에 영혼 참격이 적중하면 주변 최대 2마리에게 각각 저항력 피해 10을 주고 1.5초 동안 20% 추가 둔화를 적용합니다. 내부 쿨다운 6초.")
    };

    public static IReadOnlyList<CombinedSynergyDefinition> All => Definitions;

    public static bool TryGet(
        CombinedSynergyId id,
        out CombinedSynergyDefinition definition)
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            if (Definitions[i].Id == id)
            {
                definition = Definitions[i];
                return true;
            }
        }

        definition = null;
        return false;
    }
}

[Serializable]
public sealed class CombinedSynergyCombatSettings
{
    public const float DefaultConductiveMarkDuration = 8f;
    public const float DefaultThunderSwordPrimaryDamage = 18f;
    public const int DefaultThunderSwordAdditionalTargets = 2;
    public const float DefaultThunderSwordAdditionalDamage = 8f;
    public const float DefaultThunderSwordCooldown = 8f;
    public const int DefaultSuperconductivityAdditionalTargets = 2;
    public const float DefaultSuperconductivityDamage = 8f;
    public const float DefaultSuperconductivitySlowExtension = 0.5f;
    public const float DefaultSuperconductivityCooldown = 6f;
    public const int DefaultFrostSwordAdditionalTargets = 2;
    public const float DefaultFrostSwordDamage = 10f;
    public const float DefaultFrostSwordSlowPercentage = 0.2f;
    public const float DefaultFrostSwordSlowDuration = 1.5f;
    public const float DefaultFrostSwordCooldown = 6f;

    [Header("Thunder Sword Resonance / 뇌검 공명")]
    [Min(0f)] [SerializeField] private float conductiveMarkDuration = DefaultConductiveMarkDuration;
    [Min(0f)] [SerializeField] private float thunderSwordPrimaryDamage = DefaultThunderSwordPrimaryDamage;
    [Min(0)] [SerializeField] private int thunderSwordAdditionalTargets = DefaultThunderSwordAdditionalTargets;
    [Min(0f)] [SerializeField] private float thunderSwordAdditionalDamage = DefaultThunderSwordAdditionalDamage;
    [Min(0f)] [SerializeField] private float thunderSwordCooldown = DefaultThunderSwordCooldown;

    [Header("Superconductivity / 초전도")]
    [Min(0)] [SerializeField] private int superconductivityAdditionalTargets = DefaultSuperconductivityAdditionalTargets;
    [Min(0f)] [SerializeField] private float superconductivityDamage = DefaultSuperconductivityDamage;
    [Min(0f)] [SerializeField] private float superconductivitySlowExtension = DefaultSuperconductivitySlowExtension;
    [Min(0f)] [SerializeField] private float superconductivityCooldown = DefaultSuperconductivityCooldown;

    [Header("Frost Sword Resonance / 빙검 공명")]
    [Min(0)] [SerializeField] private int frostSwordAdditionalTargets = DefaultFrostSwordAdditionalTargets;
    [Min(0f)] [SerializeField] private float frostSwordDamage = DefaultFrostSwordDamage;
    [Range(0f, 1f)] [SerializeField] private float frostSwordSlowPercentage = DefaultFrostSwordSlowPercentage;
    [Min(0f)] [SerializeField] private float frostSwordSlowDuration = DefaultFrostSwordSlowDuration;
    [Min(0f)] [SerializeField] private float frostSwordCooldown = DefaultFrostSwordCooldown;

    public float ConductiveMarkDuration => Mathf.Max(0f, conductiveMarkDuration);
    public float ThunderSwordPrimaryDamage => Mathf.Max(0f, thunderSwordPrimaryDamage);
    public int ThunderSwordAdditionalTargets => Math.Max(0, thunderSwordAdditionalTargets);
    public float ThunderSwordAdditionalDamage => Mathf.Max(0f, thunderSwordAdditionalDamage);
    public float ThunderSwordCooldown => Mathf.Max(0f, thunderSwordCooldown);
    public int SuperconductivityAdditionalTargets => Math.Max(0, superconductivityAdditionalTargets);
    public float SuperconductivityDamage => Mathf.Max(0f, superconductivityDamage);
    public float SuperconductivitySlowExtension => Mathf.Max(0f, superconductivitySlowExtension);
    public float SuperconductivityCooldown => Mathf.Max(0f, superconductivityCooldown);
    public int FrostSwordAdditionalTargets => Math.Max(0, frostSwordAdditionalTargets);
    public float FrostSwordDamage => Mathf.Max(0f, frostSwordDamage);
    public float FrostSwordSlowPercentage => Mathf.Clamp01(frostSwordSlowPercentage);
    public float FrostSwordSlowDuration => Mathf.Max(0f, frostSwordSlowDuration);
    public float FrostSwordCooldown => Mathf.Max(0f, frostSwordCooldown);

    public void Normalize()
    {
        conductiveMarkDuration = ConductiveMarkDuration;
        thunderSwordPrimaryDamage = ThunderSwordPrimaryDamage;
        thunderSwordAdditionalTargets = ThunderSwordAdditionalTargets;
        thunderSwordAdditionalDamage = ThunderSwordAdditionalDamage;
        thunderSwordCooldown = ThunderSwordCooldown;
        superconductivityAdditionalTargets = SuperconductivityAdditionalTargets;
        superconductivityDamage = SuperconductivityDamage;
        superconductivitySlowExtension = SuperconductivitySlowExtension;
        superconductivityCooldown = SuperconductivityCooldown;
        frostSwordAdditionalTargets = FrostSwordAdditionalTargets;
        frostSwordDamage = FrostSwordDamage;
        frostSwordSlowPercentage = FrostSwordSlowPercentage;
        frostSwordSlowDuration = FrostSwordSlowDuration;
        frostSwordCooldown = FrostSwordCooldown;
    }
}

public sealed class CombinedSynergyCombatRuntime
{
    private readonly Dictionary<SynergyTargetKey, float> conductiveMarks = new();
    private float thunderSwordReadyAt = float.NegativeInfinity;
    private float superconductivityReadyAt = float.NegativeInfinity;
    private float frostSwordReadyAt = float.NegativeInfinity;

    public int ConductiveMarkCount => conductiveMarks.Count;

    public void ApplyConductiveMark(SynergyTargetKey target, float scaledTime, float duration)
    {
        if (target.InstanceId == 0 || !IsValidTime(scaledTime)) return;
        conductiveMarks[target] = AddDuration(scaledTime, duration);
    }

    public bool HasConductiveMark(SynergyTargetKey target, float scaledTime)
    {
        if (target.InstanceId == 0 || !IsValidTime(scaledTime) ||
            !conductiveMarks.TryGetValue(target, out float expiresAt))
        {
            return false;
        }

        if (scaledTime < expiresAt)
        {
            return true;
        }

        conductiveMarks.Remove(target);
        return false;
    }

    public bool TryConsumeConductiveMark(SynergyTargetKey target, float scaledTime, float cooldown)
    {
        if (!HasConductiveMark(target, scaledTime) || scaledTime < thunderSwordReadyAt) return false;
        conductiveMarks.Remove(target);
        thunderSwordReadyAt = AddDuration(scaledTime, cooldown);
        return true;
    }

    public bool TryBeginSuperconductivity(float scaledTime, float cooldown) =>
        TryBegin(ref superconductivityReadyAt, scaledTime, cooldown);

    public bool TryBeginFrostSword(float scaledTime, float cooldown) =>
        TryBegin(ref frostSwordReadyAt, scaledTime, cooldown);

    public void ClearTarget(int instanceId)
    {
        List<SynergyTargetKey> stale = null;
        foreach (SynergyTargetKey key in conductiveMarks.Keys)
        {
            if (key.InstanceId != instanceId) continue;
            stale ??= new List<SynergyTargetKey>();
            stale.Add(key);
        }
        if (stale == null) return;
        for (int i = 0; i < stale.Count; i++) conductiveMarks.Remove(stale[i]);
    }

    public void Reset()
    {
        conductiveMarks.Clear();
        thunderSwordReadyAt = float.NegativeInfinity;
        superconductivityReadyAt = float.NegativeInfinity;
        frostSwordReadyAt = float.NegativeInfinity;
    }

    private static bool TryBegin(ref float readyAt, float scaledTime, float cooldown)
    {
        if (!IsValidTime(scaledTime) || scaledTime < readyAt) return false;
        readyAt = AddDuration(scaledTime, cooldown);
        return true;
    }

    private static float AddDuration(float scaledTime, float duration)
    {
        double result = (double)scaledTime + Math.Max(0f, duration);
        return result >= float.MaxValue ? float.MaxValue : (float)result;
    }

    private static bool IsValidTime(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
}

public enum CombinedSynergyActivationResult
{
    Success = 0,
    FeatureUnavailable = 1,
    NoSelection = 2,
    InvalidCombination = 3,
    Ineligible = 4,
    AlreadyActive = 5,
    CooldownActive = 6
}

public sealed class RunCombinedSynergyState
{
    private readonly float switchCooldownDuration;
    private readonly CombinedSynergyUnlockSettings unlockSettings;
    private float nextSwitchAllowedAt;

    public RunCombinedSynergyState(
        float switchCooldownDuration = 30f,
        CombinedSynergyUnlockSettings unlockSettings = null)
    {
        this.switchCooldownDuration =
            float.IsNaN(switchCooldownDuration) || switchCooldownDuration < 0f
                ? 30f
                : switchCooldownDuration;
        this.unlockSettings = unlockSettings ?? new CombinedSynergyUnlockSettings();
    }

    public CombinedSynergyId SelectedId { get; private set; }
    public CombinedSynergyId ActiveId { get; private set; }
    public float SwitchCooldownDuration => switchCooldownDuration;

    public bool IsEligible(
        CombinedSynergyId id,
        RunItemInventory inventory) =>
        CombinedSynergyCatalog.TryGet(id, out CombinedSynergyDefinition definition) &&
        definition.IsEligible(inventory, unlockSettings);

    public int GetRequiredFirstLevel(CombinedSynergyDefinition definition) =>
        unlockSettings.GetRequiredFirstLevel(definition);

    public int GetRequiredSecondLevel(CombinedSynergyDefinition definition) =>
        unlockSettings.GetRequiredSecondLevel(definition);

    public bool TrySelect(
        CombinedSynergyId id,
        RunItemInventory inventory)
    {
        if (!IsEligible(id, inventory))
        {
            return false;
        }

        SelectedId = id;
        return true;
    }

    public bool TryAutoActivateFirstEligible(
        RunItemInventory inventory,
        float scaledGameplayTime,
        bool combatEffectsAvailable)
    {
        if (!combatEffectsAvailable || ActiveId != CombinedSynergyId.None ||
            inventory == null || !IsValidTime(scaledGameplayTime))
        {
            return false;
        }

        IReadOnlyList<CombinedSynergyDefinition> catalog = CombinedSynergyCatalog.All;
        for (int i = 0; i < catalog.Count; i++)
        {
            if (!catalog[i].IsEligible(inventory, unlockSettings))
            {
                continue;
            }

            ActiveId = catalog[i].Id;
            SelectedId = ActiveId;
            nextSwitchAllowedAt = scaledGameplayTime;
            return true;
        }

        return false;
    }

    public CombinedSynergyActivationResult TryActivateSelected(
        RunItemInventory inventory,
        float scaledGameplayTime,
        bool combatEffectsAvailable)
    {
        if (!combatEffectsAvailable)
        {
            return CombinedSynergyActivationResult.FeatureUnavailable;
        }

        if (SelectedId == CombinedSynergyId.None)
        {
            return CombinedSynergyActivationResult.NoSelection;
        }

        if (!CombinedSynergyCatalog.TryGet(SelectedId, out CombinedSynergyDefinition definition))
        {
            return CombinedSynergyActivationResult.InvalidCombination;
        }

        if (!definition.IsEligible(inventory, unlockSettings))
        {
            return CombinedSynergyActivationResult.Ineligible;
        }

        if (SelectedId == ActiveId)
        {
            return CombinedSynergyActivationResult.AlreadyActive;
        }

        if (!IsValidTime(scaledGameplayTime) ||
            GetRemainingSwitchCooldown(scaledGameplayTime) > 0f)
        {
            return CombinedSynergyActivationResult.CooldownActive;
        }

        ActiveId = SelectedId;
        nextSwitchAllowedAt = scaledGameplayTime + switchCooldownDuration;
        return CombinedSynergyActivationResult.Success;
    }

    public float GetRemainingSwitchCooldown(float scaledGameplayTime)
    {
        if (!IsValidTime(scaledGameplayTime))
        {
            return 0f;
        }

        return Math.Max(0f, nextSwitchAllowedAt - scaledGameplayTime);
    }

    private static bool IsValidTime(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0f;
}
