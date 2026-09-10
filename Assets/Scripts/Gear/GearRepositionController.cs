using UnityEngine;
using UnityEngine.InputSystem;

public class GearRepositionController : MonoBehaviour
{
    public static bool IsRepositioning
    {
        get;
        private set;
    }

    public static bool IsRepositionModifierHeld
    {
        get
        {
            if (Keyboard.current == null)
            {
                return false;
            }

            return
                Keyboard.current.leftCtrlKey.isPressed ||
                Keyboard.current.rightCtrlKey.isPressed;
        }
    }

    [Header("Reposition")]
    [SerializeField] private float reinstallDelay = 0.5f;
    [SerializeField] private float selectionRadius = 0.2f;

    private Camera mainCamera;

    private NetController selectedNet;
    private FishingRodController selectedRod;

    private Transform selectedTransform;

    private Vector3 originalPosition;
    private Vector3 grabOffset;

    private void Awake()
    {
        mainCamera = Camera.main;
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
            NetPlacementController.IsNetModeActive
            ||
            FishingRodPlacementController.IsRodModeActive;

        if (inputBlocked)
        {
            if (IsRepositioning)
            {
                CancelReposition();
            }

            return;
        }

        if (!IsRepositioning)
        {
            TryStartReposition();
            return;
        }

        HandleActiveReposition();
    }

    private void TryStartReposition()
    {
        if (!IsRepositionModifierHeld)
        {
            return;
        }

        if (!Mouse.current.leftButton
            .wasPressedThisFrame)
        {
            return;
        }

        Vector2 mousePosition =
            GetMouseWorldPosition();

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                mousePosition,
                selectionRadius
            );

        // ³¬½Ë´ë¸¦ ¿ì¼± ¼±ÅÃÇÑ´Ù.
        foreach (Collider2D hit in hits)
        {
            FishingRodController rod =
                hit.GetComponentInParent<
                    FishingRodController
                >();

            if (rod != null)
            {
                BeginRodReposition(
                    rod,
                    mousePosition
                );

                return;
            }
        }

        foreach (Collider2D hit in hits)
        {
            NetController net =
                hit.GetComponentInParent<
                    NetController
                >();

            if (net != null)
            {
                BeginNetReposition(
                    net,
                    mousePosition
                );

                return;
            }
        }
    }

    private void BeginRodReposition(
        FishingRodController rod,
        Vector2 mousePosition)
    {
        selectedRod = rod;
        selectedNet = null;

        selectedTransform =
            rod.transform;

        BeginCommonReposition(
            mousePosition
        );

        rod.BeginReposition();
    }

    private void BeginNetReposition(
        NetController net,
        Vector2 mousePosition)
    {
        selectedNet = net;
        selectedRod = null;

        selectedTransform =
            net.transform;

        BeginCommonReposition(
            mousePosition
        );

        net.BeginReposition();
    }

    private void BeginCommonReposition(
        Vector2 mousePosition)
    {
        if (selectedTransform == null)
        {
            return;
        }

        originalPosition =
            selectedTransform.position;

        grabOffset =
            selectedTransform.position -
            (Vector3)mousePosition;

        IsRepositioning = true;
    }

    private void HandleActiveReposition()
    {
        if (selectedTransform == null)
        {
            ClearSelection();
            return;
        }

        bool cancelPressed =
            Mouse.current.rightButton
                .wasPressedThisFrame
            ||
            Keyboard.current.escapeKey
                .wasPressedThisFrame;

        if (cancelPressed)
        {
            CancelReposition();
            return;
        }

        UpdateSelectedPosition();

        if (Mouse.current.leftButton
            .wasReleasedThisFrame)
        {
            FinishReposition();
        }
    }

    private void UpdateSelectedPosition()
    {
        Vector2 mousePosition =
            GetMouseWorldPosition();

        Vector3 targetPosition =
            (Vector3)mousePosition +
            grabOffset;

        targetPosition.z =
            selectedTransform.position.z;

        selectedTransform.position =
            targetPosition;
    }

    private void FinishReposition()
    {
        ReactivateSelectedGear(
            reinstallDelay
        );

        ClearSelection();
    }

    private void CancelReposition()
    {
        if (selectedTransform != null)
        {
            selectedTransform.position =
                originalPosition;
        }

        ReactivateSelectedGear(
            reinstallDelay
        );

        ClearSelection();
    }

    private void ReactivateSelectedGear(
        float delay)
    {
        if (selectedNet != null)
        {
            selectedNet.EndReposition(
                delay
            );
        }

        if (selectedRod != null)
        {
            selectedRod.EndReposition(
                delay
            );
        }
    }

    private void ClearSelection()
    {
        selectedNet = null;
        selectedRod = null;
        selectedTransform = null;

        IsRepositioning = false;
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

    private void OnDisable()
    {
        if (!IsRepositioning)
        {
            return;
        }

        if (selectedTransform != null)
        {
            selectedTransform.position =
                originalPosition;
        }

        ReactivateSelectedGear(0f);

        ClearSelection();
    }
}