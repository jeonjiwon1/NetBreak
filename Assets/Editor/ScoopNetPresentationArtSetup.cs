using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class ScoopNetPresentationArtSetup
{
    private const string ReadyPath = "Assets/Art/Tools/ScoopNet/ScoopNet_Ready.png";
    private const string SwingPath = "Assets/Art/Tools/ScoopNet/ScoopNet_Swing.png";
    private const string HitPath = "Assets/Art/VFX/Tools/ScoopNet_Hit.png";
    private const string SoundPath = "Assets/Audio/SFX/Tools/ScoopNet_Swing.wav";
    private const string ProfilePath = "Assets/Resources/ScoopNetPresentation.asset";

    [MenuItem("NETBREAK/Art/Setup Scoop Net Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(ReadyPath) is not TextureImporter ready ||
            AssetImporter.GetAtPath(SwingPath) is not TextureImporter swing ||
            AssetImporter.GetAtPath(HitPath) is not TextureImporter hit ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath) == null)
        {
            Debug.LogError("Scoop net setup: a source PNG or WAV is missing.");
            return;
        }

        Configure(ready, 64, 48, 1, "ScoopNet_Ready");
        Configure(swing, 64, 48, 5, "ScoopNet_Swing");
        Configure(hit, 83, 24, 4, "ScoopNet_Hit");
        Sprite[] readyFrames = LoadFrames(ReadyPath, "ScoopNet_Ready", 1);
        Sprite[] swingFrames = LoadFrames(SwingPath, "ScoopNet_Swing", 5);
        Sprite[] hitFrames = LoadFrames(HitPath, "ScoopNet_Hit", 4);
        if (readyFrames == null || swingFrames == null || hitFrames == null)
        {
            Debug.LogError("Scoop net setup: sprite slicing failed.");
            return;
        }

        ScoopNetPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<ScoopNetPresentationProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Scoop net setup: profile path is occupied.");
                return;
            }
            profile = ScriptableObject.CreateInstance<ScoopNetPresentationProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        SerializedObject data = new(profile);
        data.FindProperty("readySprite").objectReferenceValue = readyFrames[0];
        Assign(data.FindProperty("swingFrames"), swingFrames);
        Assign(data.FindProperty("hitFrames"), hitFrames);
        data.FindProperty("swingClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Scoop Net Presentation")]
    public static void Validate()
    {
        ScoopNetPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<ScoopNetPresentationProfile>(ProfilePath);
        Sprite[] ready = LoadFrames(ReadyPath, "ScoopNet_Ready", 1);
        Sprite[] swing = LoadFrames(SwingPath, "ScoopNet_Swing", 5);
        Sprite[] hit = LoadFrames(HitPath, "ScoopNet_Hit", 4);
        Texture2D readyTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ReadyPath);
        Texture2D swingTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SwingPath);
        Texture2D hitTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HitPath);
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        bool valid = profile != null && ready != null && swing != null && hit != null &&
            readyTexture != null && readyTexture.width == 48 && readyTexture.height == 48 &&
            swingTexture != null && swingTexture.width == 240 && swingTexture.height == 48 &&
            hitTexture != null && hitTexture.width == 96 && hitTexture.height == 24 &&
            clip != null && clip.channels == 1 && clip.frequency == 44100 &&
            profile.ReadySprite == ready[0] && profile.HasSwing && profile.HasHit &&
            profile.SwingClip == clip &&
            ValidImport(ReadyPath, 64) && ValidImport(SwingPath, 64) &&
            ValidImport(HitPath, 83) &&
            typeof(LandingNetController).GetProperty("CaptureRadius") != null &&
            typeof(LandingNetController).GetMethod("IncreaseCaptureRadius") != null &&
            typeof(LandingNetPresentation).GetMethod("ShowUse") != null &&
            AssetDatabase.FindAssets("t:ScoopNetPresentationProfile").Length == 1;
        if (valid)
        {
            for (int i = 0; i < 5; i++) valid &= profile.SwingFrames[i] == swing[i];
            for (int i = 0; i < 4; i++) valid &= profile.HitFrames[i] == hit[i];
        }

        if (valid)
            Debug.Log("Scoop net presentation valid: pixel swing, hit frames, mono WAV, Resources profile and runtime hook.");
        else
            Debug.LogError("Scoop net presentation validation failed. Run Setup and inspect the assets.");
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
