using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    private int currentGold;
    private int capturedFishCount;

    private int capturedCatchValue;
    private int totalCatchValue;

    private int currentLevel = 1;
    private int currentExp = 0;
    private int expToNextLevel = 10;

    private bool levelUpPending;

    public int CurrentGold => currentGold;
    public int CapturedFishCount => capturedFishCount;

    public int CapturedCatchValue => capturedCatchValue;
    public int TotalCatchValue => totalCatchValue;

    public int CurrentLevel => currentLevel;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => expToNextLevel;

    public float CatchRate
    {
        get
        {
            if (totalCatchValue <= 0)
            {
                return 0f;
            }

            return (float)capturedCatchValue / totalCatchValue;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterFishSpawned(FishData fishData)
    {
        if (fishData == null)
        {
            return;
        }

        totalCatchValue += fishData.CatchValue;
    }

    public void RegisterFishCaptured(FishData fishData)
    {
        if (fishData == null)
        {
            return;
        }

        capturedFishCount++;
        capturedCatchValue += fishData.CatchValue;

        currentGold += fishData.GoldReward;
        currentExp += fishData.ExpReward;

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (levelUpPending)
        {
            return;
        }

        if (currentExp < expToNextLevel)
        {
            return;
        }

        currentExp -= expToNextLevel;

        currentLevel++;

        expToNextLevel =
            Mathf.RoundToInt(expToNextLevel * 1.35f);

        levelUpPending = true;

        if (PrototypeAugmentManager.Instance != null)
        {
            PrototypeAugmentManager.Instance.ShowChoices();
        }
    }

    public void ResolveLevelUp()
    {
        levelUpPending = false;

        CheckLevelUp();
    }
}