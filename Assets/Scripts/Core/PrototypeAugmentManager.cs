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

    private bool canSelect;
    private float selectionUnlockTime;
    private bool showChoices;

    public bool IsChoosingAugment =>
        showChoices;

    private AugmentOption[] currentChoices =
        new AugmentOption[3];

    private readonly Dictionary<
        AugmentCategory,
        float
    > categoryWeights = new();

    private readonly HashSet<AugmentType>
        acquiredUniqueAugments = new();

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
        if (!showChoices || canSelect)
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

    public void ShowChoices()
    {
        if (showChoices)
        {
            return;
        }

        GenerateChoices();

        showChoices = true;
        canSelect = false;

        selectionUnlockTime =
            Time.realtimeSinceStartup
            + selectionInputDelay;

        Time.timeScale = 0f;
    }

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

            pool.Remove(selected);
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

    private List<AugmentOption>
        CreateAugmentPool()
    {
        List<AugmentOption> pool =
            new List<AugmentOption>();

        pool.Add(
            new AugmentOption(
                AugmentType.LandingNetPower,
                AugmentCategory.LandingNet,
                "강화 뜰채",
                "뜰채 포획력 +2"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.LandingNetRadius,
                AugmentCategory.LandingNet,
                "넓은 뜰채",
                "뜰채 범위 +0.25"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.CastNetPower,
                AugmentCategory.CastNet,
                "강화 투망",
                "투망 포획력 +5"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.CastNetRadius,
                AugmentCategory.CastNet,
                "대형 투망",
                "투망 범위 +0.25"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.CastNetCooldown,
                AugmentCategory.CastNet,
                "신속 투망",
                "투망 재사용 대기시간 -0.5초"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.BaitRadius,
                AugmentCategory.Bait,
                "강한 향",
                "미끼 유인 범위 +0.75"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.BaitDuration,
                AugmentCategory.Bait,
                "지속형 미끼",
                "미끼 지속시간 +1초"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.NetLength,
                AugmentCategory.Net,
                "긴 그물",
                "그물 최대 길이 +1"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.FishingRodPower,
                AugmentCategory.FishingRod,
                "강화 낚싯대",
                "낚싯대 포획력 +1"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.FishingRodRange,
                AugmentCategory.FishingRod,
                "장거리 낚시",
                "낚싯대 사거리 +0.5"
            )
        );

        pool.Add(
            new AugmentOption(
                AugmentType.FishingRodSpeed,
                AugmentCategory.FishingRod,
                "빠른 릴링",
                "낚싯대 공격 간격 -0.1초"
            )
        );

        AddUniqueAugments(pool);

        return pool;
    }

    private void AddUniqueAugments(
        List<AugmentOption> pool)
    {
        if (!acquiredUniqueAugments.Contains(
            AugmentType.CastNetFullHaul))
        {
            pool.Add(
                new AugmentOption(
                    AugmentType.CastNetFullHaul,
                    AugmentCategory.CastNet,
                    "만선",
                    "투망으로 한 번에 8마리 이상 포획하면\n재사용 대기시간 2초 반환",
                    0.7f,
                    true
                )
            );
        }

        if (!acquiredUniqueAugments.Contains(
            AugmentType.FishingRodExtraHook))
        {
            pool.Add(
                new AugmentOption(
                    AugmentType.FishingRodExtraHook,
                    AugmentCategory.FishingRod,
                    "추가 바늘",
                    "낚싯대가 동시에 2마리를 공격",
                    0.7f,
                    true
                )
            );
        }

        if (!acquiredUniqueAugments.Contains(
            AugmentType.LandingNetChainCapture))
        {
            pool.Add(
                new AugmentOption(
                    AugmentType.LandingNetChainCapture,
                    AugmentCategory.LandingNet,
                    "연쇄 포획",
                    "뜰채로 물고기를 포획하면\n주변 물고기 1마리에 추가 포획 피해",
                    0.7f,
                    true
                )
            );
        }
    }

    private void OnGUI()
    {
        if (!showChoices)
        {
            return;
        }

        float width = 240f;
        float height = 120f;
        float spacing = 20f;

        float totalWidth =
            width * 3f +
            spacing * 2f;

        float startX =
            (Screen.width -
             totalWidth) *
            0.5f;

        float y =
            Screen.height * 0.5f -
            height * 0.5f;

        GUI.Box(
            new Rect(
                Screen.width * 0.5f -
                150f,
                y - 80f,
                300f,
                50f
            ),
            "레벨 업! 증강을 선택하세요"
        );

        for (int i = 0;
             i < currentChoices.Length;
             i++)
        {
            AugmentOption option =
                currentChoices[i];

            if (option == null)
            {
                continue;
            }

            Rect rect =
                new Rect(
                    startX +
                    i *
                    (width + spacing),
                    y,
                    width,
                    height
                );

            GUI.enabled =
                canSelect;

            string buttonText =
                $"{option.Name}\n\n" +
                option.Description;

            if (GUI.Button(
                rect,
                buttonText))
            {
                ApplyAugment(
                    option
                );
            }

            GUI.enabled = true;
        }
    }

    private void ApplyAugment(
        AugmentOption option)
    {
        switch (option.Type)
        {
            case AugmentType.LandingNetPower:
                landingNet
                    .IncreaseCapturePower(
                        2f
                    );
                break;

            case AugmentType.LandingNetRadius:
                landingNet
                    .IncreaseCaptureRadius(
                        0.25f
                    );
                break;

            case AugmentType.LandingNetChainCapture:
                landingNet
                    .EnableChainCapture();
                break;

            case AugmentType.CastNetPower:
                castNet
                    .IncreaseCapturePower(
                        5f
                    );
                break;

            case AugmentType.CastNetRadius:
                castNet
                    .IncreaseCaptureRadius(
                        0.25f
                    );
                break;

            case AugmentType.CastNetCooldown:
                castNet
                    .ReduceCooldown(
                        0.5f
                    );
                break;

            case AugmentType.CastNetFullHaul:
                castNet
                    .EnableMassCatchRefund();
                break;

            case AugmentType.BaitRadius:
                bait
                    .IncreaseAttractionRadius(
                        0.75f
                    );
                break;

            case AugmentType.BaitDuration:
                bait
                    .IncreaseDuration(
                        1f
                    );
                break;

            case AugmentType.NetLength:
                netPlacement
                    .IncreaseMaxLength(
                        1f
                    );
                break;

            case AugmentType.FishingRodPower:
                rodPlacement
                    .IncreaseRodCapturePower(
                        1f
                    );
                break;

            case AugmentType.FishingRodRange:
                rodPlacement
                    .IncreaseRodRange(
                        0.5f
                    );
                break;

            case AugmentType.FishingRodSpeed:
                rodPlacement
                    .ReduceRodAttackInterval(
                        0.1f
                    );
                break;

            case AugmentType.FishingRodExtraHook:
                rodPlacement
                    .AddRodAdditionalTarget(
                        1
                    );
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

        Time.timeScale = 1f;

        if (RunManager.Instance != null)
        {
            RunManager.Instance
                .ResolveLevelUp();
        }
    }

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
                typeof(
                    AugmentCategory
                )
            );

        foreach (
            AugmentCategory category
            in categories)
        {
            categoryWeights[
                category
            ] = 1f;
        }
    }

    private void OnDisable()
    {
        if (showChoices)
        {
            Time.timeScale = 1f;
        }
    }
}