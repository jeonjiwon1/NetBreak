using System.Collections;
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

    private bool isOperational = true;

    private Coroutine reactivationCoroutine;

    private readonly HashSet<FishMovement>
        contactedFish = new();

    public bool IsOperational =>
        isOperational;

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
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!isOperational)
        {
            return;
        }

        RegisterFish(other);

        FishController fish =
            other.GetComponent<FishController>();

        if (fish != null)
        {
            fish.TakeCaptureDamage(
                captureDamagePerSecond
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

    private void ReleaseAllFish()
    {
        foreach (
            FishMovement movement
            in contactedFish)
        {
            if (movement != null)
            {
                movement.ExitNet(this);
            }
        }

        contactedFish.Clear();
    }

    public void BeginReposition()
    {
        if (reactivationCoroutine != null)
        {
            StopCoroutine(
                reactivationCoroutine
            );

            reactivationCoroutine = null;
        }

        SetOperational(false);
    }

    public void EndReposition(
        float delay)
    {
        if (reactivationCoroutine != null)
        {
            StopCoroutine(
                reactivationCoroutine
            );
        }

        if (delay <= 0f)
        {
            SetOperational(true);
            return;
        }

        reactivationCoroutine =
            StartCoroutine(
                ReactivateAfterDelay(delay)
            );
    }

    private IEnumerator ReactivateAfterDelay(
        float delay)
    {
        yield return new WaitForSeconds(
            delay
        );

        SetOperational(true);

        reactivationCoroutine = null;
    }

    private void SetOperational(
        bool operational)
    {
        if (isOperational == operational)
        {
            return;
        }

        if (!operational)
        {
            isOperational = false;

            ReleaseAllFish();

            if (netCollider != null)
            {
                netCollider.enabled = false;
            }

            return;
        }

        isOperational = true;

        if (netCollider != null)
        {
            netCollider.enabled = true;
        }

        Physics2D.SyncTransforms();
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