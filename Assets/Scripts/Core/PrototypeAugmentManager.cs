using System;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeAugmentManager : MonoBehaviour
{
    public static PrototypeAugmentManager Instance
    {
        get;
        private set;
    }

    [Header("References")]
    [SerializeField] private LandingNetController landingNet;
    [SerializeField] private CastNetController castNet;
    [SerializeField] private BaitController bait;
    [SerializeField] private NetPlacementController netPlacement;

    private bool showChoices;

    private AugmentType[] currentChoices =
        new AugmentType[3];

    private enum AugmentType
    {
        LandingNetPower,
        LandingNetRadius,

        CastNetPower,
        CastNetRadius,
        CastNetCooldown,

        BaitRadius,
        BaitDuration,

        NetLength
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowChoices()
    {
        GenerateChoices();

        showChoices = true;

        Time.timeScale = 0f;
    }

    private void GenerateChoices()
    {
        List<AugmentType> pool =
            new List<AugmentType>(
                (AugmentType[])Enum.GetValues(
                    typeof(AugmentType)
                )
            );

        for (int i = 0; i < 3; i++)
        {
            int index =
                UnityEngine.Random.Range(
                    0,
                    pool.Count
                );

            currentChoices[i] = pool[index];

            pool.RemoveAt(index);
        }
    }

    private void OnGUI()
    {
        if (!showChoices)
        {
            return;
        }

        float width = 240f;
        float height = 120f;
        float spacing = 20f;

        float totalWidth =
            width * 3f + spacing * 2f;

        float startX =
            (Screen.width - totalWidth) * 0.5f;

        float y =
            Screen.height * 0.5f - height * 0.5f;

        GUI.Box(
            new Rect(
                Screen.width * 0.5f - 150f,
                y - 80f,
                300f,
                50f
            ),
            "레벨 업! 증강을 선택하세요"
        );

        for (int i = 0; i < 3; i++)
        {
            Rect rect = new Rect(
                startX + i * (width + spacing),
                y,
                width,
                height
            );

            if (GUI.Button(
                rect,
                GetDescription(currentChoices[i])
            ))
            {
                ApplyAugment(currentChoices[i]);
            }
        }
    }

    private string GetDescription(AugmentType type)
    {
        switch (type)
        {
            case AugmentType.LandingNetPower:
                return "강화 뜰채\n포획력 +2";

            case AugmentType.LandingNetRadius:
                return "넓은 뜰채\n범위 +0.25";

            case AugmentType.CastNetPower:
                return "강화 투망\n포획력 +5";

            case AugmentType.CastNetRadius:
                return "대형 투망\n범위 +0.5";

            case AugmentType.CastNetCooldown:
                return "신속 투망\n재사용 대기시간 -0.5초";

            case AugmentType.BaitRadius:
                return "강한 향\n미끼 유인 범위 +0.75";

            case AugmentType.BaitDuration:
                return "지속형 미끼\n미끼 지속시간 +1초";

            case AugmentType.NetLength:
                return "긴 그물\n최대 길이 +1";

            default:
                return "알 수 없는 증강";
        }
    }

    private void ApplyAugment(AugmentType type)
    {
        switch (type)
        {
            case AugmentType.LandingNetPower:
                landingNet.IncreaseCapturePower(2f);
                break;

            case AugmentType.LandingNetRadius:
                landingNet.IncreaseCaptureRadius(0.25f);
                break;

            case AugmentType.CastNetPower:
                castNet.IncreaseCapturePower(5f);
                break;

            case AugmentType.CastNetRadius:
                castNet.IncreaseCaptureRadius(0.5f);
                break;

            case AugmentType.CastNetCooldown:
                castNet.ReduceCooldown(0.5f);
                break;

            case AugmentType.BaitRadius:
                bait.IncreaseAttractionRadius(0.75f);
                break;

            case AugmentType.BaitDuration:
                bait.IncreaseDuration(1f);
                break;

            case AugmentType.NetLength:
                netPlacement.IncreaseMaxLength(1f);
                break;
        }

        showChoices = false;

        Time.timeScale = 1f;

        if (RunManager.Instance != null)
        {
            RunManager.Instance.ResolveLevelUp();
        }
    }

    private void OnDisable()
    {
        if (showChoices)
        {
            Time.timeScale = 1f;
        }
    }
}