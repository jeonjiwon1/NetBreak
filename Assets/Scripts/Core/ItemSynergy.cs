using System;
using System.Collections.Generic;
using UnityEngine;

public enum SingleElementSynergyTier
{
    Level2 = 0,
    Level4 = 1,
    Level6 = 2,
    Level8 = 3,
    Level10 = 4
}

public readonly struct SingleElementSynergyState
{
    public SingleElementSynergyState(
        ItemElement element,
        int elementLevel,
        int level2Threshold,
        int level4Threshold,
        int level6Threshold,
        int level8Threshold,
        int level10Threshold)
    {
        Element = element;
        ElementLevel = Math.Max(0, elementLevel);
        IsLevel2Active = ElementLevel >= level2Threshold;
        IsLevel4Active = ElementLevel >= level4Threshold;
        IsLevel6Active = ElementLevel >= level6Threshold;
        IsLevel8Active = ElementLevel >= level8Threshold;
        IsLevel10Active = ElementLevel >= level10Threshold;
    }

    public ItemElement Element { get; }
    public int ElementLevel { get; }
    public bool IsLevel2Active { get; }
    public bool IsLevel4Active { get; }
    public bool IsLevel6Active { get; }
    public bool IsLevel8Active { get; }
    public bool IsLevel10Active { get; }

    public bool IsActive(SingleElementSynergyTier tier) => tier switch
    {
        SingleElementSynergyTier.Level2 => IsLevel2Active,
        SingleElementSynergyTier.Level4 => IsLevel4Active,
        SingleElementSynergyTier.Level6 => IsLevel6Active,
        SingleElementSynergyTier.Level8 => IsLevel8Active,
        SingleElementSynergyTier.Level10 => IsLevel10Active,
        _ => false
    };
}

[Serializable]
public sealed class SingleElementSynergyThresholds
{
    public const int DefaultLevel2Threshold = 2;
    public const int DefaultLevel4Threshold = 4;
    public const int DefaultLevel6Threshold = 6;
    public const int DefaultLevel8Threshold = 8;
    public const int DefaultLevel10Threshold = 10;

    [InspectorName("Level 2 Threshold")]
    [Min(1)] [SerializeField] private int level2Threshold = DefaultLevel2Threshold;
    [InspectorName("Level 4 Threshold")]
    [Min(1)] [SerializeField] private int level4Threshold = DefaultLevel4Threshold;
    [InspectorName("Level 6 Threshold")]
    [Min(1)] [SerializeField] private int level6Threshold = DefaultLevel6Threshold;
    [InspectorName("Level 8 Threshold")]
    [Min(1)] [SerializeField] private int level8Threshold = DefaultLevel8Threshold;
    [InspectorName("Level 10 Threshold")]
    [Min(1)] [SerializeField] private int level10Threshold = DefaultLevel10Threshold;

    public SingleElementSynergyThresholds()
    {
    }

    public SingleElementSynergyThresholds(
        int level2Threshold,
        int level4Threshold,
        int level6Threshold,
        int level8Threshold,
        int level10Threshold)
    {
        this.level2Threshold = level2Threshold;
        this.level4Threshold = level4Threshold;
        this.level6Threshold = level6Threshold;
        this.level8Threshold = level8Threshold;
        this.level10Threshold = level10Threshold;
        Normalize();
    }

    public int Level2Threshold => Math.Max(
        1,
        level2Threshold > 0 ? level2Threshold : DefaultLevel2Threshold);
    public int Level4Threshold => Math.Max(
        Level2Threshold,
        level4Threshold > 0 ? level4Threshold : DefaultLevel4Threshold);
    public int Level6Threshold => Math.Max(
        Level4Threshold,
        level6Threshold > 0 ? level6Threshold : DefaultLevel6Threshold);
    public int Level8Threshold => Math.Max(
        Level6Threshold,
        level8Threshold > 0 ? level8Threshold : DefaultLevel8Threshold);
    public int Level10Threshold => Math.Max(
        Level8Threshold,
        level10Threshold > 0 ? level10Threshold : DefaultLevel10Threshold);

    public int GetThreshold(SingleElementSynergyTier tier) => tier switch
    {
        SingleElementSynergyTier.Level2 => Level2Threshold,
        SingleElementSynergyTier.Level4 => Level4Threshold,
        SingleElementSynergyTier.Level6 => Level6Threshold,
        SingleElementSynergyTier.Level8 => Level8Threshold,
        SingleElementSynergyTier.Level10 => Level10Threshold,
        _ => int.MaxValue
    };

    public SingleElementSynergyState Evaluate(
        RunItemInventory inventory,
        ItemElement element)
    {
        int elementLevel = inventory?.GetElementLevel(element) ?? 0;
        return new SingleElementSynergyState(
            element,
            elementLevel,
            Level2Threshold,
            Level4Threshold,
            Level6Threshold,
            Level8Threshold,
            Level10Threshold);
    }

    public void Normalize()
    {
        level2Threshold = Level2Threshold;
        level4Threshold = Level4Threshold;
        level6Threshold = Level6Threshold;
        level8Threshold = Level8Threshold;
        level10Threshold = Level10Threshold;
    }
}

[Serializable]
public sealed class SynergyCrowdControlPolicy
{
    public const float DefaultMiniBossMultiplier = 0.5f;
    public const float DefaultBossMultiplier = 0.5f;

    [InspectorName("MiniBoss Crowd-Control Multiplier")]
    [Range(0f, 1f)] [SerializeField]
    private float miniBossMultiplier = DefaultMiniBossMultiplier;
    [InspectorName("Boss Crowd-Control Multiplier")]
    [Range(0f, 1f)] [SerializeField]
    private float bossMultiplier = DefaultBossMultiplier;

    public SynergyCrowdControlPolicy()
    {
    }

    public SynergyCrowdControlPolicy(
        float miniBossMultiplier,
        float bossMultiplier)
    {
        this.miniBossMultiplier = miniBossMultiplier;
        this.bossMultiplier = bossMultiplier;
        Normalize();
    }

    public float MiniBossMultiplier => Mathf.Clamp01(miniBossMultiplier);
    public float BossMultiplier => Mathf.Clamp01(bossMultiplier);

    public float GetMultiplier(FishSpecialType specialType) => specialType switch
    {
        FishSpecialType.MiniBoss => MiniBossMultiplier,
        FishSpecialType.Boss => BossMultiplier,
        _ => 1f
    };

    public float GetAdjustedDuration(
        float baseDuration,
        FishSpecialType specialType) =>
        Mathf.Max(0f, baseDuration) * GetMultiplier(specialType);

    public float GetAdjustedAdditionalSlowPercentage(
        float additionalSlowPercentage,
        FishSpecialType specialType) =>
        Mathf.Clamp01(additionalSlowPercentage) * GetMultiplier(specialType);

    public void Normalize()
    {
        miniBossMultiplier = MiniBossMultiplier;
        bossMultiplier = BossMultiplier;
    }
}

[Serializable]
public sealed class ElectricSynergySettings
{
    public const float DefaultChainCooldown = 7f;
    public const float DefaultChainRadius = 3f;
    public const int DefaultChainTargetCount = 2;
    public const float DefaultChainDamage = 10f;
    public const float DefaultChainVisualDuration = 0.45f;
    public const int DefaultConductiveAdditionalTargets = 1;
    public const float DefaultStunDuration = 1f;
    public const float DefaultRestunLockout = 3f;
    public const float DefaultOverchargeDamageMultiplier = 1.25f;
    public const float DefaultThunderstormCooldown = 15f;
    public const int DefaultThunderstormTargetCount = 5;
    public const float DefaultThunderstormDamage = 18f;
    public const float DefaultThunderstormVisualDuration = 0.65f;

    [Header("Level 2 / 연쇄 방전")]
    [InspectorName("Cooldown")]
    [Min(0f)] [SerializeField] private float chainCooldown = DefaultChainCooldown;
    [InspectorName("Radius")]
    [Min(0f)] [SerializeField] private float chainRadius = DefaultChainRadius;
    [InspectorName("Maximum Targets")]
    [Min(0)] [SerializeField] private int chainTargetCount = DefaultChainTargetCount;
    [InspectorName("Resistance Damage")]
    [Min(0f)] [SerializeField] private float chainDamage = DefaultChainDamage;
    [InspectorName("Visual Duration")]
    [Min(0f)] [SerializeField]
    private float chainVisualDuration = DefaultChainVisualDuration;

    [Header("Level 4 / 전도 확장")]
    [InspectorName("Additional Targets")]
    [Min(0)] [SerializeField]
    private int conductiveAdditionalTargets = DefaultConductiveAdditionalTargets;

    [Header("Level 6 / 감전 방전")]
    [InspectorName("Stun Duration")]
    [Min(0f)] [SerializeField] private float stunDuration = DefaultStunDuration;
    [InspectorName("Re-Stun Lockout After End")]
    [Min(0f)] [SerializeField] private float restunLockout = DefaultRestunLockout;

    [Header("Level 8 / 과충전")]
    [InspectorName("Synergy Damage Multiplier")]
    [Min(0f)] [SerializeField]
    private float overchargeDamageMultiplier = DefaultOverchargeDamageMultiplier;

    [Header("Level 10 / 천둥 폭풍")]
    [InspectorName("Cooldown")]
    [Min(0f)] [SerializeField]
    private float thunderstormCooldown = DefaultThunderstormCooldown;
    [InspectorName("Maximum Targets")]
    [Min(0)] [SerializeField]
    private int thunderstormTargetCount = DefaultThunderstormTargetCount;
    [InspectorName("Resistance Damage")]
    [Min(0f)] [SerializeField]
    private float thunderstormDamage = DefaultThunderstormDamage;
    [InspectorName("Visual Duration")]
    [Min(0f)] [SerializeField]
    private float thunderstormVisualDuration = DefaultThunderstormVisualDuration;

    public float ChainCooldown => Mathf.Max(0f, chainCooldown);
    public float ChainRadius => Mathf.Max(0f, chainRadius);
    public int ChainTargetCount => Math.Max(0, chainTargetCount);
    public float ChainDamage => Mathf.Max(0f, chainDamage);
    public float ChainVisualDuration => Mathf.Max(0f, chainVisualDuration);
    public int ConductiveAdditionalTargets => Math.Max(0, conductiveAdditionalTargets);
    public float StunDuration => Mathf.Max(0f, stunDuration);
    public float RestunLockout => Mathf.Max(0f, restunLockout);
    public float OverchargeDamageMultiplier => Mathf.Max(0f, overchargeDamageMultiplier);
    public float ThunderstormCooldown => Mathf.Max(0f, thunderstormCooldown);
    public int ThunderstormTargetCount => Math.Max(0, thunderstormTargetCount);
    public float ThunderstormDamage => Mathf.Max(0f, thunderstormDamage);
    public float ThunderstormVisualDuration => Mathf.Max(0f, thunderstormVisualDuration);

    public int GetEffectiveChainTargetCount(SingleElementSynergyState state)
    {
        long count = ChainTargetCount +
            (state.IsLevel4Active ? (long)ConductiveAdditionalTargets : 0L);
        return count >= int.MaxValue ? int.MaxValue : (int)count;
    }

    public float GetEffectiveDamage(float baseDamage, SingleElementSynergyState state) =>
        ElectricSynergyMath.CalculateDamage(
            baseDamage,
            state.IsLevel8Active ? OverchargeDamageMultiplier : 1f);

    public void Normalize()
    {
        chainCooldown = ChainCooldown;
        chainRadius = ChainRadius;
        chainTargetCount = ChainTargetCount;
        chainDamage = ChainDamage;
        chainVisualDuration = ChainVisualDuration;
        conductiveAdditionalTargets = ConductiveAdditionalTargets;
        stunDuration = StunDuration;
        restunLockout = RestunLockout;
        overchargeDamageMultiplier = OverchargeDamageMultiplier;
        thunderstormCooldown = ThunderstormCooldown;
        thunderstormTargetCount = ThunderstormTargetCount;
        thunderstormDamage = ThunderstormDamage;
        thunderstormVisualDuration = ThunderstormVisualDuration;
    }
}

public readonly struct SynergyTargetKey : IEquatable<SynergyTargetKey>
{
    public SynergyTargetKey(int instanceId, int lifecycleVersion)
    {
        InstanceId = instanceId;
        LifecycleVersion = lifecycleVersion;
    }

    public int InstanceId { get; }
    public int LifecycleVersion { get; }

    public bool Equals(SynergyTargetKey other) =>
        InstanceId == other.InstanceId &&
        LifecycleVersion == other.LifecycleVersion;

    public override bool Equals(object obj) =>
        obj is SynergyTargetKey other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            return InstanceId * 397 ^ LifecycleVersion;
        }
    }
}

public enum ElectricSynergyActivation
{
    None = 0,
    ChainDischarge = 1,
    Thunderstorm = 2
}

public sealed class ElectricSynergyRuntimeState
{
    private readonly Dictionary<SynergyTargetKey, float> stunLockoutEndsAt = new();
    private float chainDischargeReadyAt = float.NegativeInfinity;
    private float thunderstormReadyAt = float.NegativeInfinity;

    public int StunLockoutCount => stunLockoutEndsAt.Count;
    public float ChainDischargeReadyAt => chainDischargeReadyAt;
    public float ThunderstormReadyAt => thunderstormReadyAt;

    public ElectricSynergyActivation TryBeginActivation(
        SingleElementSynergyState state,
        float scaledTime,
        float chainCooldown,
        float thunderstormCooldown,
        bool hasChainTargets,
        bool hasThunderstormTargets)
    {
        if (float.IsNaN(scaledTime))
        {
            return ElectricSynergyActivation.None;
        }

        if (state.IsLevel10Active &&
            hasThunderstormTargets &&
            scaledTime >= thunderstormReadyAt)
        {
            thunderstormReadyAt = AddDuration(scaledTime, thunderstormCooldown);
            chainDischargeReadyAt = AddDuration(scaledTime, chainCooldown);
            return ElectricSynergyActivation.Thunderstorm;
        }

        if (state.IsLevel2Active &&
            hasChainTargets &&
            scaledTime >= chainDischargeReadyAt)
        {
            chainDischargeReadyAt = AddDuration(scaledTime, chainCooldown);
            return ElectricSynergyActivation.ChainDischarge;
        }

        return ElectricSynergyActivation.None;
    }

    public bool TryBeginStun(
        SynergyTargetKey target,
        float scaledTime,
        float stunDuration,
        float restunLockout)
    {
        if (target.InstanceId == 0 || stunDuration <= 0f || float.IsNaN(scaledTime))
        {
            return false;
        }

        if (stunLockoutEndsAt.TryGetValue(target, out float lockoutEnd) &&
            scaledTime < lockoutEnd)
        {
            return false;
        }

        stunLockoutEndsAt[target] = AddDuration(
            AddDuration(scaledTime, stunDuration),
            restunLockout);
        return true;
    }

    public bool IsStunLocked(SynergyTargetKey target, float scaledTime) =>
        stunLockoutEndsAt.TryGetValue(target, out float lockoutEnd) &&
        scaledTime < lockoutEnd;

    public void ClearTarget(int instanceId)
    {
        if (instanceId == 0 || stunLockoutEndsAt.Count == 0)
        {
            return;
        }

        List<SynergyTargetKey> stale = null;
        foreach (SynergyTargetKey key in stunLockoutEndsAt.Keys)
        {
            if (key.InstanceId != instanceId)
            {
                continue;
            }

            stale ??= new List<SynergyTargetKey>();
            stale.Add(key);
        }

        if (stale == null)
        {
            return;
        }

        for (int i = 0; i < stale.Count; i++)
        {
            stunLockoutEndsAt.Remove(stale[i]);
        }
    }

    public void Reset()
    {
        stunLockoutEndsAt.Clear();
        chainDischargeReadyAt = float.NegativeInfinity;
        thunderstormReadyAt = float.NegativeInfinity;
    }

    private static float AddDuration(float scaledTime, float duration)
    {
        double result = (double)scaledTime + Math.Max(0f, duration);
        return result >= float.MaxValue ? float.MaxValue : (float)result;
    }
}

public static class ElectricSynergyTriggerPolicy
{
    public static bool IsValidToolDamage(CombatDamageResult result) =>
        result.Target != null &&
        result.AppliedDamage > 0f &&
        result.Context.Origin == CombatDamageOrigin.Tool;
}

public static class ElectricSynergyMath
{
    public static float CalculateDamage(float baseDamage, float multiplier)
    {
        double damage = Math.Max(0f, baseDamage) * Math.Max(0f, multiplier);
        if (double.IsNaN(damage) || damage <= 0d)
        {
            return 0f;
        }

        return damage >= float.MaxValue ? float.MaxValue : (float)damage;
    }
}

public static class ElectricSynergyTargeting
{
    public static List<FishController> SelectDistinctEligibleTargets(
        IReadOnlyList<FishController> candidates,
        Vector2 origin,
        float radius,
        int maximumTargets,
        ISet<FishController> excluded = null)
    {
        List<FishController> selected = new();
        if (candidates == null || maximumTargets <= 0 || radius < 0f)
        {
            return selected;
        }

        float radiusSquared = radius * radius;
        HashSet<FishController> distinct = new();
        for (int i = 0; i < candidates.Count; i++)
        {
            FishController fish = candidates[i];
            if (!IsEligibleFish(fish) ||
                (excluded != null && excluded.Contains(fish)) ||
                !distinct.Add(fish) ||
                ((Vector2)fish.transform.position - origin).sqrMagnitude > radiusSquared)
            {
                continue;
            }

            selected.Add(fish);
        }

        selected.Sort((a, b) => CompareByDistanceThenId(a, b, origin));
        if (selected.Count > maximumTargets)
        {
            selected.RemoveRange(maximumTargets, selected.Count - maximumTargets);
        }

        return selected;
    }

    private static bool IsEligibleFish(FishController fish) =>
        fish != null &&
        fish.gameObject.activeInHierarchy &&
        fish.Data != null &&
        !fish.IsCaptured;

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
}
