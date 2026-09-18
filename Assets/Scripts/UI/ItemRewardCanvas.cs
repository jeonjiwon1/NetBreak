using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ItemRewardCanvas : MonoBehaviour
{
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button[] choiceButtons = Array.Empty<Button>();
    [SerializeField] private TMP_Text[] choiceTexts = Array.Empty<TMP_Text>();

    private void Awake()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            if (choiceButtons[i] != null)
            {
                choiceButtons[i].onClick.AddListener(
                    () => ItemRewardManager.Instance?.SelectChoice(choiceIndex));
            }
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    private void Update()
    {
        ItemRewardManager manager = ItemRewardManager.Instance;
        bool shouldShow = manager != null && manager.IsChoosing;
        if (rewardPanel != null && rewardPanel.activeSelf != shouldShow)
        {
            rewardPanel.SetActive(shouldShow);
        }

        if (!shouldShow)
        {
            return;
        }

        transform.SetAsLastSibling();
        rewardPanel.transform.SetAsLastSibling();
        if (titleText != null)
        {
            titleText.text = manager.ActiveRequestType == ItemRewardRequestType.Upgrade
                ? "강화할 패시브 아이템을 선택하세요\n<size=70%>선택 전에는 조업을 계속할 수 없습니다.</size>"
                : "패시브 아이템을 선택하세요\n<size=70%>선택 전에는 조업을 계속할 수 없습니다.</size>";
        }

        int viewCount = Mathf.Max(choiceButtons.Length, choiceTexts.Length);
        for (int i = 0; i < viewCount; i++)
        {
            ItemDefinition choice = manager.GetChoice(i);
            bool visible = choice != null;
            if (i < choiceButtons.Length && choiceButtons[i] != null)
            {
                choiceButtons[i].gameObject.SetActive(visible);
                choiceButtons[i].interactable = visible && manager.CanSelect;
            }

            if (i < choiceTexts.Length && choiceTexts[i] != null)
            {
                choiceTexts[i].text = visible
                    ? GetChoiceText(manager, choice, i)
                    : string.Empty;
            }
        }

        LayoutVisibleChoices(manager.ChoiceCount);
    }

    private static string GetChoiceText(
        ItemRewardManager manager,
        ItemDefinition choice,
        int choiceIndex)
    {
        if (manager.ActiveRequestType != ItemRewardRequestType.Upgrade ||
            !manager.TryGetUpgradePreview(choiceIndex, out ItemUpgradePreview preview))
        {
            return $"<b>{choice.DisplayName}</b>\n" +
                $"<size=80%>[{choice.ElementDisplayName}] · 획득 가능</size>\n\n" +
                choice.PlannedEffectDescription;
        }

        return $"<b>{choice.DisplayName}</b>\n" +
            $"<size=80%>[{choice.ElementDisplayName}]</size>\n\n" +
            $"Lv.{preview.CurrentLevel} → Lv.{preview.NextLevel}\n" +
            $"{choice.PrimaryEffectDisplayName}: " +
            $"{preview.CurrentPrimaryValue:0.##}{choice.PrimaryValueSuffix} → " +
            $"{preview.NextPrimaryValue:0.##}{choice.PrimaryValueSuffix}\n\n" +
            $"<color=#9DDEF2>{choice.ElementDisplayName} 속성 레벨 +1</color>";
    }

    private void LayoutVisibleChoices(int visibleCount)
    {
        int count = Mathf.Min(Mathf.Max(0, visibleCount), choiceButtons.Length);
        for (int i = 0; i < count; i++)
        {
            if (choiceButtons[i] == null ||
                choiceButtons[i].transform is not RectTransform rect)
            {
                continue;
            }

            Vector2 position = rect.anchoredPosition;
            position.x = (i - (count - 1) * 0.5f) * 400f;
            rect.anchoredPosition = position;
        }
    }
}
