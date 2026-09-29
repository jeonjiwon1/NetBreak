using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Player-facing Area 1 text uses Galmuri; Nanum remains a glyph fallback.
internal static class Area1Typography
{
    internal enum Role
    {
        Body,
        Title,
        Number,
        Key
    }

    private static TMP_FontAsset regular;
    private static TMP_FontAsset bold;
    private static readonly Dictionary<TMP_FontAsset, TMP_FontAsset> runtimeFonts =
        new Dictionary<TMP_FontAsset, TMP_FontAsset>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeFonts()
    {
        regular = null;
        bold = null;
        runtimeFonts.Clear();
    }

    internal static void Apply(TMP_Text text, Role role)
    {
        if (text == null) return;

        TMP_FontAsset font = role == Role.Title || role == Role.Key
            ? Bold
            : Regular;
        if (font == null) return;

        text.font = font;
        text.fontSharedMaterial = font.material;
    }

    internal static void ApplyToHierarchy(Transform root)
    {
        if (root == null) return;
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            Role role = text.name.Contains("Title") || text.name.Contains("Name")
                ? Role.Title : Role.Body;
            Apply(text, role);
        }
    }

    private static TMP_FontAsset Regular
    {
        get
        {
            if (regular == null)
                regular = CreateRuntimeFont(Resources.Load<TMP_FontAsset>(
                    "UI/Fonts/Galmuri11 SDF"));
            return regular;
        }
    }

    private static TMP_FontAsset Bold
    {
        get
        {
            if (bold == null)
                bold = CreateRuntimeFont(Resources.Load<TMP_FontAsset>(
                    "UI/Fonts/Galmuri11 Bold SDF"));
            return bold;
        }
    }

    private static TMP_FontAsset CreateRuntimeFont(TMP_FontAsset source)
    {
        if (source == null) return null;
        if (runtimeFonts.TryGetValue(source, out TMP_FontAsset existing))
            return existing;

        // A fresh atlas keeps TMP's dynamic glyph and multi-atlas writes off
        // the persistent font, material, and texture sub-assets.
        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(
            source.sourceFontFile,
            Mathf.RoundToInt(source.faceInfo.pointSize),
            source.atlasPadding,
            source.atlasRenderMode,
            source.atlasWidth,
            source.atlasHeight,
            source.atlasPopulationMode,
            source.isMultiAtlasTexturesEnabled);
        if (font == null) return null;

        font.name = source.name;
        font.hideFlags = HideFlags.DontSave;
        font.material.hideFlags = HideFlags.DontSave;
        font.atlasTexture.hideFlags = HideFlags.DontSave;
        runtimeFonts.Add(source, font);

        List<TMP_FontAsset> runtimeFallbacks = new List<TMP_FontAsset>();
        if (source.fallbackFontAssetTable != null)
        {
            foreach (TMP_FontAsset fallback in source.fallbackFontAssetTable)
            {
                TMP_FontAsset runtimeFallback = CreateRuntimeFont(fallback);
                if (runtimeFallback != null)
                    runtimeFallbacks.Add(runtimeFallback);
            }
        }
        font.fallbackFontAssetTable = runtimeFallbacks;

        return font;
    }
}
