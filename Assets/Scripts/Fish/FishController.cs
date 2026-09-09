using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private FishData fishData;

    private float currentResistance;
    private bool isCaptured;

    public FishData Data => fishData;
    public float CurrentResistance => currentResistance;

    public void Initialize(FishData data)
    {
        fishData = data;

        currentResistance = fishData.MaxResistance;
        isCaptured = false;

        gameObject.name = $"Fish_{fishData.FishName}";
    }

    private void OnEnable()
    {
        if (fishData != null)
        {
            currentResistance = fishData.MaxResistance;
        }
    }

    public bool TakeCaptureDamage(float amount)
    {
        if (fishData == null || isCaptured)
        {
            return false;
        }

        currentResistance -= amount;

        if (currentResistance <= 0f)
        {
            Capture();
            return true;
        }

        return false;
    }

    private void Capture()
    {
        if (isCaptured)
        {
            return;
        }

        isCaptured = true;

        if (RunManager.Instance != null)
        {
            RunManager.Instance.RegisterFishCaptured(fishData);
        }

        gameObject.SetActive(false);
    }
}