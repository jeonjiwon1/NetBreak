using System;
using System.Collections.Generic;

public static class SkillTreeEffectIds
{
    public const string FishingRodPower = "fishing_rod_power";
    public const string FishingRodRange = "fishing_rod_range";
    public const string FishingRodCapacity = "fishing_rod_capacity";
    public const string FishingRodCostMultiplier = "fishing_rod_cost_multiplier";
    public const string NetLength = "net_length";
    public const string NetCount = "net_count";
    public const string NetCostMultiplier = "net_cost_multiplier";
    public const string NetPowerMultiplier = "net_power_multiplier";
    public const string CastNetPower = "cast_net_power";
    public const string CastNetRadius = "cast_net_radius";
    public const string CastNetCharges = "cast_net_charges";
    public const string BaitRadius = "bait_radius";
    public const string BaitDuration = "bait_duration";
    public const string TacticalEffectMultiplier = "tactical_effect_multiplier";
    public const string TacticalCooldownMultiplier = "tactical_cooldown_multiplier";
    public const string SignatureTraversalCount = "signature_traversal_count";
    public const string SignatureDurationMultiplier = "signature_duration_multiplier";
    public const string SignatureDamageMultiplier = "signature_damage_multiplier";
    public const string SignatureCastCount = "signature_cast_count";
}

public static class VerticalSliceSkillTreeDefaults
{
    public static IReadOnlyList<SkillTreeDefinition> CreateAll()
    {
        List<SkillTreeDefinition> definitions = new();
        AddForRole(definitions, ToolId.FishingRod, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.Net, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.CastNet, GrowthToolRole.Core);
        AddForRole(definitions, ToolId.FishingRod, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.Net, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.CastNet, GrowthToolRole.Partner);
        AddForRole(definitions, ToolId.Bait, GrowthToolRole.Partner);
        AddTacticalDefinitions(definitions);
        AddSignatureDefinitions(definitions);
        return definitions;
    }

    private static void AddTacticalDefinitions(
        ICollection<SkillTreeDefinition> definitions)
    {
        AddTactical(definitions, TacticalSkillManager.RapidReelingAbilityId,
            "급속 릴링", 1.25f, 20f / 24f);
        AddTactical(definitions, TacticalSkillManager.EmergencyLockdownAbilityId,
            "긴급 봉쇄", 2f / 1.75f, 24f / 28f);
        AddTactical(definitions, TacticalSkillManager.EmergencyCastNetAbilityId,
            "비상 투망", 1.2f, 15f / 18f);
        AddTactical(definitions, TacticalSkillManager.OverbaitingAbilityId,
            "과잉 집어", 2.1f / 1.75f, 18f / 22f);
        AddTactical(definitions, TacticalSkillManager.FocusedOperationAbilityId,
            "집중 조업", 1.8f / 1.5f, 22f / 26f);
    }

    private static void AddTactical(
        ICollection<SkillTreeDefinition> definitions,
        string abilityId,
        string skillName,
        float effectMultiplier,
        float cooldownMultiplier)
    {
        string suffix = abilityId.Replace('.', '_');
        definitions.Add(SkillTreeDefinition.CreateAbilityRuntime(
            $"vs_e_{suffix}",
            GrowthTreeCategory.TacticalE,
            abilityId,
            new SkillTreeNodeDefinition(
                $"{suffix}_effect", "효과", $"{skillName}의 효과를 강화합니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[] { AbilityRank(1, SkillTreeEffectIds.TacticalEffectMultiplier, effectMultiplier) },
                new UnityEngine.Vector2(-90f, 0f)),
            new SkillTreeNodeDefinition(
                $"{suffix}_cooldown", "재사용", $"{skillName}의 재사용 대기시간을 줄입니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[] { AbilityRank(2, SkillTreeEffectIds.TacticalCooldownMultiplier, cooldownMultiplier) },
                new UnityEngine.Vector2(90f, 0f))));
    }

    private static void AddSignatureDefinitions(
        ICollection<SkillTreeDefinition> definitions)
    {
        definitions.Add(SkillTreeDefinition.CreateAbilityRuntime(
            "vs_r_fishing_ground_crossing", GrowthTreeCategory.SignatureR,
            SignatureSkillManager.FishingGroundCrossingAbilityId,
            new SkillTreeNodeDefinition(
                "r_traversals", "횡단", "어장 대횡단의 연속 횡단 횟수를 늘립니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[]
                {
                    AbilityRank(2, SkillTreeEffectIds.SignatureTraversalCount, 2f),
                    AbilityRank(3, SkillTreeEffectIds.SignatureTraversalCount, 3f)
                }, UnityEngine.Vector2.zero)));

        definitions.Add(SkillTreeDefinition.CreateAbilityRuntime(
            "vs_r_cross_lockdown", GrowthTreeCategory.SignatureR,
            SignatureSkillManager.CrossLockdownAbilityId,
            new SkillTreeNodeDefinition(
                "r_duration", "지속", "교차 봉쇄의 지속시간을 늘립니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[]
                {
                    AbilityRank(2, SkillTreeEffectIds.SignatureDurationMultiplier, 8f / 6f),
                    AbilityRank(3, SkillTreeEffectIds.SignatureDurationMultiplier, 10f / 6f)
                }, new UnityEngine.Vector2(-90f, 0f)),
            new SkillTreeNodeDefinition(
                "r_damage", "저항 피해", "교차 봉쇄의 Resistance 초당 피해를 높입니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[]
                {
                    AbilityRank(2, SkillTreeEffectIds.SignatureDamageMultiplier, 1.33f),
                    AbilityRank(3, SkillTreeEffectIds.SignatureDamageMultiplier, 1.67f)
                }, new UnityEngine.Vector2(90f, 0f))));

        definitions.Add(SkillTreeDefinition.CreateAbilityRuntime(
            "vs_r_heavenly_net", GrowthTreeCategory.SignatureR,
            SignatureSkillManager.HeavenlyNetAbilityId,
            new SkillTreeNodeDefinition(
                "r_casts", "연속 투망", "천망의 연속 시전 횟수를 늘립니다.",
                Array.Empty<SkillTreeNodePrerequisite>(),
                new[]
                {
                    AbilityRank(2, SkillTreeEffectIds.SignatureCastCount, 2f),
                    AbilityRank(3, SkillTreeEffectIds.SignatureCastCount, 3f)
                }, UnityEngine.Vector2.zero)));
    }

    private static SkillTreeRankDefinition AbilityRank(
        int cost, string effectId, float value) =>
        new(cost, new SkillTreeRankUnlockCondition(0, 1),
            new SkillTreeBalanceEffect(effectId, value));

    private static void AddForRole(
        ICollection<SkillTreeDefinition> definitions,
        ToolId tool,
        GrowthToolRole role)
    {
        string treeId = $"vs_{role.ToString().ToLowerInvariant()}_{tool.ToString().ToLowerInvariant()}";
        List<SkillTreeNodeDefinition> nodes = new();

        switch (tool)
        {
            case ToolId.FishingRod:
                nodes.Add(CreatePrimary(
                    "rod_power",
                    "강한 낚싯바늘",
                    "낚싯대의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.FishingRodPower,
                    1f, 1.5f, 2f));
                nodes.Add(CreateSecondary(
                    "rod_range",
                    "긴 낚싯줄",
                    "낚싯대의 포획 범위를 0.5 늘립니다.",
                    "rod_power",
                    SkillTreeEffectIds.FishingRodRange,
                    0.5f));
                nodes.Add(CreateAdvanced(
                    "rod_capacity",
                    "다중 낚싯대 운용",
                    "미니보스 포획 후 낚싯대 설치 상한을 단계적으로 늘립니다.",
                    "rod_power",
                    SkillTreeEffectIds.FishingRodCapacity,
                    (1, 3f), (1, 3f), (2, 3f)));
                nodes.Add(CreateAdvanced(
                    "rod_economy",
                    "효율적인 낚싯대 설치",
                    "미니보스 포획 후 낚싯대 설치 비용을 단계적으로 줄입니다.",
                    "rod_capacity",
                    SkillTreeEffectIds.FishingRodCostMultiplier,
                    (1, 0.9486833f), (1, 0.9486833f)));
                break;

            case ToolId.Net:
                nodes.Add(CreatePrimary(
                    "net_length",
                    "질긴 그물실",
                    "설치 가능한 그물의 최대 길이를 늘립니다.",
                    SkillTreeEffectIds.NetLength,
                    1f, 1.5f, 2f));
                nodes.Add(new SkillTreeNodeDefinition(
                    "net_count",
                    "여분의 그물",
                    "그물 설치 상한을 늘립니다. 후속 랭크는 미니보스 포획 후 열립니다.",
                    new[] { new SkillTreeNodePrerequisite("net_length", 1) },
                    new[]
                    {
                        Rank(1, 5, SkillTreeEffectIds.NetCount, 1f),
                        AdvancedRank(1, SkillTreeEffectIds.NetCount, 2f),
                        AdvancedRank(2, SkillTreeEffectIds.NetCount, 4f)
                    }));
                nodes.Add(CreateAdvanced(
                    "net_economy",
                    "효율적인 그물 설치",
                    "미니보스 포획 후 그물 설치 비용을 단계적으로 줄입니다.",
                    "net_count",
                    SkillTreeEffectIds.NetCostMultiplier,
                    (1, 0.83666f), (1, 0.83666f)));
                nodes.Add(CreateAdvanced(
                    "net_power",
                    "강화 그물코",
                    "미니보스 포획 후 그물 포획력을 단계적으로 높입니다.",
                    "net_count",
                    SkillTreeEffectIds.NetPowerMultiplier,
                    (1, 1.73205f), (2, 1.73205f)));
                break;

            case ToolId.CastNet:
                nodes.Add(CreatePrimary(
                    "cast_power",
                    "무거운 투망",
                    "투망의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.CastNetPower,
                    5f, 7f, 10f));
                nodes.Add(CreateSecondary(
                    "cast_radius",
                    "넓은 투망",
                    "투망의 포획 반경을 0.25 늘립니다.",
                    "cast_power",
                    SkillTreeEffectIds.CastNetRadius,
                    0.25f));
                nodes.Add(CreateAdvanced(
                    "cast_charges",
                    "여분의 투망",
                    "미니보스 포획 후 투망 최대 충전을 단계적으로 늘립니다.",
                    "cast_power",
                    SkillTreeEffectIds.CastNetCharges,
                    (1, 1f), (2, 1f)));
                break;

            case ToolId.Bait:
                nodes.Add(CreatePrimary(
                    "bait_radius",
                    "진한 미끼 향",
                    "미끼의 유인 반경을 랭크마다 늘립니다.",
                    SkillTreeEffectIds.BaitRadius,
                    0.75f, 1f, 1.25f));
                nodes.Add(CreateSecondary(
                    "bait_duration",
                    "오래가는 미끼",
                    "미끼의 지속시간을 1초 늘립니다.",
                    "bait_radius",
                    SkillTreeEffectIds.BaitDuration,
                    1f));
                break;

            default:
                return;
        }

        definitions.Add(SkillTreeDefinition.CreateRuntime(
            treeId,
            tool,
            role,
            nodes.ToArray()));
    }

    private static SkillTreeNodeDefinition CreatePrimary(
        string nodeId,
        string name,
        string description,
        string effectId,
        float rank1,
        float rank2,
        float rank3)
    {
        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            Array.Empty<SkillTreeNodePrerequisite>(),
            new[]
            {
                Rank(1, 4, effectId, rank1),
                Rank(2, 6, effectId, rank2),
                Rank(3, 0, effectId, rank3, 2)
            });
    }

    private static SkillTreeNodeDefinition CreateSecondary(
        string nodeId,
        string name,
        string description,
        string prerequisiteId,
        string effectId,
        float value)
    {
        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            new[] { new SkillTreeNodePrerequisite(prerequisiteId, 1) },
            new[] { Rank(1, 5, effectId, value) });
    }

    private static SkillTreeRankDefinition Rank(
        int cost,
        int minimumLevel,
        string effectId,
        float value,
        int minimumArea = 1)
    {
        return new SkillTreeRankDefinition(
            cost,
            new SkillTreeRankUnlockCondition(minimumLevel, minimumArea),
            new SkillTreeBalanceEffect(effectId, value));
    }

    private static SkillTreeNodeDefinition CreateAdvanced(
        string nodeId,
        string name,
        string description,
        string prerequisiteId,
        string effectId,
        params (int cost, float value)[] ranks)
    {
        SkillTreeRankDefinition[] rankDefinitions =
            new SkillTreeRankDefinition[ranks.Length];
        for (int i = 0; i < ranks.Length; i++)
        {
            rankDefinitions[i] = AdvancedRank(
                ranks[i].cost,
                effectId,
                ranks[i].value);
        }

        return new SkillTreeNodeDefinition(
            nodeId,
            name,
            description,
            new[] { new SkillTreeNodePrerequisite(prerequisiteId, 1) },
            rankDefinitions);
    }

    private static SkillTreeRankDefinition AdvancedRank(
        int cost,
        string effectId,
        float value) =>
        new(
            cost,
            new SkillTreeRankUnlockCondition(0, 1, miniBossClear: true),
            new SkillTreeBalanceEffect(effectId, value));
}
