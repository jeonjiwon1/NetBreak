using System;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class NetPresentationArtSetup
{
    private const string MeshPath = "Assets/Art/Tools/Net/Net_Mesh.png";
    private const string RopePath = "Assets/Art/Tools/Net/Net_Rope.png";
    private const string ContactPath = "Assets/Art/VFX/Tools/Net_Contact.png";
    private const string SoundPath = "Assets/Audio/SFX/Tools/Net_Place.wav";
    private const string ProfilePath = "Assets/Resources/NetPresentation.asset";
    private const string PrefabPath = "Assets/Prefabs/Gear/Net.prefab";

    [MenuItem("NETBREAK/Art/Setup Net Presentation")]
    public static void Setup()
    {
        if (AssetImporter.GetAtPath(MeshPath) is not TextureImporter mesh ||
            AssetImporter.GetAtPath(RopePath) is not TextureImporter rope ||
            AssetImporter.GetAtPath(ContactPath) is not TextureImporter contact ||
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath) == null)
        {
            Debug.LogError("Net presentation setup: source asset missing.");
            return;
        }
        ConfigureTile(mesh, 64);
        ConfigureTile(rope, 64);
        ConfigureContact(contact);

        Sprite meshSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MeshPath);
        Sprite ropeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RopePath);
        Sprite[] frames = LoadContactFrames();
        if (meshSprite == null || ropeSprite == null || frames == null)
        {
            Debug.LogError("Net presentation setup: sprite import incomplete.");
            return;
        }

        NetPresentationProfile profile = AssetDatabase.LoadAssetAtPath<NetPresentationProfile>(ProfilePath);
        if (profile == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(ProfilePath) != null)
            {
                Debug.LogError("Net presentation setup: profile path occupied.");
                return;
            }
            profile = ScriptableObject.CreateInstance<NetPresentationProfile>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }
        SerializedObject data = new(profile);
        data.FindProperty("meshTile").objectReferenceValue = meshSprite;
        data.FindProperty("ropeTile").objectReferenceValue = ropeSprite;
        SerializedProperty array = data.FindProperty("contactFrames");
        array.arraySize = 4;
        for (int i = 0; i < 4; i++)
            array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        data.FindProperty("placeClip").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);

        AssetDatabase.SaveAssets();
        Validate();
    }

    [MenuItem("NETBREAK/Art/Validate Net Presentation")]
    public static void Validate()
    {
        NetPresentationProfile profile = AssetDatabase.LoadAssetAtPath<NetPresentationProfile>(ProfilePath);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Sprite[] frames = LoadContactFrames();
        AudioClip sound = AssetDatabase.LoadAssetAtPath<AudioClip>(SoundPath);
        bool valid = profile != null && prefab != null && frames != null &&
            profile.MeshTile == AssetDatabase.LoadAssetAtPath<Sprite>(MeshPath) &&
            profile.RopeTile == AssetDatabase.LoadAssetAtPath<Sprite>(RopePath) &&
            profile.HasContact && profile.PlaceClip == sound && sound != null &&
            sound.channels == 1 && sound.frequency == 44100 &&
            prefab.GetComponent<NetController>() != null &&
            prefab.GetComponent<BoxCollider2D>() != null &&
            ValidImport(MeshPath, 64, false) && ValidImport(RopePath, 64, false) &&
            ValidImport(ContactPath, 83, true);
        if (valid)
            for (int i = 0; i < 4; i++) valid &= profile.ContactFrames[i] == frames[i];
        if (valid)
            Debug.Log("Net presentation valid: tiled mesh/rope, contact, place WAV, Resources profile and runtime prefab binding.");
        else
            Debug.LogError("Net presentation validation failed. Run Setup and inspect the assets.");
    }

    private static void ConfigureTile(TextureImporter importer, int ppu)
    {
        ConfigureCommon(importer, ppu);
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();
    }

    private static void ConfigureContact(TextureImporter importer)
    {
        ConfigureCommon(importer, 83);
        importer.spriteImportMode = SpriteImportMode.Multiple;
        SpriteDataProviderFactories factory = new();
        factory.Init();
        ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        SpriteRect[] existing = provider.GetSpriteRects();
        SpriteRect[] wanted = new SpriteRect[4];
        for (int i = 0; i < 4; i++)
        {
            string name = $"Net_Contact_{i}";
            SpriteRect rect = existing.FirstOrDefault(value => value.name == name) ??
                new SpriteRect { name = name, spriteID = GUID.Generate() };
            rect.rect = new Rect(i * 24, 0, 24, 24);
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

    private static void ConfigureCommon(TextureImporter importer, int ppu)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = ppu;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings settings = new();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
    }

    private static Sprite[] LoadContactFrames()
    {
        Sprite[] all = AssetDatabase.LoadAllAssetsAtPath(ContactPath).OfType<Sprite>().ToArray();
        if (all.Length != 4) return null;
        Sprite[] frames = new Sprite[4];
        for (int i = 0; i < 4; i++)
        {
            frames[i] = all.SingleOrDefault(sprite => sprite.name == $"Net_Contact_{i}");
            if (frames[i] == null) return null;
        }
        return frames;
    }

    private static bool ValidImport(string path, int ppu, bool multiple)
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
