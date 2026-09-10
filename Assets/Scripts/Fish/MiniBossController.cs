using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FishController))]
[RequireComponent(typeof(FishMovement))]
public class MiniBossController : MonoBehaviour
{
    private FishController fishController;
    private FishMovement fishMovement;

    private float dashTimer;

    private bool isDashing;

    private Coroutine dashCoroutine;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;

    private void Awake()
    {
        fishController =
            GetComponent<FishController>();

        fishMovement =
            GetComponent<FishMovement>();

        spriteRenderer =
            GetComponentInChildren<
                SpriteRenderer
            >();
    }

    private void OnEnable()
    {
        isDashing = false;

        if (fishMovement != null)
        {
            fishMovement
                .SetSpecialSpeedMultiplier(
                    1f
                );
        }

        if (IsMiniBoss())
        {
            dashTimer =
                fishController.Data
                    .FirstDashDelay;
        }
        else
        {
            dashTimer = 0f;
        }
    }

    private void Update()
    {
        if (!IsMiniBoss())
        {
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            flow.IsPreparation ||
            flow.IsGameEnded)
        {
            return;
        }

        if (isDashing)
        {
            return;
        }

        dashTimer -=
            Time.deltaTime;

        if (dashTimer > 0f)
        {
            return;
        }

        StartDash();
    }

    private bool IsMiniBoss()
    {
        return
            fishController != null
            &&
            fishController.Data != null
            &&
            fishController.Data.SpecialType ==
            FishSpecialType.MiniBoss;
    }

    private void StartDash()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(
                dashCoroutine
            );
        }

        dashCoroutine =
            StartCoroutine(
                DashRoutine()
            );
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        FishData data =
            fishController.Data;

        if (spriteRenderer != null)
        {
            normalColor =
                spriteRenderer.color;

            spriteRenderer.color =
                Color.Lerp(
                    normalColor,
                    Color.white,
                    0.5f
                );
        }

        fishMovement
            .SetSpecialSpeedMultiplier(
                data.DashSpeedMultiplier
            );

        yield return new WaitForSeconds(
            data.DashDuration
        );

        fishMovement
            .SetSpecialSpeedMultiplier(
                1f
            );

        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                normalColor;
        }

        isDashing = false;

        dashTimer =
            data.DashInterval;

        dashCoroutine = null;
    }

    private void OnDisable()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(
                dashCoroutine
            );

            dashCoroutine = null;
        }

        isDashing = false;

        if (fishMovement != null)
        {
            fishMovement
                .SetSpecialSpeedMultiplier(
                    1f
                );
        }

        if (spriteRenderer != null &&
            IsMiniBoss())
        {
            spriteRenderer.color =
                normalColor;
        }
    }
}