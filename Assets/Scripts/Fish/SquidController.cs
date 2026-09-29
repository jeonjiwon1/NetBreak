using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(FishController))]
public class SquidController : MonoBehaviour
{
    private sealed class PendingImpact
    {
        public float Remaining;
        public bool Resolved;
        public Action Apply;
    }

    private FishController fishController;
    private FishVisualController visualController;
    private SquidInkPresentationProfile presentationProfile;
    public event Action<SquidController> InkPresentationTriggered;

    private float inkTimer;
    private int attackGeneration;
    private readonly List<PendingImpact> pendingImpacts = new List<PendingImpact>();
    private static readonly CombatVfxSettings fallbackVfxSettings = new CombatVfxSettings();

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
        attackGeneration++;
        pendingImpacts.Clear();
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
            pendingImpacts.Clear();
            return;
        }

        TickPendingImpacts(Time.deltaTime);

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

        if (affectedNets.Count > 0 || affectedRods.Count > 0)
        {
            Vector2 origin = transform.position;
            int generation = attackGeneration;
            int fishVersion = fishController.LifecycleVersion;
            List<Action<ItemEffectManager, float>> attacks = new List<Action<ItemEffectManager, float>>(
                affectedNets.Count + affectedRods.Count);
            foreach (NetController net in affectedNets)
            {
                if (net == null) continue;
                Vector2 targetPosition = net.transform.position;
                attacks.Add((effects, travelDuration) =>
                {
                    PendingImpact impact = new PendingImpact
                    {
                        Remaining = travelDuration,
                        Apply = () =>
                        {
                            if (!CanApplyImpact(generation, fishVersion) ||
                            net == null || !net.isActiveAndEnabled ||
                            ((Vector2)net.transform.position - targetPosition).sqrMagnitude > 0.01f)
                                return;
                            net.DisableTemporarily(data.InkDisableDuration);
                        }
                    };
                    pendingImpacts.Add(impact);
                    effects?.ShowSquidInkAttack(origin, targetPosition,
                        presentationProfile, () => ResolveImpact(impact));
                });
            }
            foreach (FishingRodController rod in affectedRods)
            {
                if (rod == null) continue;
                Vector2 targetPosition = rod.transform.position;
                attacks.Add((effects, travelDuration) =>
                {
                    PendingImpact impact = new PendingImpact
                    {
                        Remaining = travelDuration,
                        Apply = () =>
                        {
                            if (!CanApplyImpact(generation, fishVersion) ||
                            rod == null || !rod.isActiveAndEnabled ||
                            ((Vector2)rod.transform.position - targetPosition).sqrMagnitude > 0.01f)
                                return;
                            rod.DisableTemporarily(data.InkDisableDuration);
                        }
                    };
                    pendingImpacts.Add(impact);
                    effects?.ShowSquidInkAttack(origin, targetPosition,
                        presentationProfile, () => ResolveImpact(impact));
                });
            }

            if (attacks.Count == 0) return;
            InkPresentationTriggered?.Invoke(this);
            Action releasePresentation = () =>
            {
                if (!CanApplyImpact(generation, fishVersion)) return;
                ItemEffectManager effects = ItemEffectManager.Instance;
                float travelDuration = effects != null
                    ? effects.SquidInkTravelDuration
                    : fallbackVfxSettings.SquidTrajectoryDuration;
                effects?.ShowSquidInkBurst(origin, presentationProfile);
                for (int i = 0; i < attacks.Count; i++)
                    attacks[i](effects, travelDuration);
                effects?.PlaySquidInkSound(presentationProfile);
            };
            if (visualController == null)
                visualController = GetComponent<FishVisualController>();
            if (visualController == null ||
                !visualController.PlaySpecial(presentationProfile, releasePresentation))
                releasePresentation();
        }
    }

    private void TickPendingImpacts(float scaledDeltaTime)
    {
        if (scaledDeltaTime <= 0f) return;
        for (int i = pendingImpacts.Count - 1; i >= 0; i--)
        {
            PendingImpact impact = pendingImpacts[i];
            if (!impact.Resolved)
            {
                impact.Remaining -= scaledDeltaTime;
                if (impact.Remaining <= 0f) ResolveImpact(impact);
            }
            if (impact.Resolved) pendingImpacts.RemoveAt(i);
        }
    }

    private static void ResolveImpact(PendingImpact impact)
    {
        if (impact.Resolved) return;
        impact.Resolved = true;
        impact.Apply?.Invoke();
        impact.Apply = null;
    }

    private bool CanApplyImpact(int generation, int fishVersion)
    {
        PrototypeGameFlowManager flow = PrototypeGameFlowManager.Instance;
        return isActiveAndEnabled && generation == attackGeneration &&
            IsSquid() && !fishController.IsCaptured &&
            fishController.LifecycleVersion == fishVersion &&
            (flow == null || (!flow.IsPreparation && !flow.IsGameEnded));
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
