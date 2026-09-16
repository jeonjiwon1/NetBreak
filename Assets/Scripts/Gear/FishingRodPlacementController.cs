using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FishingRodPlacementController : MonoBehaviour
{
    public static bool IsRodModeActive
    {
        get;
        private set;
    }

    [Header("References")]
    [SerializeField] private FishingRodController rodPrefab;
    [SerializeField] private Transform placementPreview;

    [Header("Placement Economy")]
    [SerializeField] private int basePlacementCost = 25;
    [SerializeField] private float placementCostGrowthMultiplier = 1.5f;
    [SerializeField] private int maxActiveRods = 2;

    private Camera mainCamera;

    private readonly List<FishingRodController>
        activeRods = new();

    private float costMultiplier = 1f;

    private float rodCapturePowerBonus;
    private float rodRangeBonus;
    private float rodAttackIntervalReduction;
    private int rodAdditionalTargets;

    public int ActiveRodCount =>
        activeRods.Count;

    public int MaxActiveRods =>
        maxActiveRods;

    public int PlacementCost =>
        CalculatePlacementCost();

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
        ToolInputState input = ToolSlotInput.Read(ToolId.FishingRod);
        if (input.Cancelled)
        {
            CancelPlacementMode();
            return;
        }

        HandleModeInput(input);

        if (!IsRodModeActive)
        {
            return;
        }

        UpdatePreview();
        HandlePlacementInput();
    }

    private void HandleModeInput(ToolInputState input)
    {
        if (input.Pressed)
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

    }

    private void OnDisable()
    {
        CancelPlacementMode();
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

        int cost =
            CalculatePlacementCost();

        if (!RunManager.Instance.TrySpendGold(
            cost))
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

        ApplyStoredUpgrades(rod);

        activeRods.Add(rod);

        CancelPlacementMode();
    }

    private int CalculatePlacementCost()
    {
        return Mathf.RoundToInt(
            basePlacementCost *
            Mathf.Pow(
                placementCostGrowthMultiplier,
                activeRods.Count
            ) *
            costMultiplier
        );
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

    private void ApplyStoredUpgrades(
        FishingRodController rod)
    {
        if (rod == null)
        {
            return;
        }

        if (rodCapturePowerBonus > 0f)
        {
            rod.IncreaseCapturePower(
                rodCapturePowerBonus
            );
        }

        if (rodRangeBonus > 0f)
        {
            rod.IncreaseCaptureRange(
                rodRangeBonus
            );
        }

        if (rodAttackIntervalReduction > 0f)
        {
            rod.ReduceAttackInterval(
                rodAttackIntervalReduction
            );
        }

        if (rodAdditionalTargets > 0)
        {
            rod.AddAdditionalTarget(
                rodAdditionalTargets
            );
        }
    }

    public void IncreaseRodCapturePower(
        float amount)
    {
        rodCapturePowerBonus += amount;

        foreach (FishingRodController rod
                 in activeRods)
        {
            if (rod != null)
            {
                rod.IncreaseCapturePower(
                    amount
                );
            }
        }
    }

    public void IncreaseRodRange(
        float amount)
    {
        rodRangeBonus += amount;

        foreach (FishingRodController rod
                 in activeRods)
        {
            if (rod != null)
            {
                rod.IncreaseCaptureRange(
                    amount
                );
            }
        }
    }

    public void ReduceRodAttackInterval(
        float amount)
    {
        rodAttackIntervalReduction += amount;

        foreach (FishingRodController rod
                 in activeRods)
        {
            if (rod != null)
            {
                rod.ReduceAttackInterval(
                    amount
                );
            }
        }
    }

    public void AddRodAdditionalTarget(
        int amount)
    {
        rodAdditionalTargets += amount;

        foreach (FishingRodController rod
                 in activeRods)
        {
            if (rod != null)
            {
                rod.AddAdditionalTarget(
                    amount
                );
            }
        }
    }

    public void IncreaseMaxActiveRods(
        int amount)
    {
        maxActiveRods += amount;
    }

    public void MultiplyPlacementCost(
        float multiplier)
    {
        if (multiplier > 0f)
        {
            costMultiplier *= multiplier;
        }
    }

    public void EnableAnglerJob()
    {
        // G4-A transition: legacy Job selection remains, but its gameplay
        // modifiers are now purchased from the Fishing Rod skill tree.
    }
}
