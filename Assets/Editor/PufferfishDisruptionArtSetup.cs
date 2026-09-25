using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class PufferfishDisruptionArtSetup
{
    private const string ActionPath = "Assets/Art/Fish/Pufferfish/Pufferfish_Disrupt.png";
    private const string ImpactPath = "Assets/Art/VFX/Pufferfish/Pufferfish_NetImpact.png";
    private const string AudioPath = "Assets/Audio/SFX/SpecialFish/Pufferfish_NetDisrupt.wav";
    private const string ProfilePath = "Assets/Resources/PufferfishDisruption.asset";
    private const string FishDataPath = "Assets/Data/Fish/FishData_Pufferfish.asset";
    private static readonly string[] Directions =
        { "horizontal", "vertical", "diagonal", "diagonal_nw" };

    [MenuItem("NETBREAK/Art/Setup Pufferfish Disruption Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(ActionPath) is not TextureImporter action ||
            AssetImporter.GetAtPath(ImpactPath) is not TextureImporter impact ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath) == null ||
            AssetDatabase.LoadAssetAtPath<FishData>(FishDataPath) == null)
        {
            Debug.LogError("Pufferfish disruption setup failed: action, impact, audio or FishData missing.");
            return;
        }

        Configure(action, 48, 4, 4, (row, col) =>
            $"puffer_disrupt_{Directions[row]}_{col}");
        Configure(impact, 24, 4, 1, (_, col) => $"puffer_net_impact_{col}");
        Sprite[] poses = Load(ActionPath, 16, (row, col) =>
            $"puffer_disrupt_{Directions[row]}_{col}");
        Sprite[] bursts = Load(ImpactPath, 4, (_, col) => $"puffer_net_impact_{col}");
        if (poses == null || bursts == null)
        {
            Debug.LogError("Pufferfish disruption setup failed: sprite slicing incomplete.");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        PufferfishDisruptionProfile profile =
            AssetDatabase.LoadAssetAtPath<PufferfishDisruptionProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Pufferfish disruption setup failed: unexpected asset at profile path.");
                return;
            }
            profile = ScriptableObject.CreateInstance<PufferfishDisruptionProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        SerializedObject serialized = new SerializedObject(profile);
        string[] properties = { "horizontalFrames", "verticalFrames", "diagonalFrames",
                                "diagonalNorthWestFrames" };
        for (int row = 0; row < 4; row++)
            SetFrames(serialized.FindProperty(properties[row]), poses, row * 4);
        SetFrames(serialized.FindProperty("impactFrames"), bursts, 0);
        serialized.FindProperty("soundClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Pufferfish Disruption Presentation")]
    public static void Validate()
    {
        var profile = AssetDatabase.LoadAssetAtPath<PufferfishDisruptionProfile>(ProfilePath);
        var fish = AssetDatabase.LoadAssetAtPath<FishData>(FishDataPath);
        var action = AssetImporter.GetAtPath(ActionPath) as TextureImporter;
        var impact = AssetImporter.GetAtPath(ImpactPath) as TextureImporter;
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        var actionTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ActionPath);
        var impactTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ImpactPath);
        bool valid = profile != null && profile.HasAnimation && profile.HasImpact &&
                     profile.SoundClip == clip && clip != null &&
                     clip.channels == 1 && clip.frequency == 44100 &&
                     fish != null && fish.SpecialType == FishSpecialType.Pufferfish &&
                     fish.VisualProfile != null && fish.VisualProfile.IsValid &&
                     actionTexture != null && actionTexture.width == 192 && actionTexture.height == 192 &&
                     impactTexture != null && impactTexture.width == 96 && impactTexture.height == 24 &&
                     ValidImporter(action) && ValidImporter(impact) &&
                     Mathf.Approximately(profile.FramesPerSecond, 12f) &&
                     AssetDatabase.FindAssets("t:PufferfishDisruptionProfile").Length == 1;
        if (valid)
        {
            Sprite[] poses = Load(ActionPath, 16, (row, col) =>
                $"puffer_disrupt_{Directions[row]}_{col}");
            Sprite[] bursts = Load(ImpactPath, 4, (_, col) => $"puffer_net_impact_{col}");
            valid = poses != null && bursts != null;
            if (valid)
            {
                for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                    valid &= profile.GetFrames((FishVisualSet)row)[col] == poses[row * 4 + col] &&
                             poses[row * 4 + col].rect ==
                             new Rect(col * 48, (3 - row) * 48, 48, 48) &&
                             poses[row * 4 + col].pivot == new Vector2(24, 24);
                for (int col = 0; col < 4; col++)
                    valid &= profile.ImpactFrames[col] == bursts[col] &&
                             bursts[col].rect == new Rect(col * 24, 0, 24, 24) &&
                             bursts[col].pivot == new Vector2(12, 12);
            }
        }
        if (valid) Debug.Log("Pufferfish disruption presentation valid: 16 poses, impact, mono WAV, profile and FishData.");
        else Debug.LogError("Pufferfish disruption presentation validation failed. Run Setup and inspect the assets.");
    }

    private static bool ValidImporter(TextureImporter importer)
    {
        if (importer == null || importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Multiple ||
            !Mathf.Approximately(importer.spritePixelsPerUnit, 83f) ||
            importer.filterMode != FilterMode.Point ||
            importer.textureCompression != TextureImporterCompression.Uncompressed ||
            importer.mipmapEnabled || importer.wrapMode != TextureWrapMode.Clamp)
            return false;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        return settings.spriteMeshType == SpriteMeshType.FullRect;
    }

    private static void Configure(TextureImporter importer, int cell, int columns, int rows,
        Func<int, int, string> name)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 83f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] existing = provider.GetSpriteRects();
        SpriteRect[] wanted = new SpriteRect[columns * rows];
        bool changed = existing.Length != wanted.Length;
        for (int row = 0; row < rows; row++)
        for (int col = 0; col < columns; col++)
        {
            string spriteName = name(row, col);
            SpriteRect rect = existing.FirstOrDefault(item => item.name == spriteName) ??
                new SpriteRect { name = spriteName, spriteID = GUID.Generate() };
            Rect bounds = new Rect(col * cell, (rows - 1 - row) * cell, cell, cell);
            changed |= rect.rect != bounds || rect.alignment != SpriteAlignment.Center ||
                       rect.pivot != new Vector2(.5f, .5f);
            rect.rect = bounds;
            rect.alignment = SpriteAlignment.Center;
            rect.pivot = new Vector2(.5f, .5f);
            wanted[row * columns + col] = rect;
        }
        if (changed)
        {
            provider.SetSpriteRects(wanted);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(
                wanted.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)).ToList());
            provider.Apply();
        }
        importer.SaveAndReimport();
    }

    private static Sprite[] Load(string path, int count, Func<int, int, string> name)
    {
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        if (sprites.Length != count) return null;
        Sprite[] ordered = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            ordered[i] = sprites.SingleOrDefault(sprite => sprite.name == name(i / 4, i % 4));
            if (ordered[i] == null) return null;
        }
        return ordered;
    }

    private static void SetFrames(SerializedProperty property, Sprite[] sprites, int offset)
    {
        property.arraySize = 4;
        for (int i = 0; i < 4; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[offset + i];
    }
}
