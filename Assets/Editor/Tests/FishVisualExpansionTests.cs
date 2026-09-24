#if UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
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

    [TestCase("Sardine", 32)]
    [TestCase("Mackerel", 48)]
    [TestCase("Tuna", 64)]
    [TestCase("Pufferfish", 48)]
    [TestCase("Squid", 64)]
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
        Assert.That(sheet.height, Is.EqualTo(cell * 4));
        string[] names = { "horizontal", "vertical", "diagonal", "diagonal_nw" };
        for (int row = 0; row < 4; row++)
        {
            Sprite[] frames = profile.GetFrames((FishVisualSet)row);
            Assert.That(frames.Length, Is.EqualTo(4));
            for (int col = 0; col < 4; col++)
            {
                Assert.That(frames[col].name, Is.EqualTo(species.ToLowerInvariant() + "_" + names[row] + "_" + col));
                Assert.That(frames[col].rect, Is.EqualTo(new Rect(col * cell, (3 - row) * cell, cell, cell)));
                Assert.That(frames[col].pixelsPerUnit, Is.EqualTo(83f));
            }
        }
    }

    [Test]
    public void SardineAndPrototypeFallbackLinksRemainCorrect()
    {
        Assert.That(Fish("Sardine").VisualProfile, Is.SameAs(Profile("Sardine")));
        foreach (string name in new[] { "CoastMiniBoss", "CoastBoss" })
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
    public void SpecialFishKeepTheirGameplayAndInkSettings()
    {
        FishData pufferfish = Fish("Pufferfish");
        Assert.That(pufferfish.SpecialType, Is.EqualTo(FishSpecialType.Pufferfish));
        Assert.That(pufferfish.MoveSpeed, Is.EqualTo(1.6f));
        Assert.That(pufferfish.MaxResistance, Is.EqualTo(18f));
        Assert.That(pufferfish.NetDisruptionMultiplier, Is.EqualTo(0.35f));
        Assert.That(pufferfish.CatchValue, Is.EqualTo(3));
        Assert.That(pufferfish.GoldReward, Is.EqualTo(5));

        FishData squid = Fish("Squid");
        Assert.That(squid.SpecialType, Is.EqualTo(FishSpecialType.Squid));
        Assert.That(squid.MoveSpeed, Is.EqualTo(1.8f));
        Assert.That(squid.MaxResistance, Is.EqualTo(24f));
        Assert.That(squid.InkRange, Is.EqualTo(2.5f));
        Assert.That(squid.InkInterval, Is.EqualTo(5f));
        Assert.That(squid.InkDisableDuration, Is.EqualTo(2.5f));
        Assert.That(squid.FirstInkDelay, Is.EqualTo(2f));
        Assert.That(squid.CatchValue, Is.EqualTo(4));
        Assert.That(squid.GoldReward, Is.EqualTo(6));
    }

    [TestCase("Sardine")]
    [TestCase("Mackerel")]
    [TestCase("Tuna")]
    [TestCase("Pufferfish")]
    [TestCase("Squid")]
    public void SpecialFishUseImportedFramesForAllEightHeadings(string species)
    {
        Texture2D texture = Own(new Texture2D(1, 1));
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite prototype = Own(Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f));
        GameObject root = Own(new GameObject(species));
        root.AddComponent<SpriteRenderer>().sprite = prototype;
        FishController controller = root.AddComponent<FishController>();
        typeof(FishController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, null);
        root.AddComponent<FishMovement>();
        FishVisualController visual = root.GetComponent<FishVisualController>();
        FishData data = Fish(species);

        foreach (Vector2 direction in new[]
                 {
                     Vector2.right, Vector2.left, Vector2.up, Vector2.down,
                     new Vector2(1, 1), new Vector2(-1, 1),
                     new Vector2(1, -1), new Vector2(-1, -1)
                 })
        {
            controller.Initialize(data);
            visual.Tick(0.125f, direction);
            FishVisualDirection expected = FishVisualDirectionResolver.Resolve(direction, FishVisualHeading.East);
            SpriteRenderer pixel = root.transform.Find("FishPixelVisual").GetComponent<SpriteRenderer>();
            Assert.That(pixel.sprite, Is.SameAs(data.VisualProfile.GetFrames(expected.Set)[1]));
            Assert.That(pixel.flipX, Is.EqualTo(expected.FlipX));
            Assert.That(pixel.flipY, Is.EqualTo(expected.FlipY));
            Assert.That(controller.CurrentResistance, Is.EqualTo(data.MaxResistance));
        }
    }

    [TestCase("Sardine", 32, 190, 221, 207)]
    [TestCase("Mackerel", 48, 176, 218, 207)]
    [TestCase("Tuna", 64, 181, 218, 208)]
    [TestCase("Pufferfish", 48, 248, 238, 190)]
    [TestCase("Squid", 64, 247, 195, 192)]
    public void SouthTurnsTheNorthBellyToTheLeft(string species, int cell, int red, int green, int blue)
    {
        string path = Path.Combine(Application.dataPath, "Art", "Fish", species,
            species + "_Swim.png");
        Texture2D image = Own(new Texture2D(2, 2, TextureFormat.RGBA32, false));
        Assert.That(image.LoadImage(File.ReadAllBytes(path)), Is.True);
        Color32[] pixels = image.GetPixels32();
        int count = 0;
        float sumX = 0f;
        // Unity 픽셀 좌표에서 북쪽 행은 아래에서 세 번째다.
        for (int y = cell * 2; y < cell * 3; y++)
        for (int x = 0; x < cell; x++)
        {
            Color32 pixel = pixels[y * image.width + x];
            if (pixel.r != red || pixel.g != green || pixel.b != blue || pixel.a != 255) continue;
            sumX += x;
            count++;
        }
        Assert.That(count, Is.GreaterThan(0), species);
        float northBellyX = sumX / count;
        Assert.That(northBellyX, Is.GreaterThan((cell - 1) / 2f), species);
        FishVisualDirection south = new FishVisualDirection(FishVisualHeading.South);
        Assert.That(south.Set, Is.EqualTo(FishVisualSet.Vertical));
        Assert.That(south.FlipX && south.FlipY, Is.True);
        Assert.That(cell - 1 - northBellyX, Is.LessThan((cell - 1) / 2f), species);
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

        foreach (string name in new[] { "Sardine", "Mackerel", "Tuna", "Pufferfish", "Mackerel", "Squid", "CoastMiniBoss", "Pufferfish", "CoastBoss", "Squid", "Sardine" })
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
                Assert.That(pixel.flipX && pixel.flipY, Is.True);
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
