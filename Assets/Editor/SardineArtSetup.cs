using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SardineArtSetup
{
    private const string SheetPath = "Assets/Art/Fish/Sardine/Sardine_Swim.png";
    private const string ProfilePath = "Assets/Art/Fish/Sardine/Sardine_VisualProfile.asset";
    private const string SardineDataPath = "Assets/Data/Fish/FishData_Sardine.asset";
    private static readonly string[] Directions = { "horizontal", "vertical", "diagonal" };

    [MenuItem("NETBREAK/Art/Setup Sardine Prototype")]
    public static void Setup()
    {
        if (!TryFindSardine(out FishData sardine)) return;
        TextureImporter importer = AssetImporter.GetAtPath(SheetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError($"Sardine sheet missing: {SheetPath}");
            return;
        }

        bool importChanged = ConfigureImporter(importer);
        if (importChanged) importer.SaveAndReimport();

        Sprite[] sprites = LoadFrames();
        if (sprites == null) return;

        FishVisualProfile profile = AssetDatabase.LoadAssetAtPath<FishVisualProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError($"Unexpected asset at {ProfilePath}");
                return;
            }
            profile = ScriptableObject.CreateInstance<FishVisualProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
            Undo.RegisterCreatedObjectUndo(profile, "Create sardine visual profile");
        }

        Undo.RecordObject(profile, "Configure sardine visual profile");
        SerializedObject profileObject = new SerializedObject(profile);
        SetFrames(profileObject.FindProperty("horizontalFrames"), sprites, 0);
        SetFrames(profileObject.FindProperty("verticalFrames"), sprites, 4);
        SetFrames(profileObject.FindProperty("diagonalFrames"), sprites, 8);
        profileObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(profile);

        if (sardine.VisualProfile != profile)
        {
            Undo.RecordObject(sardine, "Link sardine visual profile");
            SerializedObject fishObject = new SerializedObject(sardine);
            fishObject.FindProperty("visualProfile").objectReferenceValue = profile;
            fishObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(sardine);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Sardine art setup complete: 12 slices, profile {ProfilePath}, FishData link. Import changed: {importChanged}");
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Fish Sprite Pipeline")]
    public static void Validate()
    {
        FishArtSetup.Validate();
    }

    private static bool TryFindSardine(out FishData sardine)
    {
        sardine = null;
        List<FishData> candidates = new List<FishData>();
        foreach (string guid in AssetDatabase.FindAssets("t:FishData"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FishData data = AssetDatabase.LoadAssetAtPath<FishData>(path);
            if (data != null && (path.EndsWith("FishData_Sardine.asset", StringComparison.OrdinalIgnoreCase) ||
                                 data.FishName == "정어리")) candidates.Add(data);
        }
        if (candidates.Count != 1 || AssetDatabase.GetAssetPath(candidates[0]) != SardineDataPath)
        {
            Debug.LogError($"Expected one Sardine FishData at {SardineDataPath}, found {candidates.Count}. No changes applied.");
            return false;
        }
        sardine = candidates[0];
        return true;
    }

    private static bool ConfigureImporter(TextureImporter importer)
    {
        bool changed = importer.textureType != TextureImporterType.Sprite ||
                       importer.spriteImportMode != SpriteImportMode.Multiple ||
                       !Mathf.Approximately(importer.spritePixelsPerUnit, 83f) ||
                       importer.filterMode != FilterMode.Point ||
                       importer.textureCompression != TextureImporterCompression.Uncompressed ||
                       importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 83f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.spriteMeshType != SpriteMeshType.FullRect) changed = true;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);

        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] existing = provider.GetSpriteRects();
        Dictionary<string, SpriteRect> existingByName = new Dictionary<string, SpriteRect>();
        foreach (SpriteRect rect in existing)
        {
            if (existingByName.ContainsKey(rect.name)) changed = true;
            else existingByName.Add(rect.name, rect);
        }
        SpriteRect[] wanted = new SpriteRect[12];
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
        {
            string name = $"sardine_{Directions[row]}_{col}";
            if (!existingByName.TryGetValue(name, out SpriteRect rect))
            {
                rect = new SpriteRect { name = name, spriteID = GUID.Generate() };
                changed = true;
            }
            Rect bounds = new Rect(col * 32, (2 - row) * 32, 32, 32);
            if (rect.rect != bounds || rect.alignment != SpriteAlignment.Center || rect.pivot != new Vector2(0.5f, 0.5f))
                changed = true;
            rect.rect = bounds;
            rect.alignment = SpriteAlignment.Center;
            rect.pivot = new Vector2(0.5f, 0.5f);
            wanted[row * 4 + col] = rect;
        }
        if (existing.Length != 12) changed = true;
        if (changed)
        {
            provider.SetSpriteRects(wanted);
            ISpriteNameFileIdDataProvider names = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            names.SetNameFileIdPairs(wanted.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)).ToList());
            provider.Apply();
        }
        return changed;
    }

    private static Sprite[] LoadFrames()
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(SheetPath).OfType<Sprite>().ToArray();
        if (sprites.Length != 12)
        {
            Debug.LogError($"Expected 12 sardine sprites at {SheetPath}; found {sprites.Length}.");
            return null;
        }
        Sprite[] ordered = new Sprite[12];
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
        {
            ordered[row * 4 + col] = sprites.SingleOrDefault(sprite => sprite.name == $"sardine_{Directions[row]}_{col}");
            if (ordered[row * 4 + col] == null)
            {
                Debug.LogError("Sardine sprite names do not match the 3 x 4 convention.");
                return null;
            }
        }
        return ordered;
    }

    private static void SetFrames(SerializedProperty property, Sprite[] sprites, int start)
    {
        property.arraySize = 4;
        for (int i = 0; i < 4; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[start + i];
    }
}
