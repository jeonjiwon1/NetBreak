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
            attackTimer = attackInterval;
        }

        UpdateTargetLine();
    }

    private void Attack()
    {
        currentTarget = FindBestTarget();

        if (currentTarget == null)
        {
            HideTargetLine();
            return;
        }

        bool captured =
            currentTarget.TakeCaptureDamage(
                capturePower
            );

        if (captured)
        {
            currentTarget = null;
            HideTargetLine();
        }
    }

    private FishController FindBestTarget()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                captureRange
            );

        FishController bestTarget = null;

        int bestCatchValue = -1;
        float bestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null ||
                fish.Data == null)
            {
                continue;
            }

            int catchValue =
                fish.Data.CatchValue;

            float distance =
                Vector2.Distance(
                    transform.position,
                    fish.transform.position
                );

            if (catchValue > bestCatchValue)
            {
                bestTarget = fish;
                bestCatchValue = catchValue;
                bestDistance = distance;
            }
            else if (
                catchValue == bestCatchValue &&
                distance < bestDistance)
            {
                bestTarget = fish;
                bestDistance = distance;
            }
        }

        return bestTarget;
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
}