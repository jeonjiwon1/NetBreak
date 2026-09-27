using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class CastNetPresentationArtSetup
{
    private const string FoldedPath = "Assets/Art/Tools/CastNet/CastNet_Folded.png";
    private const string OpenPath = "Assets/Art/Tools/CastNet/CastNet_Open.png";
    private const string AreaPath = "Assets/Art/VFX/Tools/CastNet_Area.png";
    private const string HitPath = "Assets/Art/VFX/Tools/CastNet_Hit.png";
    private const string SoundPath = "Assets/Audio/SFX/Tools/CastNet_Open.wav";
    private const string ProfilePath = "Assets/Resources/CastNetPresentation.asset";

    [MenuItem("NETBREAK/Art/Setup Cast Net Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(FoldedPath) is not TextureImporter folded ||
            AssetImporter.GetAtPath(OpenPath) is not TextureImporter opening ||
            AssetImporter.GetAtPath(AreaPath) is not TextureImporter area ||
            AssetImporter.GetAtPath(HitPath) is not TextureImporter hit ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath) == null)
        {
            Debug.LogError("Cast net setup: a source PNG or WAV is missing.");
            return;
        }

        Configure(folded, 64, 16, 1, "CastNet_Folded");
        Configure(opening, 64, 64, 5, "CastNet_Open");
        Configure(area, 64, 64, 4, "CastNet_Area");
        Configure(hit, 64, 24, 4, "CastNet_Hit");
        Sprite[] foldedFrames = LoadFrames(FoldedPath, "CastNet_Folded", 1);
        Sprite[] openingFrames = LoadFrames(OpenPath, "CastNet_Open", 5);
        Sprite[] areaFrames = LoadFrames(AreaPath, "CastNet_Area", 4);
        Sprite[] hitFrames = LoadFrames(HitPath, "CastNet_Hit", 4);
        if (foldedFrames == null || openingFrames == null || areaFrames == null || hitFrames == null)
        {
            Debug.LogError("Cast net setup: sprite slicing failed.");
            return;
        }

        CastNetPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<CastNetPresentationProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Cast net setup: profile path is occupied.");
                return;
            }
            profile = ScriptableObject.CreateInstance<CastNetPresentationProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }
        SerializedObject data = new(profile);
        data.FindProperty("foldedSprite").objectReferenceValue = foldedFrames[0];
        Assign(data.FindProperty("openingFrames"), openingFrames);
        Assign(data.FindProperty("areaFrames"), areaFrames);
        Assign(data.FindProperty("hitFrames"), hitFrames);
        data.FindProperty("castClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Cast Net Presentation")]
    public static void Validate()
    {
        CastNetPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<CastNetPresentationProfile>(ProfilePath);
        Sprite[] folded = LoadFrames(FoldedPath, "CastNet_Folded", 1);
        Sprite[] opening = LoadFrames(OpenPath, "CastNet_Open", 5);
        Sprite[] area = LoadFrames(AreaPath, "CastNet_Area", 4);
        Sprite[] hit = LoadFrames(HitPath, "CastNet_Hit", 4);
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        bool valid = profile != null && folded != null && opening != null &&
            area != null && hit != null && clip != null && clip.channels == 1 &&
            clip.frequency == 44100 &&
            TextureSize(FoldedPath, 16, 16) && TextureSize(OpenPath, 320, 64) &&
            TextureSize(AreaPath, 256, 64) && TextureSize(HitPath, 96, 24) &&
            ValidImport(FoldedPath, 64) && ValidImport(OpenPath, 64) &&
            ValidImport(AreaPath, 64) && ValidImport(HitPath, 64) &&
            profile.FoldedSprite == folded[0] && profile.HasOpening &&
            profile.HasArea && profile.HasHit && profile.CastClip == clip &&
            typeof(CastNetController).GetProperty("CaptureRadius") != null &&
            typeof(CastNetController).GetMethod("ConfirmTacticalCast") != null &&
            typeof(CastNetPresentation).GetMethod("ShowCast") != null &&
            AssetDatabase.FindAssets("t:CastNetPresentationProfile").Length == 1;
        if (valid)
        {
            for (int i = 0; i < 5; i++) valid &= profile.OpeningFrames[i] == opening[i];
            for (int i = 0; i < 4; i++)
                valid &= profile.AreaFrames[i] == area[i] && profile.HitFrames[i] == hit[i];
        }
        if (valid)
            Debug.Log("Cast net presentation valid: pixel assets, WAV, profile, radius and runtime hook.");
        else
            Debug.LogError("Cast net presentation validation failed. Run Setup and inspect the assets.");
    }

    private static bool TextureSize(string path, int width, int height)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        return texture != null && texture.width == width && texture.height == height;
    }

    private static void Configure(TextureImporter importer, int ppu, int cell,
        int count, string prefix)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings settings = new();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        SpriteDataProviderFactories factory = new();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] existing = provider.GetSpriteRects();
        SpriteRect[] wanted = new SpriteRect[count];
        for (int i = 0; i < count; i++)
        {
            string name = $"{prefix}_{i}";
            SpriteRect rect = existing.FirstOrDefault(value => value.name == name) ??
                new SpriteRect { name = name, spriteID = GUID.Generate() };
            rect.rect = new Rect(i * cell, 0, cell, cell);
            rect.alignment = SpriteAlignment.Center;
            rect.pivot = new Vector2(.5f, .5f);
            wanted[i] = rect;
        }
        provider.SetSpriteRects(wanted);
        provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
            wanted.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)).ToList());
        provider.Apply();
        importer.SaveAndReimport();
    }

    private static Sprite[] LoadFrames(string path, string prefix, int count)
    {
        Sprite[] all = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (all.Length != count) return null;
        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            frames[i] = all.SingleOrDefault(sprite => sprite.name == $"{prefix}_{i}");
            if (frames[i] == null) return null;
        }
        return frames;
    }

    private static void Assign(SerializedProperty array, Sprite[] frames)
    {
        array.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
    }

    private static bool ValidImport(string path, int ppu)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Multiple ||
            !Mathf.Approximately(importer.spritePixelsPerUnit, ppu) ||
            importer.filterMode != FilterMode.Point ||
            importer.textureCompression != TextureImporterCompression.Uncompressed ||
            importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp)
            return false;
        TextureImporterSettings settings = new();
        importer.ReadTextureSettings(settings);
        return settings.spriteMeshType == SpriteMeshType.FullRect;
    }
}
