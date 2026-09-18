using System;
using System.Collections.Generic;

public enum ItemElement
{
    Electric = 0,
    Sword = 1,
    Ice = 2
}

public sealed class ItemDefinition
{
    public ItemDefinition(
        string itemId,
        ItemElement element,
        string displayName,
        string shortLabel,
        string plannedEffectDescription,
        string primaryEffectDisplayName,
        string primaryValueSuffix = "")
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID is required.", nameof(itemId));
        }

        ItemId = itemId;
        Element = element;
        DisplayName = displayName ?? string.Empty;
        ShortLabel = shortLabel ?? string.Empty;
        PlannedEffectDescription = plannedEffectDescription ?? string.Empty;
        PrimaryEffectDisplayName = primaryEffectDisplayName ?? string.Empty;
        PrimaryValueSuffix = primaryValueSuffix ?? string.Empty;
    }

    public string ItemId { get; }
    public ItemElement Element { get; }
    public string DisplayName { get; }
    public string ShortLabel { get; }
    public string PlannedEffectDescription { get; }
    public string PrimaryEffectDisplayName { get; }
    public string PrimaryValueSuffix { get; }

    public string ElementDisplayName => Element switch
    {
        ItemElement.Electric => "전기",
        ItemElement.Sword => "검",
        ItemElement.Ice => "얼음",
        _ => "알 수 없음"
    };
}

public static class ItemCatalog
{
    public const string StormOrbId = "storm_orb";
    public const string CapacitorCoilId = "capacitor_coil";
    public const string SpectralScabbardId = "spectral_scabbard";
    public const string AutonomousSwordArrayId = "autonomous_sword_array";
    public const string FrostSigilId = "frost_sigil";
    public const string FrostCrystalId = "frost_crystal";

    private static readonly ItemDefinition[] Definitions =
    {
        new(StormOrbId, ItemElement.Electric, "폭풍 구슬", "폭풍",
            "8초마다 조업 영역에서 커서에 가장 가까운 물고기를 번개로 공격해 저항력 12 피해를 줍니다.",
            "번개 저항 피해"),
        new(CapacitorCoilId, ItemElement.Electric, "축전 코일", "축전",
            "유효한 도구 적중 5회마다 마지막 적중 지점 주변의 다른 물고기 최대 두 마리에게 저항력 8 피해를 줍니다.",
            "연쇄 저항 피해"),
        new(SpectralScabbardId, ItemElement.Sword, "유령 검집", "검집",
            "같은 물고기에 유효한 도구 적중 4회마다 유령 검으로 저항력 16 피해를 줍니다.",
            "유령 검 저항 피해"),
        new(AutonomousSwordArrayId, ItemElement.Sword, "자동 검진", "검진",
            "10초마다 조업 영역에서 남은 저항력이 가장 높은 물고기를 공격해 저항력 18 피해를 줍니다.",
            "자동 검 저항 피해"),
        new(FrostSigilId, ItemElement.Ice, "서리 인장", "서리",
            "같은 물고기에 유효한 도구 적중 3회마다 2초 동안 이동 속도를 40% 낮춥니다.",
            "둔화 지속시간", "초"),
        new(FrostCrystalId, ItemElement.Ice, "빙결 결정", "빙결",
            "12초마다 커서 반경 안의 가까운 물고기 최대 두 마리에게 저항력 5 피해와 3초간 30% 둔화를 줍니다.",
            "둔화 지속시간", "초")
    };

    public static IReadOnlyList<ItemDefinition> All => Definitions;

    public static bool TryGet(string itemId, out ItemDefinition definition)
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            if (string.Equals(Definitions[i].ItemId, itemId, StringComparison.Ordinal))
            {
                definition = Definitions[i];
                return true;
            }
        }

        definition = null;
        return false;
    }
}
