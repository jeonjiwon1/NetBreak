using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastNetController : MonoBehaviour
{
    [Header("Cast Net")]
    [SerializeField] private float capturePower = 20f;
    [SerializeField] private float captureRadius = 2.5f;
    [SerializeField] private float cooldown = 5f;

    [Header("Visual")]
    [SerializeField] private Transform castVisual;
    [SerializeField] private float visualDuration = 0.2f;

    private Camera mainCamera;
    private float cooldownTimer;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);
            UpdateVisualScale();
        }
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

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (Mouse.current == null ||
            Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame &&
            cooldownTimer <= 0f &&
            !NetPlacementController.IsNetModeActive)
        {
            UseCastNet();
        }
    }

    private void UseCastNet()
    {
        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        Vector2 castPosition =
            new Vector2(
                worldPosition.x,
                worldPosition.y
            );

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                castPosition,
                captureRadius
            );

        int capturedCount = 0;

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null)
            {
                continue;
            }

            bool captured =
                fish.TakeCaptureDamage(capturePower);

            if (captured)
            {
                capturedCount++;
            }
        }

        cooldownTimer = cooldown;

        Debug.Log($"+{capturedCount} CATCH!");

        if (castVisual != null)
        {
            StartCoroutine(
                ShowCastVisual(castPosition)
            );
        }
    }

    private IEnumerator ShowCastVisual(Vector2 position)
    {
        castVisual.position = position;

        castVisual.gameObject.SetActive(true);

        yield return new WaitForSeconds(visualDuration);

        castVisual.gameObject.SetActive(false);
    }

    private void UpdateVisualScale()
    {
        float diameter = captureRadius * 2f;

        castVisual.localScale =
            new Vector3(
                diameter,
                diameter,
                1f
            );
    }

    public void IncreaseCapturePower(float amount)
    {
        capturePower += amount;
    }

    public void IncreaseCaptureRadius(float amount)
    {
        captureRadius += amount;

        UpdateVisualScale();
    }

    public void ReduceCooldown(float amount)
    {
        cooldown =
            Mathf.Max(0.5f, cooldown - amount);
    }
}