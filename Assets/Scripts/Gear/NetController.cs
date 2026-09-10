using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetController : MonoBehaviour
{
    [Header("Net Effect")]
    [SerializeField] private float slowMultiplier = 0.35f;
    [SerializeField] private float captureDamagePerSecond = 3f;

    private BoxCollider2D netCollider;
    private Rigidbody2D rigidBody;

    private readonly HashSet<FishMovement>
        contactedFish = new();

    private float currentDamageMultiplier = 1f;

    private bool isBeingRepositioned;
    private float repositionBlockedUntil;
    private float specialDisabledUntil;

    private bool isOperational = true;

    private SpriteRenderer netRenderer;
    private Color normalNetColor;

    public bool IsOperational =>
        isOperational;

    public float CurrentDamageMultiplier =>
        currentDamageMultiplier;

    private void Awake()
    {
        netCollider =
            GetComponent<BoxCollider2D>();

        rigidBody =
            GetComponent<Rigidbody2D>();

        netCollider.isTrigger = true;

        rigidBody.bodyType =
            RigidbodyType2D.Kinematic;

        rigidBody.gravityScale = 0f;

        netRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (netRenderer != null)
        {
            normalNetColor =
                netRenderer.color;
        }

        RefreshOperationalState();
    }

    private void Update()
    {
        RefreshOperationalState();
    }

    public void Initialize(
        Vector2 start,
        Vector2 end,
        float thickness)
    {
        Vector2 direction =
            end - start;

        float length =
            direction.magnitude;

        Vector2 center =
            (start + end) * 0.5f;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.position =
            center;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        transform.localScale =
            new Vector3(
                length,
                thickness,
                1f
            );
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!isOperational)
        {
            return;
        }

        RegisterFish(other);
        RecalculateNetDisruption();
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!isOperational)
        {
            return;
        }

        RegisterFish(other);

        RecalculateNetDisruption();

        FishController fish =
            other.GetComponent<FishController>();

        if (fish != null)
        {
            fish.TakeCaptureDamage(
                captureDamagePerSecond
                * currentDamageMultiplier
                * Time.fixedDeltaTime
            );
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        FishMovement movement =
            other.GetComponent<FishMovement>();

        if (movement == null)
        {
            return;
        }

        if (!contactedFish.Remove(
            movement))
        {
            return;
        }

        movement.ExitNet(this);

        RecalculateNetDisruption();
    }

    private void RegisterFish(
        Collider2D other)
    {
        FishMovement movement =
            other.GetComponent<FishMovement>();

        if (movement == null)
        {
            return;
        }

        if (!contactedFish.Add(
            movement))
        {
            return;
        }

        movement.EnterNet(
            this,
            slowMultiplier
        );
    }

    private void RecalculateNetDisruption()
    {
        float newMultiplier = 1f;

        List<FishMovement> inactiveFish =
            null;

        foreach (FishMovement movement
                 in contactedFish)
        {
            if (movement == null ||
                !movement.gameObject.activeInHierarchy)
            {
                inactiveFish ??=
                    new List<FishMovement>();

                inactiveFish.Add(
                    movement
                );

                continue;
            }

            FishController fish =
                movement.GetComponent<
                    FishController
                >();

            if (fish == null ||
                fish.Data == null)
            {
                continue;
            }

            if (fish.Data.SpecialType !=
                FishSpecialType.Pufferfish)
            {
                continue;
            }

            newMultiplier =
                Mathf.Min(
                    newMultiplier,
                    fish.Data
                        .NetDisruptionMultiplier
                );
        }

        if (inactiveFish != null)
        {
            foreach (FishMovement movement
                     in inactiveFish)
            {
                contactedFish.Remove(
                    movement
                );
            }
        }

        currentDamageMultiplier =
            newMultiplier;

        UpdateVisualState();
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
                Mathf.Max(0f, delay)
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
                Time.time + duration
            );

        RefreshOperationalState();
    }

    private void RefreshOperationalState()
    {
        bool shouldOperate =
            !isBeingRepositioned
            &&
            Time.time >= repositionBlockedUntil
            &&
            Time.time >= specialDisabledUntil;

        if (shouldOperate ==
            isOperational)
        {
            UpdateVisualState();
            return;
        }

        SetOperational(
            shouldOperate
        );
    }

    private void SetOperational(
        bool operational)
    {
        isOperational =
            operational;

        if (!isOperational)
        {
            ReleaseAllFish();

            if (netCollider != null)
            {
                netCollider.enabled =
                    false;
            }

            UpdateVisualState();
            return;
        }

        if (netCollider != null)
        {
            netCollider.enabled =
                true;
        }

        Physics2D.SyncTransforms();

        UpdateVisualState();
    }

    private void ReleaseAllFish()
    {
        foreach (FishMovement movement
                 in contactedFish)
        {
            if (movement != null)
            {
                movement.ExitNet(this);
            }
        }

        contactedFish.Clear();

        currentDamageMultiplier = 1f;
    }

    private void UpdateVisualState()
    {
        if (netRenderer == null)
        {
            return;
        }

        if (!isOperational)
        {
            netRenderer.color =
                Color.Lerp(
                    normalNetColor,
                    Color.black,
                    0.65f
                );

            return;
        }

        if (currentDamageMultiplier <
            0.999f)
        {
            netRenderer.color =
                Color.Lerp(
                    normalNetColor,
                    Color.red,
                    0.55f
                );

            return;
        }

        netRenderer.color =
            normalNetColor;
    }

    public void MultiplyCaptureDamage(
        float multiplier)
    {
        captureDamagePerSecond *=
            Mathf.Max(
                0f,
                multiplier
            );
    }

    private void OnDisable()
    {
        ReleaseAllFish();
    }
}