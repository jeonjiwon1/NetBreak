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
            // Tile the authored pixels instead of stretching grain and mottling
            // into long flat strips. Corners remain fixed by the sprite border.
            image.type = name.StartsWith("ref_") ? Image.Type.Simple : Image.Type.Tiled;
            image.pixelsPerUnitMultiplier = name == "key" ? 1.5f :
                name == "header" ? 1.25f : 1f;
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
            "ref_hud_board", false);

        CreateDragHeader(panel, rows[0], "NETBREAK", true);
        GameObject contentObject = new GameObject("Area1RunContent",
            typeof(RectTransform), typeof(RectMask2D));
        contentObject.layer = panel.gameObject.layer;
        contentObject.transform.SetParent(panel, false);
        RectTransform content = contentObject.GetComponent<RectTransform>();
        Place(content, new Vector2(30f, -39f), new Vector2(196f, 202f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));

        string[] labels = { "골드", "포획 수", "어획률", "레벨", "경험치",
            "조업 단계", "현재 구간" };
        string[] icons = { "icon_stat_gold", "icon_stat_catch", "icon_stat_rate",
            "icon_stat_level", "icon_stat_exp", "icon_stat_stage", "icon_stat_area" };
        for (int i = 0; i < rows.Length && i < labels.Length; i++)
        {
            TMP_Text row = rows[i];
            if (row == null) continue;
            float y = i < 5 ? -i * 23f : -135f - (i - 5) * 24f;
            float rowHeight = i == 4 ? 18f : 22f;
            Image stripe = NewImage("Area1RowBackground_" + i, content, null);
            stripe.color = i % 2 == 0
                ? new Color32(5, 55, 73, 90)
                : new Color32(7, 66, 82, 60);
            Place(stripe.rectTransform, new Vector2(0f, y),
                new Vector2(196f, 22f), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            Image icon = NewImage("Area1StatIcon_" + i, content,
                Frame(icons[i]));
            icon.preserveAspect = true;
            Place(icon.rectTransform, new Vector2(2f, y - 2f),
                new Vector2(20f, 20f), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            TMP_Text label = Object.Instantiate(row, content);
            label.name = "Area1StatLabel_" + i;
            label.text = labels[i];
            ConfigureHudText(label, 15f, 14f, TextAlignmentOptions.Left,
                Cream);
            Place(label.rectTransform, new Vector2(25f, y),
                new Vector2(77f, rowHeight), new Vector2(0f, 1f),
                new Vector2(0f, 1f));

            row.transform.SetParent(content, false);
            row.text = string.Empty;
            ConfigureHudText(row, 16f, 14f, TextAlignmentOptions.Right,
                i == 0 ? Gold : i >= 5 ? Aqua : Cream);
            Place(row.rectTransform, new Vector2(104f, y),
                new Vector2(87f, rowHeight), new Vector2(0f, 1f),
                new Vector2(0f, 1f));
        }

        Image divider = NewImage("Area1HUDDivider", content, null);
        divider.color = new Color32(77, 200, 210, 210);
        Place(divider.rectTransform, new Vector2(1f, -125f),
            new Vector2(194f, 2f), new Vector2(0f, 1f), new Vector2(0f, 1f));
    }

    internal static Image CreateExperienceGauge(TMP_Text expText)
    {
        if (expText == null || expText.transform.parent == null) return null;
        Image track = NewImage("Area1ExperienceTrack", expText.transform.parent, Frame("ref_exp_track"));
        SetFrame(track, "ref_exp_track", false);
        Place(track.rectTransform, new Vector2(26f, -116f), new Vector2(164f, 8f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        Image fill = NewImage("Area1ExperienceFill", track.transform, Frame("ref_exp_fill"));
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
        text.margin = new Vector4(1f, 0f, 1f, 0f);
        text.characterSpacing = 0.3f;
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

        Image backdrop = NewImage("Area1TimeFrame", canvas, Frame("ref_time_board"));
        SetFrame(backdrop, "ref_time_board", false);
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
        SetFrame(root.GetComponent<Image>(), "ref_ready_board", false);
        // One joined lower frame and crest replace the source board's old rail.
        Image wave = NewImage("Area1FoamBand", rect, Frame("ref_wave_strip"));
        Place(wave.rectTransform, new Vector2(-6f, -84f),
            new Vector2(324f, 28f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        wave.transform.SetAsFirstSibling();

        TMP_Text title = root.transform.Find("PreparationTitleText")?.GetComponent<TMP_Text>();
        if (title != null)
        {
            Place(title.rectTransform, new Vector2(0f, -28f),
                new Vector2(250f, 29f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            ConfigureHudText(title, 24f, 24f, TextAlignmentOptions.Center, Cream);
        }

        if (start == null) return;
        Place(start.transform as RectTransform, new Vector2(0f, 11f),
            new Vector2(248f, 42f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        SetFrame(start.GetComponent<Image>(), "ref_start_button", true);
        TMP_Text label = start.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            Place(label.rectTransform, Vector2.zero, new Vector2(224f, 32f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            ConfigureHudText(label, 21f, 21f, TextAlignmentOptions.Center,
                new Color32(57, 37, 27, 255));
        }
    }

    private static void StyleItemSlots(Transform canvas, TMP_Text textTemplate)
    {
        RectTransform panel = canvas.Find("ItemSystemUI/ItemHUD") as RectTransform;
        if (panel == null) return;
        Place(panel, new Vector2(-20f, -20f), new Vector2(388f, 132f),
            new Vector2(1f, 1f), new Vector2(1f, 1f));
        SetFrame(panel.GetComponent<Image>(), "ref_inventory_board", false);
        CreateDragHeader(panel, textTemplate, "아이템", false);
        Decorate(panel, "shell", new Vector2(-8f, -109f), new Vector2(30f, 30f));
        Decorate(panel, "starfish", new Vector2(373f, -114f), new Vector2(29f, 29f));
        Decorate(panel, "rope_knot", new Vector2(367f, -78f), new Vector2(25f, 25f));

        Image[] icons = new Image[4];
        TMP_Text[] details = new TMP_Text[4];
        for (int i = 0; i < 4; i++)
        {
            RectTransform slot = panel.Find("ItemSlot_" + (i + 1)) as RectTransform;
            if (slot == null) continue;
            Place(slot, new Vector2(15f + i * 91f, -40f),
                new Vector2(84f, 82f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            SetFrame(slot.GetComponent<Image>(), "ref_item_slot", true);
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

            Image numberBadge = NewImage("Area1SlotNumberBadge", slot, Frame("ref_number_badge"));
            SetFrame(numberBadge, "ref_number_badge", false);
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
        TMP_Text template, string title, bool raisedSign)
    {
        if (template == null) return;
        string signName = raisedSign ? "ref_hud_sign" : "ref_inventory_sign";
        Image header = NewImage("Area1DragHandle", panel, Frame(signName));
        SetFrame(header, signName, true);
        Place(header.rectTransform, new Vector2(raisedSign ? 40f : 0f,
                raisedSign ? 8f : 8f),
            new Vector2(raisedSign ? 174f : 388f, raisedSign ? 44f : 50f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        TMP_Text label = Object.Instantiate(template, header.transform);
        label.name = "Area1DragLabel";
        label.text = raisedSign ? title : "≡  " + title;
        ConfigureHudText(label, raisedSign ? 19f : 15f, raisedSign ? 19f : 15f,
            raisedSign ? TextAlignmentOptions.Center : TextAlignmentOptions.Left,
            raisedSign ? Gold : Cream);
        if (raisedSign)
            Place(label.rectTransform, new Vector2(0f, -5f),
                new Vector2(148f, 27f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f));
        else
            Place(label.rectTransform, new Vector2(44f, -16f),
                new Vector2(274f, 25f), new Vector2(0f, 1f),
                new Vector2(0f, 1f));
        header.gameObject.AddComponent<DraggableHudPanel>().Configure(panel);
    }

    private static void StyleGrowthButton(Transform canvas)
    {
        RectTransform rect = canvas.Find("SkillTreeUI/OpenTreeButton") as RectTransform;
        if (rect == null) return;
        Place(rect, new Vector2(-20f, 20f), new Vector2(225f, 58f),
            new Vector2(1f, 0f), new Vector2(1f, 0f));
        SetFrame(rect.GetComponent<Image>(), "ref_growth_board", true);
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
        ConfigureHudText(label, 20f, 18f, TextAlignmentOptions.Center, Cream);
    }

    private static Image[] StyleSpeedButtons(Transform canvas)
    {
        Image[] images = new Image[3];
        RectTransform panel = canvas.Find("TestSpeedPanel") as RectTransform;
        if (panel == null) return images;
        Place(panel, new Vector2(-20f, 130f), new Vector2(122f, 138f),
            new Vector2(1f, 0f), new Vector2(1f, 0f));
        Image backing = NewImage("Area1SpeedStack", panel, Frame("ref_speed_stack"));
        Place(backing.rectTransform, Vector2.zero, new Vector2(122f, 138f),
            new Vector2(0f, 1f), new Vector2(0f, 1f));
        backing.transform.SetAsFirstSibling();
        for (int i = 0; i < images.Length; i++)
        {
            RectTransform button = panel.Find("Speed" + (i + 1) + "Button") as RectTransform;
            if (button == null) continue;
            Place(button, new Vector2(0f, -4f - i * 45f),
                new Vector2(116f, 40f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            Image image = button.GetComponent<Image>();
            SetFrame(image, "ref_speed_button", true);
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
            {
                Sprite sprite = Frame(i == selected ? "ref_speed_selected" : "ref_speed_button");
                if (sprite != null) buttons[i].sprite = sprite;
                buttons[i].color = Color.white;
            }
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
            Image post = NewImage("Area1HotbarJoin_" + i, root, Frame("ref_rope_connector"));
            SetFrame(post, "ref_rope_connector", false);
            post.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            Place(post.rectTransform, new Vector2(143f + i * 140f, 6f),
                new Vector2(16f, 108f), Vector2.zero, Vector2.zero);
            Decorate(root, "rope_knot", new Vector2(136f + i * 140f, 96f),
                new Vector2(24f, 24f), Vector2.zero, true);
            Decorate(root, "rope_knot", new Vector2(136f + i * 140f, 0f),
                new Vector2(24f, 24f), Vector2.zero, true);
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
        Sprite sprite = Frame("ref_decor_" + name);
        if (sprite == null) sprite = Frame("decor_" + name);
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
