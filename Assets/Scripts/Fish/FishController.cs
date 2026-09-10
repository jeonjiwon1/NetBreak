using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private FishData fishData;

    private float currentResistance;
    private bool isCaptured;

    private SpriteRenderer spriteRenderer;

    public FishData Data => fishData;
    public float CurrentResistance => currentResistance;

    private void Awake()
    {
        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(FishData data)
    {
        fishData = data;

        currentResistance =
            fishData.MaxResistance;

        isCaptured = false;

        transform.localScale =
            new Vector3(
                fishData.VisualScale.x,
                fishData.VisualScale.y,
                1f
            );

        if (spriteRenderer != null)
        {
            spriteRenderer.color =
                fishData.VisualColor;
        }

        gameObject.name =
            $"Fish_{fishData.FishName}";
    }

    private void OnEnable()
    {
        if (fishData != null)
        {
            currentResistance =
                fishData.MaxResistance;

            isCaptured = false;
        }
    }

    public bool TakeCaptureDamage(float amount)
    {
        if (fishData == null ||
            isCaptured)
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
            RunManager.Instance
                .RegisterFishCaptured(
                    fishData
                );
        }

        gameObject.SetActive(false);
    }
}