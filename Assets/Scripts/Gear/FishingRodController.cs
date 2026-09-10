using System.Collections.Generic;
using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    [Header("Fishing Rod")]
    [SerializeField] private float capturePower = 4f;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float captureRange = 3f;

    [Header("Visual")]
    [SerializeField] private Transform rangeVisual;
    [SerializeField] private LineRenderer targetLine;

    private float attackTimer;

    private FishController currentTarget;

    private int additionalTargets;

    private bool isBeingRepositioned;
    private float repositionBlockedUntil;
    private float specialDisabledUntil;

    private bool isOperational = true;

    private SpriteRenderer rodRenderer;
    private Color normalRodColor;

    public float CapturePower =>
        capturePower;

    public float AttackInterval =>
        attackInterval;

    public float CaptureRange =>
        captureRange;

    public bool IsOperational =>
        isOperational;

    private void Awake()
    {
        UpdateRangeVisual();

        if (targetLine != null)
        {
            targetLine.positionCount = 2;

            targetLine.startWidth = 0.04f;
            targetLine.endWidth = 0.04f;

            targetLine.enabled = false;
        }

        rodRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (rodRenderer != null)
        {
            normalRodColor =
                rodRenderer.color;
        }

        RefreshOperationalState();
    }

    private void Update()
    {
        RefreshOperationalState();

        if (!isOperational)
        {
            currentTarget = null;
            HideTargetLine();
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            flow.IsPreparation ||
            flow.IsGameEnded)
        {
            currentTarget = null;
            HideTargetLine();
            return;
        }

        attackTimer -=
            Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();

            attackTimer =
                attackInterval;
        }

        UpdateTargetLine();
    }

    private void Attack()
    {
        int targetCount =
            1 + additionalTargets;

        List<FishController> targets =
            FindBestTargets(
                targetCount
            );

        if (targets.Count == 0)
        {
            currentTarget = null;

            HideTargetLine();
            return;
        }

        currentTarget =
            targets[0];

        foreach (FishController target
                 in targets)
        {
            if (target == null)
            {
                continue;
            }

            target.TakeCaptureDamage(
                capturePower
            );
        }

        if (currentTarget == null ||
            !currentTarget.gameObject.activeSelf)
        {
            currentTarget = null;

            HideTargetLine();
        }
    }

    private List<FishController>
        FindBestTargets(
            int maxTargets)
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                captureRange
            );

        List<FishController> candidates =
            new List<FishController>();

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null ||
                fish.Data == null)
            {
                continue;
            }

            // Collider 가장자리만 범위에 걸친 경우를 제외한다.
            // 물고기 중심점이 실제 낚싯대 범위 안에 있어야 한다.
            float centerDistance =
                Vector2.Distance(
                    transform.position,
                    fish.transform.position
                );

            if (centerDistance >
                captureRange)
            {
                continue;
            }

            if (!candidates.Contains(
                fish))
            {
                candidates.Add(
                    fish
                );
            }
        }

        candidates.Sort(
            (a, b) =>
            {
                int valueComparison =
                    b.Data.CatchValue
                        .CompareTo(
                            a.Data.CatchValue
                        );

                if (valueComparison != 0)
                {
                    return valueComparison;
                }

                float distanceA =
                    Vector2.Distance(
                        transform.position,
                        a.transform.position
                    );

                float distanceB =
                    Vector2.Distance(
                        transform.position,
                        b.transform.position
                    );

                return distanceA.CompareTo(
                    distanceB
                );
            }
        );

        if (candidates.Count >
            maxTargets)
        {
            candidates.RemoveRange(
                maxTargets,
                candidates.Count -
                maxTargets
            );
        }

        return candidates;
    }

    private void UpdateTargetLine()
    {
        if (targetLine == null)
        {
            return;
        }

        if (currentTarget == null ||
            !currentTarget.gameObject.activeSelf)
        {
            HideTargetLine();
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                currentTarget.transform.position
            );

        if (distance > captureRange)
        {
            currentTarget = null;

            HideTargetLine();
            return;
        }

        targetLine.enabled = true;

        targetLine.SetPosition(
            0,
            transform.position
        );

        targetLine.SetPosition(
            1,
            currentTarget.transform.position
        );
    }

    private void HideTargetLine()
    {
        if (targetLine != null)
        {
            targetLine.enabled =
                false;
        }
    }

    private void UpdateRangeVisual()
    {
        if (rangeVisual == null)
        {
            return;
        }

        float diameter =
            captureRange * 2f;

        Transform parent =
            rangeVisual.parent;

        if (parent == null)
        {
            rangeVisual.localScale =
                new Vector3(
                    diameter,
                    diameter,
                    1f
                );

            return;
        }

        Vector3 parentScale =
            parent.lossyScale;

        float scaleX =
            Mathf.Abs(parentScale.x) >
            0.0001f
                ? diameter /
                  Mathf.Abs(parentScale.x)
                : diameter;

        float scaleY =
            Mathf.Abs(parentScale.y) >
            0.0001f
                ? diameter /
                  Mathf.Abs(parentScale.y)
                : diameter;

        rangeVisual.localScale =
            new Vector3(
                scaleX,
                scaleY,
                1f
            );
    }

    public void BeginReposition()
    {
        isBeingRepositioned = true;

        RefreshOperationalState();
    }

    public void EndReposition(
        float delay)
    {
        isBeingRepositioned = false;

        repositionBlockedUntil =
            Mathf.Max(
                repositionBlockedUntil,
                Time.time +
                Mathf.Max(
                    0f,
                    delay
                )
            );

        RefreshOperationalState();
    }

    public void DisableTemporarily(
        float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        specialDisabledUntil =
            Mathf.Max(
                specialDisabledUntil,
                Time.time +
                duration
            );

        RefreshOperationalState();
    }

    private void RefreshOperationalState()
    {
        bool shouldOperate =
            !isBeingRepositioned
            &&
            Time.time >=
            repositionBlockedUntil
            &&
            Time.time >=
            specialDisabledUntil;

        if (shouldOperate ==
            isOperational)
        {
            UpdateVisualState();
            return;
        }

        isOperational =
            shouldOperate;

        if (!isOperational)
        {
            currentTarget = null;

            HideTargetLine();
        }
        else
        {
            attackTimer = 0f;
        }

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (rodRenderer == null)
        {
            return;
        }

        if (!isOperational)
        {
            rodRenderer.color =
                Color.Lerp(
                    normalRodColor,
                    Color.black,
                    0.65f
                );
        }
        else
        {
            rodRenderer.color =
                normalRodColor;
        }
    }

    public void IncreaseCapturePower(
        float amount)
    {
        capturePower +=
            amount;
    }

    public void ReduceAttackInterval(
        float amount)
    {
        attackInterval =
            Mathf.Max(
                0.1f,
                attackInterval -
                amount
            );
    }

    public void IncreaseCaptureRange(
        float amount)
    {
        captureRange +=
            amount;

        UpdateRangeVisual();
    }

    public void AddAdditionalTarget(
        int amount)
    {
        additionalTargets +=
            amount;
    }
}