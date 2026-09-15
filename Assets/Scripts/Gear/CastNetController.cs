using System.Collections;
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
    [SerializeField] private float visualDuration = 0.2f;

    private Camera mainCamera;

    private int currentCharges;
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

        ToolInputState input = ToolSlotInput.Read(ToolId.CastNet);
        if (input.Cancelled)
        {
            CancelAiming();
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
