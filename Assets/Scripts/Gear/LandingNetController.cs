using UnityEngine;
using UnityEngine.InputSystem;

public class LandingNetController : MonoBehaviour
{
    [SerializeField] private float capturePower = 5f;
    [SerializeField] private float captureRadius = 1.2f;
    [SerializeField] private float attackCooldown = 0.3f;

    [SerializeField] private Transform rangeVisual;

    [SerializeField] private int maxTargets = 3;

    private Camera mainCamera;
    private float nextAttackTime;

    private void Awake()
    {
        mainCamera = Camera.main;
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
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition.z = 0f;

        if (rangeVisual != null)
        {
            rangeVisual.position = mouseWorldPosition;
        }

        if (!NetPlacementController.IsNetModeActive &&
             Mouse.current.leftButton.wasPressedThisFrame &&
             Time.time >= nextAttackTime)
        {
            UseLandingNet(mouseWorldPosition);

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UseLandingNet(Vector3 mouseWorldPosition)
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

                return distanceA.CompareTo(distanceB);
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

            fish.TakeCaptureDamage(capturePower);

            hitCount++;

            if (hitCount >= maxTargets)
            {
                break;
            }
        }
    }

    public void IncreaseCapturePower(float amount)
    {
        capturePower += amount;
    }

    public void IncreaseCaptureRadius(float amount)
    {
        captureRadius += amount;

        if (rangeVisual != null)
        {
            float diameter = captureRadius * 2f;

            rangeVisual.localScale =
                new Vector3(diameter, diameter, 1f);
        }
    }
}