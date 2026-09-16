using System;
using System.Collections.Generic;

public static class SkillTreeEffectIds
{
    public const string FishingRodPower = "fishing_rod_power";
    public const string FishingRodRange = "fishing_rod_range";
    public const string NetLength = "net_length";
    public const string NetCount = "net_count";
    public const string CastNetPower = "cast_net_power";
    public const string CastNetRadius = "cast_net_radius";
    public const string BaitRadius = "bait_radius";
    public const string BaitDuration = "bait_duration";
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
        return definitions;
    }

    private static void AddForRole(
        ICollection<SkillTreeDefinition> definitions,
        ToolId tool,
        GrowthToolRole role)
    {
        string treeId = $"vs_{role.ToString().ToLowerInvariant()}_{tool.ToString().ToLowerInvariant()}";
        SkillTreeNodeDefinition primary;
        SkillTreeNodeDefinition secondary;

        switch (tool)
        {
            case ToolId.FishingRod:
                primary = CreatePrimary(
                    "rod_power",
                    "강한 낚싯바늘",
                    "낚싯대의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.FishingRodPower,
                    1f, 1.5f, 2f);
                secondary = CreateSecondary(
                    "rod_range",
                    "긴 낚싯줄",
                    "낚싯대의 포획 범위를 0.5 늘립니다.",
                    "rod_power",
                    SkillTreeEffectIds.FishingRodRange,
                    0.5f);
                break;

            case ToolId.Net:
                primary = CreatePrimary(
                    "net_length",
                    "질긴 그물실",
                    "설치 가능한 그물의 최대 길이를 늘립니다.",
                    SkillTreeEffectIds.NetLength,
                    1f, 1.5f, 2f);
                secondary = CreateSecondary(
                    "net_count",
                    "여분의 그물",
                    "동시에 설치할 수 있는 그물 수를 1 늘립니다.",
                    "net_length",
                    SkillTreeEffectIds.NetCount,
                    1f);
                break;

            case ToolId.CastNet:
                primary = CreatePrimary(
                    "cast_power",
                    "무거운 투망",
                    "투망의 포획력을 랭크마다 높입니다.",
                    SkillTreeEffectIds.CastNetPower,
                    5f, 7f, 10f);
                secondary = CreateSecondary(
                    "cast_radius",
                    "넓은 투망",
                    "투망의 포획 반경을 0.25 늘립니다.",
                    "cast_power",
                    SkillTreeEffectIds.CastNetRadius,
                    0.25f);
                break;

            case ToolId.Bait:
                primary = CreatePrimary(
                    "bait_radius",
                    "진한 미끼 향",
                    "미끼의 유인 반경을 랭크마다 늘립니다.",
                    SkillTreeEffectIds.BaitRadius,
                    0.75f, 1f, 1.25f);
                secondary = CreateSecondary(
                    "bait_duration",
                    "오래가는 미끼",
                    "미끼의 지속시간을 1초 늘립니다.",
                    "bait_radius",
                    SkillTreeEffectIds.BaitDuration,
                    1f);
                break;

            default:
                return;
        }

        definitions.Add(SkillTreeDefinition.CreateRuntime(
            treeId,
            tool,
            role,
            primary,
            secondary));
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
}
