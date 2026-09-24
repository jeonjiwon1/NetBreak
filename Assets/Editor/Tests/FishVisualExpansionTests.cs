#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class FishVisualExpansionTests
{
    private const string Art = "Assets/Art/Fish/";
    private const string Data = "Assets/Data/Fish/FishData_";
    private readonly List<Object> owned = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = owned.Count - 1; i >= 0; i--)
            if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        owned.Clear();
    }

    [TestCase("Mackerel", 48)]
    [TestCase("Tuna", 64)]
    public void ExpandedProfileHasExactImportedFramesAndFishDataLink(string species, int cell)
    {
        FishVisualProfile profile = Profile(species);
        FishData data = Fish(species);
        Assert.That(profile, Is.Not.Null);
        Assert.That(data.VisualProfile, Is.SameAs(profile));
        Assert.That(profile.IsValid, Is.True);
        Assert.That(profile.FramesPerSecond, Is.EqualTo(8f));
        Assert.That(profile.VisualScale, Is.EqualTo(Vector2.one));
        Assert.That(profile.Tint, Is.EqualTo(Color.white));

        Texture2D sheet = AssetDatabase.LoadAssetAtPath<Texture2D>(Art + species + "/" + species + "_Swim.png");
        Assert.That(sheet.width, Is.EqualTo(cell * 4));
        Assert.That(sheet.height, Is.EqualTo(cell * 3));
        string[] names = { "horizontal", "vertical", "diagonal" };
        for (int row = 0; row < 3; row++)
        {
            Sprite[] frames = profile.GetFrames((FishVisualSet)row);
            Assert.That(frames.Length, Is.EqualTo(4));
            for (int col = 0; col < 4; col++)
            {
                Assert.That(frames[col].name, Is.EqualTo(species.ToLowerInvariant() + "_" + names[row] + "_" + col));
                Assert.That(frames[col].rect, Is.EqualTo(new Rect(col * cell, (2 - row) * cell, cell, cell)));
                Assert.That(frames[col].pixelsPerUnit, Is.EqualTo(83f));
            }
        }
    }

    [Test]
    public void SardineAndPrototypeFallbackLinksRemainCorrect()
    {
        Assert.That(Fish("Sardine").VisualProfile, Is.SameAs(Profile("Sardine")));
        foreach (string name in new[] { "Pufferfish", "Squid", "CoastMiniBoss", "CoastBoss" })
            Assert.That(Fish(name).VisualProfile, Is.Null, name);
    }

    [Test]
    public void GameplayValuesOfExpandedSpeciesRemainUnchanged()
    {
        FishData mackerel = Fish("Mackerel");
        Assert.That(mackerel.MoveSpeed, Is.EqualTo(2f));
        Assert.That(mackerel.MaxResistance, Is.EqualTo(10f));
        Assert.That(mackerel.CatchValue, Is.EqualTo(2));
        Assert.That(mackerel.GoldReward, Is.EqualTo(3));
        Assert.That(mackerel.ExpReward, Is.EqualTo(1));
        Assert.That(mackerel.BaitAttraction, Is.EqualTo(0.8f));
        Assert.That(mackerel.SchoolStrength, Is.EqualTo(0.7f));
        Assert.That(mackerel.MinSchoolSize, Is.EqualTo(12));
        Assert.That(mackerel.MaxSchoolSize, Is.EqualTo(20));

        FishData tuna = Fish("Tuna");
        Assert.That(tuna.MoveSpeed, Is.EqualTo(2.8f));
        Assert.That(tuna.MaxResistance, Is.EqualTo(25f));
        Assert.That(tuna.CatchValue, Is.EqualTo(6));
        Assert.That(tuna.GoldReward, Is.EqualTo(10));
        Assert.That(tuna.ExpReward, Is.EqualTo(1));
        Assert.That(tuna.BaitAttraction, Is.EqualTo(0.4f));
        Assert.That(tuna.SchoolStrength, Is.EqualTo(0.3f));
        Assert.That(tuna.MinSchoolSize, Is.EqualTo(4));
        Assert.That(tuna.MaxSchoolSize, Is.EqualTo(8));
    }

    [Test]
    public void ReusedPoolFishClearsAllSpeciesVisualState()
    {
        Texture2D texture = Own(new Texture2D(1, 1));
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite prototype = Own(Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f));
        GameObject root = Own(new GameObject("Pooled fish"));
        SpriteRenderer original = root.AddComponent<SpriteRenderer>();
        original.sprite = prototype;
        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        FishController controller = root.AddComponent<FishController>();
        typeof(FishController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null);
        root.AddComponent<FishMovement>();
        FishVisualController visual = root.GetComponent<FishVisualController>();

        foreach (string name in new[] { "Sardine", "Mackerel", "Tuna", "Pufferfish", "Mackerel", "Tuna", "Squid", "Tuna", "Sardine" })
        {
            FishData data = Fish(name);
            controller.Initialize(data);
            SpriteRenderer pixel = root.transform.Find("FishPixelVisual")?.GetComponent<SpriteRenderer>();
            Assert.That(controller.CurrentResistance, Is.EqualTo(data.MaxResistance), name);
            Assert.That(collider.radius, Is.EqualTo(0.5f), name);
            Assert.That(root.transform.localScale.x, Is.EqualTo(data.VisualScale.x), name);
            Assert.That(visual.FrameIndex, Is.Zero, name);
            Assert.That(original.flipX || original.flipY, Is.False, name);
            Assert.That(original.color, Is.EqualTo(data.VisualColor), name);
            if (data.VisualProfile == null)
            {
                Assert.That(visual.UsesCustomVisual, Is.False);
                Assert.That(original.enabled, Is.True);
                Assert.That(original.sprite, Is.SameAs(prototype));
                Assert.That(pixel.enabled, Is.False);
                Assert.That(pixel.sprite, Is.Null);
                Assert.That(pixel.color, Is.EqualTo(Color.white));
                Assert.That(pixel.flipX || pixel.flipY, Is.False);
                Assert.That(pixel.transform.localScale, Is.EqualTo(Vector3.one));
            }
            else
            {
                Assert.That(visual.UsesCustomVisual, Is.True);
                Assert.That(original.enabled, Is.False);
                Assert.That(pixel.enabled, Is.True);
                Assert.That(pixel.sprite, Is.SameAs(data.VisualProfile.HorizontalFrames[0]));
                Assert.That(pixel.color, Is.EqualTo(Color.white));
                Assert.That(pixel.flipX || pixel.flipY, Is.False);
                Assert.That(pixel.transform.lossyScale.x, Is.EqualTo(1f).Within(0.001f));
                visual.Tick(0.125f, Vector2.left);
                Assert.That(pixel.flipX, Is.True);
                Assert.That(visual.FrameIndex, Is.EqualTo(1));
            }
            original.flipX = true;
            original.flipY = true;
            original.color = Color.red;
            pixel.color = Color.magenta;
            pixel.transform.localScale = new Vector3(2f, 3f, 1f);
        }
    }

    private static FishVisualProfile Profile(string species) =>
        AssetDatabase.LoadAssetAtPath<FishVisualProfile>(Art + species + "/" + species + "_VisualProfile.asset");

    private static FishData Fish(string species) =>
        AssetDatabase.LoadAssetAtPath<FishData>(Data + species + ".asset");

    private T Own<T>(T item) where T : Object
    {
        owned.Add(item);
        return item;
    }
}
#endif
