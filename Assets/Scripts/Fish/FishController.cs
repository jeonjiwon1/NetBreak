using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private FishData fishData;

    private float currentResistance;

    public FishData Data => fishData;
    public float CurrentResistance => currentResistance;

    private void OnEnable()
    {
        ResetFish();
    }

    private void ResetFish()
    {
        if (fishData == null)
        {
            return;
        }

        currentResistance = fishData.MaxResistance;
    }

    public void TakeCaptureDamage(float amount)
    {
        if (fishData == null)
        {
            return;
        }

        currentResistance -= amount;

        Debug.Log(
            $"{fishData.FishName} Resistance: " +
            $"{currentResistance}/{fishData.MaxResistance}"
        );

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