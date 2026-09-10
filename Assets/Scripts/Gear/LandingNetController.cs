using UnityEngine;
using UnityEngine.InputSystem;

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

    private bool chainCaptureEnabled;
    private bool autoUseEnabled;

    private void Awake()
    {
        mainCamera = Camera.main;

        UpdateRangeVisual();
    }

    private void Update()
    {
        if (PrototypeAugmentManager.Instance != null &&
            PrototypeAugmentManager.Instance.IsChoosingAugment)
        {
            return;
        }

        if (PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsGameEnded)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsPreparation)
        {
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

        if (NetPlacementController.IsNetModeActive ||
            FishingRodPlacementController.IsRodModeActive ||
            GearRepositionController.IsRepositioning ||
            GearRepositionController.IsRepositionModifierHeld)
        {
            return;
        }

        bool shouldUse;

        if (autoUseEnabled)
        {
            shouldUse =
                Time.time >= nextAttackTime;
        }
        else
        {
            shouldUse =
                Mouse.current.leftButton.wasPressedThisFrame &&
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

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null)
            {
                continue;
            }

            Vector2 fishPosition =
                fish.transform.position;

            bool captured =
                fish.TakeCaptureDamage(
                    capturePower
                );

            if (captured &&
                chainCaptureEnabled)
            {
                ApplyChainCapture(
                    fishPosition
                );
            }

            hitCount++;

            if (hitCount >= maxTargets)
            {
                break;
            }
        }
    }

    private void ApplyChainCapture(
        Vector2 center)
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
            nearestFish.TakeCaptureDamage(
                chainDamage
            );
        }
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
        captureRadius *= 2f;

        maxTargets =
            Mathf.Max(
                maxTargets,
                8
            );

        autoUseEnabled = true;

        nextAttackTime =
            Time.time;

        UpdateRangeVisual();
    }
}