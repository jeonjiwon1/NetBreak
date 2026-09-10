using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastNetController : MonoBehaviour
{
    [Header("Cast Net")]
    [SerializeField] private float capturePower = 15f;
    [SerializeField] private float captureRadius = 1f;
    [SerializeField] private float cooldown = 7f;

    [Header("Mass Catch Refund")]
    [SerializeField] private int refundThreshold = 8;
    [SerializeField] private float refundAmount = 2f;

    [Header("Visual")]
    [SerializeField] private Transform castVisual;
    [SerializeField] private float visualDuration = 0.2f;

    private Camera mainCamera;

    private int maxCharges = 1;
    private int currentCharges = 1;
    private float rechargeTimer;

    private bool isAiming;

    private Vector2 currentAimPosition;
    private int currentTargetCount;

    private int lastCapturedCount;
    private Vector2 lastCastPosition;
    private float catchFeedbackTimer;

    private bool massCatchRefundEnabled;

    public bool IsAiming => isAiming;

    public bool IsReady =>
        currentCharges > 0;

    public float CooldownTimer =>
        Mathf.Max(0f, rechargeTimer);

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
        mainCamera = Camera.main;

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);

            UpdateVisualScale();
        }
    }

    private void Update()
    {
        UpdateRecharge();

        if (catchFeedbackTimer > 0f)
        {
            catchFeedbackTimer -=
                Time.deltaTime;
        }

        if (Mouse.current == null ||
            Keyboard.current == null)
        {
            return;
        }

        bool inputBlocked =
            (PrototypeAugmentManager.Instance != null &&
             PrototypeAugmentManager.Instance.IsChoosingAugment)
            ||
            (PrototypeGameFlowManager.Instance != null &&
             PrototypeGameFlowManager.Instance.IsPreparation)
            ||
            (PrototypeGameFlowManager.Instance != null &&
             PrototypeGameFlowManager.Instance.IsGameEnded)
            ||
            FishingRodPlacementController.IsRodModeActive
            ||
            NetPlacementController.IsNetModeActive;

        if (inputBlocked)
        {
            CancelAiming();
            return;
        }

        HandleCastNetInput();
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

    private void HandleCastNetInput()
    {
        if (!isAiming &&
            IsReady &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartAiming();
        }

        if (!isAiming)
        {
            return;
        }

        UpdateAimPosition();

        bool cancelPressed =
            Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame;

        if (cancelPressed)
        {
            CancelAiming();
            return;
        }

        if (Keyboard.current.eKey.wasReleasedThisFrame)
        {
            UseCastNet(
                currentAimPosition
            );

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

        UpdateTargetCount();
    }

    private void UpdateTargetCount()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                currentAimPosition,
                captureRadius
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

    private void UseCastNet(
        Vector2 castPosition)
    {
        if (currentCharges <= 0)
        {
            return;
        }

        bool wasFull =
            currentCharges == maxCharges;

        currentCharges--;

        if (wasFull)
        {
            rechargeTimer = cooldown;
        }

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
                fish.TakeCaptureDamage(
                    capturePower
                );

            if (captured)
            {
                capturedCount++;
            }
        }

        if (massCatchRefundEnabled &&
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

        if (castVisual != null)
        {
            StartCoroutine(
                ShowCastEffect(
                    castPosition
                )
            );
        }
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

    private IEnumerator ShowCastEffect(
        Vector2 position)
    {
        castVisual.position =
            position;

        castVisual.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            visualDuration
        );

        castVisual.gameObject.SetActive(false);
    }

    private void CancelAiming()
    {
        isAiming = false;
        currentTargetCount = 0;

        if (castVisual != null)
        {
            castVisual.gameObject.SetActive(false);
        }
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

    public void EnableCastNetFisherJob()
    {
        if (maxCharges >= 3)
        {
            return;
        }

        maxCharges = 3;
        currentCharges = 3;
        rechargeTimer = 0f;
    }
}