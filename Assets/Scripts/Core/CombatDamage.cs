using UnityEngine;

public enum CombatDamageOrigin
{
    Tool = 0,
    Item = 1
}

public readonly struct CombatDamageContext
{
    private CombatDamageContext(
        CombatDamageOrigin origin,
        string attackId,
        int sourceInstanceId,
        bool isContinuous)
    {
        Origin = origin;
        AttackId = attackId ?? string.Empty;
        SourceInstanceId = sourceInstanceId;
        IsContinuous = isContinuous;
    }

    public CombatDamageOrigin Origin { get; }
    public string AttackId { get; }
    public int SourceInstanceId { get; }
    public bool IsContinuous { get; }

    public static CombatDamageContext Tool(
        string attackId,
        Object source,
        bool isContinuous = false) =>
        new(
            CombatDamageOrigin.Tool,
            attackId,
            source != null ? source.GetInstanceID() : 0,
            isContinuous);

    public static CombatDamageContext Item(
        string itemId,
        Object source) =>
        new(
            CombatDamageOrigin.Item,
            itemId,
            source != null ? source.GetInstanceID() : 0,
            false);
}

public readonly struct CombatDamageResult
{
    public CombatDamageResult(
        FishController target,
        int targetLifecycleVersion,
        CombatDamageContext context,
        float appliedDamage,
        Vector2 hitPosition,
        bool capturedByHit)
    {
        Target = target;
        TargetLifecycleVersion = targetLifecycleVersion;
        Context = context;
        AppliedDamage = appliedDamage;
        HitPosition = hitPosition;
        CapturedByHit = capturedByHit;
    }

    public FishController Target { get; }
    public int TargetLifecycleVersion { get; }
    public CombatDamageContext Context { get; }
    public float AppliedDamage { get; }
    public Vector2 HitPosition { get; }
    public bool CapturedByHit { get; }
}
