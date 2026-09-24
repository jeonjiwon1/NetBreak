#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class FishVisualPipelineTests
{
    private readonly List<Object> owned = new List<Object>();
    private Sprite[] frames;
    private Sprite prototypeSprite;
    private FishVisualProfile profile;
    private FishData sardine;
    private FishData fallback;
    private FishController fish;
    private FishVisualController visual;
    private SpriteRenderer rootRenderer;

    [SetUp]
    public void SetUp()
    {
        Texture2D texture = Own(new Texture2D(5, 1, TextureFormat.RGBA32, false));
        texture.SetPixels(new[] { Color.white, Color.red, Color.green, Color.blue, Color.yellow });
        texture.Apply();
        prototypeSprite = Own(Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f)));
        frames = new Sprite[4];
        for (int i = 0; i < 4; i++)
            frames[i] = Own(Sprite.Create(texture, new Rect(i + 1, 0, 1, 1), new Vector2(0.5f, 0.5f)));

        profile = Own(ScriptableObject.CreateInstance<FishVisualProfile>());
        SerializedObject visualData = new SerializedObject(profile);
        foreach (string name in new[] { "horizontalFrames", "verticalFrames", "diagonalFrames" })
        {
            SerializedProperty array = visualData.FindProperty(name);
            array.arraySize = 4;
            for (int i = 0; i < 4; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
        }
        visualData.ApplyModifiedPropertiesWithoutUndo();

        sardine = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject sardineData = new SerializedObject(sardine);
        sardineData.FindProperty("visualProfile").objectReferenceValue = profile;
        sardineData.FindProperty("visualScale").vector2Value = new Vector2(0.35f, 0.18f);
        sardineData.FindProperty("maxResistance").floatValue = 5f;
        sardineData.ApplyModifiedPropertiesWithoutUndo();
        fallback = Own(ScriptableObject.CreateInstance<FishData>());
        SerializedObject fallbackData = new SerializedObject(fallback);
        fallbackData.FindProperty("visualScale").vector2Value = new Vector2(0.5f, 0.25f);
        fallbackData.FindProperty("visualColor").colorValue = Color.cyan;
        fallbackData.FindProperty("maxResistance").floatValue = 10f;
        fallbackData.ApplyModifiedPropertiesWithoutUndo();

        GameObject root = Own(new GameObject("Fish visual test"));
        rootRenderer = root.AddComponent<SpriteRenderer>();
        rootRenderer.sprite = prototypeSprite;
        root.AddComponent<CircleCollider2D>().radius = 0.5f;
        fish = root.AddComponent<FishController>();
        typeof(FishController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(fish, null);
        root.AddComponent<FishMovement>();
        visual = root.GetComponent<FishVisualController>();
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [TestCase(1f, 0f, FishVisualSet.Horizontal, false, false)]
    [TestCase(-1f, 0f, FishVisualSet.Horizontal, true, false)]
    [TestCase(0f, 1f, FishVisualSet.Vertical, false, false)]
    [TestCase(0f, -1f, FishVisualSet.Vertical, false, true)]
    [TestCase(1f, 1f, FishVisualSet.Diagonal, false, false)]
    [TestCase(-1f, 1f, FishVisualSet.Diagonal, true, false)]
    [TestCase(1f, -1f, FishVisualSet.Diagonal, false, true)]
    [TestCase(-1f, -1f, FishVisualSet.Diagonal, true, true)]
    public void DirectionMapping(float x, float y, FishVisualSet set, bool flipX, bool flipY)
    {
        FishVisualDirection result = FishVisualDirectionResolver.Resolve(
            new Vector2(x, y), FishVisualHeading.East);
        Assert.That(result.Set, Is.EqualTo(set));
        Assert.That(result.FlipX, Is.EqualTo(flipX));
        Assert.That(result.FlipY, Is.EqualTo(flipY));
    }

    [Test]
    public void TinyVelocityAndBoundaryHoldLastHeading()
    {
        FishVisualDirection zero = FishVisualDirectionResolver.Resolve(Vector2.zero, FishVisualHeading.NorthWest);
        FishVisualDirection tiny = FishVisualDirectionResolver.Resolve(new Vector2(0.0001f, 0f), FishVisualHeading.South);
        FishVisualDirection boundary = FishVisualDirectionResolver.Resolve(
            new Vector2(1f, Mathf.Tan(25f * Mathf.Deg2Rad)), FishVisualHeading.East);
        Assert.That(zero.Heading, Is.EqualTo(FishVisualHeading.NorthWest));
        Assert.That(tiny.Heading, Is.EqualTo(FishVisualHeading.South));
        Assert.That(boundary.Heading, Is.EqualTo(FishVisualHeading.East));
    }

    [Test]
    public void FourFrameLoopUsesScaledDeltaAndPauseHoldsFrame()
    {
        fish.Initialize(sardine);
        SpriteRenderer pixel = PixelRenderer();
        Assert.That(pixel.sprite, Is.EqualTo(frames[0]));
        for (int i = 1; i <= 4; i++)
        {
            visual.Tick(0.125f, Vector2.right);
            Assert.That(pixel.sprite, Is.EqualTo(frames[i % 4]));
        }
        visual.Tick(0f, Vector2.right);
        Assert.That(visual.FrameIndex, Is.Zero);
        Assert.That(pixel.sprite, Is.EqualTo(frames[0]));
    }

    [Test]
    public void MissingProfileKeepsPrototypeSpriteAndColor()
    {
        fish.Initialize(fallback);
        Assert.That(visual.UsesCustomVisual, Is.False);
        Assert.That(rootRenderer.enabled, Is.True);
        Assert.That(rootRenderer.sprite, Is.EqualTo(prototypeSprite));
        Assert.That(rootRenderer.color, Is.EqualTo(Color.cyan));
        Assert.That(fish.transform.localScale.x, Is.EqualTo(0.5f));
    }

    [Test]
    public void SardineUsesCustomFramesWithoutChangingGameplayRoot()
    {
        fish.Initialize(sardine);
        Assert.That(visual.UsesCustomVisual, Is.True);
        Assert.That(rootRenderer.enabled, Is.False);
        Assert.That(PixelRenderer().sprite, Is.EqualTo(frames[0]));
        Assert.That(PixelRenderer().color, Is.EqualTo(Color.white));
        Assert.That(fish.CurrentResistance, Is.EqualTo(5f));
        Assert.That(fish.GetComponent<CircleCollider2D>().radius, Is.EqualTo(0.5f));
        Assert.That(fish.transform.localScale.x, Is.EqualTo(0.35f));
        Assert.That(PixelRenderer().transform.lossyScale.x, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void PoolReuseSardineToFallbackResetsSpriteFlipScaleTintAndFrame()
    {
        fish.Initialize(sardine);
        visual.Tick(0.125f, Vector2.left);
        Assert.That(PixelRenderer().flipX, Is.True);
        fish.gameObject.SetActive(false);
        fish.Initialize(fallback);
        fish.gameObject.SetActive(true);
        Assert.That(visual.UsesCustomVisual, Is.False);
        Assert.That(rootRenderer.sprite, Is.EqualTo(prototypeSprite));
        Assert.That(rootRenderer.flipX, Is.False);
        Assert.That(rootRenderer.flipY, Is.False);
        Assert.That(rootRenderer.color, Is.EqualTo(Color.cyan));
        Assert.That(rootRenderer.enabled, Is.True);
        Assert.That(PixelRenderer().enabled, Is.False);
        Assert.That(PixelRenderer().sprite, Is.Null);
        Assert.That(fish.transform.localScale.x, Is.EqualTo(0.5f));
        Assert.That(visual.FrameIndex, Is.Zero);
    }

    [Test]
    public void PoolReuseFallbackToSardineRestoresCustomVisual()
    {
        fish.Initialize(fallback);
        fish.gameObject.SetActive(false);
        fish.Initialize(sardine);
        fish.gameObject.SetActive(true);
        Assert.That(visual.UsesCustomVisual, Is.True);
        Assert.That(rootRenderer.enabled, Is.False);
        Assert.That(PixelRenderer().enabled, Is.True);
        Assert.That(PixelRenderer().sprite, Is.EqualTo(frames[0]));
        Assert.That(PixelRenderer().flipX, Is.False);
        Assert.That(PixelRenderer().flipY, Is.False);
    }

    private SpriteRenderer PixelRenderer() => fish.transform.Find("FishPixelVisual").GetComponent<SpriteRenderer>();

    private T Own<T>(T item) where T : Object
    {
        owned.Add(item);
        return item;
    }
}
#endif
