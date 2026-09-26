using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class NetController : MonoBehaviour
{
    [Header("Net Effect")]
    [SerializeField] private float slowMultiplier = 0.35f;
    [SerializeField] private float captureDamagePerSecond = 3f;
    [SerializeField] private float pufferfishDisableDuration = 3f;

    private BoxCollider2D netCollider;
    private Rigidbody2D rigidBody;

    private readonly HashSet<FishMovement>
        contactedFish = new();

    private float currentDamageMultiplier = 1f;
    private float tacticalDamageMultiplier = 1f;
    private float tacticalSlowStrengthMultiplier = 1f;

    private bool isBeingRepositioned;
    private float repositionBlockedUntil;
    private float specialDisabledUntil;

    private bool isOperational = true;
    private PufferfishDisruptionProfile pufferfishPresentation;
    private NetPresentationProfile netPresentation;
    public event Action<FishController, Vector2> FishContactPresented;
    public event Action<FishController, Vector2> PufferfishPresentationTriggered;

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

        pufferfishPresentation = Resources.Load<PufferfishDisruptionProfile>(
            "PufferfishDisruption");
        netPresentation = Resources.Load<NetPresentationProfile>("NetPresentation");

        netRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (netRenderer != null)
        {
            normalNetColor =
                netRenderer.color;
        }

        NetPresentation visual = GetComponent<NetPresentation>();
        if (visual == null) visual = gameObject.AddComponent<NetPresentation>();
        visual.Configure(false, netPresentation);

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

        if (TryTriggerPufferfishDisruption(other))
        {
            return;
        }

        RegisterFish(other);
        RecalculateNetDisruption();
    }

    private bool TryTriggerPufferfishDisruption(
        Collider2D other)
    {
        FishController fish =
            other.GetComponent<FishController>();

        if (fish == null ||
            fish.Data == null ||
            fish.Data.SpecialType != FishSpecialType.Pufferfish)
        {
            return false;
        }

        Vector2 contactPoint = netCollider != null
            ? netCollider.ClosestPoint(other.bounds.center)
            : ((Vector2)transform.position + (Vector2)fish.transform.position) * 0.5f;

        DisableTemporarily(
            pufferfishDisableDuration
        );

        // The net has already stopped; every presentation path is optional.
        PufferfishPresentationTriggered?.Invoke(fish, contactPoint);
        fish.GetComponent<FishVisualController>()?.PlaySpecial(pufferfishPresentation);
        ItemEffectManager effects = ItemEffectManager.Instance;
        effects?.ShowPufferfishNetImpact(contactPoint, pufferfishPresentation);
        effects?.PlayPufferfishNetSound(pufferfishPresentation);

        return true;
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!isOperational)
        {
            return;
        }

        if (TryTriggerPufferfishDisruption(other))
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
                * tacticalDamageMultiplier
                * Time.fixedDeltaTime,
                CombatDamageContext.Tool(
                    "net.damage_over_time",
                    this,
                    true)
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
            GetEffectiveSlowMultiplier()
        );

        FishController fish = other.GetComponent<FishController>();
        if (fish != null && fish.Data != null)
        {
            Vector2 position = fish.transform.position;
            FishContactPresented?.Invoke(fish, position);
            ItemEffectManager.Instance?.ShowNetContact(position, netPresentation);
        }
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

    public void SetTacticalEffectiveness(
        float damageMultiplier,
        float slowStrengthMultiplier,
        bool reactivate)
    {
        tacticalDamageMultiplier = Mathf.Max(1f, damageMultiplier);
        tacticalSlowStrengthMultiplier = Mathf.Max(1f, slowStrengthMultiplier);

        if (reactivate && !isBeingRepositioned)
        {
            specialDisabledUntil = Time.time;
            RefreshOperationalState();
        }

        foreach (FishMovement movement in contactedFish)
        {
            if (movement != null)
            {
                movement.EnterNet(this, GetEffectiveSlowMultiplier());
            }
        }
    }

    private float GetEffectiveSlowMultiplier() =>
        Mathf.Clamp01(slowMultiplier / tacticalSlowStrengthMultiplier);

    private void OnDisable()
    {
        ReleaseAllFish();
        PufferfishPresentationTriggered = null;
        FishContactPresented = null;
    }
}
