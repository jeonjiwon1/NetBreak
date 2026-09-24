using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SquidInkArtSetup
{
    private const string AttackPath = "Assets/Art/Fish/Squid/Squid_InkAttack.png";
    private const string PuffPath = "Assets/Art/VFX/Squid/Squid_InkPuff.png";
    private const string AudioPath = "Assets/Audio/SFX/SpecialFish/Squid_InkRelease.wav";
    private const string ProfilePath = "Assets/Resources/SquidInkPresentation.asset";
    private static readonly string[] Directions =
        { "horizontal", "vertical", "diagonal", "diagonal_nw" };

    [MenuItem("NETBREAK/Art/Setup Squid Ink Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(AttackPath) is not TextureImporter attack ||
            AssetImporter.GetAtPath(PuffPath) is not TextureImporter puff ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath) == null)
        {
            Debug.LogError("Squid ink setup failed: action sheet, puff sheet or audio clip missing.");
            return;
        }

        Configure(attack, 64, 4, 4, (row, col) =>
            $"squid_ink_{Directions[row]}_{col}");
        Configure(puff, 32, 4, 1, (_, col) => $"squid_ink_puff_{col}");

        Sprite[] action = Load(AttackPath, 16, (row, col) =>
            $"squid_ink_{Directions[row]}_{col}");
        Sprite[] cloud = Load(PuffPath, 4, (_, col) => $"squid_ink_puff_{col}");
        if (action == null || cloud == null) return;

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        SquidInkPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<SquidInkPresentationProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Squid ink setup failed: unexpected asset at profile path.");
                return;
            }
            profile = ScriptableObject.CreateInstance<SquidInkPresentationProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        SerializedObject serialized = new SerializedObject(profile);
        string[] properties = { "horizontalFrames", "verticalFrames", "diagonalFrames",
                                "diagonalNorthWestFrames" };
        for (int row = 0; row < 4; row++)
            SetFrames(serialized.FindProperty(properties[row]), action, row * 4);
        SetFrames(serialized.FindProperty("puffFrames"), cloud, 0);
        serialized.FindProperty("inkClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Squid Ink Presentation")]
    public static void Validate()
    {
        var profile = AssetDatabase.LoadAssetAtPath<SquidInkPresentationProfile>(ProfilePath);
        var attack = AssetImporter.GetAtPath(AttackPath) as TextureImporter;
        var puff = AssetImporter.GetAtPath(PuffPath) as TextureImporter;
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        var attackTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AttackPath);
        var puffTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(PuffPath);
        bool valid = profile != null && profile.HasAnimation && profile.HasPuff &&
                     profile.InkClip == clip && clip != null && clip.channels == 1 &&
                     clip.frequency == 44100 &&
                     attackTexture != null && attackTexture.width == 256 && attackTexture.height == 256 &&
                     puffTexture != null && puffTexture.width == 128 && puffTexture.height == 32 &&
                     ValidImporter(attack) && ValidImporter(puff) &&
                     Mathf.Approximately(profile.FramesPerSecond, 8f) &&
                     AssetDatabase.FindAssets("t:SquidInkPresentationProfile").Length == 1;
        if (valid)
        {
            Sprite[] action = Load(AttackPath, 16, (row, col) =>
                $"squid_ink_{Directions[row]}_{col}");
            Sprite[] cloud = Load(PuffPath, 4, (_, col) => $"squid_ink_puff_{col}");
            valid = action != null && cloud != null;
            if (valid)
            {
                for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                    valid &= profile.GetFrames((FishVisualSet)row)[col] == action[row * 4 + col] &&
                             action[row * 4 + col].rect ==
                             new Rect(col * 64, (3 - row) * 64, 64, 64) &&
                             action[row * 4 + col].pivot == new Vector2(32, 32);
                for (int col = 0; col < 4; col++)
                    valid &= profile.PuffFrames[col] == cloud[col] &&
                             cloud[col].rect == new Rect(col * 32, 0, 32, 32) &&
                             cloud[col].pivot == new Vector2(16, 16);
            }
        }
        if (valid) Debug.Log("Squid ink presentation valid: 16 action frames, 4 puff frames, profile and mono WAV linked.");
        else Debug.LogError("Squid ink presentation validation failed. Run Setup Squid Ink Presentation and inspect the assets.");
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
        int columns = 4;
        for (int i = 0; i < count; i++)
        {
            ordered[i] = sprites.SingleOrDefault(sprite => sprite.name == name(i / columns, i % columns));
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
