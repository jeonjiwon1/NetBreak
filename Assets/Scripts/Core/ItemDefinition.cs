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
        string plannedEffectDescription)
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
    }

    public string ItemId { get; }
    public ItemElement Element { get; }
    public string DisplayName { get; }
    public string ShortLabel { get; }
    public string PlannedEffectDescription { get; }

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
            "8초마다 대상이 될 수 있는 물고기 한 마리를 번개로 자동 공격합니다."),
        new(CapacitorCoilId, ItemElement.Electric, "축전 코일", "축전",
            "유효한 도구 적중 5회를 모으면 주변 물고기 최대 두 마리에게 연쇄 번개를 일으킵니다."),
        new(SpectralScabbardId, ItemElement.Sword, "유령 검집", "검집",
            "같은 물고기에 유효한 도구 적중 4회를 기록하면 그 물고기를 검으로 공격합니다."),
        new(AutonomousSwordArrayId, ItemElement.Sword, "자동 검진", "검진",
            "10초마다 남은 저항력이 가장 높은 대상 물고기를 자동 공격합니다."),
        new(FrostSigilId, ItemElement.Ice, "서리 인장", "서리",
            "같은 물고기에 유효한 도구 적중 3회를 기록하면 2초 동안 이동 속도를 40% 낮춥니다."),
        new(FrostCrystalId, ItemElement.Ice, "빙결 결정", "빙결",
            "12초마다 대상 물고기 최대 두 마리에게 3초 동안 30% 둔화와 소량의 저항력 피해를 줍니다.")
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
