using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RunManager : MonoBehaviour
{
    public RunToolLoadout ToolSlots { get; private set; }
    public RunGrowthState GrowthState { get; private set; }
    public ToolSlotInput ToolInput { get; private set; }
    public SkillTreeManager SkillTree { get; private set; }
    public TacticalSkillManager TacticalSkills { get; private set; }
    public SignatureSkillManager SignatureSkills { get; private set; }
    public ItemRewardManager ItemRewards { get; private set; }
    // Legacy manager reads this guard. Random level-up Augments are retired by G3.
    public bool AreAugmentsUnlocked => false;

    public static RunManager Instance
    {
        get;
        private set;
    }

    [Header("Starting Resources")]
    [SerializeField] private int startingGold = 60;

    [Header("Experience Balance")]
    [Min(1)] [SerializeField] private int level2ExpRequirement = 30;
    [Min(1)] [SerializeField] private int level3ExpRequirement = 80;
    [Min(1)] [SerializeField] private int level4ExpRequirement = 120;
    [Min(1)] [SerializeField] private int level4PlusBaseExpRequirement = 120;
    [Min(0.01f)] [SerializeField] private float level4PlusGrowthMultiplier = 1.30f;

    [Header("Growth Rewards")]
    [Min(1)] [SerializeField] private int masteryPointsPerLevel = 1;
    [Min(1)] [SerializeField] private int miniBossMasteryReward = 1;
    [Min(1)] [SerializeField] private int bossMasteryReward = 2;

    private int currentGold;
    private int capturedFishCount;

    private int capturedCatchValue;
    private int totalCatchValue;

    private int currentLevel = 1;
    private int currentExp = 0;
    private int expToNextLevel;

    private bool levelUpPending;

    public int CurrentGold =>
        currentGold;

    public int CapturedFishCount =>
        capturedFishCount;

    public int CapturedCatchValue =>
        capturedCatchValue;

    public int TotalCatchValue =>
        totalCatchValue;

    public int CurrentLevel =>
        currentLevel;

    public int CurrentExp =>
        currentExp;

    public int ExpToNextLevel =>
        expToNextLevel;

    public float CatchRate
    {
        get
        {
            if (totalCatchValue <= 0)
            {
                return 0f;
            }

            return
                (float)capturedCatchValue /
                totalCatchValue;
        }
    }

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ToolSlots = new RunToolLoadout();
        GrowthState = new RunGrowthState();
        ToolInput = new ToolSlotInput(ToolSlots);
        SkillTree = GetComponent<SkillTreeManager>();
        if (SkillTree == null)
        {
            SkillTree = gameObject.AddComponent<SkillTreeManager>();
        }

        TacticalSkills = GetComponent<TacticalSkillManager>();
        if (TacticalSkills == null)
        {
            TacticalSkills = gameObject.AddComponent<TacticalSkillManager>();
        }

        SignatureSkills = GetComponent<SignatureSkillManager>();
        if (SignatureSkills == null)
        {
            SignatureSkills = gameObject.AddComponent<SignatureSkillManager>();
        }

        ItemRewards = GetComponent<ItemRewardManager>();
        if (ItemRewards == null)
        {
            ItemRewards = gameObject.AddComponent<ItemRewardManager>();
        }

        currentGold =
            startingGold;

        expToNextLevel =
            level2ExpRequirement;
    }

    private void Update()
    {
        ToolInput.Sample();
    }

    // =========================================================
    // FISH RESULT
    // =========================================================

    public void RegisterFishSpawned(
        FishData fishData)
    {
        if (fishData == null)
        {
            return;
        }

        totalCatchValue +=
            fishData.CatchValue;
    }

    public void RegisterFishCaptured(
        FishData fishData)
    {
        if (fishData == null)
        {
            return;
        }

        capturedFishCount++;

        capturedCatchValue +=
            fishData.CatchValue;

        currentGold +=
            fishData.GoldReward;

        currentExp +=
            fishData.ExpReward;

        switch (fishData.SpecialType)
        {
            case FishSpecialType.MiniBoss:
                TryGrantMiniBossMasteryReward();
                break;
            case FishSpecialType.Boss:
                // Fish capture is registered before the Boss result flow ends
                // the Run, so the final reward is retained in RunGrowthState.
                TryGrantBossMasteryReward();
                break;
        }

        CheckLevelUp();
    }

    // 게임 종료 순간 아직 맵에 남아 있어
    // 포획/도주 결과가 확정되지 않은 물고기를
    // 최종 어획률 계산에서 제외한다.
    public void ExcludeUnresolvedFish(
        FishData fishData)
    {
        if (fishData == null)
        {
            return;
        }

        int catchValueToRemove =
            Mathf.Max(
                0,
                fishData.CatchValue
            );

        totalCatchValue =
            Mathf.Max(
                capturedCatchValue,
                totalCatchValue -
                catchValueToRemove
            );
    }

    // =========================================================
    // BONUS
    // =========================================================

    public void GrantBonusReward(
        int gold,
        int exp)
    {
        if (gold > 0)
        {
            currentGold +=
                gold;
        }

        if (exp > 0)
        {
            currentExp +=
                exp;

            CheckLevelUp();
        }
    }

    public bool TryGrantMiniBossMasteryReward() =>
        GrowthState != null &&
        GrowthState.TryGrantMilestoneMasteryPoints(
            RunMilestoneType.MiniBoss,
            GrowthState.CurrentArea,
            miniBossMasteryReward);

    public bool TryGrantBossMasteryReward()
    {
        if (GrowthState == null ||
            !GrowthState.TryGrantMilestoneMasteryPoints(
                RunMilestoneType.Boss,
                GrowthState.CurrentArea,
                bossMasteryReward))
        {
            return false;
        }

        SignatureSkills?.UnlockForBossReward();
        return true;
    }

    // =========================================================
    // GOLD
    // =========================================================

    public bool TrySpendGold(
        int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (currentGold < amount)
        {
            return false;
        }

        currentGold -=
            amount;

        return true;
    }

    // =========================================================
    // LEVEL
    // =========================================================

    private void CheckLevelUp()
    {
        if (levelUpPending)
        {
            return;
        }

        if (currentExp <
            expToNextLevel)
        {
            return;
        }

        currentExp -=
            expToNextLevel;

        currentLevel++;

        GrowthState.TryGrantMasteryPoints(masteryPointsPerLevel);

        expToNextLevel =
            CalculateExpRequirement(
                currentLevel
            );

        levelUpPending =
            true;

        TryShowPendingLevelReward();
    }

    private int CalculateExpRequirement(
        int level)
    {
        if (level <= 1)
        {
            return level2ExpRequirement;
        }

        if (level == 2)
        {
            return level3ExpRequirement;
        }

        if (level == 3)
        {
            return level4ExpRequirement;
        }

        return Mathf.RoundToInt(
            level4PlusBaseExpRequirement *
            Mathf.Pow(
                level4PlusGrowthMultiplier,
                level - 3
            )
        );
    }

    private void TryShowPendingLevelReward()
    {
        if (!levelUpPending)
        {
            return;
        }

        if (currentLevel == 2 || currentLevel == 3)
        {
            if (SkillTree != null)
            {
                SkillTree.RequestMandatoryAcquisition(
                    currentLevel == 2
                        ? GrowthToolRole.Core
                        : GrowthToolRole.Partner);
            }

            return;
        }

        if (SkillTree != null)
        {
            SkillTree.NotifyMasteryPointGranted(
                masteryPointsPerLevel);
        }


        ResolveLevelUp();
    }

    public void ResolveLevelUp()
    {
        levelUpPending =
            false;

        CheckLevelUp();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
