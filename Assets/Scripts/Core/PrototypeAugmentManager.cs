using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AugmentCategory
{
    LandingNet,
    CastNet,
    Bait,
    Net,
    FishingRod,
    General
}

public enum AugmentType
{
    LandingNetPower,
    LandingNetRadius,
    LandingNetChainCapture,

    CastNetPower,
    CastNetRadius,
    CastNetCooldown,
    CastNetFullHaul,

    BaitRadius,
    BaitDuration,

    NetLength,

    FishingRodPower,
    FishingRodRange,
    FishingRodSpeed,
    FishingRodExtraHook
}

public class PrototypeAugmentManager : MonoBehaviour
{
    public static PrototypeAugmentManager Instance
    {
        get;
        private set;
    }

    [Header("References")]
    [SerializeField] private LandingNetController landingNet;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private BaitController bait;
    [SerializeField] private NetPlacementController netPlacement;
    [SerializeField] private FishingRodPlacementController rodPlacement;

    [Header("Input")]
    [SerializeField] private float selectionInputDelay = 0.25f;

    [Header("Reroll")]
    [SerializeField] private int rerollBaseCost = 20;
    [SerializeField] private int rerollCostIncrease = 15;

    private bool canSelect;
    private float selectionUnlockTime;
    private bool showChoices;

    private int rerollCount;

    private AugmentOption[] currentChoices =
        new AugmentOption[3];

    private readonly Dictionary<
        AugmentCategory,
        float
    > categoryWeights = new();

    private readonly HashSet<AugmentType>
        acquiredUniqueAugments = new();

    public bool IsChoosingAugment =>
        showChoices ||
        (
            PrototypeJobManager.Instance != null &&
            PrototypeJobManager.Instance.IsChoosingJob
        );

    public bool IsShowingChoices =>
        showChoices;

    public bool CanSelect =>
        canSelect;

    public int ChoiceCount =>
        currentChoices.Length;

    public int CurrentRerollCost =>
        GetRerollCost();

    public bool CanReroll
    {
        get
        {
            return
                showChoices &&
                canSelect &&
                RunManager.Instance != null &&
                RunManager.Instance.CurrentGold >=
                GetRerollCost();
        }
    }

    private class AugmentOption
    {
        public AugmentType Type;
        public AugmentCategory Category;
        public string Name;
        public string Description;
        public float BaseWeight;
        public bool IsUnique;

        public AugmentOption(
            AugmentType type,
            AugmentCategory category,
            string name,
            string description,
            float baseWeight = 1f,
            bool isUnique = false)
        {
            Type = type;
            Category = category;
            Name = name;
            Description = description;
            BaseWeight = baseWeight;
            IsUnique = isUnique;
        }
    }

    private void Awake()
    {
        Instance = this;

        ResetCategoryWeights();
    }

    private void Update()
    {
        if (!showChoices ||
            canSelect)
        {
            return;
        }

        if (Time.realtimeSinceStartup <
            selectionUnlockTime)
        {
            return;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed)
        {
            return;
        }

        canSelect = true;
    }

    // =========================================================
    // OPEN
    // =========================================================

    public void ShowChoices()
    {
        if (showChoices)
        {
            return;
        }

        rerollCount = 0;

        GenerateChoices();

        showChoices = true;

        LockSelectionInput();

        Time.timeScale = 0f;
    }

    private void LockSelectionInput()
    {
        canSelect = false;

        selectionUnlockTime =
            Time.realtimeSinceStartup +
            selectionInputDelay;
    }

    // =========================================================
    // CANVAS DATA
    // =========================================================

    public string GetChoiceName(
        int index)
    {
        AugmentOption option =
            GetChoice(
                index
            );

        return option != null
            ? option.Name
            : "";
    }

    public string GetChoiceDescription(
        int index)
    {
        AugmentOption option =
            GetChoice(
                index
            );

        return option != null
            ? option.Description
            : "";
    }

    private AugmentOption GetChoice(
        int index)
    {
        if (index < 0 ||
            index >= currentChoices.Length)
        {
            return null;
        }

        return currentChoices[index];
    }

    // =========================================================
    // CANVAS INPUT
    // =========================================================

    public void SelectChoiceFromUI(
        int index)
    {
        if (!showChoices ||
            !canSelect)
        {
            return;
        }

        AugmentOption option =
            GetChoice(
                index
            );

        if (option == null)
        {
            return;
        }

        ApplyAugment(
            option
        );
    }

    public void TryRerollFromUI()
    {
        if (!CanReroll)
        {
            return;
        }

        TryReroll();
    }

    // =========================================================
    // CHOICE GENERATION
    // =========================================================

    private void GenerateChoices()
    {
        List<AugmentOption> pool =
            CreateAugmentPool();

        for (int i = 0;
             i < currentChoices.Length;
             i++)
        {
            if (pool.Count <= 0)
            {
                currentChoices[i] = null;
                continue;
            }

            AugmentOption selected =
                SelectWeightedAugment(
                    pool
                );

            currentChoices[i] =
                selected;

            pool.Remove(
                selected
            );
        }
    }

    private AugmentOption SelectWeightedAugment(
        List<AugmentOption> pool)
    {
        float totalWeight = 0f;

        foreach (AugmentOption option in pool)
        {
            totalWeight +=
                option.BaseWeight *
                GetCategoryWeight(
                    option.Category
                );
        }

        float randomValue =
            UnityEngine.Random.Range(
                0f,
                totalWeight
            );

        float accumulatedWeight = 0f;

        foreach (AugmentOption option in pool)
        {
            accumulatedWeight +=
                option.BaseWeight *
                GetCategoryWeight(
                    option.Category
                );

            if (randomValue <=
                accumulatedWeight)
            {
                return option;
            }
        }

        return pool[
            pool.Count - 1
        ];
    }

    // =========================================================
    // AUGMENT POOL
    // =========================================================

    private List<AugmentOption>
        CreateAugmentPool()
    {
        List<AugmentOption> pool =
            new List<AugmentOption>();

        pool.Add(new AugmentOption(
            AugmentType.LandingNetPower,
            AugmentCategory.LandingNet,
            "강화 뜰채",
            "뜰채 포획력 +2"
        ));

        pool.Add(new AugmentOption(
            AugmentType.LandingNetRadius,
            AugmentCategory.LandingNet,
            "넓은 뜰채",
            "뜰채 범위 +0.25"
        ));

        pool.Add(new AugmentOption(
            AugmentType.CastNetPower,
            AugmentCategory.CastNet,
            "강화 투망",
            "투망 포획력 +5"
        ));

        pool.Add(new AugmentOption(
            AugmentType.CastNetRadius,
            AugmentCategory.CastNet,
            "대형 투망",
            "투망 범위 +0.25"
        ));

        pool.Add(new AugmentOption(
            AugmentType.CastNetCooldown,
            AugmentCategory.CastNet,
            "신속 투망",
            "투망 충전시간 -0.5초"
        ));

        pool.Add(new AugmentOption(
            AugmentType.BaitRadius,
            AugmentCategory.Bait,
            "강한 향",
            "미끼 유인 범위 +0.75"
        ));

        pool.Add(new AugmentOption(
            AugmentType.BaitDuration,
            AugmentCategory.Bait,
            "지속형 미끼",
            "미끼 지속시간 +1초"
        ));

        pool.Add(new AugmentOption(
            AugmentType.NetLength,
            AugmentCategory.Net,
            "긴 그물",
            "그물 최대 길이 +1"
        ));

        pool.Add(new AugmentOption(
            AugmentType.FishingRodPower,
            AugmentCategory.FishingRod,
            "강화 낚싯대",
            "낚싯대 포획력 +1"
        ));

        pool.Add(new AugmentOption(
            AugmentType.FishingRodRange,
            AugmentCategory.FishingRod,
            "장거리 낚시",
            "낚싯대 사거리 +0.5"
        ));

        pool.Add(new AugmentOption(
            AugmentType.FishingRodSpeed,
            AugmentCategory.FishingRod,
            "빠른 릴링",
            "낚싯대 공격 간격 -0.1초"
        ));

        AddUniqueAugments(
            pool
        );

        return pool;
    }

    private void AddUniqueAugments(
        List<AugmentOption> pool)
    {
        if (!acquiredUniqueAugments.Contains(
            AugmentType.CastNetFullHaul))
        {
            pool.Add(new AugmentOption(
                AugmentType.CastNetFullHaul,
                AugmentCategory.CastNet,
                "만선",
                "투망으로 한 번에 8마리 이상 포획하면\n" +
                "충전시간 2초 반환",
                0.7f,
                true
            ));
        }

        if (!acquiredUniqueAugments.Contains(
            AugmentType.FishingRodExtraHook))
        {
            pool.Add(new AugmentOption(
                AugmentType.FishingRodExtraHook,
                AugmentCategory.FishingRod,
                "추가 바늘",
                "낚싯대가 동시에 2마리를 공격",
                0.7f,
                true
            ));
        }

        if (!acquiredUniqueAugments.Contains(
            AugmentType.LandingNetChainCapture))
        {
            pool.Add(new AugmentOption(
                AugmentType.LandingNetChainCapture,
                AugmentCategory.LandingNet,
                "연쇄 포획",
                "뜰채로 물고기를 포획하면\n" +
                "주변 물고기 1마리에 추가 포획 피해",
                0.7f,
                true
            ));
        }
    }

    // =========================================================
    // REROLL
    // =========================================================

    private int GetRerollCost()
    {
        return
            rerollBaseCost +
            rerollCount *
            rerollCostIncrease;
    }

    private void TryReroll()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        int cost =
            GetRerollCost();

        if (!RunManager.Instance
            .TrySpendGold(
                cost
            ))
        {
            return;
        }

        rerollCount++;

        GenerateChoices();

        // 한 번의 클릭이 새 후보까지 바로 눌러버리는
        // 것을 방지한다.
        LockSelectionInput();
    }

    // =========================================================
    // APPLY
    // =========================================================

    private void ApplyAugment(
        AugmentOption option)
    {
        switch (option.Type)
        {
            case AugmentType.LandingNetPower:

                if (landingNet != null)
                {
                    landingNet
                        .IncreaseCapturePower(
                            2f
                        );
                }

                break;

            case AugmentType.LandingNetRadius:

                if (landingNet != null)
                {
                    landingNet
                        .IncreaseCaptureRadius(
                            0.25f
                        );
                }

                break;

            case AugmentType.LandingNetChainCapture:

                if (landingNet != null)
                {
                    landingNet
                        .EnableChainCapture();
                }

                break;

            case AugmentType.CastNetPower:

                if (castNet != null)
                {
                    castNet
                        .IncreaseCapturePower(
                            5f
                        );
                }

                break;

            case AugmentType.CastNetRadius:

                if (castNet != null)
                {
                    castNet
                        .IncreaseCaptureRadius(
                            0.25f
                        );
                }

                break;

            case AugmentType.CastNetCooldown:

                if (castNet != null)
                {
                    castNet
                        .ReduceCooldown(
                            0.5f
                        );
                }

                break;

            case AugmentType.CastNetFullHaul:

                if (castNet != null)
                {
                    castNet
                        .EnableMassCatchRefund();
                }

                break;

            case AugmentType.BaitRadius:

                if (bait != null)
                {
                    bait
                        .IncreaseAttractionRadius(
                            0.75f
                        );
                }

                break;

            case AugmentType.BaitDuration:

                if (bait != null)
                {
                    bait
                        .IncreaseDuration(
                            1f
                        );
                }

                break;

            case AugmentType.NetLength:

                if (netPlacement != null)
                {
                    netPlacement
                        .IncreaseMaxLength(
                            1f
                        );
                }

                break;

            case AugmentType.FishingRodPower:

                if (rodPlacement != null)
                {
                    rodPlacement
                        .IncreaseRodCapturePower(
                            1f
                        );
                }

                break;

            case AugmentType.FishingRodRange:

                if (rodPlacement != null)
                {
                    rodPlacement
                        .IncreaseRodRange(
                            0.5f
                        );
                }

                break;

            case AugmentType.FishingRodSpeed:

                if (rodPlacement != null)
                {
                    rodPlacement
                        .ReduceRodAttackInterval(
                            0.1f
                        );
                }

                break;

            case AugmentType.FishingRodExtraHook:

                if (rodPlacement != null)
                {
                    rodPlacement
                        .AddRodAdditionalTarget(
                            1
                        );
                }

                break;
        }

        if (option.IsUnique)
        {
            acquiredUniqueAugments.Add(
                option.Type
            );
        }

        FinishSelection();
    }

    private void FinishSelection()
    {
        showChoices = false;
        canSelect = false;

        Time.timeScale = 1f;

        if (RunManager.Instance != null)
        {
            RunManager.Instance
                .ResolveLevelUp();
        }
    }

    // =========================================================
    // CATEGORY WEIGHT
    // =========================================================

    public void SetCategoryWeight(
        AugmentCategory category,
        float weight)
    {
        categoryWeights[category] =
            Mathf.Max(
                0f,
                weight
            );
    }

    public float GetCategoryWeight(
        AugmentCategory category)
    {
        if (categoryWeights.TryGetValue(
            category,
            out float weight))
        {
            return weight;
        }

        return 1f;
    }

    public void ResetCategoryWeights()
    {
        categoryWeights.Clear();

        Array categories =
            Enum.GetValues(
                typeof(AugmentCategory)
            );

        foreach (
            AugmentCategory category
            in categories)
        {
            categoryWeights[category] =
                1f;
        }
    }

    private void OnDisable()
    {
        if (showChoices)
        {
            Time.timeScale = 1f;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}