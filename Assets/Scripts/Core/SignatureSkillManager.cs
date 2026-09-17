using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SignatureSkillId
{
    None = 0,
    FishingGroundCrossing = 1,
    CrossLockdown = 2,
    HeavenlyNet = 3
}

[System.Serializable]
public sealed class SignatureFishValueSet
{
    [Min(0f)] [SerializeField] private float normal;
    [Min(0f)] [SerializeField] private float pufferfish;
    [Min(0f)] [SerializeField] private float squid;
    [Min(0f)] [SerializeField] private float miniBoss;
    [Min(0f)] [SerializeField] private float boss;

    public SignatureFishValueSet(
        float normal,
        float pufferfish,
        float squid,
        float miniBoss,
        float boss)
    {
        this.normal = normal;
        this.pufferfish = pufferfish;
        this.squid = squid;
        this.miniBoss = miniBoss;
        this.boss = boss;
    }

    public float Get(FishSpecialType type) => type switch
    {
        FishSpecialType.Pufferfish => pufferfish,
        FishSpecialType.Squid => squid,
        FishSpecialType.MiniBoss => miniBoss,
        FishSpecialType.Boss => boss,
        _ => normal
    };
}

[DisallowMultipleComponent]
[DefaultExecutionOrder(-80)]
public sealed class SignatureSkillManager : MonoBehaviour
{
    public const string FishingGroundCrossingAbilityId =
        "signature.fishing_ground_crossing";
    public const string CrossLockdownAbilityId =
        "signature.cross_lockdown";
    public const string HeavenlyNetAbilityId =
        "signature.heavenly_net";

    [Header("Common")]
    [Min(0.1f)] [SerializeField] private float cooldown = 90f;
    [Tooltip("Camera pixel viewport 안에서 R 효과가 사용하는 정규화된 실제 조업 영역입니다. 하단 HUD와 상단 HUD를 제외한 기본값입니다.")]
    [SerializeField] private Rect gameplayViewport = new(0f, 0.12f, 1f, 0.72f);

    [Header("Fishing Rod R - Fishing Ground Crossing")]
    [Min(1)] [SerializeField] private int fishingLaneCount = 9;
    [Min(0.1f)] [SerializeField] private float fishingTraversalDuration = 2.5f;
    [Min(0.05f)] [SerializeField] private float fishingFloatRadius = 0.35f;
    [Min(1)] [SerializeField] private int fishingTraversalCount = 1;
    [Min(0f)] [SerializeField] private float fishingInterTraversalDelay = 0.35f;
    [SerializeField] private SignatureFishValueSet fishingDamage =
        new(25f, 18f, 24f, 55f, 70f);

    [Header("Net R - Cross Lockdown")]
    [Min(0.1f)] [SerializeField] private float crossLockdownDuration = 6f;
    [Min(0.05f)] [SerializeField] private float crossArmThickness = 0.55f;
    [Tooltip("초당 최대 Resistance 비율입니다. 0.03은 초당 3%입니다.")]
    [SerializeField] private SignatureFishValueSet crossResistanceDps =
        new(0.03f, 0.03f, 0.03f, 0.015f, 0.0075f);
    [Tooltip("접촉 중 이동 속도 배율입니다. 0은 완전 정지입니다.")]
    [SerializeField] private SignatureFishValueSet crossSpeedMultipliers =
        new(0f, 0f, 0f, 0.5f, 0.65f);

    [Header("Cast Net R - Heavenly Net")]
    [Min(0.1f)] [SerializeField] private float heavenlyNetDiameterMultiplier = 1f;
    [Min(0f)] [SerializeField] private float heavenlyNetDamageMultiplier = 1f;
    [Min(1)] [SerializeField] private int heavenlyNetCastCount = 1;
    [Min(0f)] [SerializeField] private float heavenlyNetCastInterval = 0.6f;
    [Min(0.05f)] [SerializeField] private float heavenlyNetVisualDuration = 0.45f;

    [Header("References (Optional)")]
    [SerializeField] private CastNetController castNet;

    private readonly HashSet<FishController> damagedThisTraversal = new();
    private readonly HashSet<FishController> crossContactFish = new();
    private readonly HashSet<FishController> crossFrameFish = new();
    private readonly List<LineRenderer> fishingFloatVisuals = new();

    private SignatureSkillId equippedSkill;
    private float cooldownRemaining;
    private bool isTargeting;
    private bool targetCancelled;
    private Vector2 targetPosition;
    private Coroutine activeEffect;
    private float crossRemaining;
    private float activeCrossDamageMultiplier = 1f;
    private float castVisualRemaining;
    private LineRenderer crossArmA;
    private LineRenderer crossArmB;
    private LineRenderer castAreaVisual;
    private Material runtimeMaterial;
    private string pendingUnlockNotification;

    public static SignatureSkillManager Instance { get; private set; }
    public static bool IsTargeting => Instance != null && Instance.isTargeting;

    public bool HasEquippedSkill => equippedSkill != SignatureSkillId.None;
    public float RemainingCooldown => Mathf.Max(0f, cooldownRemaining);
    public float CooldownNormalized => cooldown > 0f
        ? Mathf.Clamp01(RemainingCooldown / cooldown)
        : 0f;
    public string EquippedSkillName => GetSkillName(equippedSkill);
    public string DescribeUpgrade(string effectId, float value) => effectId switch
    {
        SkillTreeEffectIds.SignatureTraversalCount => $"횡단 {Mathf.RoundToInt(value)}회",
        SkillTreeEffectIds.SignatureDurationMultiplier =>
            $"지속시간 {crossLockdownDuration * value:0.#}초",
        SkillTreeEffectIds.SignatureDamageMultiplier =>
            $"기본 Resistance DPS ×{value:0.##}",
        SkillTreeEffectIds.SignatureCastCount => $"연속 시전 {Mathf.RoundToInt(value)}회",
        _ => string.Empty
    };
    public string HotbarStatus
    {
        get
        {
            if (!HasEquippedSkill)
            {
                return "잠김\n보스 보상";
            }

            if (isTargeting)
            {
                return $"{EquippedSkillName}\n조준 중";
            }

            return RemainingCooldown > 0f
                ? $"{EquippedSkillName}\n{RemainingCooldown:F1}초"
                : $"{EquippedSkillName}\n준비 완료";
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ResolveReferences();
        RestoreEquippedSkillFromRunState();
    }

    private void Update()
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        if (flow != null && (flow.IsGameEnded || flow.IsBossRewardPending))
        {
            StopAllEffects();
            return;
        }

        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
        }

        UpdateCrossLockdown();
        UpdateCastVisual();

        if (isTargeting)
        {
            UpdateHeavenlyNetTargeting();
            return;
        }

        if (!CanUseGameplayInput() || Keyboard.current == null ||
            !Keyboard.current.rKey.wasPressedThisFrame ||
            cooldownRemaining > 0f || !HasEquippedSkill ||
            !CanActivateSignature())
        {
            return;
        }

        ActivateEquippedSkill();
    }

    public bool UnlockForBossReward()
    {
        if (RunManager.Instance == null || RunManager.Instance.GrowthState == null)
        {
            return false;
        }

        SignatureSkillId skill = GetToolSkill(
            RunManager.Instance.GrowthState.SelectedCoreTool);
        string abilityId = GetAbilityId(skill);
        if (skill == SignatureSkillId.None ||
            !RunManager.Instance.GrowthState.TryUnlockAndEquipAbility(
                GrowthAbilitySlot.SignatureR,
                abilityId))
        {
            return false;
        }

        equippedSkill = skill;
        pendingUnlockNotification = $"R 스킬 해금: {GetSkillName(skill)}";
        return true;
    }

    public bool TryConsumeUnlockNotification(out string message)
    {
        message = pendingUnlockNotification;
        pendingUnlockNotification = string.Empty;
        return !string.IsNullOrEmpty(message);
    }

    [ContextMenu("Development/Unlock Core Signature R")]
    private void DevelopmentUnlockCoreSignature()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (RunManager.Instance == null || RunManager.Instance.GrowthState == null)
        {
            Debug.LogWarning("Signature R development unlock requires an active RunManager.");
            return;
        }

        SignatureSkillId skill = GetToolSkill(
            RunManager.Instance.GrowthState.SelectedCoreTool);
        string abilityId = GetAbilityId(skill);
        RunGrowthAbilityState state = RunManager.Instance.GrowthState
            .GetAbility(GrowthAbilitySlot.SignatureR);
        bool changed = state.IsUnlocked
            ? RunManager.Instance.GrowthState.TryEquipAbility(
                GrowthAbilitySlot.SignatureR,
                abilityId)
            : RunManager.Instance.GrowthState.TryUnlockAndEquipAbility(
                GrowthAbilitySlot.SignatureR,
                abilityId);

        if (skill == SignatureSkillId.None || string.IsNullOrEmpty(abilityId))
        {
            Debug.LogWarning("Select a Fishing Rod, Net, or Cast Net Core before unlocking Signature R.");
            return;
        }

        equippedSkill = skill;
        cooldownRemaining = 0f;
        Debug.Log(changed
            ? $"Development Signature R unlocked: {GetSkillName(skill)}"
            : $"Development Signature R ready: {GetSkillName(skill)}");
#else
        Debug.LogWarning("Signature R development unlock is available only in the Editor or a Development Build.");
#endif
    }

    private void ActivateEquippedSkill()
    {
        switch (equippedSkill)
        {
            case SignatureSkillId.FishingGroundCrossing:
                activeEffect = StartCoroutine(RunFishingGroundCrossing());
                cooldownRemaining = cooldown;
                break;
            case SignatureSkillId.CrossLockdown:
                BeginCrossLockdown();
                cooldownRemaining = cooldown;
                break;
            case SignatureSkillId.HeavenlyNet:
                if (!CanBeginTargeting()) return;
                isTargeting = true;
                targetCancelled = false;
                UpdateTargetPosition();
                ShowCastArea(targetPosition, GetHeavenlyNetRadius());
                break;
        }
    }

    private IEnumerator RunFishingGroundCrossing()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            activeEffect = null;
            yield break;
        }

        EnsureFishingFloatVisuals();
        int traversals = Mathf.Max(1, Mathf.RoundToInt(GetUpgradeValue(
            SkillTreeEffectIds.SignatureTraversalCount,
            fishingTraversalCount)));
        for (int traversal = 0; traversal < traversals; traversal++)
        {
            damagedThisTraversal.Clear();
            bool rightToLeft = traversal % 2 == 0;
            float elapsed = 0f;
            while (elapsed < fishingTraversalDuration)
            {
                Rect bounds = GetGameplayWorldBounds(camera);
                float progress = Mathf.Clamp01(elapsed / fishingTraversalDuration);
                float fromX = rightToLeft
                    ? bounds.xMax + fishingFloatRadius
                    : bounds.xMin - fishingFloatRadius;
                float toX = rightToLeft
                    ? bounds.xMin - fishingFloatRadius
                    : bounds.xMax + fishingFloatRadius;
                float x = Mathf.Lerp(fromX, toX, progress);
                UpdateFishingFloats(bounds, x);
                DamageFishAtFloats();
                elapsed += Time.deltaTime;
                yield return null;
            }

            HideFishingFloats();
            if (traversal + 1 < traversals && fishingInterTraversalDelay > 0f)
            {
                yield return new WaitForSeconds(fishingInterTraversalDelay);
            }
        }

        activeEffect = null;
    }

    private void UpdateFishingFloats(Rect bounds, float x)
    {
        int count = Mathf.Max(1, fishingLaneCount);
        for (int i = 0; i < fishingFloatVisuals.Count; i++)
        {
            bool active = i < count;
            fishingFloatVisuals[i].gameObject.SetActive(active);
            if (!active) continue;

            float y = bounds.yMin + bounds.height * (i + 0.5f) / count;
            fishingFloatVisuals[i].transform.position = new Vector3(x, y, 0f);
        }
    }

    private void DamageFishAtFloats()
    {
        for (int i = 0; i < fishingFloatVisuals.Count; i++)
        {
            LineRenderer visual = fishingFloatVisuals[i];
            if (!visual.gameObject.activeSelf) continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                visual.transform.position,
                fishingFloatRadius);
            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                FishController fish = hits[hitIndex].GetComponent<FishController>();
                if (fish == null || fish.Data == null ||
                    !damagedThisTraversal.Add(fish))
                {
                    continue;
                }

                fish.TakeCaptureDamage(
                    fishingDamage.Get(fish.Data.SpecialType),
                    CombatDamageContext.Tool(
                        "signature.fishing_ground_crossing",
                        this));
            }
        }
    }

    private void BeginCrossLockdown()
    {
        crossRemaining = crossLockdownDuration * GetUpgradeValue(
            SkillTreeEffectIds.SignatureDurationMultiplier, 1f);
        activeCrossDamageMultiplier = GetUpgradeValue(
            SkillTreeEffectIds.SignatureDamageMultiplier, 1f);
        EnsureCrossVisuals();
        UpdateCrossVisualGeometry();
        crossArmA.gameObject.SetActive(true);
        crossArmB.gameObject.SetActive(true);
    }

    private void UpdateCrossLockdown()
    {
        if (crossRemaining <= 0f)
        {
            return;
        }

        crossRemaining = Mathf.Max(0f, crossRemaining - Time.deltaTime);
        UpdateCrossVisualGeometry();

        Camera camera = Camera.main;
        if (camera == null)
        {
            EndCrossLockdown();
            return;
        }

        Rect bounds = GetGameplayWorldBounds(camera);
        Vector2 a0 = new(bounds.xMin, bounds.yMin);
        Vector2 a1 = new(bounds.xMax, bounds.yMax);
        Vector2 b0 = new(bounds.xMin, bounds.yMax);
        Vector2 b1 = new(bounds.xMax, bounds.yMin);
        crossFrameFish.Clear();

        Collider2D[] hits = Physics2D.OverlapAreaAll(bounds.min, bounds.max);
        for (int i = 0; i < hits.Length; i++)
        {
            FishController fish = hits[i].GetComponent<FishController>();
            if (fish == null || fish.Data == null || !crossFrameFish.Add(fish))
            {
                continue;
            }

            Vector2 position = fish.transform.position;
            bool intersects = DistanceToSegment(position, a0, a1) <= crossArmThickness * 0.5f ||
                DistanceToSegment(position, b0, b1) <= crossArmThickness * 0.5f;
            FishMovement movement = fish.GetComponent<FishMovement>();
            if (!intersects)
            {
                if (crossContactFish.Remove(fish))
                {
                    movement?.ClearSignatureNetSpeedMultiplier();
                }
                continue;
            }

            crossContactFish.Add(fish);
            movement?.SetSignatureNetSpeedMultiplier(
                crossSpeedMultipliers.Get(fish.Data.SpecialType));
            float dpsRatio = crossResistanceDps.Get(fish.Data.SpecialType);
            fish.TakeCaptureDamage(
                fish.Data.MaxResistance * dpsRatio *
                activeCrossDamageMultiplier * Time.deltaTime,
                CombatDamageContext.Tool(
                    "signature.cross_lockdown",
                    this,
                    true));
        }

        RemoveExitedCrossFish();
        if (crossRemaining <= 0f)
        {
            EndCrossLockdown();
        }
    }

    private void RemoveExitedCrossFish()
    {
        List<FishController> exited = null;
        foreach (FishController fish in crossContactFish)
        {
            if (fish != null && fish.gameObject.activeInHierarchy &&
                crossFrameFish.Contains(fish))
            {
                continue;
            }

            exited ??= new List<FishController>();
            exited.Add(fish);
        }

        if (exited == null) return;
        for (int i = 0; i < exited.Count; i++)
        {
            FishController fish = exited[i];
            if (fish != null)
            {
                fish.GetComponent<FishMovement>()?.ClearSignatureNetSpeedMultiplier();
            }
            crossContactFish.Remove(fish);
        }
    }

    private void EndCrossLockdown()
    {
        crossRemaining = 0f;
        activeCrossDamageMultiplier = 1f;
        foreach (FishController fish in crossContactFish)
        {
            if (fish != null)
            {
                fish.GetComponent<FishMovement>()?.ClearSignatureNetSpeedMultiplier();
            }
        }
        crossContactFish.Clear();
        crossFrameFish.Clear();
        if (crossArmA != null) crossArmA.gameObject.SetActive(false);
        if (crossArmB != null) crossArmB.gameObject.SetActive(false);
    }

    private void UpdateHeavenlyNetTargeting()
    {
        if (!CanUseGameplayInput() || Mouse.current == null || Keyboard.current == null)
        {
            CancelTargeting();
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            targetCancelled = true;
            CancelTargeting();
            return;
        }

        UpdateTargetPosition();
        ShowCastArea(targetPosition, GetHeavenlyNetRadius());
        if (!Keyboard.current.rKey.wasReleasedThisFrame)
        {
            return;
        }

        isTargeting = false;
        HideCastArea();
        if (targetCancelled || castNet == null)
        {
            targetCancelled = false;
            return;
        }

        activeEffect = StartCoroutine(RunHeavenlyNetCasts());
        cooldownRemaining = cooldown;
    }

    private IEnumerator RunHeavenlyNetCasts()
    {
        int count = Mathf.Max(1, Mathf.RoundToInt(GetUpgradeValue(
            SkillTreeEffectIds.SignatureCastCount,
            heavenlyNetCastCount)));
        for (int i = 0; i < count; i++)
        {
            if (i > 0 && heavenlyNetCastInterval > 0f)
            {
                yield return new WaitForSeconds(heavenlyNetCastInterval);
            }

            UpdateTargetPosition();
            float radius = GetHeavenlyNetRadius();
            castNet.ApplySignatureCast(
                targetPosition,
                radius,
                heavenlyNetDamageMultiplier);
            ShowCastArea(targetPosition, radius);
            castVisualRemaining = heavenlyNetVisualDuration;
        }

        activeEffect = null;
    }

    private void UpdateCastVisual()
    {
        if (isTargeting || castVisualRemaining <= 0f)
        {
            return;
        }

        castVisualRemaining = Mathf.Max(0f, castVisualRemaining - Time.deltaTime);
        if (castVisualRemaining <= 0f)
        {
            HideCastArea();
        }
    }

    private void CancelTargeting()
    {
        isTargeting = false;
        HideCastArea();
    }

    private bool CanBeginTargeting() =>
        castNet != null && CanActivateSignature();

    private bool CanActivateSignature() =>
        !NetPlacementController.IsNetModeActive &&
        !FishingRodPlacementController.IsRodModeActive &&
        !GearRepositionController.IsRepositioning &&
        !GearRepositionController.IsRepositionModifierHeld &&
        !TacticalSkillManager.IsTargeting &&
        !castNet.IsAiming;

    private static bool CanUseGameplayInput()
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        return Time.timeScale > 0f &&
            !ToolSlotInput.IsSelectionOrEndBlocked &&
            !TacticalSkillManager.IsTargeting &&
            flow != null && flow.IsFishingStarted &&
            !flow.IsPreparation && !flow.IsGameEnded;
    }

    private void UpdateTargetPosition()
    {
        Camera camera = Camera.main;
        if (camera == null || Mouse.current == null) return;

        Vector3 world = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Rect bounds = GetGameplayWorldBounds(camera);
        targetPosition = new Vector2(
            Mathf.Clamp(world.x, bounds.xMin, bounds.xMax),
            Mathf.Clamp(world.y, bounds.yMin, bounds.yMax));
    }

    private float GetHeavenlyNetRadius()
    {
        Camera camera = Camera.main;
        if (camera == null) return 0f;
        return GetGameplayWorldBounds(camera).height * 0.5f *
            heavenlyNetDiameterMultiplier;
    }

    private Rect GetGameplayWorldBounds(Camera camera)
    {
        Rect pixel = camera.pixelRect;
        float xMin = pixel.xMin + pixel.width * gameplayViewport.xMin;
        float xMax = pixel.xMin + pixel.width * gameplayViewport.xMax;
        float yMin = pixel.yMin + pixel.height * gameplayViewport.yMin;
        float yMax = pixel.yMin + pixel.height * gameplayViewport.yMax;
        Vector3 min = camera.ScreenToWorldPoint(new Vector3(xMin, yMin, 0f));
        Vector3 max = camera.ScreenToWorldPoint(new Vector3(xMax, yMax, 0f));
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void EnsureFishingFloatVisuals()
    {
        int count = Mathf.Max(1, fishingLaneCount);
        while (fishingFloatVisuals.Count < count)
        {
            LineRenderer line = CreateLineRenderer(
                $"SignatureFishingFloat_{fishingFloatVisuals.Count + 1}",
                25,
                0.09f,
                new Color(1f, 0.3f, 0.2f, 0.95f));
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 20;
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * fishingFloatRadius,
                    Mathf.Sin(angle) * fishingFloatRadius,
                    0f));
            }
            fishingFloatVisuals.Add(line);
        }
    }

    private void HideFishingFloats()
    {
        for (int i = 0; i < fishingFloatVisuals.Count; i++)
        {
            fishingFloatVisuals[i].gameObject.SetActive(false);
        }
    }

    private void EnsureCrossVisuals()
    {
        crossArmA ??= CreateLineRenderer(
            "SignatureCrossArmA", 24, crossArmThickness,
            new Color(0.2f, 0.85f, 1f, 0.9f));
        crossArmB ??= CreateLineRenderer(
            "SignatureCrossArmB", 24, crossArmThickness,
            new Color(0.2f, 0.85f, 1f, 0.9f));
    }

    private void UpdateCrossVisualGeometry()
    {
        Camera camera = Camera.main;
        if (camera == null || crossArmA == null || crossArmB == null) return;
        Rect bounds = GetGameplayWorldBounds(camera);
        crossArmA.startWidth = crossArmA.endWidth = crossArmThickness;
        crossArmB.startWidth = crossArmB.endWidth = crossArmThickness;
        crossArmA.SetPosition(0, new Vector3(bounds.xMin, bounds.yMin, 0f));
        crossArmA.SetPosition(1, new Vector3(bounds.xMax, bounds.yMax, 0f));
        crossArmB.SetPosition(0, new Vector3(bounds.xMin, bounds.yMax, 0f));
        crossArmB.SetPosition(1, new Vector3(bounds.xMax, bounds.yMin, 0f));
    }

    private void ShowCastArea(Vector2 position, float radius)
    {
        castAreaVisual ??= CreateLineRenderer(
            "SignatureHeavenlyNetArea", 26, 0.12f,
            new Color(1f, 0.75f, 0.15f, 0.95f));
        castAreaVisual.gameObject.SetActive(true);
        castAreaVisual.transform.position = position;
        castAreaVisual.useWorldSpace = false;
        castAreaVisual.loop = true;
        castAreaVisual.positionCount = 49;
        for (int i = 0; i < castAreaVisual.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / castAreaVisual.positionCount;
            castAreaVisual.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f));
        }
    }

    private void HideCastArea()
    {
        if (castAreaVisual != null)
        {
            castAreaVisual.gameObject.SetActive(false);
        }
    }

    private LineRenderer CreateLineRenderer(
        string objectName,
        int sortingOrder,
        float width,
        Color color)
    {
        GameObject visual = new(objectName);
        visual.transform.SetParent(transform, false);
        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = color;
        line.sortingOrder = sortingOrder;
        line.numCapVertices = 4;

        if (runtimeMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                runtimeMaterial = new Material(shader);
            }
        }
        if (runtimeMaterial != null) line.material = runtimeMaterial;
        return line;
    }

    private void ResolveReferences()
    {
        if (castNet == null)
        {
            castNet = FindFirstObjectByType<CastNetController>();
        }
    }

    private float GetUpgradeValue(string effectId, float fallback) =>
        SkillTreeManager.Instance != null
            ? SkillTreeManager.Instance.GetAbilityEffectValue(
                GrowthAbilitySlot.SignatureR, effectId, fallback)
            : fallback;

    private void RestoreEquippedSkillFromRunState()
    {
        if (RunManager.Instance == null || RunManager.Instance.GrowthState == null) return;
        string abilityId = RunManager.Instance.GrowthState
            .GetAbility(GrowthAbilitySlot.SignatureR).EquippedAbilityId;
        equippedSkill = GetSkillId(abilityId);
    }

    private void StopAllEffects()
    {
        if (activeEffect != null)
        {
            StopCoroutine(activeEffect);
            activeEffect = null;
        }
        CancelTargeting();
        HideFishingFloats();
        EndCrossLockdown();
        castVisualRemaining = 0f;
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon) return Vector2.Distance(point, start);
        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * t);
    }

    private static SignatureSkillId GetToolSkill(ToolId tool) => tool switch
    {
        ToolId.FishingRod => SignatureSkillId.FishingGroundCrossing,
        ToolId.Net => SignatureSkillId.CrossLockdown,
        ToolId.CastNet => SignatureSkillId.HeavenlyNet,
        _ => SignatureSkillId.None
    };

    private static string GetSkillName(SignatureSkillId skill) => skill switch
    {
        SignatureSkillId.FishingGroundCrossing => "어장 대횡단",
        SignatureSkillId.CrossLockdown => "교차 봉쇄",
        SignatureSkillId.HeavenlyNet => "천망",
        _ => string.Empty
    };

    private static string GetAbilityId(SignatureSkillId skill) => skill switch
    {
        SignatureSkillId.FishingGroundCrossing => FishingGroundCrossingAbilityId,
        SignatureSkillId.CrossLockdown => CrossLockdownAbilityId,
        SignatureSkillId.HeavenlyNet => HeavenlyNetAbilityId,
        _ => string.Empty
    };

    private static SignatureSkillId GetSkillId(string abilityId) => abilityId switch
    {
        FishingGroundCrossingAbilityId => SignatureSkillId.FishingGroundCrossing,
        CrossLockdownAbilityId => SignatureSkillId.CrossLockdown,
        HeavenlyNetAbilityId => SignatureSkillId.HeavenlyNet,
        _ => SignatureSkillId.None
    };

    private void OnDisable()
    {
        StopAllEffects();
        if (Instance == this) Instance = null;
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null) Destroy(runtimeMaterial);
    }

    private void OnValidate()
    {
        fishingLaneCount = Mathf.Max(1, fishingLaneCount);
        fishingTraversalCount = Mathf.Max(1, fishingTraversalCount);
        heavenlyNetCastCount = Mathf.Max(1, heavenlyNetCastCount);
        gameplayViewport.x = Mathf.Clamp01(gameplayViewport.x);
        gameplayViewport.y = Mathf.Clamp01(gameplayViewport.y);
        gameplayViewport.width = Mathf.Clamp(
            gameplayViewport.width, 0.01f, 1f - gameplayViewport.x);
        gameplayViewport.height = Mathf.Clamp(
            gameplayViewport.height, 0.01f, 1f - gameplayViewport.y);
    }
}
