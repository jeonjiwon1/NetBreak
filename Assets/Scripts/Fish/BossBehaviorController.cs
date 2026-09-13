using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FishController))]
[RequireComponent(typeof(FishMovement))]
public class BossBehaviorController : MonoBehaviour
{
    [Header("Phase 2 - Rush")]
    [SerializeField] private float phase2RushInterval = 5f;
    [SerializeField] private float phase2WindupDuration = 0.7f;
    [SerializeField] private float phase2WindupSpeedMultiplier = 0.65f;
    [SerializeField] private float phase2RushSpeedMultiplier = 1.9f;
    [SerializeField] private float phase2RushDuration = 0.9f;

    [Header("Phase 3 - Frenzy")]
    [SerializeField] private float phase3RushInterval = 3.2f;
    [SerializeField] private float phase3WindupDuration = 0.45f;
    [SerializeField] private float phase3WindupSpeedMultiplier = 0.55f;
    [SerializeField] private float phase3RushSpeedMultiplier = 2.25f;
    [SerializeField] private float phase3RushDuration = 1f;
    [SerializeField] private float phase3LaneOffset = 0.9f;

    [Header("Recovery")]
    [SerializeField] private float recoveryDuration = 0.45f;
    [SerializeField] private float recoverySpeedMultiplier = 0.7f;

    [Header("Telegraph")]
    [SerializeField] private float flashInterval = 0.12f;

    private FishController fishController;
    private FishMovement fishMovement;
    private SpriteRenderer spriteRenderer;

    private Coroutine behaviorCoroutine;

    private Color normalColor;

    private bool isBoss;

    private int currentPhase = 1;

    private bool isTelegraphing;
    private bool isRushing;
    private bool isRecovering;

    public int CurrentPhase =>
        currentPhase;

    public bool IsTelegraphing =>
        isTelegraphing;

    public bool IsRushing =>
        isRushing;

    public bool IsRecovering =>
        isRecovering;

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
        currentPhase = 1;

        isTelegraphing = false;
        isRushing = false;
        isRecovering = false;

        if (fishController == null ||
            fishController.Data == null)
        {
            isBoss = false;
            return;
        }

        isBoss =
            fishController.Data.SpecialType ==
            FishSpecialType.Boss;

        if (!isBoss)
        {
            return;
        }

        if (spriteRenderer != null)
        {
            normalColor =
                spriteRenderer.color;
        }

        behaviorCoroutine =
            StartCoroutine(
                BehaviorLoop()
            );
    }

    public void SetPhase(
        int phase)
    {
        if (!isBoss)
        {
            return;
        }

        int newPhase =
            Mathf.Clamp(
                phase,
                1,
                3
            );

        if (newPhase <= currentPhase)
        {
            return;
        }

        currentPhase =
            newPhase;

        if (currentPhase < 3 &&
            fishMovement != null)
        {
            fishMovement
                .SetRouteLaneOffset(
                    0f
                );
        }
    }

    private IEnumerator BehaviorLoop()
    {
        while (
            gameObject.activeInHierarchy &&
            isBoss)
        {
            if (currentPhase <= 1)
            {
                yield return new WaitForSeconds(
                    0.2f
                );

                continue;
            }

            float interval =
                currentPhase >= 3
                    ? phase3RushInterval
                    : phase2RushInterval;

            yield return new WaitForSeconds(
                interval
            );

            if (!gameObject.activeInHierarchy ||
                !isBoss)
            {
                yield break;
            }

            yield return ExecuteRush();
        }
    }

    private IEnumerator ExecuteRush()
    {
        int phaseAtStart =
            currentPhase;

        float windupDuration =
            phaseAtStart >= 3
                ? phase3WindupDuration
                : phase2WindupDuration;

        float windupMultiplier =
            phaseAtStart >= 3
                ? phase3WindupSpeedMultiplier
                : phase2WindupSpeedMultiplier;

        float rushMultiplier =
            phaseAtStart >= 3
                ? phase3RushSpeedMultiplier
                : phase2RushSpeedMultiplier;

        float rushDuration =
            phaseAtStart >= 3
                ? phase3RushDuration
                : phase2RushDuration;

        // =========================================
        // Phase 3: 회피 기동
        // =========================================

        if (phaseAtStart >= 3 &&
            fishMovement != null)
        {
            float side =
                Random.Range(
                    0,
                    2
                ) == 0
                    ? -1f
                    : 1f;

            fishMovement
                .SetRouteLaneOffset(
                    side *
                    phase3LaneOffset
                );
        }

        // =========================================
        // Telegraph
        // =========================================

        isTelegraphing = true;
        isRushing = false;
        isRecovering = false;

        fishMovement
            .SetSpecialSpeedMultiplier(
                windupMultiplier
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

            timer +=
                waitTime;
        }

        RestoreColor();

        isTelegraphing = false;

        // =========================================
        // Rush
        // =========================================

        isRushing = true;

        fishMovement
            .SetSpecialSpeedMultiplier(
                rushMultiplier
            );

        yield return new WaitForSeconds(
            rushDuration
        );

        isRushing = false;

        // =========================================
        // Recovery
        // =========================================

        isRecovering = true;

        fishMovement
            .SetSpecialSpeedMultiplier(
                recoverySpeedMultiplier
            );

        yield return new WaitForSeconds(
            recoveryDuration
        );

        isRecovering = false;

        fishMovement
            .SetSpecialSpeedMultiplier(
                1f
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

    private void OnDisable()
    {
        if (behaviorCoroutine != null)
        {
            StopCoroutine(
                behaviorCoroutine
            );

            behaviorCoroutine = null;
        }

        if (fishMovement != null)
        {
            fishMovement
                .SetSpecialSpeedMultiplier(
                    1f
                );

            fishMovement
                .SetRouteLaneOffset(
                    0f
                );
        }

        RestoreColor();

        currentPhase = 1;

        isBoss = false;

        isTelegraphing = false;
        isRushing = false;
        isRecovering = false;
    }
}