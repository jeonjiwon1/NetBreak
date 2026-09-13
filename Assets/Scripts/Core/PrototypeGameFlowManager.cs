using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeGameFlowManager : MonoBehaviour
{
    public static PrototypeGameFlowManager Instance
    {
        get;
        private set;
    }

    [Header("Legacy Final Fishing")]
    [SerializeField] private float finalFishingDuration = 20f;

    [Header("Legacy Clear")]
    [Range(0f, 1f)]
    [SerializeField] private float clearCatchRate = 0.8f;

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

    private bool isFinalFishing;
    private bool isBossEncounter;

    private bool isGameEnded;
    private bool isSuccess;

    private float finalFishingTimer;

    public bool IsFinalFishing =>
        isFinalFishing;

    public bool IsBossEncounter =>
        isBossEncounter;

    public bool IsGameEnded =>
        isGameEnded;

    public bool IsSuccess =>
        isSuccess;

    public float FinalFishingTimer =>
        Mathf.Max(
            0f,
            finalFishingTimer
        );

    // 이전 코드와의 호환용.
    public float ClearCatchRate =>
        clearCatchRate;

    public bool IsPreparation =>
        !isFishingStarted &&
        !isGameEnded;

    public bool IsFishingStarted =>
        isFishingStarted;

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

            return isSuccess
                ? "최종 보스를 포획했습니다."
                : "최종 보스가 마지막 회유에서 도주했습니다.";
        }
    }

    public string CatchRank
    {
        get
        {
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
        if (!isFinalFishing ||
            isBossEncounter ||
            isGameEnded)
        {
            return;
        }

        finalFishingTimer -=
            Time.deltaTime;

        if (finalFishingTimer <= 0f)
        {
            EndLegacyFinalFishing();
        }
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

        isFishingStarted =
            true;

        if (fishSpawner != null)
        {
            fishSpawner
                .StartSpawning();
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

        isFinalFishing =
            false;

        isBossEncounter =
            true;
    }

    public void CompleteBossEncounter(
        bool bossCaptured)
    {
        if (isGameEnded)
        {
            return;
        }

        // 결과를 확정하기 전에
        // 아직 화면에 남아 있는 일반 물고기를
        // 어획률 계산에서 제외한다.
        ResolveUnfinishedFishForResult();

        isBossEncounter =
            false;

        isFinalFishing =
            false;

        isGameEnded =
            true;

        isSuccess =
            bossCaptured;

        Time.timeScale =
            0f;
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

            // 이미 포획 처리된 물고기는
            // 정상적으로 결과에 포함한다.
            if (fish.IsCaptured)
            {
                continue;
            }

            // 최종 Boss는 별도의 Encounter 결과로 처리하므로
            // 여기서 CatchValue를 빼지 않는다.
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

            // 결과 화면 뒤에서 계속 남아 있지 않도록 제거.
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
        if (catchRate >=
            perfectRankRate)
        {
            return "PERFECT";
        }

        if (catchRate >=
            sRankRate)
        {
            return "S";
        }

        if (catchRate >=
            aRankRate)
        {
            return "A";
        }

        if (catchRate >=
            bRankRate)
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
        Time.timeScale =
            1f;

        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.buildIndex
        );
    }

    // =========================================================
    // LEGACY FINAL FISHING
    // =========================================================

    public void BeginFinalFishing()
    {
        if (isFinalFishing ||
            isBossEncounter ||
            isGameEnded)
        {
            return;
        }

        isFinalFishing =
            true;

        finalFishingTimer =
            finalFishingDuration;
    }

    private void EndLegacyFinalFishing()
    {
        // 기존 Final Fishing에서도 같은 규칙을 적용.
        ResolveUnfinishedFishForResult();

        isFinalFishing =
            false;

        isGameEnded =
            true;

        float catchRate = 0f;

        if (RunManager.Instance != null)
        {
            catchRate =
                RunManager.Instance.CatchRate;
        }

        isSuccess =
            catchRate >=
            clearCatchRate;

        Time.timeScale =
            0f;
    }

    // =========================================================
    // UNITY
    // =========================================================

    private void OnDisable()
    {
        Time.timeScale =
            1f;

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