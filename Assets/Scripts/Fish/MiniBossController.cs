using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FishController))]
[RequireComponent(typeof(FishMovement))]
public class MiniBossController : MonoBehaviour
{
    public static MiniBossController ActiveMiniBoss
    {
        get;
        private set;
    }

    public static event Action<
        string,
        int,
        int
    > MiniBossCaptured;

    [Header("Reward")]
    [SerializeField] private int bonusGold = 25;
    [SerializeField] private int bonusExp = 4;

    [Header("Dash Telegraph")]
    [SerializeField] private float windupDuration = 0.65f;
    [SerializeField] private float windupSpeedMultiplier = 0.6f;
    [SerializeField] private float flashInterval = 0.12f;

    [Header("Dash Recovery")]
    [SerializeField] private float recoveryDuration = 0.4f;
    [SerializeField] private float recoverySpeedMultiplier = 0.65f;

    private FishController fishController;
    private FishMovement fishMovement;
    private SpriteRenderer spriteRenderer;

    private Coroutine dashCoroutine;

    private Color normalColor;

    private bool isMiniBoss;
    private bool rewardGranted;
    private bool isTelegraphing;
    private bool isDashing;

    public FishController Fish =>
        fishController;

    public bool IsTelegraphing =>
        isTelegraphing;

    public bool IsDashing =>
        isDashing;

    public float CurrentResistance
    {
        get
        {
            if (fishController == null)
            {
                return 0f;
            }

            return fishController
                .CurrentResistance;
        }
    }

    public float MaxResistance
    {
        get
        {
            if (fishController == null ||
                fishController.Data == null)
            {
                return 0f;
            }

            return fishController
                .Data
                .MaxResistance;
        }
    }

    public float ResistanceRatio
    {
        get
        {
            if (fishController == null)
            {
                return 0f;
            }

            return fishController
                .ResistanceRatio;
        }
    }

    public string DisplayName
    {
        get
        {
            if (fishController == null ||
                fishController.Data == null)
            {
                return "대형 개체";
            }

            return fishController
                .Data
                .FishName;
        }
    }

    private void Awake()
    {
        fishController =
            GetComponent<FishController>();

        fishMovement =
            GetComponent<FishMovement>();

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        rewardGranted = false;
        isTelegraphing = false;
        isDashing = false;

        if (fishController == null ||
            fishController.Data == null)
        {
            isMiniBoss = false;
            return;
        }

        isMiniBoss =
            fishController.Data.SpecialType ==
            FishSpecialType.MiniBoss;

        if (!isMiniBoss)
        {
            return;
        }

        ActiveMiniBoss = this;

        if (spriteRenderer != null)
        {
            normalColor =
                spriteRenderer.color;
        }

        fishController.Captured +=
            HandleCaptured;

        dashCoroutine =
            StartCoroutine(
                DashLoop()
            );
    }

    private void OnDisable()
    {
        if (fishController != null)
        {
            fishController.Captured -=
                HandleCaptured;
        }

        if (dashCoroutine != null)
        {
            StopCoroutine(
                dashCoroutine
            );

            dashCoroutine = null;
        }

        if (fishMovement != null)
        {
            fishMovement
                .SetSpecialSpeedMultiplier(
                    1f
                );
        }

        RestoreColor();

        isMiniBoss = false;
        isTelegraphing = false;
        isDashing = false;

        if (ActiveMiniBoss == this)
        {
            ActiveMiniBoss = null;
        }
    }

    private IEnumerator DashLoop()
    {
        float firstDelay =
            Mathf.Max(
                0f,
                fishController
                    .Data
                    .FirstDashDelay
            );

        if (firstDelay > 0f)
        {
            yield return new WaitForSeconds(
                firstDelay
            );
        }

        while (
            gameObject.activeInHierarchy &&
            isMiniBoss)
        {
            yield return TelegraphAndDash();

            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            float interval =
                Mathf.Max(
                    0.1f,
                    fishController
                        .Data
                        .DashInterval
                );

            yield return new WaitForSeconds(
                interval
            );
        }
    }

    private IEnumerator TelegraphAndDash()
    {
        // -------------------------
        // 1. 돌진 예고
        // -------------------------

        isTelegraphing = true;
        isDashing = false;

        fishMovement
            .SetSpecialSpeedMultiplier(
                windupSpeedMultiplier
            );

        float timer = 0f;
        bool bright = false;

        while (timer < windupDuration)
        {
            if (!gameObject.activeInHierarchy)
            {
                yield break;
            }

            bright = !bright;

            if (spriteRenderer != null)
            {
                spriteRenderer.color =
                    bright
                        ? Color.white
                        : normalColor;
            }

            float waitTime =
                Mathf.Min(
                    flashInterval,
                    windupDuration - timer
                );

            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(
                    waitTime
                );
            }

            timer += waitTime;
        }

        RestoreColor();

        isTelegraphing = false;

        // -------------------------
        // 2. 돌진
        // -------------------------

        isDashing = true;

        fishMovement
            .SetSpecialSpeedMultiplier(
                fishController
                    .Data
                    .DashSpeedMultiplier
            );

        yield return new WaitForSeconds(
            Mathf.Max(
                0.05f,
                fishController
                    .Data
                    .DashDuration
            )
        );

        isDashing = false;

        // -------------------------
        // 3. 돌진 후 빈틈
        // -------------------------

        fishMovement
            .SetSpecialSpeedMultiplier(
                recoverySpeedMultiplier
            );

        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                recoveryDuration
            )
        );

        fishMovement
            .SetSpecialSpeedMultiplier(
                1f
            );
    }

    private void HandleCaptured(
        FishController capturedFish)
    {
        if (!isMiniBoss ||
            rewardGranted)
        {
            return;
        }

        rewardGranted = true;

        if (RunManager.Instance != null)
        {
            RunManager.Instance
                .GrantBonusReward(
                    bonusGold,
                    bonusExp
                );
        }

        MiniBossCaptured?.Invoke(
            DisplayName,
            bonusGold,
            bonusExp
        );
    }

    private void RestoreColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                normalColor;
        }
    }
}