using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastNetController : MonoBehaviour
{
    [Header("Cast Net")]
    [SerializeField] private float capturePower = 15f;
    [SerializeField] private float captureRadius = 1f;
    [SerializeField] private float cooldown = 7f;
    [Min(1)] [SerializeField] private int maxCharges = 1;

    [Header("Mass Catch Refund")]
    [SerializeField] private int refundThreshold = 8;
    [SerializeField] private float refundAmount = 2f;

    [Header("Visual")]
    [SerializeField] private Transform castVisual;

    private Camera mainCamera;
    private CastNetPresentation presentation;
    private int observedLoadoutRevision = -1;

    private int currentCharges;
    private float rechargeTimer;

    private bool isAiming;
    private bool isTacticalAiming;

    private Vector2 currentAimPosition;
    private int currentTargetCount;
    private float tacticalRadiusMultiplier = 1f;

    private int lastCapturedCount;
    private Vector2 lastCastPosition;
    private float catchFeedbackTimer;

    private bool massCatchRefundEnabled;

    public bool IsAiming => isAiming || isTacticalAiming;
    public float CaptureRadius => captureRadius;
    public float CurrentAimRadius => captureRadius * (isTacticalAiming ? tacticalRadiusMultiplier : 1f);

    public bool IsReady =>
        currentCharges > 0;

    public float CooldownTimer =>
        Mathf.Max(0f, rechargeTimer);

    public float CooldownNormalized =>
        cooldown > 0f
            ? Mathf.Clamp01(CooldownTimer / cooldown)
            : 0f;

    public int CurrentCharges =>
        currentCharges;

    public int MaxCharges =>
        maxCharges;

    public Vector2 CurrentAimPosition =>
        currentAimPosition;

    public int CurrentTargetCount =>
        currentTargetCount;

    public int LastCapturedCount =>
        lastCapturedCount;

    public Vector2 LastCastPosition =>
        lastCastPosition;

    public bool IsShowingCatchFeedback =>
        catchFeedbackTimer > 0f;

    private void Awake()
    {
        currentCharges = maxCharges;

        mainCamera = Camera.main;
        presentation = GetComponent<CastNetPresentation>();
        if (presentation == null)
            presentation = gameObject.AddComponent<CastNetPresentation>();
        presentation.Bind(castVisual);

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);

            UpdateVisualScale();
        }
    }

    private void Update()
    {
        UpdateRecharge();

        int loadoutRevision = RunManager.Instance != null
            ? RunManager.Instance.ToolSlots.Revision : -1;
        if (observedLoadoutRevision >= 0 && loadoutRevision != observedLoadoutRevision)
            presentation?.ResetVisual();
        observedLoadoutRevision = loadoutRevision;

        if (Time.timeScale <= 0f || ToolSlotInput.IsSelectionOrEndBlocked)
        {
            CancelAiming();
            return;
        }

        if (catchFeedbackTimer > 0f)
        {
            catchFeedbackTimer -=
                Time.deltaTime;
        }

        if (isTacticalAiming)
        {
            UpdateAimPosition();
            return;
        }

        ToolInputState input = ToolSlotInput.Read(ToolId.CastNet);
        if (input.Cancelled)
        {
            if (IsAiming) CancelAiming();
            return;
        }

        HandleCastNetInput(input);
    }

    private void UpdateRecharge()
    {
        if (currentCharges >= maxCharges)
        {
            rechargeTimer = 0f;
            return;
        }

        if (rechargeTimer <= 0f)
        {
            rechargeTimer = cooldown;
        }

        rechargeTimer -= Time.deltaTime;

        if (rechargeTimer > 0f)
        {
            return;
        }

        currentCharges++;

        if (currentCharges < maxCharges)
        {
            rechargeTimer = cooldown;
        }
        else
        {
            rechargeTimer = 0f;
        }
    }

    private void HandleCastNetInput(ToolInputState input)
    {
        if (!isAiming &&
            IsReady &&
            input.Pressed)
        {
            StartAiming();
        }

        if (!isAiming)
        {
            return;
        }

        UpdateAimPosition();

        if (input.Released)
        {
            UseCastNet(currentAimPosition, true);

            isAiming = false;

            if (castVisual != null)
            {
                castVisual.gameObject.SetActive(false);
            }
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        UpdateVisualScale();

        UpdateAimPosition();

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(true);
        }
    }

    private void UpdateAimPosition()
    {
        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        currentAimPosition =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        if (castVisual != null)
        {
            castVisual.position =
                currentAimPosition;
        }

        presentation?.SetAim(currentAimPosition, CurrentAimRadius);

        UpdateTargetCount();
    }

    private void UpdateTargetCount()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                currentAimPosition,
                captureRadius * (isTacticalAiming ? tacticalRadiusMultiplier : 1f)
            );

        int count = 0;

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish != null)
            {
                count++;
            }
        }

        currentTargetCount = count;
    }

    private bool UseCastNet(
        Vector2 castPosition,
        bool consumeCharge,
        string attackId = "cast_net")
    {
        if (consumeCharge && currentCharges <= 0)
        {
            return false;
        }

        if (consumeCharge)
        {
            bool wasFull = currentCharges == maxCharges;

            currentCharges--;

            if (wasFull)
            {
                rechargeTimer = cooldown;
            }
        }

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                castPosition,
                captureRadius
            );

        HashSet<FishController> damagedFish = new();
        List<Vector2> hitPositions = new();
        int capturedCount = 0;

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null || !damagedFish.Add(fish))
            {
                continue;
            }

            float previousResistance = fish.CurrentResistance;
            Vector2 hitPosition = fish.transform.position;
            bool captured =
                fish.TakeCaptureDamage(
                    capturePower,
                    CombatDamageContext.Tool(
                        attackId,
                        this)
                );

            if (fish.CurrentResistance < previousResistance)
                hitPositions.Add(hitPosition);

            if (captured)
            {
                capturedCount++;
            }
        }

        if (consumeCharge && massCatchRefundEnabled &&
            capturedCount >= refundThreshold)
        {
            ReduceCurrentRecharge(
                refundAmount
            );
        }

        lastCapturedCount =
            capturedCount;

        lastCastPosition =
            castPosition;

        catchFeedbackTimer =
            1.2f;

        currentTargetCount = 0;

        presentation?.ShowCast(castPosition, captureRadius, hitPositions);

        return true;
    }

    private void ReduceCurrentRecharge(
        float amount)
    {
        if (currentCharges >= maxCharges)
        {
            return;
        }

        rechargeTimer -= amount;

        if (rechargeTimer > 0f)
        {
            return;
        }

        currentCharges++;

        if (currentCharges < maxCharges)
        {
            rechargeTimer = cooldown;
        }
        else
        {
            rechargeTimer = 0f;
        }
    }

    private void CancelAiming()
    {
        isAiming = false;
        isTacticalAiming = false;
        tacticalRadiusMultiplier = 1f;
        currentTargetCount = 0;

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);
        }
        presentation?.ResetVisual();
        UpdateVisualScale();
    }

    public bool BeginTacticalAim(float radiusMultiplier = 1f)
    {
        if (Time.timeScale <= 0f || ToolSlotInput.IsSelectionOrEndBlocked)
            return false;
        CancelAiming();
        isTacticalAiming = true;
        tacticalRadiusMultiplier = Mathf.Max(1f, radiusMultiplier);
        UpdateAimPosition();

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(true);
            float diameter = captureRadius * 2f * tacticalRadiusMultiplier;
            castVisual.localScale = new Vector3(diameter, diameter, 1f);
        }

        return true;
    }

    public int ApplySignatureCast(
        Vector2 castPosition,
        float radius,
        float damageMultiplier,
        string attackId = "signature.heavenly_net",
        bool showCastNetPresentation = false)
    {
        if (radius <= 0f || damageMultiplier <= 0f)
        {
            return 0;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            castPosition,
            radius);
        HashSet<FishController> damagedFish = new();
        List<Vector2> hitPositions = new();
        int capturedCount = 0;

        foreach (Collider2D hit in hits)
        {
            FishController fish = hit.GetComponent<FishController>();
            if (fish == null || !damagedFish.Add(fish))
            {
                continue;
            }

            float previousResistance = fish.CurrentResistance;
            Vector2 hitPosition = fish.transform.position;
            if (fish.TakeCaptureDamage(
                    capturePower * damageMultiplier,
                    CombatDamageContext.Tool(
                        attackId,
                        this)))
            {
                capturedCount++;
            }
            if (fish.CurrentResistance < previousResistance)
                hitPositions.Add(hitPosition);
        }

        lastCapturedCount = capturedCount;
        lastCastPosition = castPosition;
        catchFeedbackTimer = 1.2f;
        if (showCastNetPresentation)
            presentation?.ShowCast(castPosition, radius, hitPositions);
        return capturedCount;
    }

    public void UpdateTacticalAim()
    {
        if (isTacticalAiming)
        {
            UpdateAimPosition();
        }
    }

    public bool ConfirmTacticalCast(float radiusMultiplier = 1f)
    {
        if (!isTacticalAiming || Time.timeScale <= 0f ||
            ToolSlotInput.IsSelectionOrEndBlocked)
        {
            return false;
        }

        Vector2 position = currentAimPosition;
        isTacticalAiming = false;
        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);
        }
        UpdateVisualScale();

        float multiplier = Mathf.Max(1f, radiusMultiplier);
        tacticalRadiusMultiplier = 1f;
        if (multiplier <= 1f)
        {
            return UseCastNet(
                position,
                false,
                "tactical.emergency_cast_net");
        }

        ApplySignatureCast(
            position,
            captureRadius * multiplier,
            1f,
            "tactical.emergency_cast_net",
            true);
        currentTargetCount = 0;
        return true;
    }

    public void CancelTacticalAim()
    {
        if (isTacticalAiming)
        {
            CancelAiming();
        }
    }

    private void OnDisable()
    {
        CancelAiming();
    }

    private void UpdateVisualScale()
    {
        if (castVisual == null)
        {
            return;
        }

        float diameter =
            captureRadius * 2f;

        castVisual.localScale =
            new Vector3(
                diameter,
                diameter,
                1f
            );
    }

    public void IncreaseCapturePower(
        float amount)
    {
        capturePower += amount;
    }

    public void IncreaseCaptureRadius(
        float amount)
    {
        captureRadius += amount;

        UpdateVisualScale();
    }

    public void ReduceCooldown(
        float amount)
    {
        cooldown =
            Mathf.Max(
                0.5f,
                cooldown - amount
            );

        if (rechargeTimer > cooldown)
        {
            rechargeTimer = cooldown;
        }
    }

    public void EnableMassCatchRefund()
    {
        massCatchRefundEnabled = true;
    }

    public void IncreaseMaxCharges(
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        maxCharges += amount;
        currentCharges = Mathf.Min(maxCharges, currentCharges + amount);
    }

    public void EnableCastNetFisherJob()
    {
        // G4-A transition: legacy Job selection remains, but its gameplay
        // modifier is now purchased from the Cast Net skill tree.
    }
}
