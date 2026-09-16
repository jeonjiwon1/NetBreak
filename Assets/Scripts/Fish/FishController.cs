using System;
using UnityEngine;

public class FishController : MonoBehaviour
{
    [SerializeField] private FishData fishData;

    private float currentResistance;
    private bool isCaptured;

    private SpriteRenderer spriteRenderer;

    public FishData Data =>
        fishData;

    public float CurrentResistance =>
        currentResistance;

    public bool IsCaptured =>
        isCaptured;

    public float ResistanceRatio
    {
        get
        {
            if (fishData == null ||
                fishData.MaxResistance <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                currentResistance /
                fishData.MaxResistance
            );
        }
    }

    public event Action<FishController> Captured;

    private void Awake()
    {
        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();
    }

    public void Initialize(
        FishData data)
    {
        fishData = data;

        if (fishData == null)
        {
            currentResistance = 0f;
            isCaptured = false;
            Captured = null;

            return;
        }

        currentResistance =
            fishData.MaxResistance;

        isCaptured = false;

        // Pool에서 이전 사용 시 등록된 이벤트가
        // 다음 물고기에게 남지 않도록 초기화.
        Captured = null;

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

    // 보스의 다음 회유에서 이전 Resistance를
    // 일부 회복한 값으로 복원하기 위해 사용한다.
    public void SetCurrentResistance(
        float resistance)
    {
        if (fishData == null)
        {
            return;
        }

        currentResistance =
            Mathf.Clamp(
                resistance,
                0f,
                fishData.MaxResistance
            );
    }

    public bool TakeCaptureDamage(
        float amount)
    {
        if (fishData == null ||
            isCaptured)
        {
            return false;
        }

        if (amount <= 0f)
        {
            return false;
        }

        float multiplier = TacticalSkillManager.Instance != null
            ? TacticalSkillManager.Instance.GetCaptureDamageMultiplier(transform.position)
            : 1f;

        currentResistance -= amount * multiplier;

        if (currentResistance <= 0f)
        {
            currentResistance = 0f;

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

        Captured?.Invoke(
            this
        );

        gameObject.SetActive(
            false
        );
    }
}