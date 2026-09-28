using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

// Generates only missing TMP assets when the official source fonts are imported.
// The Main scene and prefab serialization are left untouched.
[InitializeOnLoad]
internal static class GalmuriFontAssetSetup
{
    private const string SourceDirectory = "Assets/UI/Fonts/Galmuri";
    private const string OutputDirectory = "Assets/Resources/UI/Fonts";
    private const string NanumPath = "Assets/UI/Fonts/NanumGothic-Bold SDF.asset";

    static GalmuriFontAssetSetup()
    {
        EditorApplication.delayCall += EnsureAssets;
    }

    [MenuItem("Tools/NETBREAK/Ensure Galmuri TMP Font Assets")]
    private static void EnsureAssets()
    {
        Font regularSource = AssetDatabase.LoadAssetAtPath<Font>(
            SourceDirectory + "/Galmuri11.ttf");
        Font boldSource = AssetDatabase.LoadAssetAtPath<Font>(
            SourceDirectory + "/Galmuri11-Bold.ttf");
        if (regularSource == null || boldSource == null) return;

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");
        EnsureFolder(OutputDirectory);

        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NanumPath);
        EnsureFont(regularSource, "Galmuri11 SDF", fallback);
        EnsureFont(boldSource, "Galmuri11 Bold SDF", fallback);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int separator = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, separator),
            path.Substring(separator + 1));
    }

    private static void EnsureFont(Font source, string name, TMP_FontAsset fallback)
    {
        string path = OutputDirectory + "/" + name + ".asset";
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path) != null) return;

        TMP_FontAsset font = TMP_FontAsset.CreateFontAsset(source, 90, 9,
            GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        if (font == null)
        {
            Debug.LogError("Galmuri TMP Font Asset creation failed: " + name);
            return;
        }

        font.name = name;
        font.atlasTexture.name = name + " Atlas";
        font.material.name = name + " Material";
        if (fallback != null)
            font.fallbackFontAssetTable = new List<TMP_FontAsset> { fallback };

        AssetDatabase.CreateAsset(font, path);
        AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
        AssetDatabase.AddObjectToAsset(font.material, font);
        EditorUtility.SetDirty(font);
        Debug.Log("Created " + path);
    }
}
