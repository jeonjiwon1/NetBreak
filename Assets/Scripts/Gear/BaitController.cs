using UnityEngine;
using UnityEngine.InputSystem;

public class BaitController : MonoBehaviour
{
    public static BaitController Instance { get; private set; }

    [Header("Bait")]
    [SerializeField] private float attractionRadius = 4f;
    [SerializeField] private float duration = 5f;
    [SerializeField] private float cooldown = 2f;

    [Header("Visual")]
    [SerializeField] private GameObject baitVisual;
    [SerializeField] private GameObject rangeVisual;

    private Camera mainCamera;

    private float activeTimer;
    private float cooldownTimer;

    public bool IsActive { get; private set; }

    public Vector2 Position => transform.position;
    public float AttractionRadius => attractionRadius;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;

        SetActive(false);
    }

    private void Update()
    {
        UpdateTimers();
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame &&
            cooldownTimer <= 0f)
        {
            PlaceBait();
        }
    }

    private void PlaceBait()
    {
        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        mouseWorldPosition.z = 0f;

        transform.position = mouseWorldPosition;

        activeTimer = duration;
        cooldownTimer = cooldown;

        SetActive(true);
    }

    private void UpdateTimers()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (!IsActive)
        {
            return;
        }

        activeTimer -= Time.deltaTime;

        if (activeTimer <= 0f)
        {
            SetActive(false);
        }
    }

    private void SetActive(bool value)
    {
        IsActive = value;

        if (baitVisual != null)
        {
            baitVisual.SetActive(value);
        }

        if (rangeVisual != null)
        {
            rangeVisual.SetActive(value);
        }
    }
}