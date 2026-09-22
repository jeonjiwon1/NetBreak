using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CombatVfxSettings
{
    [InspectorName("Maximum Active Visuals")]
    [Min(1)] [SerializeField] private int maximumActiveVisuals = 48;

    [InspectorName("Line Width Multiplier")]
    [Min(0.1f)] [SerializeField] private float lineWidthMultiplier = 1f;

    [InspectorName("Effect Size Multiplier")]
    [Min(0.1f)] [SerializeField] private float effectSizeMultiplier = 1f;

    [Header("Squid Ink / 오징어 먹물")]
    [Min(0f)] [SerializeField] private float squidTrajectoryDuration = 0.22f;
    [Min(0f)] [SerializeField] private float squidImpactDuration = 0.3f;
    [SerializeField] private Color squidTrajectoryColor =
        new(0.24f, 0.05f, 0.34f, 0.95f);
    [SerializeField] private Color squidImpactColor =
        new(0.55f, 0.12f, 0.68f, 1f);

    [Header("Electric / 전기")]
    [Min(0f)] [SerializeField] private float hitEmphasisDuration = 0.22f;
    [SerializeField] private Color electricChainColor =
        new(0.45f, 1f, 0.95f, 1f);
    [SerializeField] private Color electricHitColor =
        new(0.72f, 1f, 1f, 1f);
    [SerializeField] private Color electricStunColor =
        new(0.25f, 0.95f, 1f, 0.95f);
    [SerializeField] private Color thunderstormColor =
        new(1f, 0.84f, 0.22f, 1f);

    [Header("Sword / 검")]
    [SerializeField] private Color soulSlashColor =
        new(0.72f, 0.9f, 1f, 1f);
    [SerializeField] private Color additionalSlashColor =
        new(0.78f, 0.55f, 1f, 1f);
    [SerializeField] private Color swordRainColor =
        new(1f, 0.76f, 0.28f, 1f);

    [Header("Ice / 얼음")]
    [SerializeField] private Color coldWaveColor =
        new(0.4f, 0.8f, 1f, 0.95f);
    [SerializeField] private Color freezeColor =
        new(0.72f, 0.94f, 1f, 1f);
    [SerializeField] private Color frostBurstColor =
        new(0.78f, 0.96f, 1f, 1f);

    [Header("Items / 개별 아이템")]
    [SerializeField] private Color stormOrbColor =
        new(0.32f, 0.82f, 1f, 0.95f);
    [SerializeField] private Color capacitorCoilColor =
        new(0.78f, 0.96f, 1f, 0.9f);
    [SerializeField] private Color spectralScabbardColor =
        new(0.82f, 0.62f, 1f, 0.95f);
    [SerializeField] private Color swordArrayColor =
        new(1f, 0.42f, 0.22f, 0.95f);
    [SerializeField] private Color frostSigilColor =
        new(0.42f, 0.86f, 1f, 0.9f);
    [SerializeField] private Color frostCrystalColor =
        new(0.72f, 0.95f, 1f, 0.95f);

    [Header("Combined Synergy / 복합 시너지")]
    [Min(0f)] [SerializeField] private float combinedBurstDuration = 0.42f;
    [Min(0f)] [SerializeField] private float combinedHitDuration = 0.28f;
    [SerializeField] private Color conductiveMarkColor =
        new(1f, 0.84f, 0.25f, 0.95f);
    [SerializeField] private Color thunderSwordColor =
        new(0.95f, 0.72f, 1f, 1f);
    [SerializeField] private Color superconductivityColor =
        new(0.34f, 1f, 0.92f, 1f);
    [SerializeField] private Color frostSwordColor =
        new(0.58f, 0.84f, 1f, 1f);

    public int MaximumActiveVisuals => Mathf.Max(1, maximumActiveVisuals);
    public float LineWidthMultiplier => Mathf.Max(0.1f, lineWidthMultiplier);
    public float EffectSizeMultiplier => Mathf.Max(0.1f, effectSizeMultiplier);
    public float SquidTrajectoryDuration => Mathf.Max(0f, squidTrajectoryDuration);
    public float SquidImpactDuration => Mathf.Max(0f, squidImpactDuration);
    public Color SquidTrajectoryColor => squidTrajectoryColor;
    public Color SquidImpactColor => squidImpactColor;
    public float HitEmphasisDuration => Mathf.Max(0f, hitEmphasisDuration);
    public Color ElectricChainColor => electricChainColor;
    public Color ElectricHitColor => electricHitColor;
    public Color ElectricStunColor => electricStunColor;
    public Color ThunderstormColor => thunderstormColor;
    public Color SoulSlashColor => soulSlashColor;
    public Color AdditionalSlashColor => additionalSlashColor;
    public Color SwordRainColor => swordRainColor;
    public Color ColdWaveColor => coldWaveColor;
    public Color FreezeColor => freezeColor;
    public Color FrostBurstColor => frostBurstColor;
    public Color StormOrbColor => stormOrbColor;
    public Color CapacitorCoilColor => capacitorCoilColor;
    public Color SpectralScabbardColor => spectralScabbardColor;
    public Color SwordArrayColor => swordArrayColor;
    public Color FrostSigilColor => frostSigilColor;
    public Color FrostCrystalColor => frostCrystalColor;
    public float CombinedBurstDuration => Mathf.Max(0f, combinedBurstDuration);
    public float CombinedHitDuration => Mathf.Max(0f, combinedHitDuration);
    public Color ConductiveMarkColor => conductiveMarkColor;
    public Color ThunderSwordColor => thunderSwordColor;
    public Color SuperconductivityColor => superconductivityColor;
    public Color FrostSwordColor => frostSwordColor;

    public void Normalize()
    {
        maximumActiveVisuals = MaximumActiveVisuals;
        lineWidthMultiplier = LineWidthMultiplier;
        effectSizeMultiplier = EffectSizeMultiplier;
        squidTrajectoryDuration = SquidTrajectoryDuration;
        squidImpactDuration = SquidImpactDuration;
        hitEmphasisDuration = HitEmphasisDuration;
        combinedBurstDuration = CombinedBurstDuration;
        combinedHitDuration = CombinedHitDuration;
    }
}

internal enum CombatVfxPriority
{
    RepeatedHit = 0,
    Item = 1,
    SingleSynergy = 2,
    CombinedSynergy = 3
}

internal sealed class CombatVfxPool
{
    private sealed class Entry
    {
        public GameObject GameObject;
        public LineRenderer Line;
        public float Remaining;
        public bool IsActive;
        public bool IsPersistent;
        public long Sequence;
        public CombatVfxPriority Priority;
    }

    private readonly Transform owner;
    private readonly CombatVfxSettings settings;
    private readonly List<Entry> entries = new();

    private Material sharedMaterial;
    private long nextSequence;

    public CombatVfxPool(
        Transform owner,
        CombatVfxSettings settings)
    {
        this.owner = owner;
        this.settings = settings ?? new CombatVfxSettings();
    }

    public int ActiveCount { get; private set; }
    public int CreatedCount => entries.Count;

    public LineRenderer AcquireTransient(
        string objectName,
        Color color,
        float width,
        int positionCount,
        float duration,
        int sortingOrder,
        CombatVfxPriority priority = CombatVfxPriority.Item)
    {
        if (duration <= 0f)
        {
            return null;
        }

        return Acquire(
            objectName,
            color,
            width,
            positionCount,
            duration,
            false,
            sortingOrder,
            priority);
    }

    public LineRenderer AcquirePersistent(
        string objectName,
        Color color,
        float width,
        int positionCount,
        int sortingOrder) =>
        Acquire(
            objectName,
            color,
            width,
            positionCount,
            float.PositiveInfinity,
            true,
            sortingOrder,
            CombatVfxPriority.CombinedSynergy);

    public void Tick(float scaledDeltaTime)
    {
        if (scaledDeltaTime <= 0f)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (!entry.IsActive || entry.IsPersistent)
            {
                continue;
            }

            entry.Remaining -= scaledDeltaTime;
            if (entry.Remaining <= 0f)
            {
                Release(entry);
            }
        }
    }

    public void Release(LineRenderer line)
    {
        if (line == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Line == line)
            {
                Release(entries[i]);
                return;
            }
        }
    }

    public void Clear()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            Release(entries[i]);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            DestroyObject(entries[i].GameObject);
        }

        entries.Clear();
        ActiveCount = 0;

        if (sharedMaterial != null)
        {
            DestroyObject(sharedMaterial);
            sharedMaterial = null;
        }
    }

    private LineRenderer Acquire(
        string objectName,
        Color color,
        float width,
        int positionCount,
        float duration,
        bool persistent,
        int sortingOrder,
        CombatVfxPriority priority)
    {
        if (owner == null || positionCount < 2)
        {
            return null;
        }

        Entry entry = FindInactive();
        if (entry == null && ActiveCount < settings.MaximumActiveVisuals)
        {
            entry = CreateEntry();
        }

        if (entry == null)
        {
            entry = FindTransientToRecycle(priority);
            if (entry == null)
            {
                return null;
            }

            Release(entry);
        }

        entry.GameObject.name = string.IsNullOrEmpty(objectName)
            ? "CombatVfx"
            : objectName;
        entry.GameObject.transform.SetParent(owner, false);
        entry.GameObject.transform.localPosition = Vector3.zero;
        entry.GameObject.transform.localRotation = Quaternion.identity;
        entry.GameObject.transform.localScale = Vector3.one;

        LineRenderer line = entry.Line;
        line.useWorldSpace = true;
        line.loop = false;
        line.positionCount = positionCount;
        line.startWidth = line.endWidth =
            Mathf.Max(0.001f, width * settings.LineWidthMultiplier);
        line.startColor = line.endColor = color;
        line.sortingOrder = sortingOrder;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.sharedMaterial = ResolveMaterial();

        entry.Remaining = duration;
        entry.IsPersistent = persistent;
        entry.IsActive = true;
        entry.Sequence = ++nextSequence;
        entry.Priority = priority;
        entry.GameObject.SetActive(true);
        ActiveCount++;
        return line;
    }

    private Entry FindInactive()
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (!entries[i].IsActive)
            {
                return entries[i];
            }
        }

        return null;
    }

    private Entry FindTransientToRecycle(CombatVfxPriority incomingPriority)
    {
        Entry candidateToRecycle = null;
        for (int i = 0; i < entries.Count; i++)
        {
            Entry candidate = entries[i];
            if (!candidate.IsActive || candidate.IsPersistent ||
                candidate.Priority > incomingPriority)
            {
                continue;
            }

            if (candidateToRecycle == null ||
                candidate.Priority < candidateToRecycle.Priority ||
                (candidate.Priority == candidateToRecycle.Priority &&
                 candidate.Sequence < candidateToRecycle.Sequence))
            {
                candidateToRecycle = candidate;
            }
        }

        return candidateToRecycle;
    }

    private Entry CreateEntry()
    {
        GameObject visual = new("CombatVfx");
        visual.transform.SetParent(owner, false);
        visual.SetActive(false);

        Entry entry = new()
        {
            GameObject = visual,
            Line = visual.AddComponent<LineRenderer>()
        };

        entries.Add(entry);
        return entry;
    }

    private Material ResolveMaterial()
    {
        if (sharedMaterial != null)
        {
            return sharedMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            sharedMaterial = new Material(shader)
            {
                name = "Runtime Combat VFX Material"
            };
        }

        return sharedMaterial;
    }

    private void Release(Entry entry)
    {
        if (entry == null || !entry.IsActive)
        {
            return;
        }

        entry.IsActive = false;
        entry.IsPersistent = false;
        entry.Remaining = 0f;
        entry.Priority = CombatVfxPriority.RepeatedHit;
        entry.Line.positionCount = 0;
        entry.GameObject.SetActive(false);
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
    }

    private static void DestroyObject(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(target);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
