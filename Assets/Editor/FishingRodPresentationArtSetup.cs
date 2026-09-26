using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class FishingRodPresentationArtSetup
{
    private const string IdlePath = "Assets/Art/Tools/FishingRod/FishingRod_Idle.png";
    private const string AttackPath = "Assets/Art/Tools/FishingRod/FishingRod_Attack.png";
    private const string HitPath = "Assets/Art/VFX/Tools/FishingRod_Hit.png";
    private const string AudioPath = "Assets/Audio/SFX/Tools/FishingRod_Hit.wav";
    private const string ProfilePath = "Assets/Resources/FishingRodPresentation.asset";
    private const string PrefabPath = "Assets/Prefabs/Gear/FishingRod.prefab";

    [MenuItem("NETBREAK/Art/Setup Fishing Rod Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(IdlePath) is not TextureImporter idle ||
            AssetImporter.GetAtPath(AttackPath) is not TextureImporter attack ||
            AssetImporter.GetAtPath(HitPath) is not TextureImporter hit ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
        {
            Debug.LogError("Fishing rod presentation setup: an asset is missing.");
            return;
        }

        Configure(idle, 32, 32, 1, "FishingRod_Idle");
        Configure(attack, 32, 32, 3, "FishingRod_Attack");
        Configure(hit, 83, 16, 4, "FishingRod_Hit");

        Sprite[] idleSprites = LoadFrames(IdlePath, "FishingRod_Idle", 1);
        Sprite idleSprite = idleSprites != null ? idleSprites[0] : null;
        Sprite[] attackSprites = LoadFrames(AttackPath, "FishingRod_Attack", 3);
        Sprite[] hitSprites = LoadFrames(HitPath, "FishingRod_Hit", 4);
        if (idleSprite == null || attackSprites == null || hitSprites == null)
        {
            Debug.LogError("Fishing rod presentation setup: sprite import incomplete.");
            return;
        }

        FishingRodPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<FishingRodPresentationProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Fishing rod presentation setup: profile path is occupied.");
                return;
            }
            profile = ScriptableObject.CreateInstance<FishingRodPresentationProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        SerializedObject data = new(profile);
        data.FindProperty("idleSprite").objectReferenceValue = idleSprite;
        AssignFrames(data.FindProperty("attackFrames"), attackSprites);
        AssignFrames(data.FindProperty("hitFrames"), hitSprites);
        data.FindProperty("hitClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            FishingRodPresentation presentation = root.GetComponent<FishingRodPresentation>();
            if (presentation == null) presentation = root.AddComponent<FishingRodPresentation>();
            SerializedObject component = new(presentation);
            component.FindProperty("profile").objectReferenceValue = profile;
            component.ApplyModifiedPropertiesWithoutUndo();
            root.GetComponent<SpriteRenderer>().sprite = idleSprite;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Fishing Rod Presentation")]
    public static void Validate()
    {
        FishingRodPresentationProfile profile =
            AssetDatabase.LoadAssetAtPath<FishingRodPresentationProfile>(ProfilePath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Sprite[] idleSprites = LoadFrames(IdlePath, "FishingRod_Idle", 1);
        Sprite idle = idleSprites != null ? idleSprites[0] : null;
        Sprite[] attack = LoadFrames(AttackPath, "FishingRod_Attack", 3);
        Sprite[] hit = LoadFrames(HitPath, "FishingRod_Hit", 4);
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        Texture2D idleTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(IdlePath);
        Texture2D attackTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AttackPath);
        Texture2D hitTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(HitPath);
        FishingRodPresentation presentation =
            prefab != null ? prefab.GetComponent<FishingRodPresentation>() : null;
        bool valid = profile != null && prefab != null && presentation != null &&
                     prefab.GetComponent<FishingRodController>() != null &&
                     prefab.GetComponent<LineRenderer>() != null &&
                     presentation.Profile == profile &&
                     prefab.GetComponent<SpriteRenderer>().sprite == idle &&
                     profile.IdleSprite == idle && profile.HasAttack && profile.HasHit &&
                     attack != null && hit != null && clip != null &&
                     profile.HitClip == clip && clip.channels == 1 && clip.frequency == 44100 &&
                     idleTexture != null && idleTexture.width == 32 && idleTexture.height == 32 &&
                     attackTexture != null && attackTexture.width == 96 && attackTexture.height == 32 &&
                     hitTexture != null && hitTexture.width == 64 && hitTexture.height == 16 &&
                     ValidImporter(IdlePath, 32, true) &&
                     ValidImporter(AttackPath, 32, true) &&
                     ValidImporter(HitPath, 83, true) &&
                     AssetDatabase.FindAssets("t:FishingRodPresentationProfile").Length == 1;
        if (valid)
        {
            for (int i = 0; i < 3; i++) valid &= profile.AttackFrames[i] == attack[i];
            for (int i = 0; i < 4; i++) valid &= profile.HitFrames[i] == hit[i];
        }

        if (valid)
            Debug.Log("Fishing rod presentation valid: body, attack, line, hit, mono WAV, profile and prefab.");
        else
            Debug.LogError("Fishing rod presentation validation failed. Run Setup and inspect the assets.");
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
        Sprite[] result = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = all.SingleOrDefault(sprite => sprite.name == $"{prefix}_{i}");
            if (result[i] == null) return null;
        }
        return result;
    }

    private static void AssignFrames(SerializedProperty property, Sprite[] frames)
    {
        property.arraySize = frames.Length;
        for (int i = 0; i < frames.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
    }

    private static bool ValidImporter(string path, int ppu, bool multiple)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != (multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single) ||
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
