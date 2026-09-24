using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(FishController))]
public class SquidController : MonoBehaviour
{
    private FishController fishController;
    private FishVisualController visualController;
    private SquidInkPresentationProfile presentationProfile;
    public event Action<SquidController> InkPresentationTriggered;

    private float inkTimer;

    private void Awake()
    {
        fishController =
            GetComponent<FishController>();
        visualController = GetComponent<FishVisualController>();
        presentationProfile = Resources.Load<SquidInkPresentationProfile>(
            "SquidInkPresentation");
    }

    private void OnEnable()
    {
        ResetInkTimer();
    }

    private void OnDisable()
    {
        InkPresentationTriggered = null;
    }

    private void Update()
    {
        if (!IsSquid())
        {
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            flow.IsPreparation ||
            flow.IsGameEnded)
        {
            return;
        }

        inkTimer -=
            Time.deltaTime;

        if (inkTimer > 0f)
        {
            return;
        }

        ReleaseInk();

        inkTimer =
            fishController.Data.InkInterval;
    }

    private bool IsSquid()
    {
        return
            fishController != null
            &&
            fishController.Data != null
            &&
            fishController.Data.SpecialType ==
            FishSpecialType.Squid;
    }

    private void ResetInkTimer()
    {
        if (!IsSquid())
        {
            inkTimer = 0f;
            return;
        }

        inkTimer =
            fishController.Data.FirstInkDelay;
    }

    private void ReleaseInk()
    {
        FishData data =
            fishController.Data;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                data.InkRange
            );

        HashSet<NetController> affectedNets =
            new HashSet<NetController>();

        HashSet<FishingRodController> affectedRods =
            new HashSet<FishingRodController>();

        foreach (Collider2D hit in hits)
        {
            NetController net =
                hit.GetComponentInParent<
                    NetController
                >();

            if (net != null)
            {
                affectedNets.Add(
                    net
                );
            }

            FishingRodController rod =
                hit.GetComponentInParent<
                    FishingRodController
                >();

            if (rod != null)
            {
                affectedRods.Add(
                    rod
                );
            }
        }

        foreach (NetController net
                 in affectedNets)
        {
            if (net != null)
            {
                net.DisableTemporarily(
                    data.InkDisableDuration
                );
            }
        }

        foreach (FishingRodController rod
                 in affectedRods)
        {
            if (rod != null)
            {
                rod.DisableTemporarily(
                    data.InkDisableDuration
                );
            }
        }

        if (affectedNets.Count > 0 || affectedRods.Count > 0)
        {
            Vector2 origin = transform.position;
            List<Vector2> rodPositions = new List<Vector2>(affectedRods.Count);
            foreach (FishingRodController rod in affectedRods)
                if (rod != null) rodPositions.Add(rod.transform.position);

            InkPresentationTriggered?.Invoke(this);
            Action releasePresentation = () =>
            {
                ItemEffectManager effects = ItemEffectManager.Instance;
                if (effects == null) return;
                effects.ShowSquidInkBurst(origin, presentationProfile);
                for (int i = 0; i < rodPositions.Count; i++)
                    effects.ShowSquidInkAttack(origin, rodPositions[i]);
                effects.PlaySquidInkSound(presentationProfile);
            };
            if (visualController == null)
                visualController = GetComponent<FishVisualController>();
            if (visualController == null ||
                !visualController.PlaySpecial(presentationProfile, releasePresentation))
                releasePresentation();
        }
    }

    private void OnDrawGizmosSelected()
    {
        FishController fish =
            GetComponent<FishController>();

        if (fish == null ||
            fish.Data == null ||
            fish.Data.SpecialType !=
            FishSpecialType.Squid)
        {
            return;
        }

        Gizmos.DrawWireSphere(
            transform.position,
            fish.Data.InkRange
        );
    }
}
