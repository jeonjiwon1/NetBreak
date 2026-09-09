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
            "LEVEL UP - Choose Augment"
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
                return "Heavy Landing Net\nCapture Power +2";

            case AugmentType.LandingNetRadius:
                return "Wide Landing Net\nRadius +0.25";

            case AugmentType.CastNetPower:
                return "Reinforced Cast Net\nPower +5";

            case AugmentType.CastNetRadius:
                return "Large Cast Net\nRadius +0.5";

            case AugmentType.CastNetCooldown:
                return "Rapid Cast\nCooldown -0.5s";

            case AugmentType.BaitRadius:
                return "Strong Scent\nBait Radius +0.75";

            case AugmentType.BaitDuration:
                return "Long-Lasting Bait\nDuration +1s";

            case AugmentType.NetLength:
                return "Long Net\nMax Length +1";

            default:
                return "Unknown";
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