using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Cost")]
    [SerializeField] private int baseCost = 5;
    [SerializeField] private float costPerUnitLength = 4f;

    private Camera mainCamera;

    private bool isDragging;

    private Vector2 startPosition;
    private Vector2 currentEndPosition;

    private readonly List<NetController> activeNets = new();

    public bool IsDragging => isDragging;

    public int ActiveNetCount =>
        activeNets.Count;

    public int MaxActiveNets =>
        maxActiveNets;

    public int CurrentPlacementCost
    {
        get
        {
            if (!isDragging)
            {
                return 0;
            }

            float length =
                Vector2.Distance(
                    startPosition,
                    currentEndPosition
                );

            return CalculateCost(length);
        }
    }

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

        HandleModeInput();

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
            mainCamera.ScreenToWorldPoint(
                screenPosition
            );

        return new Vector2(
            worldPosition.x,
            worldPosition.y
        );
    }

    private void StartPlacement()
    {
        if (activeNets.Count >= maxActiveNets)
        {
            IsNetModeActive = false;
            return;
        }

        isDragging = true;

        startPosition =
            GetMouseWorldPosition();

        currentEndPosition =
            startPosition;

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

        if (length < minLength)
        {
            IsNetModeActive = false;
            return;
        }

        int cost =
            CalculateCost(length);

        if (RunManager.Instance == null ||
            !RunManager.Instance.TrySpendGold(cost))
        {
            IsNetModeActive = false;
            return;
        }

        NetController net =
            Instantiate(netPrefab);

        net.Initialize(
            startPosition,
            currentEndPosition,
            thickness
        );

        activeNets.Add(net);

        IsNetModeActive = false;
    }

    private int CalculateCost(float length)
    {
        return baseCost +
            Mathf.CeilToInt(
                length * costPerUnitLength
            );
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

        visual.position = center;

        visual.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

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