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
    public const bool CombatEffectsImplemented = false;

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
            "연쇄 방전이 유효한 물고기에 8초 표식을 남깁니다. 영혼 참격이 표식을 소비하면 대상에게 추가 저항력 피해 18, 주변 최대 2마리에게 각각 8 피해를 줍니다. 내부 쿨다운 8초. (G6-C2 구현 예정)"),
        new(
            CombinedSynergyId.Superconductivity,
            "초전도",
            ItemElement.Electric,
            ItemElement.Ice,
            2,
            2,
            "얼음 둔화 대상의 연쇄 방전이 추가 번개를 전파합니다.",
            "얼음 시너지 둔화 중인 물고기에 연쇄 방전이 적중하면 주변 최대 2마리에게 각각 저항력 피해 8을 주고 둔화를 최대 0.5초 연장합니다. 내부 쿨다운 6초. (G6-C2 구현 예정)"),
        new(
            CombinedSynergyId.FrostSwordResonance,
            "빙검 공명",
            ItemElement.Sword,
            ItemElement.Ice,
            2,
            2,
            "얼음 제어 대상의 영혼 참격이 추가 범위 참격을 만듭니다.",
            "얼음 시너지 둔화 또는 빙결 중인 물고기에 영혼 참격이 적중하면 주변 최대 2마리에게 각각 저항력 피해 10을 주고 1.5초 동안 20% 추가 둔화를 적용합니다. 내부 쿨다운 6초. (G6-C2 구현 예정)")
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
