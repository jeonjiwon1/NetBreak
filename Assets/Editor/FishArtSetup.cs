using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class FishArtSetup
{
    private sealed class Species
    {
        public readonly string Name;
        public readonly string FishName;
        public readonly int Cell;

        public Species(string name, string fishName, int cell)
        {
            Name = name;
            FishName = fishName;
            Cell = cell;
        }

        public string Sheet => $"Assets/Art/Fish/{Name}/{Name}_Swim.png";
        public string Profile => $"Assets/Art/Fish/{Name}/{Name}_VisualProfile.asset";
        public string Data => $"Assets/Data/Fish/FishData_{Name}.asset";
        public string Prefix => Name.ToLowerInvariant();
    }

    private static readonly Species Sardine = new Species("Sardine", "정어리", 32);
    private static readonly Species Mackerel = new Species("Mackerel", "고등어", 48);
    private static readonly Species Tuna = new Species("Tuna", "참치", 64);
    private static readonly Species[] All = { Sardine, Mackerel, Tuna };
    private static readonly string[] Directions = { "horizontal", "vertical", "diagonal" };

    [MenuItem("NETBREAK/Art/Setup Mackerel And Tuna")]
    public static void SetupMackerelAndTuna()
    {
        // Resolve both destinations before writing either asset.
        if (!TryGetData(Mackerel, out FishData mackerel) ||
            !TryGetData(Tuna, out FishData tuna) ||
            AssetImporter.GetAtPath(Mackerel.Sheet) is not TextureImporter ||
            AssetImporter.GetAtPath(Tuna.Sheet) is not TextureImporter)
        {
            Debug.LogError("Mackerel/Tuna setup aborted: exact FishData or sprite sheet missing.");
            return;
        }

        SetupOne(Mackerel, mackerel);
        SetupOne(Tuna, tuna);
        AssetDatabase.SaveAssets();
        Validate();
    }

    private static void SetupOne(Species species, FishData data)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(species.Sheet);
        bool changed = ConfigureImporter(species, importer);
        if (changed) importer.SaveAndReimport();

        Sprite[] frames = LoadFrames(species);
        if (frames == null) return;

        FishVisualProfile profile = AssetDatabase.LoadAssetAtPath<FishVisualProfile>(species.Profile);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(species.Profile) != null)
            {
                Debug.LogError($"Unexpected asset at {species.Profile}");
                return;
            }
            profile = ScriptableObject.CreateInstance<FishVisualProfile>();
            AssetDatabase.CreateAsset(profile, species.Profile);
            Undo.RegisterCreatedObjectUndo(profile, $"Create {species.Name} visual profile");
        }

        SerializedObject profileObject = new SerializedObject(profile);
        Undo.RecordObject(profile, $"Configure {species.Name} visual profile");
        SetFrames(profileObject.FindProperty("horizontalFrames"), frames, 0);
        SetFrames(profileObject.FindProperty("verticalFrames"), frames, 4);
        SetFrames(profileObject.FindProperty("diagonalFrames"), frames, 8);
        profileObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(profile);

        if (data.VisualProfile != profile)
        {
            Undo.RecordObject(data, $"Link {species.Name} visual profile");
            SerializedObject fishObject = new SerializedObject(data);
            fishObject.FindProperty("visualProfile").objectReferenceValue = profile;
            fishObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(data);
        }
        Debug.Log($"{species.Name} art setup complete: 12 slices, {species.Cell}px cells, profile and FishData link. Import changed: {changed}");
    }

    public static void Validate()
    {
        bool valid = true;
        var expectedData = new Dictionary<FishData, FishVisualProfile>();
        foreach (Species species in All)
        {
            if (!TryGetData(species, out FishData data))
            {
                valid = false;
                continue;
            }
            TextureImporter importer = AssetImporter.GetAtPath(species.Sheet) as TextureImporter;
            Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(species.Sheet);
            Sprite[] frames = LoadFrames(species);
            FishVisualProfile profile = AssetDatabase.LoadAssetAtPath<FishVisualProfile>(species.Profile);
            bool oneValid = importer != null && sheet != null && frames != null && profile != null &&
                            sheet.width == species.Cell * 4 && sheet.height == species.Cell * 3 &&
                            profile.IsValid && data.VisualProfile == profile &&
                            importer.textureType == TextureImporterType.Sprite &&
                            importer.spriteImportMode == SpriteImportMode.Multiple &&
                            Mathf.Approximately(importer.spritePixelsPerUnit, 83f) &&
                            importer.filterMode == FilterMode.Point &&
                            importer.textureCompression == TextureImporterCompression.Uncompressed &&
                            !importer.mipmapEnabled && importer.wrapMode == TextureWrapMode.Clamp;
            if (oneValid)
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                oneValid &= settings.spriteMeshType == SpriteMeshType.FullRect;
                for (int row = 0; row < 3; row++)
                for (int col = 0; col < 4; col++)
                {
                    Sprite frame = frames[row * 4 + col];
                    oneValid &= frame.rect == new Rect(col * species.Cell,
                        (2 - row) * species.Cell, species.Cell, species.Cell);
                    oneValid &= frame.pivot == new Vector2(species.Cell / 2f, species.Cell / 2f);
                    oneValid &= profile.GetFrames((FishVisualSet)row)[col] == frame;
                }
                oneValid &= Mathf.Approximately(profile.FramesPerSecond, 8f) &&
                            profile.VisualScale == Vector2.one && profile.Tint == Color.white;
            }
            int count = AssetDatabase.FindAssets("t:FishVisualProfile")
                .Count(guid => AssetDatabase.GUIDToAssetPath(guid).StartsWith($"Assets/Art/Fish/{species.Name}/"));
            oneValid &= count == 1;
            if (profile != null) expectedData[data] = profile;
            valid &= oneValid;
            if (!oneValid) Debug.LogError($"Fish sprite validation failed for {species.Name}.");
        }

        foreach (string guid in AssetDatabase.FindAssets("t:FishData"))
        {
            FishData data = AssetDatabase.LoadAssetAtPath<FishData>(AssetDatabase.GUIDToAssetPath(guid));
            if (data == null) continue;
            valid &= data.VisualProfile == (expectedData.TryGetValue(data, out FishVisualProfile profile) ? profile : null);
        }
        if (valid) Debug.Log("Fish sprite pipeline valid: Sardine, Mackerel and Tuna each have 12 slices, import settings and FishData links; remaining species use fallback.");
        else Debug.LogError("Fish sprite pipeline validation failed. Check import, frame references, FishData links and fallback species.");
    }

    private static bool TryGetData(Species species, out FishData data)
    {
        data = AssetDatabase.LoadAssetAtPath<FishData>(species.Data);
        if (data != null && data.FishName == species.FishName) return true;
        Debug.LogError($"Expected {species.FishName} FishData at {species.Data}; no changes applied for this species.");
        data = null;
        return false;
    }

    private static bool ConfigureImporter(Species species, TextureImporter importer)
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
        Dictionary<string, SpriteRect> byName = new Dictionary<string, SpriteRect>();
        foreach (SpriteRect rect in existing)
        {
            if (!byName.TryAdd(rect.name, rect)) changed = true;
        }
        SpriteRect[] wanted = new SpriteRect[12];
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
        {
            string name = $"{species.Prefix}_{Directions[row]}_{col}";
            if (!byName.TryGetValue(name, out SpriteRect rect))
            {
                rect = new SpriteRect { name = name, spriteID = GUID.Generate() };
                changed = true;
            }
            Rect bounds = new Rect(col * species.Cell, (2 - row) * species.Cell,
                species.Cell, species.Cell);
            if (rect.rect != bounds || rect.alignment != SpriteAlignment.Center ||
                rect.pivot != new Vector2(0.5f, 0.5f)) changed = true;
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
            names.SetNameFileIdPairs(wanted.Select(rect =>
                new SpriteNameFileIdPair(rect.name, rect.spriteID)).ToList());
            provider.Apply();
        }
        return changed;
    }

    private static Sprite[] LoadFrames(Species species)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(species.Sheet).OfType<Sprite>().ToArray();
        if (sprites.Length != 12)
        {
            Debug.LogError($"Expected 12 sprites at {species.Sheet}; found {sprites.Length}.");
            return null;
        }
        Sprite[] ordered = new Sprite[12];
        for (int row = 0; row < 3; row++)
        for (int col = 0; col < 4; col++)
        {
            ordered[row * 4 + col] = sprites.SingleOrDefault(sprite =>
                sprite.name == $"{species.Prefix}_{Directions[row]}_{col}");
            if (ordered[row * 4 + col] == null)
            {
                Debug.LogError($"Unexpected sprite names at {species.Sheet}.");
                return null;
            }
        }
        return ordered;
    }

    private static void SetFrames(SerializedProperty property, Sprite[] sprites, int start)
    {
        property.arraySize = 4;
        for (int i = 0; i < 4; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[start + i];
    }
}
