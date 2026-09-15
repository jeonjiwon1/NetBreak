using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PrototypeGameFlowManager : MonoBehaviour
{
    public static PrototypeGameFlowManager Instance
    {
        get;
        private set;
    }

    [Header("Catch Rank")]
    [Range(0f, 1f)]
    [SerializeField] private float bRankRate = 0.70f;

    [Range(0f, 1f)]
    [SerializeField] private float aRankRate = 0.80f;

    [Range(0f, 1f)]
    [SerializeField] private float sRankRate = 0.90f;

    [Range(0f, 1f)]
    [SerializeField] private float perfectRankRate = 0.999f;

    [Header("References")]
    [SerializeField] private FishSpawner fishSpawner;

    private bool isFishingStarted;
    private bool isBossEncounter;

    private bool isGameEnded;
    private bool isSuccess;
    private string failureDescription;
    private float activeRunTime;
    private float currentTestSpeedMultiplier = 1f;

    public bool IsBossEncounter =>
        isBossEncounter;

    public bool IsGameEnded =>
        isGameEnded;

    public bool IsSuccess =>
        isSuccess;

    public bool IsPreparation =>
        !isFishingStarted &&
        !isGameEnded;

    public bool IsFishingStarted =>
        isFishingStarted;

    public float ActiveRunTime =>
        activeRunTime;

    public float CurrentTestSpeedMultiplier =>
        currentTestSpeedMultiplier;

    public string ResultTitle
    {
        get
        {
            if (!isGameEnded)
            {
                return "";
            }

            return isSuccess
                ? "연안 조업 성공!"
                : "조업 실패";
        }
    }

    public string ResultDescription
    {
        get
        {
            if (!isGameEnded)
            {
                return "";
            }

            if (!string.IsNullOrEmpty(failureDescription))
            {
                return failureDescription;
            }
            return isSuccess
                ? "최종 보스를 포획했습니다."
                : "최종 보스가 마지막 회유에서 도주했습니다.";
        }
    }

    public string CatchRank
    {
        get
        {
            if (isGameEnded && !isSuccess)
            {
                return "F";
            }

            float catchRate = 0f;

            if (RunManager.Instance != null)
            {
                catchRate =
                    RunManager.Instance.CatchRate;
            }

            return GetCatchRank(
                catchRate
            );
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
    }

    private void Update()
    {
        UpdateTestSpeedInput();

        if (isFishingStarted && !isGameEnded)
        {
            activeRunTime += Time.deltaTime;
        }
    }

    private void UpdateTestSpeedInput()
    {
        if (!CanUseTestSpeed() ||
            Keyboard.current == null ||
            isGameEnded)
        {
            return;
        }

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            SetTestSpeed1x();
        }
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            SetTestSpeed2x();
        }
        else if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            SetTestSpeed3x();
        }
    }

    public void SetTestSpeed1x()
    {
        SetTestSpeed(1f);
    }

    public void SetTestSpeed2x()
    {
        SetTestSpeed(2f);
    }

    public void SetTestSpeed3x()
    {
        SetTestSpeed(3f);
    }

    private void SetTestSpeed(
        float multiplier)
    {
        if (!CanUseTestSpeed())
        {
            currentTestSpeedMultiplier = 1f;
            return;
        }

        currentTestSpeedMultiplier =
            Mathf.Clamp(
                multiplier,
                1f,
                3f
            );

        if (Time.timeScale > 0f &&
            !isGameEnded)
        {
            Time.timeScale =
                currentTestSpeedMultiplier;
        }
    }

    public void ResumeGameplayTimeScale()
    {
        if (isGameEnded)
        {
            return;
        }

        Time.timeScale =
            CanUseTestSpeed()
                ? currentTestSpeedMultiplier
                : 1f;
    }

    private static bool CanUseTestSpeed()
    {
        return Application.isEditor ||
            Debug.isDebugBuild;
    }
    // =========================================================
    // FISHING START
    // =========================================================

    public void StartFishing()
    {
        if (isFishingStarted ||
            isGameEnded)
        {
            return;
        }

        isFishingStarted = true;

        if (fishSpawner != null)
        {
            fishSpawner.StartSpawning();
        }
    }

    // =========================================================
    // BOSS ENCOUNTER
    // =========================================================

    public void BeginBossEncounter()
    {
        if (isGameEnded)
        {
            return;
        }

        isBossEncounter = true;
    }

    public void CompleteBossEncounter(
        bool bossCaptured)
    {
        if (isGameEnded)
        {
            return;
        }

        ResolveUnfinishedFishForResult();

        isBossEncounter = false;
        isGameEnded = true;
        isSuccess = bossCaptured;

        Time.timeScale = 0f;
    }

    public void FailMiniBossEncounter()
    {
        if (isGameEnded)
        {
            return;
        }

        ResolveUnfinishedFishForResult();

        isBossEncounter = false;
        isGameEnded = true;
        isSuccess = false;
        failureDescription = "미니보스 포획 실패";

        Time.timeScale = 0f;
    }
    // =========================================================
    // UNRESOLVED FISH
    // =========================================================

    private void ResolveUnfinishedFishForResult()
    {
        FishController[] activeFish =
            FindObjectsByType<FishController>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        if (activeFish == null ||
            activeFish.Length == 0)
        {
            return;
        }

        foreach (FishController fish
                 in activeFish)
        {
            if (fish == null)
            {
                continue;
            }

            if (!fish.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (fish.Data == null)
            {
                continue;
            }

            if (fish.IsCaptured)
            {
                continue;
            }

            // 최종 보스는 BossEncounterController가
            // 성공/실패 결과를 별도로 처리한다.
            if (fish.Data.SpecialType ==
                FishSpecialType.Boss)
            {
                continue;
            }

            if (RunManager.Instance != null)
            {
                RunManager.Instance
                    .ExcludeUnresolvedFish(
                        fish.Data
                    );
            }

            fish.gameObject.SetActive(
                false
            );
        }
    }

    // =========================================================
    // CATCH RANK
    // =========================================================

    private string GetCatchRank(
        float catchRate)
    {
        if (catchRate >= perfectRankRate)
        {
            return "PERFECT";
        }

        if (catchRate >= sRankRate)
        {
            return "S";
        }

        if (catchRate >= aRankRate)
        {
            return "A";
        }

        if (catchRate >= bRankRate)
        {
            return "B";
        }

        return "C";
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void RestartPrototype()
    {
        Time.timeScale = 1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void OnDisable()
    {
        Time.timeScale = 1f;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        bRankRate =
            Mathf.Clamp01(
                bRankRate
            );

        aRankRate =
            Mathf.Max(
                bRankRate,
                Mathf.Clamp01(
                    aRankRate
                )
            );

        sRankRate =
            Mathf.Max(
                aRankRate,
                Mathf.Clamp01(
                    sRankRate
                )
            );

        perfectRankRate =
            Mathf.Max(
                sRankRate,
                Mathf.Clamp01(
                    perfectRankRate
                )
            );
    }
}