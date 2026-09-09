using UnityEngine;

public class PrototypeGameFlowManager : MonoBehaviour
{
    public static PrototypeGameFlowManager Instance
    {
        get;
        private set;
    }

    [Header("Final Fishing")]
    [SerializeField] private float finalFishingDuration = 20f;

    [Header("Clear")]
    [Range(0f, 1f)]
    [SerializeField] private float clearCatchRate = 0.8f;

    [SerializeField] private FishSpawner fishSpawner;

    private bool isFishingStarted;

    private bool isFinalFishing;
    private bool isGameEnded;
    private bool isSuccess;

    private float finalFishingTimer;

    public bool IsFinalFishing => isFinalFishing;
    public bool IsGameEnded => isGameEnded;
    public bool IsSuccess => isSuccess;

    public float FinalFishingTimer =>
        Mathf.Max(0f, finalFishingTimer);

    public float ClearCatchRate => clearCatchRate;

    public bool IsPreparation => !isFishingStarted && !isGameEnded;
    public bool IsFishingStarted => isFishingStarted;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isFinalFishing ||
            isGameEnded)
        {
            return;
        }

        finalFishingTimer -= Time.deltaTime;

        if (finalFishingTimer <= 0f)
        {
            EndGame();
        }
    }

    public void BeginFinalFishing()
    {
        if (isFinalFishing ||
            isGameEnded)
        {
            return;
        }

        isFinalFishing = true;
        finalFishingTimer = finalFishingDuration;
    }

    private void EndGame()
    {
        isFinalFishing = false;
        isGameEnded = true;

        float catchRate = 0f;

        if (RunManager.Instance != null)
        {
            catchRate =
                RunManager.Instance.CatchRate;
        }

        isSuccess =
            catchRate >= clearCatchRate;

        Time.timeScale = 0f;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void StartFishing()
    {
        if (isFishingStarted || isGameEnded)
        {
            return;
        }

        isFishingStarted = true;

        if (fishSpawner != null)
        {
            fishSpawner.StartSpawning();
        }
    }
}