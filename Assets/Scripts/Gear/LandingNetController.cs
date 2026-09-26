using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LandingNetController : MonoBehaviour
{
    [Header("Landing Net")]
    [SerializeField] private float capturePower = 5f;
    [SerializeField] private float captureRadius = 1.2f;
    [SerializeField] private float attackCooldown = 0.3f;
    [SerializeField] private int maxTargets = 3;

    [Header("Visual")]
    [SerializeField] private Transform rangeVisual;

    [Header("Chain Capture")]
    [SerializeField] private float chainRadius = 1.5f;
    [SerializeField] private float chainDamage = 2f;

    private Camera mainCamera;
    private float nextAttackTime;
    private LandingNetPresentation presentation;
    private readonly List<ScoopNetHit> confirmedHits = new();

    public float CaptureRadius => captureRadius;
    public float AttackCooldown => attackCooldown;

    private bool chainCaptureEnabled;
    private bool autoUseEnabled;

    public float RemainingCooldown =>
        Mathf.Max(0f, nextAttackTime - Time.time);

    public float CooldownNormalized =>
        attackCooldown > 0f
            ? Mathf.Clamp01(RemainingCooldown / attackCooldown)
            : 0f;

    private void Awake()
    {
        mainCamera = Camera.main;

        UpdateRangeVisual();
        presentation = GetComponent<LandingNetPresentation>();
        if (presentation == null)
            presentation = gameObject.AddComponent<LandingNetPresentation>();
        presentation.Bind(this, rangeVisual);
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || ToolSlotInput.IsWorldPointerReserved)
        {
            presentation?.SetAimAvailable(false);
            return;
        }

        if (Mouse.current == null)
        {
            presentation?.SetAimAvailable(false);
            return;
        }

        if (PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsPreparation)
        {
            presentation?.SetAimAvailable(false);
            return;
        }

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                mouseScreenPosition
            );

        mouseWorldPosition.z = 0f;

        if (rangeVisual != null)
        {
            rangeVisual.position =
                mouseWorldPosition;
        }
        presentation?.SetAimPosition(mouseWorldPosition);

        if (NetPlacementController.IsNetModeActive ||
            FishingRodPlacementController.IsRodModeActive ||
            GearRepositionController.IsRepositioning ||
            GearRepositionController.IsRepositionModifierHeld)
        {
            presentation?.SetAimAvailable(false);
            return;
        }

        presentation?.SetAimAvailable(true);

        bool shouldUse;

        if (autoUseEnabled)
        {
            shouldUse =
                Time.time >= nextAttackTime;
        }
        else
        {
            shouldUse =
                Mouse.current.leftButton.isPressed &&
                Time.time >= nextAttackTime;
        }

        if (!shouldUse)
        {
            return;
        }

        UseLandingNet(
            mouseWorldPosition
        );

        nextAttackTime =
            Time.time +
            attackCooldown;
    }

    private void UseLandingNet(
        Vector3 mouseWorldPosition)
    {
        Vector2 capturePosition =
            new Vector2(
                mouseWorldPosition.x,
                mouseWorldPosition.y
            );

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                capturePosition,
                captureRadius
            );

        System.Array.Sort(
            hits,
            (a, b) =>
            {
                float distanceA =
                    Vector2.Distance(
                        capturePosition,
                        a.transform.position
                    );

                float distanceB =
                    Vector2.Distance(
                        capturePosition,
                        b.transform.position
                    );

                return distanceA.CompareTo(
                    distanceB
                );
            }
        );

        int hitCount = 0;
        confirmedHits.Clear();
        System.Collections.Generic.HashSet<FishController> damagedFish = new();

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null || !damagedFish.Add(fish))
            {
                continue;
            }

            Vector2 fishPosition =
                fish.transform.position;

            float resistanceBefore = fish.CurrentResistance;
            bool captured =
                fish.TakeCaptureDamage(
                    capturePower,
                    CombatDamageContext.Tool(
                        "landing_net",
                        this)
                );

            if (fish.CurrentResistance < resistanceBefore)
                confirmedHits.Add(new ScoopNetHit(fish, fishPosition));

            if (captured &&
                chainCaptureEnabled)
            {
                ApplyChainCapture(
                    fishPosition,
                    confirmedHits
                );
            }

            hitCount++;

            if (hitCount >= maxTargets)
            {
                break;
            }
        }
        presentation?.ShowUse(capturePosition, confirmedHits);
        confirmedHits.Clear();
    }

    private void ApplyChainCapture(
        Vector2 center,
        List<ScoopNetHit> hitsPresented)
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                center,
                chainRadius
            );

        FishController nearestFish = null;

        float nearestDistance =
            float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null ||
                !fish.gameObject.activeSelf)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    center,
                    fish.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestFish = fish;
                nearestDistance = distance;
            }
        }

        if (nearestFish != null)
        {
            float resistanceBefore = nearestFish.CurrentResistance;
            Vector2 hitPosition = nearestFish.transform.position;
            nearestFish.TakeCaptureDamage(
                chainDamage,
                CombatDamageContext.Tool(
                    "landing_net.chain",
                    this)
            );
            if (nearestFish.CurrentResistance < resistanceBefore)
                hitsPresented.Add(new ScoopNetHit(nearestFish, hitPosition));
        }
    }

    private void OnDisable()
    {
        confirmedHits.Clear();
        presentation?.ResetVisual();
    }

    private void UpdateRangeVisual()
    {
        if (rangeVisual == null)
        {
            return;
        }

        float diameter =
            captureRadius * 2f;

        rangeVisual.localScale =
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

        UpdateRangeVisual();
    }

    public void IncreaseMaxTargets(
        int amount)
    {
        maxTargets += amount;
    }

    public void EnableChainCapture()
    {
        chainCaptureEnabled = true;
    }

    public void EnableLandingNetFisherJob()
    {
        // G4-A transition: Landing Net has no Core/Partner tree. Keep the
        // legacy Job API for the existing UI flow, but do not apply it on top
        // of the new growth system.
    }
}
