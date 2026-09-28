using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Presentation only. The existing HUD components still own every displayed value and click action.
internal static class Area1HUDSkin
{
    private static readonly Color Cream = new Color32(247, 239, 207, 255);
    private static readonly Color Gold = new Color32(255, 215, 119, 255);
    private static readonly Color Aqua = new Color32(132, 239, 229, 255);
    private static readonly Dictionary<string, Sprite> Sprites =
        new Dictionary<string, Sprite>();

    internal static Sprite Frame(string name)
    {
        if (!Sprites.TryGetValue(name, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>("UI/Area1/" + name);
            Sprites.Add(name, sprite);
        }
        return sprite;
    }

    internal static Sprite ToolIcon(ToolId tool) => Frame(tool switch
    {
        ToolId.LandingNet => "icon_landing",
        ToolId.Bait => "icon_bait",
        ToolId.Net => "icon_net",
        ToolId.CastNet => "icon_cast",
        ToolId.FishingRod => "icon_rod",
        _ => "icon_empty"
    });

    internal static Sprite SkillIcon(bool signature) =>
        Frame(signature ? "icon_signature" : "icon_tactical");

    internal static Sprite ItemIcon(string itemId) => Frame(itemId switch
    {
        ItemCatalog.StormOrbId => "icon_storm_orb",
        ItemCatalog.CapacitorCoilId => "icon_capacitor_coil",
        ItemCatalog.SpectralScabbardId => "icon_spectral_scabbard",
        ItemCatalog.AutonomousSwordArrayId => "icon_autonomous_sword_array",
        ItemCatalog.FrostSigilId => "icon_frost_sigil",
        ItemCatalog.FrostCrystalId => "icon_frost_crystal",
        _ => "icon_empty"
    });

    internal static void SetFrame(Image image, string name, bool blockRaycasts)
    {
        if (image == null) return;
        Sprite sprite = Frame(name);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            // Keep the existing 5px import border; scale its display thickness only.
            image.pixelsPerUnitMultiplier = name == "key" ? 4f :
                name == "header" ? 2.5f : 1.6f;
            image.color = Color.white;
        }
        else
        {
            image.color = new Color(0.03f, 0.22f, 0.28f, 0.94f);
        }
        image.raycastTarget = blockRaycasts;
    }

    internal static Image[] ApplyStatic(
        Transform canvas,
        TMP_Text goldText,
        TMP_Text captureText,
        TMP_Text catchRateText,
        TMP_Text levelText,
        TMP_Text expText,
        TMP_Text stageText,
        TMP_Text phaseText,
        TMP_Text runTimeText,
        GameObject preparationPanel,
        Button startFishingButton)
    {
        StyleRunPanel(goldText, captureText, catchRateText,
            levelText, expText, stageText, phaseText);
        StyleTimer(canvas, runTimeText);
        StylePreparation(preparationPanel, startFishingButton);
        StyleItemSlots(canvas, goldText);
        StyleGrowthButton(canvas);
        return StyleSpeedButtons(canvas);
    }

    private static void StyleRunPanel(params TMP_Text[] rows)
    {
        if (rows.Length == 0 || rows[0] == null) return;
        RectTransform panel = rows[0].transform.parent as RectTransform;
        if (panel == null) return;
        panel.anchorMin = panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(20f, -20f);
        panel.sizeDelta = new Vector2(256f, 254f);
        SetFrame(panel.GetComponent<Image>() ?? panel.gameObject.AddComponent<Image>(),
            "panel", false);

        CreateDragHeader(panel, rows[0], "NETBREAK", 236f, 34f, true);
        Decorate(panel, "palm", new Vector2(-4f, 6f), new Vector2(40f, 40f));
        Decorate(panel, "gull", new Vector2(219f, 6f), new Vector2(30f, 30f));
        Decorate(panel, "leaf", new Vector2(-14f, -213f), new Vector2(43f, 43f));
        Decorate(panel, "starfish", new Vector2(224f, -219f), new Vector2(40f, 40f));
        Decorate(panel, "shell", new Vector2(16f, -235f), new Vector2(27f, 27f));
        GameObject contentObject = new GameObject("Area1RunContent",
            typeof(RectTransform), typeof(RectMask2D));
        contentObject.layer = panel.gameObject.layer;
        contentObject.transform.SetParent(panel, false);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        Place(content, new Vector2(12f, -39f), new Vector2(232f, 202f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));

        string[] labels = { "골드", "포획 수", "어획률", "레벨", "경험치",
            "조업 단계", "현재 구간" };
        string[] icons = { "icon_stat_gold", "icon_stat_catch", "icon_stat_rate",
            "icon_stat_level", "icon_stat_exp", "icon_stat_stage", "icon_stat_area" };
        for (int i = 0; i < rows.Length && i < labels.Length; i++)
        {
            TMP_Text row = rows[i];
            if (row == null) continue;
            float y = i < 5 ? -i * 27f : -149f - (i - 5) * 27f;
            float rowHeight = i == 4 ? 19f : 25f;
            Image stripe = NewImage("Area1RowBackground_" + i, content, null);
            stripe.color = i % 2 == 0
                ? new Color32(3, 27, 43, 190)
                : new Color32(5, 41, 57, 185);
            Place(stripe.rectTransform, new Vector2(0f, y),
                new Vector2(232f, 25f), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            Image icon = NewImage("Area1StatIcon_" + i, content,
                Frame(icons[i]));
            icon.preserveAspect = true;
            Place(icon.rectTransform, new Vector2(5f, y - 2f),
                new Vector2(20f, 20f), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text label = Object.Instantiate(row, content);
            label.name = "Area1StatLabel_" + i;
            label.text = labels[i];
            ConfigureHudText(label, 15f, 15f, TextAlignmentOptions.Left,
                Cream);
            Place(label.rectTransform, new Vector2(31f, y),
                new Vector2(81f, rowHeight), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            row.transform.SetParent(content, false);
            row.text = string.Empty;
            ConfigureHudText(row, 16f, 14f, TextAlignmentOptions.Right,
                i == 0 ? Gold : i >= 5 ? Aqua : Cream);
            Place(row.rectTransform, new Vector2(114f, y),
                new Vector2(113f, rowHeight), new Vector2(0f, 1f),
                new Vector2(0f, 1f));
        }

        Image divider = NewImage("Area1HUDDivider", content, null);
        divider.color = new Color32(77, 200, 210, 210);
        Place(divider.rectTransform, new Vector2(3f, -139f),
            new Vector2(226f, 2f), new Vector2(0f, 1f), new Vector2(0f, 1f));
    }

    internal static Image CreateExperienceGauge(TMP_Text expText)
    {
        if (expText == null || expText.transform.parent == null) return null;
        Image track = NewImage("Area1ExperienceTrack", expText.transform.parent, Frame("key"));
        SetFrame(track, "key", false);
        track.pixelsPerUnitMultiplier = 8f;
        Place(track.rectTransform, new Vector2(31f, -129f), new Vector2(196f, 8f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image fill = NewImage("Area1ExperienceFill", track.transform, null);
        fill.color = Aqua;
        Place(fill.rectTransform, new Vector2(2f, -2f), new Vector2(0f, 4f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        return fill;
    }

    internal static void RefreshExperience(Image fill, int current, int required)
    {
        if (fill == null || !(fill.transform.parent is RectTransform track)) return;
        float ratio = required > 0 ? Mathf.Clamp01((float)current / required) : 0f;
        fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
            Mathf.Max(0f, track.rect.width - 4f) * ratio);
    }

    private static void ConfigureHudText(TMP_Text text, float size,
        float minimumSize, TextAlignmentOptions alignment, Color color)
    {
        text.fontSize = size;
        text.enableAutoSizing = minimumSize < size;
        text.fontSizeMin = minimumSize;
        text.fontSizeMax = size;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Truncate;
        text.alignment = alignment;
        text.margin = Vector4.zero;
        text.lineSpacing = 0f;
        text.color = color;
        text.raycastTarget = false;
    }

    private static void StyleTimer(Transform canvas, TMP_Text time)
    {
        if (time == null) return;
        RectTransform rect = time.rectTransform;
        Place(rect, new Vector2(12f, -19f), new Vector2(260f, 38f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        ConfigureHudText(time, 21f, 21f, TextAlignmentOptions.Center, Cream);

        Image backdrop = NewImage("Area1TimeFrame", canvas, Frame("panel"));
        SetFrame(backdrop, "panel", false);
        Place(backdrop.rectTransform, new Vector2(0f, -16f),
            new Vector2(300f, 46f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        backdrop.transform.SetSiblingIndex(rect.GetSiblingIndex());
        Decorate(backdrop.rectTransform, "clock", new Vector2(12f, -8f),
            new Vector2(29f, 29f));
        Decorate(backdrop.rectTransform, "shell", new Vector2(275f, 3f),
            new Vector2(24f, 24f));
    }

    private static void StylePreparation(GameObject root, Button start)
    {
        if (root == null) return;
        RectTransform rect = root.transform as RectTransform;
        if (rect == null) return;
        Place(rect, new Vector2(0f, -70f), new Vector2(312f, 112f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        SetFrame(root.GetComponent<Image>(), "panel", false);
        AddFishWatermark(rect, new Vector2(45f, -16f), false);
        AddFishWatermark(rect, new Vector2(227f, -16f), true);
        Decorate(rect, "palm", new Vector2(-15f, 13f), new Vector2(56f, 56f));
        Decorate(rect, "bobber", new Vector2(17f, -15f), new Vector2(30f, 40f));
        Decorate(rect, "rope_knot", new Vector2(274f, 7f), new Vector2(44f, 44f));
        Decorate(rect, "coral", new Vector2(267f, -70f), new Vector2(46f, 46f));
        // Repeat independent wave tiles; never stretch a crest through a 9-slice.
        for (int i = 0; i < 8; i++)
        {
            Image wave = NewImage("Area1FoamBand_" + i, rect, Frame("decor_wave"));
            Place(wave.rectTransform, new Vector2(4f + i * 38f, -103f),
                new Vector2(39f, 20f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        }

        TMP_Text title = root.transform.Find("PreparationTitleText")?.GetComponent<TMP_Text>();
        if (title != null)
        {
            Place(title.rectTransform, new Vector2(0f, -10f),
                new Vector2(286f, 40f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            ConfigureHudText(title, 25f, 25f, TextAlignmentOptions.Center, Cream);
        }

        if (start == null) return;
        Place(start.transform as RectTransform, new Vector2(0f, 11f),
            new Vector2(248f, 42f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        SetFrame(start.GetComponent<Image>(), "button", true);
        TMP_Text label = start.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            Place(label.rectTransform, Vector2.zero, new Vector2(224f, 32f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            ConfigureHudText(label, 21f, 21f, TextAlignmentOptions.Center,
                new Color32(57, 37, 27, 255));
        }
    }

    private static void AddFishWatermark(RectTransform panel, Vector2 position, bool flipped)
    {
        Image fish = NewImage("Area1ReadyFishShadow", panel, Frame("icon_stat_catch"));
        fish.color = new Color(0.16f, 0.58f, 0.64f, 0.32f);
        fish.preserveAspect = true;
        Place(fish.rectTransform, position, new Vector2(40f, 32f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        if (flipped)
        {
            fish.rectTransform.pivot = new Vector2(1f, 1f);
            fish.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
        }
        fish.transform.SetAsFirstSibling();
    }

    private static void StyleItemSlots(Transform canvas, TMP_Text textTemplate)
    {
        RectTransform panel = canvas.Find("ItemSystemUI/ItemHUD") as RectTransform;
        if (panel == null) return;
        Place(panel, new Vector2(-20f, -20f), new Vector2(388f, 132f),
            new Vector2(1f, 1f), new Vector2(1f, 1f));
        SetFrame(panel.GetComponent<Image>(), "panel", false);
        CreateDragHeader(panel, textTemplate, "아이템", 368f, 34f);
        Decorate(panel, "leaf", new Vector2(-9f, -3f), new Vector2(43f, 43f));
        Decorate(panel, "crate", new Vector2(340f, -3f), new Vector2(44f, 44f));
        Decorate(panel, "shell", new Vector2(-8f, -109f), new Vector2(30f, 30f));
        Decorate(panel, "starfish", new Vector2(373f, -114f), new Vector2(29f, 29f));

        Image[] icons = new Image[4];
        TMP_Text[] details = new TMP_Text[4];
        for (int i = 0; i < 4; i++)
        {
            RectTransform slot = panel.Find("ItemSlot_" + (i + 1)) as RectTransform;
            if (slot == null) continue;
            Place(slot, new Vector2(15f + i * 91f, -40f),
                new Vector2(84f, 82f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            SetFrame(slot.GetComponent<Image>(), "slot", true);
            TMP_Text label = slot.Find("Label")?.GetComponent<TMP_Text>();
            if (label == null) continue;
            Place(label.rectTransform, new Vector2(0f, 7f),
                new Vector2(72f, 18f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            ConfigureHudText(label, 15f, 13f, TextAlignmentOptions.Center, Cream);
            Image icon = NewImage("Area1ItemIcon", slot, Frame("icon_empty"));
            icon.preserveAspect = true;
            Place(icon.rectTransform, new Vector2(0f, -25f),
                new Vector2(32f, 32f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f));
            icons[i] = icon;

            Image numberBadge = NewImage("Area1SlotNumberBadge", slot, Frame("key"));
            SetFrame(numberBadge, "key", false);
            Place(numberBadge.rectTransform, new Vector2(8f, -4f),
                new Vector2(22f, 20f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            TMP_Text number = Object.Instantiate(label, numberBadge.transform);
            number.name = "Area1SlotNumber";
            number.text = (i + 1).ToString();
            ConfigureHudText(number, 16f, 16f, TextAlignmentOptions.Center, Cream);
            Place(number.rectTransform, Vector2.zero,
                new Vector2(22f, 20f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            TMP_Text detail = Object.Instantiate(label, slot);
            detail.name = "Area1SlotLevel";
            detail.text = string.Empty;
            ConfigureHudText(detail, 11f, 11f, TextAlignmentOptions.Right, Aqua);
            Place(detail.rectTransform, new Vector2(-4f, -3f),
                new Vector2(34f, 16f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            details[i] = detail;
        }
        panel.GetComponentInParent<ItemHUD>()?.ConfigureVisuals(icons, details);

        RectTransform elementTooltip =
            canvas.Find("ItemSystemUI/ElementSynergyTooltip") as RectTransform;
        if (elementTooltip != null)
            elementTooltip.anchoredPosition = new Vector2(-470f, -340f);
    }

    private static void CreateDragHeader(RectTransform panel,
        TMP_Text template, string title, float width, float labelInset, bool raisedSign = false)
    {
        if (template == null) return;
        Image header = NewImage("Area1DragHandle", panel, Frame("header"));
        SetFrame(header, "header", true);
        Place(header.rectTransform, new Vector2(10f, raisedSign ? 4f : -7f),
            new Vector2(width, raisedSign ? 36f : 26f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        TMP_Text label = Object.Instantiate(template, header.transform);
        label.name = "Area1DragLabel";
        label.text = raisedSign ? title : "≡  " + title;
        ConfigureHudText(label, raisedSign ? 19f : 15f, raisedSign ? 19f : 15f,
            raisedSign ? TextAlignmentOptions.Center : TextAlignmentOptions.Left, Cream);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(labelInset, 2f);
        label.rectTransform.offsetMax = new Vector2(raisedSign ? -28f : -9f, -2f);
        header.gameObject.AddComponent<DraggableHudPanel>().Configure(panel);
    }

    private static void StyleGrowthButton(Transform canvas)
    {
        RectTransform rect = canvas.Find("SkillTreeUI/OpenTreeButton") as RectTransform;
        if (rect == null) return;
        Place(rect, new Vector2(-20f, 20f), new Vector2(225f, 58f),
            new Vector2(1f, 0f), new Vector2(1f, 0f));
        SetFrame(rect.GetComponent<Image>(), "button", true);
        Decorate(rect, "rope_knot", new Vector2(-22f, 25f), new Vector2(30f, 30f),
            new Vector2(0f, 0f));
        Decorate(rect, "leaf", new Vector2(-18f, 42f), new Vector2(36f, 36f),
            new Vector2(0f, 0f));
        Decorate(rect, "shell", new Vector2(203f, 8f), new Vector2(32f, 32f),
            new Vector2(0f, 0f));
        TMP_Text label = rect.Find("Label")?.GetComponent<TMP_Text>();
        if (label == null) return;
        Place(label.rectTransform, Vector2.zero, new Vector2(205f, 42f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        ConfigureHudText(label, 20f, 18f, TextAlignmentOptions.Center,
            new Color32(57, 37, 27, 255));
    }

    private static Image[] StyleSpeedButtons(Transform canvas)
    {
        Image[] images = new Image[3];
        RectTransform panel = canvas.Find("TestSpeedPanel") as RectTransform;
        if (panel == null) return images;
        Place(panel, new Vector2(-20f, 130f), new Vector2(122f, 138f),
            new Vector2(1f, 0f), new Vector2(1f, 0f));
        for (int i = 0; i < images.Length; i++)
        {
            RectTransform button = panel.Find("Speed" + (i + 1) + "Button") as RectTransform;
            if (button == null) continue;
            Place(button, new Vector2(0f, -4f - i * 45f),
                new Vector2(116f, 40f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            Image image = button.GetComponent<Image>();
            SetFrame(image, "key", true);
            if (i == 0)
                Decorate(button, "shell", new Vector2(101f, 3f), new Vector2(16f, 16f));
            images[i] = image;
            Image wave = NewImage("Area1SpeedIcon", button, Frame("icon_speed"));
            wave.preserveAspect = true;
            Place(wave.rectTransform, new Vector2(11f, 0f),
                new Vector2(25f, 25f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f));
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                Place(label.rectTransform, new Vector2(-8f, 0f),
                    new Vector2(65f, 30f), new Vector2(1f, 0.5f),
                    new Vector2(1f, 0.5f));
                ConfigureHudText(label, 20f, 20f,
                    TextAlignmentOptions.Center, Cream);
            }
        }
        return images;
    }

    internal static void RefreshSpeedSelection(Image[] buttons, float speed)
    {
        if (buttons == null) return;
        int selected = speed >= 2.5f ? 2 : speed >= 1.5f ? 1 : 0;
        for (int i = 0; i < buttons.Length; i++)
            if (buttons[i] != null)
                buttons[i].color = i == selected ? Color.white :
                    new Color(0.65f, 0.78f, 0.79f, 0.9f);
    }

    internal static Image NewImage(string name, Transform parent, Sprite sprite)
    {
        GameObject child = new GameObject(name, typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image));
        child.layer = parent.gameObject.layer;
        child.transform.SetParent(parent, false);
        Image image = child.GetComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    internal static void DecorateHotbar(RectTransform root)
    {
        if (root == null) return;
        for (int i = 0; i < 4; i++)
        {
            Image post = NewImage("Area1HotbarJoin_" + i, root, Frame("header"));
            SetFrame(post, "header", false);
            post.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place(post.rectTransform, new Vector2(143f + i * 140f, 6f),
                new Vector2(10f, 108f), Vector2.zero, Vector2.zero);
        }
        Decorate(root, "leaf", new Vector2(-21f, 31f), new Vector2(53f, 53f),
            new Vector2(0f, 0f), true);
        Decorate(root, "coral", new Vector2(686f, 4f), new Vector2(49f, 49f),
            new Vector2(0f, 0f), true);
        Decorate(root, "shell", new Vector2(0f, -3f), new Vector2(26f, 26f),
            new Vector2(0f, 0f), true);
        Decorate(root, "rope_knot", new Vector2(690f, 92f),
            new Vector2(40f, 40f), new Vector2(0f, 0f), true);
    }

    private static void Decorate(Transform parent, string name, Vector2 position,
        Vector2 size)
    {
        Decorate(parent, name, position, size, new Vector2(0f, 1f));
    }

    private static void Decorate(Transform parent, string name, Vector2 position,
        Vector2 size, Vector2 anchor, bool ignoreLayout = false)
    {
        Sprite sprite = Frame("decor_" + name);
        if (sprite == null) return;
        Image image = NewImage("Area1Decor_" + name, parent, sprite);
        image.preserveAspect = true;
        if (ignoreLayout)
            image.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        Place(image.rectTransform, position, size, anchor, anchor);
        image.transform.SetAsLastSibling();
    }

    internal static void Place(RectTransform rect, Vector2 position, Vector2 size,
        Vector2 anchor, Vector2 pivot)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
