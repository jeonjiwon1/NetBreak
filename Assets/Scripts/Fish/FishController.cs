using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private FishData fishData;

    private float currentResistance;

    public FishData Data => fishData;
    public float CurrentResistance => currentResistance;

    public void Initialize(FishData data)
    {
        fishData = data;
        currentResistance = fishData.MaxResistance;

        gameObject.name = $"Fish_{fishData.FishName}";
    }

    private void OnEnable()
    {
        if (fishData != null)
        {
            currentResistance = fishData.MaxResistance;
        }
    }

    public void TakeCaptureDamage(float amount)
    {
        if (fishData == null)
        {
            return;
        }

        currentResistance -= amount;

        if (currentResistance <= 0f)
        {
            Capture();
        }
    }

    private void Capture()
    {
        Debug.Log($"{fishData.FishName} CAPTURED!");

        gameObject.SetActive(false);
    }
}