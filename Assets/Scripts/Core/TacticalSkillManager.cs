using UnityEngine;
using UnityEngine.InputSystem;

public enum TacticalSkillId
{
    None = 0,
    RapidReeling = 1,
    EmergencyLockdown = 2,
    EmergencyCastNet = 3,
    Overbaiting = 4,
    FocusedOperation = 5
}

[DisallowMultipleComponent]
[DefaultExecutionOrder(-90)]
public sealed class TacticalSkillManager : MonoBehaviour
{
    public const string RapidReelingAbilityId = "tactical.rapid_reeling";
    public const string EmergencyLockdownAbilityId = "tactical.emergency_lockdown";
    public const string EmergencyCastNetAbilityId = "tactical.emergency_cast_net";
    public const string OverbaitingAbilityId = "tactical.overbaiting";
    public const string FocusedOperationAbilityId = "tactical.focused_operation";

    [Header("Selection")]
    [Min(0f)] [SerializeField] private float selectionInputDelay = 0.25f;

    [Header("Fishing Rod - Rapid Reeling")]
    [Min(0.1f)] [SerializeField] private float rapidReelingCooldown = 24f;
    [Min(0.1f)] [SerializeField] private float rapidReelingDuration = 6f;
    [Min(1f)] [SerializeField] private float rapidReelingSpeedMultiplier = 2f;

    [Header("Net - Emergency Lockdown")]
    [Min(0.1f)] [SerializeField] private float emergencyLockdownCooldown = 28f;
    [Min(0.1f)] [SerializeField] private float emergencyLockdownDuration = 6f;
    [Min(1f)] [SerializeField] private float emergencyLockdownEffectMultiplier = 1.75f;

    [Header("Cast Net - Emergency Cast Net")]
    [Min(0.1f)] [SerializeField] private float emergencyCastNetCooldown = 18f;

    [Header("Bait - Overbaiting")]
    [Min(0.1f)] [SerializeField] private float overbaitingCooldown = 22f;
    [Min(0.1f)] [SerializeField] private float overbaitingDuration = 7f;
    [Min(1f)] [SerializeField] private float overbaitingAttractionMultiplier = 1.75f;

    [Header("Generic - Focused Operation")]
    [Min(0.1f)] [SerializeField] private float focusedOperationCooldown = 26f;
    [Min(0.1f)] [SerializeField] private float focusedOperationDuration = 6f;
    [Min(0.1f)] [SerializeField] private float focusedOperationRadius = 2.5f;
    [Min(1f)] [SerializeField] private float focusedOperationDamageMultiplier = 1.5f;

    [Header("Effect References (Optional)")]
    [SerializeField] private FishingRodPlacementController fishingRodPlacement;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private BaitController bait;

    private readonly TacticalSkillId[] choices = new TacticalSkillId[3];
    private bool selectionPending;
    private bool isChoosing;
    private bool canSelect;
    private float selectionUnlockTime;
    private TacticalSkillId equippedSkill;
    private TacticalSkillId activeTimedSkill;
    private float cooldownRemaining;
    private float effectRemaining;
    private bool isTargeting;
    private Vector2 targetPosition;
    private Vector2 focusedZonePosition;
    private float activeFocusedDamageMultiplier = 1f;
    private float targetingCastRadiusMultiplier = 1f;
    private LineRenderer focusedZoneVisual;
    private Material focusedZoneMaterial;

    public static TacticalSkillManager Instance { get; private set; }
    public static bool IsSelectionPendingOrActive =>
        Instance != null && (Instance.selectionPending || Instance.isChoosing);
    public static bool IsTargeting => Instance != null && Instance.isTargeting;

    public bool IsChoosing => isChoosing;
    public bool CanSelect => isChoosing && canSelect;
    public bool HasEquippedSkill => equippedSkill != TacticalSkillId.None;
    public bool IsAiming => isTargeting;
    public float RemainingCooldown => Mathf.Max(0f, cooldownRemaining);
    public float CooldownNormalized
    {
        get
        {
            float cooldown = GetCooldown(equippedSkill);
            return cooldown > 0f ? Mathf.Clamp01(RemainingCooldown / cooldown) : 0f;
        }
    }

    public string EquippedSkillName => GetSkillName(equippedSkill);
    public string DescribeUpgrade(string effectId, float modifier)
    {
        if (effectId == SkillTreeEffectIds.TacticalCooldownMultiplier)
            return $"재사용 대기시간 {GetBaselineCooldown(equippedSkill) * modifier:0.#}초";
        if (effectId != SkillTreeEffectIds.TacticalEffectMultiplier) return string.Empty;
        return equippedSkill switch
        {
            TacticalSkillId.RapidReeling =>
                $"공격 속도 ×{rapidReelingSpeedMultiplier * modifier:0.##}",
            TacticalSkillId.EmergencyLockdown =>
                $"포획력·감속 강화 ×{emergencyLockdownEffectMultiplier * modifier:0.##}",
            TacticalSkillId.EmergencyCastNet => $"투망 반경 ×{modifier:0.##}",
            TacticalSkillId.Overbaiting =>
                $"유인 반경 ×{overbaitingAttractionMultiplier * modifier:0.##}",
            TacticalSkillId.FocusedOperation =>
                $"Resistance 피해 ×{focusedOperationDamageMultiplier * modifier:0.##}",
            _ => string.Empty
        };
    }
    public string HotbarStatus
    {
        get
        {
            if (!HasEquippedSkill)
            {
                return "잠김\n미니보스 보상";
            }

            if (isTargeting)
            {
                return $"{EquippedSkillName}\n대상 지정 중";
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
        UpdateSelection();

        if (isChoosing || selectionPending)
        {
            return;
        }

        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        if (flow != null && flow.IsGameEnded)
        {
            CancelTargeting();
            ClearTimedEffect();
            return;
        }

        UpdateGameplayTimers();

        if (isTargeting)
        {
            UpdateTargeting();
            return;
        }

        if (!CanUseGameplayInput() || Keyboard.current == null ||
            !Keyboard.current.eKey.wasPressedThisFrame || cooldownRemaining > 0f)
        {
            return;
        }

        ActivateEquippedSkill();
    }

    public bool RequestSelection()
    {
        RestoreEquippedSkillFromRunState();
        if (HasEquippedSkill || selectionPending || isChoosing ||
            RunManager.Instance == null)
        {
            return false;
        }

        RunGrowthState growth = RunManager.Instance.GrowthState;
        if (growth == null || !growth.HasClearedCurrentAreaMiniBoss)
        {
            return false;
        }

        choices[0] = GetToolSkill(growth.SelectedCoreTool);
        choices[1] = GetToolSkill(growth.SelectedPartnerTool);
        choices[2] = TacticalSkillId.FocusedOperation;

        if (choices[0] == TacticalSkillId.None ||
            choices[1] == TacticalSkillId.None ||
            choices[0] == choices[1])
        {
            Debug.LogError("Tactical skill choices require two distinct selected tools.");
            return false;
        }

        selectionPending = true;
        Time.timeScale = 0f;
        TryOpenSelection();
        return true;
    }

    public string GetChoiceName(int index) => GetSkillName(GetChoice(index));

    public string GetChoiceDescription(int index)
    {
        return GetChoice(index) switch
        {
            TacticalSkillId.RapidReeling =>
                $"설치된 낚싯대의 공격 속도가 {rapidReelingDuration:F0}초 동안 " +
                $"{rapidReelingSpeedMultiplier:0.##}배가 됩니다.",
            TacticalSkillId.EmergencyLockdown =>
                $"설치된 그물을 재가동하고 {emergencyLockdownDuration:F0}초 동안 " +
                $"포획력과 감속 효과를 {emergencyLockdownEffectMultiplier:0.##}배 강화합니다.",
            TacticalSkillId.EmergencyCastNet =>
                "위치를 지정해 충전을 소비하지 않는 투망 공격을 1회 사용합니다.",
            TacticalSkillId.Overbaiting =>
                $"{overbaitingDuration:F0}초 동안 미끼의 유효 유인 반경을 " +
                $"{overbaitingAttractionMultiplier:0.##}배로 늘립니다.",
            TacticalSkillId.FocusedOperation =>
                $"위치를 지정해 {focusedOperationDuration:F0}초 동안 반경 " +
                $"{focusedOperationRadius:0.#}의 조업 구역을 만듭니다. 구역 안 포획 피해가 " +
                $"{focusedOperationDamageMultiplier:0.##}배가 됩니다.",
            _ => string.Empty
        };
    }

    public bool SelectChoiceFromUI(int index)
    {
        if (!CanSelect || RunManager.Instance == null)
        {
            return false;
        }

        TacticalSkillId choice = GetChoice(index);
        string abilityId = GetAbilityId(choice);
        if (choice == TacticalSkillId.None || string.IsNullOrEmpty(abilityId) ||
            !RunManager.Instance.GrowthState.TryUnlockAndEquipAbility(
                GrowthAbilitySlot.TacticalE,
                abilityId))
        {
            return false;
        }

        equippedSkill = choice;
        selectionPending = false;
        isChoosing = false;
        canSelect = false;
        ResumeGameplaySpeed();
        return true;
    }

    public float GetCaptureDamageMultiplier(Vector2 fishPosition)
    {
        if (activeTimedSkill != TacticalSkillId.FocusedOperation ||
            effectRemaining <= 0f)
        {
            return 1f;
        }

        return Vector2.SqrMagnitude(fishPosition - focusedZonePosition) <=
            focusedOperationRadius * focusedOperationRadius
                ? activeFocusedDamageMultiplier
                : 1f;
    }

    private void UpdateSelection()
    {
        if (selectionPending && !isChoosing)
        {
            TryOpenSelection();
        }

        if (!isChoosing || canSelect ||
            Time.realtimeSinceStartup < selectionUnlockTime)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            return;
        }

        canSelect = true;
    }

    private void TryOpenSelection()
    {
        if (!selectionPending || IsOtherModalOpen())
        {
            return;
        }

        isChoosing = true;
        canSelect = false;
        selectionUnlockTime = Time.realtimeSinceStartup + selectionInputDelay;
        Time.timeScale = 0f;
    }

    private void UpdateGameplayTimers()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
        }

        if (effectRemaining <= 0f)
        {
            return;
        }

        effectRemaining = Mathf.Max(0f, effectRemaining - Time.deltaTime);
        if (effectRemaining <= 0f)
        {
            ClearTimedEffect();
        }
    }

    private void ActivateEquippedSkill()
    {
        ResolveReferences();
        switch (equippedSkill)
        {
            case TacticalSkillId.RapidReeling:
                if (fishingRodPlacement == null) return;
                BeginTimedEffect(equippedSkill, rapidReelingDuration);
                fishingRodPlacement.SetTacticalAttackSpeedMultiplier(
                    rapidReelingSpeedMultiplier * GetEffectUpgradeMultiplier());
                StartCooldown(GetEffectiveCooldown(rapidReelingCooldown));
                break;
            case TacticalSkillId.EmergencyLockdown:
                if (netPlacement == null) return;
                BeginTimedEffect(equippedSkill, emergencyLockdownDuration);
                netPlacement.ActivateTacticalLockdown(
                    emergencyLockdownEffectMultiplier * GetEffectUpgradeMultiplier());
                StartCooldown(GetEffectiveCooldown(emergencyLockdownCooldown));
                break;
            case TacticalSkillId.EmergencyCastNet:
                if (castNet == null || !CanBeginTargeting()) return;
                targetingCastRadiusMultiplier = GetEffectUpgradeMultiplier();
                isTargeting = castNet.BeginTacticalAim(targetingCastRadiusMultiplier);
                break;
            case TacticalSkillId.Overbaiting:
                if (bait == null) return;
                BeginTimedEffect(equippedSkill, overbaitingDuration);
                bait.SetTacticalAttractionMultiplier(
                    overbaitingAttractionMultiplier * GetEffectUpgradeMultiplier());
                StartCooldown(GetEffectiveCooldown(overbaitingCooldown));
                break;
            case TacticalSkillId.FocusedOperation:
                if (!CanBeginTargeting()) return;
                isTargeting = true;
                UpdateTargetPosition();
                ShowFocusedZoneVisual(targetPosition, focusedOperationRadius, true);
                break;
        }
    }

    private void UpdateTargeting()
    {
        if (!CanUseGameplayInput() || Mouse.current == null || Keyboard.current == null)
        {
            CancelTargeting();
            return;
        }

        bool cancel = Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame;
        if (cancel)
        {
            CancelTargeting();
            return;
        }

        UpdateTargetPosition();
        if (equippedSkill == TacticalSkillId.EmergencyCastNet)
        {
            castNet?.UpdateTacticalAim();

            if (!Keyboard.current.eKey.wasReleasedThisFrame)
            {
                return;
            }

            if (castNet == null || !castNet.ConfirmTacticalCast(
                    targetingCastRadiusMultiplier)) return;
            isTargeting = false;
            StartCooldown(GetEffectiveCooldown(emergencyCastNetCooldown));
            return;
        }

        ShowFocusedZoneVisual(targetPosition, focusedOperationRadius, true);

        if (!Mouse.current.leftButton.wasPressedThisFrame ||
            GearRepositionController.IsRepositionModifierHeld)
        {
            return;
        }

        focusedZonePosition = targetPosition;
        activeFocusedDamageMultiplier =
            focusedOperationDamageMultiplier * GetEffectUpgradeMultiplier();
        isTargeting = false;
        BeginTimedEffect(TacticalSkillId.FocusedOperation, focusedOperationDuration);
        ShowFocusedZoneVisual(focusedZonePosition, focusedOperationRadius, false);
        StartCooldown(GetEffectiveCooldown(focusedOperationCooldown));
    }

    private void CancelTargeting()
    {
        if (!isTargeting)
        {
            return;
        }

        isTargeting = false;
        castNet?.CancelTacticalAim();
        if (activeTimedSkill != TacticalSkillId.FocusedOperation)
        {
            HideFocusedZoneVisual();
        }
    }

    private bool CanBeginTargeting() =>
        !NetPlacementController.IsNetModeActive &&
        !FishingRodPlacementController.IsRodModeActive &&
        !GearRepositionController.IsRepositioning &&
        !GearRepositionController.IsRepositionModifierHeld &&
        (castNet == null || !castNet.IsAiming);

    private static bool CanUseGameplayInput()
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        return !ToolSlotInput.IsSelectionOrEndBlocked &&
            !SignatureSkillManager.IsTargeting &&
            flow != null && flow.IsFishingStarted && !flow.IsPreparation && !flow.IsGameEnded;
    }

    private void BeginTimedEffect(TacticalSkillId skill, float duration)
    {
        ClearTimedEffect();
        activeTimedSkill = skill;
        effectRemaining = Mathf.Max(0f, duration);
    }

    private void ClearTimedEffect()
    {
        TacticalSkillId skill = activeTimedSkill;
        activeTimedSkill = TacticalSkillId.None;
        effectRemaining = 0f;

        switch (skill)
        {
            case TacticalSkillId.RapidReeling:
                fishingRodPlacement?.SetTacticalAttackSpeedMultiplier(1f);
                break;
            case TacticalSkillId.EmergencyLockdown:
                netPlacement?.ClearTacticalLockdown();
                break;
            case TacticalSkillId.Overbaiting:
                bait?.SetTacticalAttractionMultiplier(1f);
                break;
            case TacticalSkillId.FocusedOperation:
                activeFocusedDamageMultiplier = 1f;
                HideFocusedZoneVisual();
                break;
        }
    }

    private void StartCooldown(float duration)
    {
        cooldownRemaining = Mathf.Max(0f, duration);
    }

    private void UpdateTargetPosition()
    {
        Camera camera = Camera.main;
        if (camera == null || Mouse.current == null)
        {
            return;
        }

        Vector3 world = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        targetPosition = new Vector2(world.x, world.y);
    }

    private void ShowFocusedZoneVisual(Vector2 position, float radius, bool preview)
    {
        EnsureFocusedZoneVisual();
        if (focusedZoneVisual == null) return;

        focusedZoneVisual.gameObject.SetActive(true);
        focusedZoneVisual.transform.position = position;
        focusedZoneVisual.startColor = focusedZoneVisual.endColor = preview
            ? new Color(0.35f, 0.85f, 1f, 0.8f)
            : new Color(1f, 0.78f, 0.2f, 0.9f);

        int count = focusedZoneVisual.positionCount;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / (count - 1);
            focusedZoneVisual.SetPosition(
                i,
                new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private void EnsureFocusedZoneVisual()
    {
        if (focusedZoneVisual != null) return;

        GameObject visual = new GameObject("FocusedOperationZone");
        visual.transform.SetParent(transform, false);
        focusedZoneVisual = visual.AddComponent<LineRenderer>();
        focusedZoneVisual.useWorldSpace = false;
        focusedZoneVisual.loop = false;
        focusedZoneVisual.positionCount = 49;
        focusedZoneVisual.startWidth = 0.06f;
        focusedZoneVisual.endWidth = 0.06f;
        focusedZoneVisual.sortingOrder = 50;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            focusedZoneMaterial = new Material(shader);
            focusedZoneVisual.material = focusedZoneMaterial;
        }
    }

    private void HideFocusedZoneVisual()
    {
        if (focusedZoneVisual != null)
        {
            focusedZoneVisual.gameObject.SetActive(false);
        }
    }

    private void ResolveReferences()
    {
        if (fishingRodPlacement == null)
            fishingRodPlacement = FindFirstObjectByType<FishingRodPlacementController>();
        if (netPlacement == null)
            netPlacement = FindFirstObjectByType<NetPlacementController>();
        if (castNet == null)
            castNet = FindFirstObjectByType<CastNetController>();
        if (bait == null)
            bait = FindFirstObjectByType<BaitController>();
    }

    private void RestoreEquippedSkillFromRunState()
    {
        if (RunManager.Instance == null || RunManager.Instance.GrowthState == null)
        {
            return;
        }

        string abilityId = RunManager.Instance.GrowthState
            .GetAbility(GrowthAbilitySlot.TacticalE).EquippedAbilityId;
        equippedSkill = GetSkillId(abilityId);
    }

    private TacticalSkillId GetChoice(int index) =>
        index >= 0 && index < choices.Length ? choices[index] : TacticalSkillId.None;

    private float GetCooldown(TacticalSkillId skill) => skill switch
    {
        TacticalSkillId.RapidReeling => GetEffectiveCooldown(rapidReelingCooldown),
        TacticalSkillId.EmergencyLockdown => GetEffectiveCooldown(emergencyLockdownCooldown),
        TacticalSkillId.EmergencyCastNet => GetEffectiveCooldown(emergencyCastNetCooldown),
        TacticalSkillId.Overbaiting => GetEffectiveCooldown(overbaitingCooldown),
        TacticalSkillId.FocusedOperation => GetEffectiveCooldown(focusedOperationCooldown),
        _ => 0f
    };

    private float GetBaselineCooldown(TacticalSkillId skill) => skill switch
    {
        TacticalSkillId.RapidReeling => rapidReelingCooldown,
        TacticalSkillId.EmergencyLockdown => emergencyLockdownCooldown,
        TacticalSkillId.EmergencyCastNet => emergencyCastNetCooldown,
        TacticalSkillId.Overbaiting => overbaitingCooldown,
        TacticalSkillId.FocusedOperation => focusedOperationCooldown,
        _ => 0f
    };

    private float GetEffectUpgradeMultiplier() =>
        SkillTreeManager.Instance != null
            ? SkillTreeManager.Instance.GetAbilityEffectValue(
                GrowthAbilitySlot.TacticalE,
                SkillTreeEffectIds.TacticalEffectMultiplier)
            : 1f;

    private float GetEffectiveCooldown(float baseline) => baseline *
        (SkillTreeManager.Instance != null
            ? SkillTreeManager.Instance.GetAbilityEffectValue(
                GrowthAbilitySlot.TacticalE,
                SkillTreeEffectIds.TacticalCooldownMultiplier)
            : 1f);

    private static TacticalSkillId GetToolSkill(ToolId tool) => tool switch
    {
        ToolId.FishingRod => TacticalSkillId.RapidReeling,
        ToolId.Net => TacticalSkillId.EmergencyLockdown,
        ToolId.CastNet => TacticalSkillId.EmergencyCastNet,
        ToolId.Bait => TacticalSkillId.Overbaiting,
        _ => TacticalSkillId.None
    };

    private static string GetSkillName(TacticalSkillId skill) => skill switch
    {
        TacticalSkillId.RapidReeling => "급속 릴링",
        TacticalSkillId.EmergencyLockdown => "긴급 봉쇄",
        TacticalSkillId.EmergencyCastNet => "비상 투망",
        TacticalSkillId.Overbaiting => "과잉 집어",
        TacticalSkillId.FocusedOperation => "집중 조업",
        _ => string.Empty
    };

    private static string GetAbilityId(TacticalSkillId skill) => skill switch
    {
        TacticalSkillId.RapidReeling => RapidReelingAbilityId,
        TacticalSkillId.EmergencyLockdown => EmergencyLockdownAbilityId,
        TacticalSkillId.EmergencyCastNet => EmergencyCastNetAbilityId,
        TacticalSkillId.Overbaiting => OverbaitingAbilityId,
        TacticalSkillId.FocusedOperation => FocusedOperationAbilityId,
        _ => string.Empty
    };

    private static TacticalSkillId GetSkillId(string abilityId) => abilityId switch
    {
        RapidReelingAbilityId => TacticalSkillId.RapidReeling,
        EmergencyLockdownAbilityId => TacticalSkillId.EmergencyLockdown,
        EmergencyCastNetAbilityId => TacticalSkillId.EmergencyCastNet,
        OverbaitingAbilityId => TacticalSkillId.Overbaiting,
        FocusedOperationAbilityId => TacticalSkillId.FocusedOperation,
        _ => TacticalSkillId.None
    };

    private static bool IsOtherModalOpen() =>
        (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen) ||
        (ToolAcquisitionManager.Instance != null && ToolAcquisitionManager.Instance.IsAcquisitionPending) ||
        (PrototypeAugmentManager.Instance != null && PrototypeAugmentManager.Instance.IsShowingChoices) ||
        (PrototypeJobManager.Instance != null && PrototypeJobManager.Instance.IsChoosingJob) ||
        ItemRewardManager.IsSelectionPendingOrActive ||
        (PrototypeGameFlowManager.Instance != null && PrototypeGameFlowManager.Instance.IsGameEnded);

    private static void ResumeGameplaySpeed()
    {
        if (PrototypeGameFlowManager.Instance != null)
            PrototypeGameFlowManager.Instance.ResumeGameplayTimeScale();
        else
            Time.timeScale = 1f;
    }

    private void OnDisable()
    {
        CancelTargeting();
        ClearTimedEffect();
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnDestroy()
    {
        if (focusedZoneMaterial != null)
        {
            Destroy(focusedZoneMaterial);
        }
    }

    private void OnValidate()
    {
        rapidReelingSpeedMultiplier = Mathf.Max(1f, rapidReelingSpeedMultiplier);
        emergencyLockdownEffectMultiplier = Mathf.Max(1f, emergencyLockdownEffectMultiplier);
        overbaitingAttractionMultiplier = Mathf.Max(1f, overbaitingAttractionMultiplier);
        focusedOperationDamageMultiplier = Mathf.Max(1f, focusedOperationDamageMultiplier);
    }
}
