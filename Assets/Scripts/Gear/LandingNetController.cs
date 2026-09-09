using UnityEngine;
using UnityEngine.InputSystem;

public class LandingNetController : MonoBehaviour
{
    [SerializeField] private float capturePower = 5f;
    [SerializeField] private float captureRadius = 1.2f;
    [SerializeField] private float attackCooldown = 0.3f;

    [SerializeField] private Transform rangeVisual;

    private Camera mainCamera;
    private float nextAttackTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition.z = 0f;

        if (rangeVisual != null)
        {
            rangeVisual.position = mouseWorldPosition;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            UseLandingNet(mouseWorldPosition);

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void UseLandingNet(Vector3 mouseWorldPosition)
    {
        Vector2 capturePosition =
            new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                capturePosition,
                captureRadius
            );

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish != null)
            {
                fish.TakeCaptureDamage(capturePower);
            }
        }
    }
}