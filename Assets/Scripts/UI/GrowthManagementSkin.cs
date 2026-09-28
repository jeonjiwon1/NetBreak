using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation for the serialized growth window. No state or button listeners live here.
internal static class GrowthManagementSkin
{
    private static readonly Color Cream = new Color32(245, 237, 208, 255);
    private static readonly Color Aqua = new Color32(151, 232, 219, 255);
    private static readonly Color Muted = new Color32(177, 198, 193, 255);
    // The shared panel sprite already contains the deep navy interior.
    private static readonly Color PanelTint = new Color32(218, 236, 231, 255);

    internal static void Apply(GameObject root, GameObject navigation,
        GameObject skillPage, GameObject itemPage, Button close,
        TMP_Text mastery, TMP_Text notice)
    {
        if (root == null) return;
        RectTransform window = root.transform as RectTransform;
        Image scrim = root.GetComponent<Image>();
        if (scrim != null) scrim.color = new Color32(2, 20, 31, 226);

        Image frame = AddImage("MarineWindowFrame", window, "panel", PanelTint);
        Stretch(frame.rectTransform, 0.018f, 0.025f, 0.982f, 0.975f);
        frame.transform.SetAsFirstSibling();
        Corner(window, "decor_rope_knot", 0.017f, 0.974f, 39f);
        Corner(window, "decor_shell", 0.98f, 0.975f, 42f);
        Corner(window, "decor_coral", 0.975f, 0.024f, 48f);
        Corner(window, "decor_leaf", 0.018f, 0.025f, 46f);

        Transform header = window.Find("Header");
        StyleImage(header, "header", Color.white);
        StyleText(header != null ? header.Find("TitleText")?.GetComponent<TMP_Text>() : null,
            Area1Typography.Role.Title, Cream, 28f, false);
        StyleText(mastery, Area1Typography.Role.Number, Aqua, 20f, false);
        StyleText(notice, Area1Typography.Role.Body, Cream, 17f, true);
        StyleButton(close, "button");
        if (close != null)
        {
            TMP_Text label = close.GetComponentInChildren<TMP_Text>(true);
            StyleText(label, Area1Typography.Role.Title, Cream, 19f, false);
        }

        if (navigation != null)
        {
            Image navImage = navigation.GetComponent<Image>();
            if (navImage != null) navImage.color = Color.clear;
            foreach (Button tab in navigation.GetComponentsInChildren<Button>(true))
            {
                StyleButton(tab, "button");
                tab.transition = Selectable.Transition.None;
                StyleText(tab.GetComponentInChildren<TMP_Text>(true),
                    Area1Typography.Role.Title, Cream, 20f, false);
            }
        }

        StyleSkillPage(skillPage);
        StyleItemPage(itemPage);
        StyleTooltips(root);
    }

    internal static void RefreshTabs(Button tree, Button items, bool treeSelected)
    {
        SetTab(tree, treeSelected);
        SetTab(items, !treeSelected);
    }

    private static void SetTab(Button button, bool selected)
    {
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = Area1HUDSkin.Frame("selected_slot");
            image.type = Image.Type.Tiled;
            image.color = selected ? Color.white : new Color32(86, 128, 137, 255);
        }
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.color = selected ? Cream : Muted;
    }

    private static void StyleSkillPage(GameObject page)
    {
        if (page == null) return;
        foreach (SkillTreeBranchView branch in page.GetComponentsInChildren<SkillTreeBranchView>(true))
        {
            StyleImage(branch.transform, "panel", PanelTint);
            foreach (TMP_Text text in branch.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.name.Contains("Description")) continue;
                StyleText(text, text.name.Contains("Title")
                    ? Area1Typography.Role.Title : Area1Typography.Role.Body,
                    Cream, 0f, false);
            }
        }
        foreach (Image image in page.GetComponentsInChildren<Image>(true))
        {
            if (image.name.Contains("Blocker")) continue;
            if (image.name.Contains("Acquisition") || image.name.Contains("Choice"))
                SetSprite(image, "panel", Color.white);
        }
    }

    private static void StyleItemPage(GameObject page)
    {
        if (page == null) return;
        Transform root = page.transform;
        Section(root, "OwnedItems", "보유 아이템");
        Section(root, "ElementSynergies", "단일 시너지");
        Section(root, "CombinedSynergies", "복합 시너지");
        Transform owned = root.Find("OwnedItems");
        if (owned != null)
        {
            for (int i = 1; i <= 4; i++)
            {
                Transform slot = owned.Find("OwnedItemSlot_" + i);
                if (slot == null) continue;
                StyleImage(slot, "ref_item_slot", Color.white);
                Image icon = AddImage("GrowthItemIcon", slot, "icon_empty", Color.white);
                PositionItemIcon(icon, false);
                TMP_Text label = slot.Find("Label")?.GetComponent<TMP_Text>();
                StyleText(label, Area1Typography.Role.Body, Cream, 16f, true);
                if (label != null) Stretch(label.rectTransform, 0.08f, 0.06f, 0.92f, 0.32f);
                TMP_Text badge = AddText("GrowthSlotNumber", slot, i.ToString("00"));
                Stretch(badge.rectTransform, 0.07f, 0.79f, 0.3f, 0.96f);
                StyleText(badge, Area1Typography.Role.Number, Aqua, 15f, false);
            }
        }

        Transform elements = root.Find("ElementSynergies");
        if (elements != null)
        {
            StyleElement(elements, "ElectricEntry", new Color32(221, 187, 103, 255));
            StyleElement(elements, "SwordEntry", new Color32(237, 151, 141, 255));
            StyleElement(elements, "IceEntry", new Color32(148, 211, 239, 255));
            StyleText(elements.Find("EmptyState")?.GetComponent<TMP_Text>(),
                Area1Typography.Role.Body, Muted, 17f, true);
        }

        Transform combined = root.Find("CombinedSynergies");
        if (combined != null)
        {
            for (int i = 1; i <= 3; i++)
            {
                Transform card = combined.Find("CombinedCard_" + i);
                StyleImage(card, "slot", Color.white);
                TMP_Text label = card?.Find("Label")?.GetComponent<TMP_Text>();
                if (label != null)
                {
                    label.fontSize = 15f;
                    label.enableAutoSizing = true;
                    label.fontSizeMin = 13f;
                    label.fontSizeMax = 15f;
                    label.textWrappingMode = TextWrappingModes.Normal;
                    label.overflowMode = TextOverflowModes.Truncate;
                    label.margin = new Vector4(4f, 3f, 4f, 3f);
                }
            }
            Transform detail = combined.Find("CombinedDetail");
            StyleImage(detail, "panel", PanelTint);
            TMP_Text detailText = detail?.Find("Text")?.GetComponent<TMP_Text>();
            StyleLongText(detailText, 15.5f, 13f);
            StyleText(combined.Find("CooldownText")?.GetComponent<TMP_Text>(),
                Area1Typography.Role.Number, Aqua, 16f, false);
            Button confirm = combined.Find("ConfirmButton")?.GetComponent<Button>();
            StyleButton(confirm, "button");
            StyleText(confirm?.GetComponentInChildren<TMP_Text>(true),
                Area1Typography.Role.Title, Cream, 16f, true);
        }
    }

    // The empty cross uses its native 32px size; owned art keeps its existing area.
    internal static void PositionItemIcon(Image icon, bool occupied)
    {
        if (icon == null) return;
        RectTransform rect = icon.rectTransform;
        rect.pivot = new Vector2(0.5f, 0.5f);
        if (occupied)
        {
            Stretch(rect, 0.29f, 0.31f, 0.67f, 0.69f);
        }
        else
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(-4f, -2f);
            rect.sizeDelta = new Vector2(32f, 32f);
        }
        icon.type = occupied ? Image.Type.Tiled : Image.Type.Simple;
        icon.preserveAspect = true;
    }

    private static void StyleElement(Transform parent, string name, Color color)
    {
        Transform entry = parent.Find(name);
        StyleImage(entry, "slot", color);
        StyleText(entry?.Find("Label")?.GetComponent<TMP_Text>(),
            Area1Typography.Role.Title, Cream, 18f, false);
    }

    private static void Section(Transform root, string name, string title)
    {
        Transform section = root.Find(name);
        if (section == null) return;
        StyleImage(section, "panel", PanelTint);
        Image band = AddImage("MarineSectionBand", section, "header", Color.white);
        Stretch(band.rectTransform, 0.01f,
            name == "CombinedSynergies" ? 0.875f : 0.81f, 0.99f, 0.99f);
        band.transform.SetAsFirstSibling();
        TMP_Text heading = section.Find("Title")?.GetComponent<TMP_Text>();
        if (heading != null) heading.text = title;
        StyleText(heading, Area1Typography.Role.Title, Cream, 22f, false);
    }

    private static void StyleTooltips(GameObject root)
    {
        foreach (SkillTreeTooltip tooltip in root.GetComponentsInChildren<SkillTreeTooltip>(true))
        {
            StyleImage(tooltip.transform, "panel", Color.white);
            TMP_Text label = tooltip.GetComponentInChildren<TMP_Text>(true);
            StyleLongText(label, 17f, 15f);
        }
        foreach (string name in new[] { "GrowthItemTooltip", "GrowthElementTooltip" })
        {
            Transform panel = FindDescendant(root.transform, name);
            StyleImage(panel, "panel", Color.white);
            StyleLongText(panel?.GetComponentInChildren<TMP_Text>(true), 16.5f, 14f);
        }
    }

    private static Transform FindDescendant(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name == name) return child;
            Transform found = FindDescendant(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private static void StyleLongText(TMP_Text text, float max, float min)
    {
        if (text == null) return;
        text.fontSize = max;
        text.enableAutoSizing = true;
        text.fontSizeMin = min;
        text.fontSizeMax = max;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = TextOverflowModes.Truncate;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.color = Cream;
        text.raycastTarget = false;
    }

    private static void StyleText(TMP_Text text, Area1Typography.Role role,
        Color color, float size, bool wrap)
    {
        if (text == null) return;
        Area1Typography.Apply(text, role);
        if (size > 0f) text.fontSize = size;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, text.fontSize - 3f);
        text.fontSizeMax = text.fontSize;
        text.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Truncate;
        text.alignment = TextAlignmentOptions.Center;
        text.margin = new Vector4(2f, 2f, 2f, 2f);
        text.characterSpacing = 0.2f;
        text.color = color;
    }

    private static void StyleButton(Button button, string sprite)
    {
        if (button == null) return;
        StyleImage(button.transform, sprite, Color.white);
    }

    private static void StyleImage(Transform target, string sprite, Color color)
    {
        if (target == null) return;
        Image image = target.GetComponent<Image>();
        if (image != null) SetSprite(image, sprite, color);
    }

    private static void SetSprite(Image image, string sprite, Color color)
    {
        Sprite source = Area1HUDSkin.Frame(sprite);
        if (source == null) return;
        image.sprite = source;
        image.type = sprite.StartsWith("ref_") ? Image.Type.Simple : Image.Type.Tiled;
        image.color = color;
    }

    private static Image AddImage(string name, Transform parent, string sprite, Color color)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent, false);
        Image image = child.GetComponent<Image>();
        SetSprite(image, sprite, color);
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text AddText(string name, Transform parent, string value)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent, false);
        TMP_Text text = child.GetComponent<TMP_Text>();
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    private static void Corner(Transform parent, string sprite, float x, float y, float size)
    {
        Image image = AddImage("MarineCorner_" + sprite, parent, sprite, Color.white);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(x, y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(size, size);
        image.preserveAspect = true;
        image.transform.SetSiblingIndex(1);
    }

    private static void Stretch(RectTransform rect, float minX, float minY,
        float maxX, float maxY)
    {
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
