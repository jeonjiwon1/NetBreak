using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FishingRodController : MonoBehaviour
{
    private const string InkInterferenceStatusName =
        "InkInterferenceStatus";

    private const string InkInterferenceStatusLabel =
        "먹물 방해";

    private const string PreferredStatusFontName =
        "NanumGothic-Bold SDF";

    private static TMP_FontAsset cachedStatusFont;

    [Header("Fishing Rod")]
    [SerializeField] private float capturePower = 4f;
    [SerializeField] private float attackInterval = 0.8f;
    [SerializeField] private float captureRange = 3f;

    [Header("Visual")]
    [SerializeField] private Transform rangeVisual;
    [SerializeField] private LineRenderer targetLine;

    [Header("Ink Interference Visual / 먹물 방해 시각")]
    [SerializeField] private Color interferenceTintColor = Color.black;

    [Range(0f, 1f)]
    [SerializeField] private float interferenceTintStrength = 0.65f;

    [SerializeField] private Color interferenceStatusColor =
        new(0.75f, 0.9f, 1f, 1f);

    [SerializeField] private Vector3 interferenceStatusOffset =
        new(0f, 1.6f, 0f);

    [Min(0.1f)]
    [SerializeField] private float interferenceStatusFontSize = 3.5f;

    private float attackTimer;
    private float tacticalAttackSpeedMultiplier = 1f;

    private FishController currentTarget;

    private int additionalTargets;

    private bool isBeingRepositioned;
    private float repositionBlockedUntil;
    private float specialDisabledUntil;

    private bool isOperational = true;

    private SpriteRenderer rodRenderer;
    private Color normalRodColor;
    private Color lastAppliedRodColor;
    private bool hasAppliedRodColor;

    private TextMeshPro interferenceStatusText;

    public float CapturePower =>
        capturePower;

    public float AttackInterval =>
        attackInterval;

    public float CaptureRange =>
        captureRange;

    public bool IsOperational =>
        isOperational;

    public bool IsInkInterferenceActive =>
        Time.time < specialDisabledUntil;

    public bool IsInkInterferenceVisible =>
        interferenceStatusText != null &&
        interferenceStatusText.gameObject.activeSelf;

    private void Awake()
    {
        UpdateRangeVisual();

        if (targetLine != null)
        {
            targetLine.positionCount = 2;

            targetLine.startWidth = 0.04f;
            targetLine.endWidth = 0.04f;

            targetLine.enabled = false;
        }

        rodRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (rodRenderer != null)
        {
            normalRodColor =
                rodRenderer.color;

            lastAppliedRodColor =
                normalRodColor;

            hasAppliedRodColor = true;
        }

        EnsureInterferenceStatus();
        RefreshOperationalState();
    }

    private void OnEnable()
    {
        RefreshOperationalState();
    }

    private void Update()
    {
        RefreshOperationalState();

        if (!isOperational)
        {
            currentTarget = null;
            HideTargetLine();
            return;
        }

        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        if (flow == null ||
            flow.IsPreparation ||
            flow.IsGameEnded)
        {
            currentTarget = null;
            HideTargetLine();
            return;
        }

        attackTimer -=
            Time.deltaTime;

        if (attackTimer <= 0f)
        {
            Attack();

            attackTimer =
                GetEffectiveAttackInterval();
        }

        UpdateTargetLine();
    }

    private void Attack()
    {
        int targetCount =
            1 + additionalTargets;

        List<FishController> targets =
            FindBestTargets(
                targetCount
            );

        if (targets.Count == 0)
        {
            currentTarget = null;

            HideTargetLine();
            return;
        }

        currentTarget =
            targets[0];

        foreach (FishController target
                 in targets)
        {
            if (target == null)
            {
                continue;
            }

            target.TakeCaptureDamage(
                capturePower,
                CombatDamageContext.Tool(
                    "fishing_rod",
                    this)
            );
        }

        if (currentTarget == null ||
            !currentTarget.gameObject.activeSelf)
        {
            currentTarget = null;

            HideTargetLine();
        }
    }

    private List<FishController>
        FindBestTargets(
            int maxTargets)
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                captureRange
            );

        List<FishController> candidates =
            new List<FishController>();

        foreach (Collider2D hit in hits)
        {
            FishController fish =
                hit.GetComponent<FishController>();

            if (fish == null ||
                fish.Data == null)
            {
                continue;
            }

            // Collider 가장자리만 범위에 걸친 경우를 제외한다.
            // 물고기 중심점이 실제 낚싯대 범위 안에 있어야 한다.
            float centerDistance =
                Vector2.Distance(
                    transform.position,
                    fish.transform.position
                );

            if (centerDistance >
                captureRange)
            {
                continue;
            }

            if (!candidates.Contains(
                fish))
            {
                candidates.Add(
                    fish
                );
            }
        }

        candidates.Sort(
            (a, b) =>
            {
                int valueComparison =
                    b.Data.CatchValue
                        .CompareTo(
                            a.Data.CatchValue
                        );

                if (valueComparison != 0)
                {
                    return valueComparison;
                }

                float distanceA =
                    Vector2.Distance(
                        transform.position,
                        a.transform.position
                    );

                float distanceB =
                    Vector2.Distance(
                        transform.position,
                        b.transform.position
                    );

                return distanceA.CompareTo(
                    distanceB
                );
            }
        );

        if (candidates.Count >
            maxTargets)
        {
            candidates.RemoveRange(
                maxTargets,
                candidates.Count -
                maxTargets
            );
        }

        return candidates;
    }

    private void UpdateTargetLine()
    {
        if (targetLine == null)
        {
            return;
        }

        if (currentTarget == null ||
            !currentTarget.gameObject.activeSelf)
        {
            HideTargetLine();
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                currentTarget.transform.position
            );

        if (distance > captureRange)
        {
            currentTarget = null;

            HideTargetLine();
            return;
        }

        targetLine.enabled = true;

        targetLine.SetPosition(
            0,
            transform.position
        );

        targetLine.SetPosition(
            1,
            currentTarget.transform.position
        );
    }

    private void HideTargetLine()
    {
        if (targetLine != null)
        {
            targetLine.enabled =
                false;
        }
    }

    private void UpdateRangeVisual()
    {
        if (rangeVisual == null)
        {
            return;
        }

        float diameter =
            captureRange * 2f;

        Transform parent =
            rangeVisual.parent;

        if (parent == null)
        {
            rangeVisual.localScale =
                new Vector3(
                    diameter,
                    diameter,
                    1f
                );

            return;
        }

        Vector3 parentScale =
            parent.lossyScale;

        float scaleX =
            Mathf.Abs(parentScale.x) >
            0.0001f
                ? diameter /
                  Mathf.Abs(parentScale.x)
                : diameter;

        float scaleY =
            Mathf.Abs(parentScale.y) >
            0.0001f
                ? diameter /
                  Mathf.Abs(parentScale.y)
                : diameter;

        rangeVisual.localScale =
            new Vector3(
                scaleX,
                scaleY,
                1f
            );
    }

    public void BeginReposition()
    {
        isBeingRepositioned = true;

        RefreshOperationalState();
    }

    public void EndReposition(
        float delay)
    {
        isBeingRepositioned = false;

        repositionBlockedUntil =
            Mathf.Max(
                repositionBlockedUntil,
                Time.time +
                Mathf.Max(
                    0f,
                    delay
                )
            );

        RefreshOperationalState();
    }

    public void DisableTemporarily(
        float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        specialDisabledUntil =
            Mathf.Max(
                specialDisabledUntil,
                Time.time +
                duration
            );

        RefreshOperationalState();
    }

    private void RefreshOperationalState()
    {
        bool shouldOperate =
            !isBeingRepositioned
            &&
            Time.time >=
            repositionBlockedUntil
            &&
            Time.time >=
            specialDisabledUntil;

        if (shouldOperate ==
            isOperational)
        {
            UpdateVisualState();
            return;
        }

        isOperational =
            shouldOperate;

        if (!isOperational)
        {
            currentTarget = null;

            HideTargetLine();
        }
        else
        {
            attackTimer = 0f;
        }

        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        bool allowGameplayVisuals =
            ShouldShowGameplayVisuals();

        bool shouldDarken =
            !isOperational &&
            allowGameplayVisuals;

        if (rodRenderer != null)
        {
            CaptureExternalRodColor();

            Color targetColor =
                shouldDarken
                    ? Color.Lerp(
                        normalRodColor,
                        interferenceTintColor,
                        interferenceTintStrength)
                    : normalRodColor;

            ApplyRodColor(targetColor);
        }

        SetInterferenceStatusVisible(
            IsInkInterferenceActive &&
            allowGameplayVisuals
        );
    }

    private bool ShouldShowGameplayVisuals()
    {
        PrototypeGameFlowManager flow =
            PrototypeGameFlowManager.Instance;

        return flow == null ||
            (!flow.IsPreparation &&
             !flow.IsGameEnded);
    }

    private void CaptureExternalRodColor()
    {
        if (rodRenderer == null)
        {
            return;
        }

        if (!hasAppliedRodColor ||
            rodRenderer.color !=
            lastAppliedRodColor)
        {
            normalRodColor =
                rodRenderer.color;
        }
    }

    private void ApplyRodColor(
        Color color)
    {
        if (rodRenderer == null)
        {
            return;
        }

        if (rodRenderer.color != color)
        {
            rodRenderer.color = color;
        }

        lastAppliedRodColor = color;
        hasAppliedRodColor = true;
    }

    private void EnsureInterferenceStatus()
    {
        if (interferenceStatusText != null)
        {
            return;
        }

        Transform existing =
            transform.Find(
                InkInterferenceStatusName
            );

        GameObject statusObject;

        if (existing != null)
        {
            statusObject =
                existing.gameObject;

            interferenceStatusText =
                statusObject.GetComponent<
                    TextMeshPro
                >();
        }
        else
        {
            statusObject =
                new GameObject(
                    InkInterferenceStatusName
                );

            statusObject.SetActive(false);
            statusObject.transform.SetParent(
                transform,
                false
            );

            interferenceStatusText =
                statusObject.AddComponent<
                    TextMeshPro
                >();
        }

        if (interferenceStatusText == null)
        {
            statusObject.SetActive(false);
            return;
        }

        Transform statusTransform =
            interferenceStatusText.transform;

        statusTransform.localPosition =
            interferenceStatusOffset;

        statusTransform.localRotation =
            Quaternion.identity;

        statusTransform.localScale =
            Vector3.one;

        interferenceStatusText.text =
            InkInterferenceStatusLabel;

        interferenceStatusText.font =
            ResolveStatusFont();

        interferenceStatusText.fontSize =
            interferenceStatusFontSize;

        interferenceStatusText.color =
            interferenceStatusColor;

        interferenceStatusText.alignment =
            TextAlignmentOptions.Center;

        interferenceStatusText.textWrappingMode =
            TextWrappingModes.NoWrap;

        interferenceStatusText.overflowMode =
            TextOverflowModes.Overflow;

        MeshRenderer textRenderer =
            interferenceStatusText.GetComponent<
                MeshRenderer
            >();

        if (textRenderer != null &&
            rodRenderer != null)
        {
            textRenderer.sortingLayerID =
                rodRenderer.sortingLayerID;

            textRenderer.sortingOrder =
                rodRenderer.sortingOrder +
                20;
        }

        statusObject.SetActive(false);
    }

    private static TMP_FontAsset ResolveStatusFont()
    {
        if (cachedStatusFont != null)
        {
            return cachedStatusFont;
        }

        TMP_FontAsset[] loadedFonts =
            Resources.FindObjectsOfTypeAll<
                TMP_FontAsset
            >();

        foreach (TMP_FontAsset font
                 in loadedFonts)
        {
            if (font != null &&
                font.name ==
                PreferredStatusFontName)
            {
                cachedStatusFont = font;
                return cachedStatusFont;
            }
        }

        return TMP_Settings.defaultFontAsset;
    }

    private void SetInterferenceStatusVisible(
        bool visible)
    {
        if (interferenceStatusText == null)
        {
            return;
        }

        GameObject statusObject =
            interferenceStatusText.gameObject;

        if (visible &&
            !statusObject.activeSelf &&
            (interferenceStatusText.font == null ||
             interferenceStatusText.font.name !=
             PreferredStatusFontName))
        {
            TMP_FontAsset resolvedFont =
                ResolveStatusFont();

            if (resolvedFont != null)
            {
                interferenceStatusText.font =
                    resolvedFont;
            }
        }

        if (statusObject.activeSelf != visible)
        {
            statusObject.SetActive(visible);
        }
    }

    private void OnDisable()
    {
        SetInterferenceStatusVisible(false);

        if (rodRenderer == null)
        {
            return;
        }

        CaptureExternalRodColor();
        ApplyRodColor(normalRodColor);
    }

    public void SetTacticalAttackSpeedMultiplier(float multiplier)
    {
        tacticalAttackSpeedMultiplier = Mathf.Max(1f, multiplier);
        attackTimer = Mathf.Min(attackTimer, GetEffectiveAttackInterval());
    }

    private float GetEffectiveAttackInterval() =>
        attackInterval / tacticalAttackSpeedMultiplier;

    public void IncreaseCapturePower(
        float amount)
    {
        capturePower +=
            amount;
    }

    public void ReduceAttackInterval(
        float amount)
    {
        attackInterval =
            Mathf.Max(
                0.1f,
                attackInterval -
                amount
            );
    }

    public void IncreaseCaptureRange(
        float amount)
    {
        captureRange +=
            amount;

        UpdateRangeVisual();
    }

    public void AddAdditionalTarget(
        int amount)
    {
        additionalTargets +=
            amount;
    }
}
