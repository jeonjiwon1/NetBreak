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

    private void Awake()
    {
        netCollider = GetComponent<BoxCollider2D>();
        rigidBody = GetComponent<Rigidbody2D>();

        netCollider.isTrigger = true;

        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        rigidBody.gravityScale = 0f;
    }

    public void Initialize(
        Vector2 start,
        Vector2 end,
        float thickness
    )
    {
        Vector2 direction = end - start;

        float length = direction.magnitude;

        Vector2 center =
            (start + end) * 0.5f;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        transform.position = center;
        transform.rotation =
            Quaternion.Euler(0f, 0f, angle);

        transform.localScale =
            new Vector3(
                length,
                thickness,
                1f
            );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FishMovement movement =
            other.GetComponent<FishMovement>();

        if (movement != null)
        {
            movement.EnterNet(slowMultiplier);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
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

    private void OnTriggerExit2D(Collider2D other)
    {
        FishMovement movement =
            other.GetComponent<FishMovement>();

        if (movement != null)
        {
            movement.ExitNet();
        }
    }
}