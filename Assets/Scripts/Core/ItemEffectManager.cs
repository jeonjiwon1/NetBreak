using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Storm Orb / 폭풍 구슬")]
    [Min(0.1f)] [SerializeField] private float stormOrbAttackInterval = 8f;
    [Min(0f)] [SerializeField] private float stormOrbResistanceDamage = 12f;
    [Min(0f)] [SerializeField] private float stormOrbVisualDuration = 0.35f;

    [Header("Capacitor Coil / 축전 코일")]
    [Min(1)] [SerializeField] private int capacitorRequiredHits = 5;
    [Min(0f)] [SerializeField] private float capacitorChainRadius = 3f;
    [Min(1)] [SerializeField] private int capacitorMaximumTargets = 2;
    [Min(0f)] [SerializeField] private float capacitorResistanceDamage = 8f;
    [Min(0f)] [SerializeField] private float capacitorVisualDuration = 0.4f;

    [Header("Spectral Scabbard / 유령 검집")]
    [Min(1)] [SerializeField] private int scabbardRequiredHits = 4;
    [Min(0f)] [SerializeField] private float scabbardResistanceDamage = 16f;
    [Min(0f)] [SerializeField] private float scabbardVisualDuration = 0.35f;

    [Header("Autonomous Sword Array / 자동 검진")]
    [Min(0.1f)] [SerializeField] private float swordArrayAttackInterval = 10f;
    [Min(0f)] [SerializeField] private float swordArrayResistanceDamage = 18f;
    [Min(0f)] [SerializeField] private float swordArrayVisualDuration = 0.45f;

    [Header("Frost Sigil / 서리 인장")]
    [Min(1)] [SerializeField] private int frostSigilRequiredHits = 3;
    [Range(0f, 1f)] [SerializeField] private float frostSigilSlowPercentage = 0.4f;
    [Min(0f)] [SerializeField] private float frostSigilSlowDuration = 2f;

    [Header("Frost Crystal / 빙결 결정")]
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

    public static ItemEffectManager Instance { get; private set; }

    private readonly Dictionary<FishController, int> scabbardHitCounts = new();
    private readonly Dictionary<FishController, int> frostSigilHitCounts = new();
    private readonly HashSet<FishController> frostSigilSlowedFish = new();
    private readonly HashSet<FishController> frostCrystalSlowedFish = new();
    private readonly Dictionary<ContinuousHitKey, float> continuousHitTimes = new();
    private readonly HashSet<string> knownOwnedItems = new(StringComparer.Ordinal);
    private readonly HashSet<GameObject> activeVisuals = new();
    private readonly List<CombatDamageResult> pendingDamageResults = new();

    private RunItemInventory boundInventory;
    private Material runtimeMaterial;
    private bool runWasActive;
    private int capacitorHitCount;
    private float stormOrbRemaining;
    private float swordArrayRemaining;
    private float frostCrystalRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        BindInventoryIfNeeded();

        bool isRunActive = IsRunActive();
        if (!isRunActive)
        {
            if (runWasActive)
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

    public void HandleCombatDamage(CombatDamageResult result)
    {
        if (!IsRunActive() || boundInventory == null ||
            result.Target == null || result.AppliedDamage <= 0f ||
            result.Context.Origin != CombatDamageOrigin.Tool)
        {
            return;
        }

        bool ownsCoil = Owns(ItemCatalog.CapacitorCoilId);
        bool ownsScabbard = Owns(ItemCatalog.SpectralScabbardId);
        bool ownsSigil = Owns(ItemCatalog.FrostSigilId);
        if (!ownsCoil && !ownsScabbard && !ownsSigil)
        {
            return;
        }

        if (result.Context.IsContinuous && !CanCountContinuousHit(result))
        {
            return;
        }

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
                frostSigilSlowDuration);
            CreateFrostMarker(
                result.Target.transform.position,
                0.65f,
                frostSigilSlowDuration);
        }
    }

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

        return itemId switch
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
    }

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
        DealItemDamage(target, ItemCatalog.StormOrbId, stormOrbResistanceDamage);
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

        List<FishController> fish = GetEligibleFish(camera);
        fish.RemoveAll(candidate =>
            candidate == triggeringFish ||
            Vector2.Distance(origin, candidate.transform.position) > capacitorChainRadius);
        fish.Sort((a, b) => CompareByDistanceThenId(a, b, origin));

        int targetCount = Mathf.Min(capacitorMaximumTargets, fish.Count);
        if (targetCount == 0)
        {
            CreateLightning(
                new[] { origin, origin + Vector2.up * 0.45f },
                capacitorVisualDuration);
            return;
        }

        Vector2[] points = new Vector2[targetCount + 1];
        points[0] = origin;
        for (int i = 0; i < targetCount; i++)
        {
            FishController target = fish[i];
            points[i + 1] = target.transform.position;
            DealItemDamage(
                target,
                ItemCatalog.CapacitorCoilId,
                capacitorResistanceDamage);
        }

        CreateLightning(points, capacitorVisualDuration);
    }

    private void ActivateSpectralScabbard(FishController target, Vector2 position)
    {
        if (IsEligibleFish(target))
        {
            DealItemDamage(
                target,
                ItemCatalog.SpectralScabbardId,
                scabbardResistanceDamage);
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
                swordArrayResistanceDamage);
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
                frostCrystalSlowDuration);
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
                Mathf.Max(frostCrystalVisualDuration, frostCrystalSlowDuration));
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

    private bool CanCountContinuousHit(CombatDamageResult result)
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
            Time.time - lastCountedAt < continuousHitCountInterval)
        {
            return false;
        }

        continuousHitTimes[key] = Time.time;
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

    private void ClearRuntimeState(bool resetKnownOwnership)
    {
        capacitorHitCount = 0;
        scabbardHitCounts.Clear();
        frostSigilHitCounts.Clear();
        frostSigilSlowedFish.Clear();
        frostCrystalSlowedFish.Clear();
        continuousHitTimes.Clear();
        pendingDamageResults.Clear();
        stormOrbRemaining = stormOrbAttackInterval;
        swordArrayRemaining = swordArrayAttackInterval;
        frostCrystalRemaining = frostCrystalAttackInterval;

        StopAllCoroutines();
        foreach (GameObject visual in activeVisuals)
        {
            if (visual != null)
            {
                Destroy(visual);
            }
        }
        activeVisuals.Clear();

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
        if (points == null || points.Count < 2)
        {
            return;
        }

        LineRenderer line = CreateLineVisual(
            "ItemElectricVisual",
            new Color(0.35f, 0.85f, 1f, 0.95f),
            0.11f,
            points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
        TrackVisual(line.gameObject, duration);
    }

    private void CreateSwordMarker(Vector2 position, float duration, bool arrayStyle)
    {
        LineRenderer line = CreateLineVisual(
            arrayStyle ? "ItemSwordArrayVisual" : "ItemSpectralSwordVisual",
            arrayStyle
                ? new Color(1f, 0.45f, 0.25f, 0.95f)
                : new Color(0.85f, 0.75f, 1f, 0.95f),
            0.12f,
            arrayStyle ? 5 : 3);
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
        TrackVisual(line.gameObject, duration);
    }

    private void CreateFrostMarker(Vector2 position, float radius, float duration)
    {
        const int segments = 32;
        LineRenderer line = CreateLineVisual(
            "ItemFrostVisual",
            new Color(0.55f, 0.9f, 1f, 0.9f),
            0.08f,
            segments + 1);
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
        TrackVisual(line.gameObject, duration);
    }

    private LineRenderer CreateLineVisual(
        string objectName,
        Color color,
        float width,
        int positionCount)
    {
        GameObject visual = new(objectName);
        visual.transform.SetParent(transform, false);
        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = positionCount;
        line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = color;
        line.sortingOrder = 28;
        line.numCapVertices = 4;

        if (runtimeMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
            }
        }
        if (runtimeMaterial != null)
        {
            line.sharedMaterial = runtimeMaterial;
        }
        return line;
    }

    private void TrackVisual(GameObject visual, float duration)
    {
        if (visual == null)
        {
            return;
        }

        activeVisuals.Add(visual);
        StartCoroutine(DestroyVisualAfter(visual, duration));
    }

    private IEnumerator DestroyVisualAfter(GameObject visual, float duration)
    {
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        activeVisuals.Remove(visual);
        if (visual != null)
        {
            Destroy(visual);
        }
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
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }

    private void OnValidate()
    {
        gameplayViewport.x = Mathf.Clamp01(gameplayViewport.x);
        gameplayViewport.y = Mathf.Clamp01(gameplayViewport.y);
        gameplayViewport.width = Mathf.Clamp(
            gameplayViewport.width, 0.01f, 1f - gameplayViewport.x);
        gameplayViewport.height = Mathf.Clamp(
            gameplayViewport.height, 0.01f, 1f - gameplayViewport.y);
        capacitorRequiredHits = Mathf.Max(1, capacitorRequiredHits);
        capacitorMaximumTargets = Mathf.Max(1, capacitorMaximumTargets);
        scabbardRequiredHits = Mathf.Max(1, scabbardRequiredHits);
        frostSigilRequiredHits = Mathf.Max(1, frostSigilRequiredHits);
        frostCrystalMaximumTargets = Mathf.Max(1, frostCrystalMaximumTargets);
    }
}
