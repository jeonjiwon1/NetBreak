using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FishController))]
public class SquidController : MonoBehaviour
{
    private FishController fishController;

    private float inkTimer;

    private void Awake()
    {
        fishController =
            GetComponent<FishController>();
    }

    private void OnEnable()
    {
        ResetInkTimer();
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