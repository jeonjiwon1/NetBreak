using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRodPlacementController : MonoBehaviour
{
    public static bool IsRodModeActive { get; private set; }

    [Header("References")]
    [SerializeField] private FishingRodController rodPrefab;
    [SerializeField] private Transform placementPreview;

    [Header("Placement")]
    [SerializeField] private int placementCost = 25;
    [SerializeField] private int maxActiveRods = 2;

    private Camera mainCamera;

    private readonly List<FishingRodController>
        activeRods = new();

    public int ActiveRodCount =>
        activeRods.Count;

    public int MaxActiveRods =>
        maxActiveRods;

    public int PlacementCost =>
        placementCost;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (placementPreview != null)
        {
            placementPreview.gameObject.SetActive(false);
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
             PrototypeGameFlowManager.Instance.IsGameEnded)
            ||
            NetPlacementController.IsNetModeActive;

        if (inputBlocked)
        {
            CancelPlacementMode();
            return;
        }

        HandleModeInput();

        if (!IsRodModeActive)
        {
            return;
        }

        UpdatePreview();
        HandlePlacementInput();
    }

    private void HandleModeInput()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (IsRodModeActive)
            {
                CancelPlacementMode();
            }
            else
            {
                StartPlacementMode();
            }
        }

        if (!IsRodModeActive)
        {
            return;
        }

        bool cancelPressed =
            Mouse.current.rightButton.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame;

        if (cancelPressed)
        {
            CancelPlacementMode();
        }
    }

    private void HandlePlacementInput()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        TryPlaceRod();
    }

    private void StartPlacementMode()
    {
        if (activeRods.Count >= maxActiveRods)
        {
            return;
        }

        IsRodModeActive = true;

        if (placementPreview != null)
        {
            placementPreview.gameObject.SetActive(true);
        }

        UpdatePreview();
    }

    private void CancelPlacementMode()
    {
        IsRodModeActive = false;

        if (placementPreview != null)
        {
            placementPreview.gameObject.SetActive(false);
        }
    }

    private void UpdatePreview()
    {
        if (placementPreview == null)
        {
            return;
        }

        placementPreview.position =
            GetMouseWorldPosition();
    }

    private void TryPlaceRod()
    {
        if (activeRods.Count >= maxActiveRods)
        {
            CancelPlacementMode();
            return;
        }

        if (RunManager.Instance == null)
        {
            CancelPlacementMode();
            return;
        }

        bool paid =
            RunManager.Instance.TrySpendGold(
                placementCost
            );

        if (!paid)
        {
            CancelPlacementMode();
            return;
        }

        Vector2 position =
            GetMouseWorldPosition();

        FishingRodController rod =
            Instantiate(
                rodPrefab,
                position,
                Quaternion.identity
            );

        activeRods.Add(rod);

        CancelPlacementMode();
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
}