using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class NetPlacementController : MonoBehaviour
{
    public static bool IsNetModeActive { get; private set; }

    [Header("References")]
    [SerializeField] private NetController netPrefab;
    [SerializeField] private Transform netPreview;

    [Header("Placement")]
    [SerializeField] private float thickness = 0.3f;
    [SerializeField] private float minLength = 0.5f;
    [SerializeField] private float maxLength = 8f;

    [Header("Net Limit")]
    [SerializeField] private int maxActiveNets = 3;

    private readonly List<NetController> activeNets = new();

    private Camera mainCamera;

    private bool isDragging;
    private Vector2 startPosition;
    private Vector2 currentEndPosition;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (netPreview != null)
        {
            netPreview.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (Mouse.current == null ||
            Keyboard.current == null)
        {
            return;
        }

        HandleModeInput();

        bool inputBlocked =
            (PrototypeAugmentManager.Instance != null &&
            PrototypeAugmentManager.Instance.IsChoosingAugment)
            ||
            (PrototypeGameFlowManager.Instance != null &&
            PrototypeGameFlowManager.Instance.IsGameEnded);

        if (inputBlocked)
        {
            IsNetModeActive = false;
            CancelPlacement();
            return;
        }

        if (!IsNetModeActive)
        {
            return;
        }

        HandlePlacement();
    }

    private void HandleModeInput()
    {
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            IsNetModeActive = !IsNetModeActive;

            if (!IsNetModeActive)
            {
                CancelPlacement();
            }
        }

        bool cancelPressed =
            Keyboard.current.escapeKey.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame;

        if (cancelPressed && IsNetModeActive)
        {
            IsNetModeActive = false;
            CancelPlacement();
        }
    }

    private void HandlePlacement()
    {
        if (!isDragging &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartPlacement();
        }

        if (!isDragging)
        {
            return;
        }

        UpdatePlacement();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            FinishPlacement();
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector2 screenPosition =
            Mouse.current.position.ReadValue();

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

    private void StartPlacement()
    {
        if (activeNets.Count >= maxActiveNets)
        {
            return;
        }

        isDragging = true;

        startPosition = GetMouseWorldPosition();
        currentEndPosition = startPosition;

        if (netPreview != null)
        {
            netPreview.gameObject.SetActive(true);
        }
    }

    private void UpdatePlacement()
    {
        Vector2 mousePosition =
            GetMouseWorldPosition();

        Vector2 direction =
            mousePosition - startPosition;

        if (direction.magnitude > maxLength)
        {
            direction =
                direction.normalized * maxLength;
        }

        currentEndPosition =
            startPosition + direction;

        UpdateVisual(
            netPreview,
            startPosition,
            currentEndPosition
        );
    }

    private void FinishPlacement()
    {
        isDragging = false;

        if (netPreview != null)
        {
            netPreview.gameObject.SetActive(false);
        }

        float length =
            Vector2.Distance(
                startPosition,
                currentEndPosition
            );

        if (length >= minLength)
        {
            NetController net =
            Instantiate(netPrefab);

            net.Initialize(
                startPosition,
                currentEndPosition,
                thickness
            );

            activeNets.Add(net);
        }

        IsNetModeActive = false;
    }

    private void CancelPlacement()
    {
        isDragging = false;

        if (netPreview != null)
        {
            netPreview.gameObject.SetActive(false);
        }
    }

    private void UpdateVisual(
        Transform visual,
        Vector2 start,
        Vector2 end
    )
    {
        if (visual == null)
        {
            return;
        }

        Vector2 direction = end - start;

        float length = direction.magnitude;

        Vector2 center =
            (start + end) * 0.5f;

        float angle =
            Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        visual.position = center;

        visual.rotation =
            Quaternion.Euler(0f, 0f, angle);

        visual.localScale =
            new Vector3(
                length,
                thickness,
                1f
            );
    }

    public void IncreaseMaxLength(float amount)
    {
        maxLength += amount;
    }
}