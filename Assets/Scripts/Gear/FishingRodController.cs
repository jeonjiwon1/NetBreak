using System.Collections.Generic;
using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    [Header("Fishing Rod")]
    [SerializeField] private float capturePower = 4f;
    [SerializeField] private float attackInterval = 0.75f;
    [SerializeField] private float captureRange = 3f;

    [Header("Visual")]
    [SerializeField] private Transform rangeVisual;
    [SerializeField] private LineRenderer targetLine;

    private float attackTimer;

    private FishController currentTarget;

    private int additionalTargets;

    public float CapturePower => capturePower;
    public float AttackInterval => attackInterval;
    public float CaptureRange => captureRange;

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
    }

    private void Update()
    {
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

        attackTimer -= Time.deltaTime;

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

        currentTarget = targets[0];

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

    private List<FishController> FindBestTargets(
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

            if (!candidates.Contains(fish))
            {
                candidates.Add(fish);
            }
        }

        candidates.Sort(
            (a, b) =>
            {
                int valueComparison =
                    b.Data.CatchValue.CompareTo(
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
            targetLine.enabled = false;
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

        rangeVisual.localScale =
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

    public void ReduceAttackInterval(float amount)
    {
        attackInterval =
            Mathf.Max(
                0.1f,
                attackInterval - amount
            );
    }

    public void IncreaseCaptureRange(float amount)
    {
        captureRange += amount;

        UpdateRangeVisual();
    }

    public void AddAdditionalTarget(int amount)
    {
        additionalTargets += amount;
    }
}