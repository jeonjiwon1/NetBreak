#if UNITY_INCLUDE_TESTS
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class Area1BackgroundTests
{
    [Test]
    public void CoastSpriteUsesPointFilteringAndOpaquePixels()
    {
        const string path = "Assets/Resources/Area1/CoastBackground.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
        Assert.That(importer.mipmapEnabled, Is.False);
        Assert.That(importer.textureCompression,
            Is.EqualTo(TextureImporterCompression.Uncompressed));
        Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(32f));
        Assert.That(sprite, Is.Not.Null);
        Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(1672f, 941f)));
        Assert.That(Resources.Load<Sprite>(Area1BackgroundController.ResourcePath),
            Is.SameAs(sprite));
    }

    [Test]
    public void BackgroundIsOpaqueAndBottomIsMostlyWater()
    {
        const string path = "Assets/Resources/Area1/CoastBackground.png";
        Texture2D pixels = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            Assert.That(ImageConversion.LoadImage(pixels, File.ReadAllBytes(path)),
                Is.True);
            Assert.That(pixels.width, Is.EqualTo(1672));
            Assert.That(pixels.height, Is.EqualTo(941));
            Assert.That((float)pixels.width / pixels.height,
                Is.EqualTo(16f / 9f).Within(0.003f));
            foreach (Color32 pixel in pixels.GetPixels32())
                if (pixel.a != 255)
                    Assert.Fail("The flattened underwater background must be opaque.");

            int waterSamples = 0;
            int totalSamples = 0;
            for (int x = 0; x < pixels.width; x += 16)
            {
                Color32 bottom = pixels.GetPixel(x, 0);
                Assert.That(bottom.a, Is.EqualTo(255));
                if (bottom.b - bottom.r > 20)
                    waterSamples++;
                totalSamples++;
            }
            Assert.That(waterSamples, Is.GreaterThan(totalSamples * 3 / 4),
                "The lower edge should remain mostly water, with only small underwater sand pockets.");
        }
        finally
        {
            Object.DestroyImmediate(pixels);
        }
    }

    [Test]
    public void BackgroundStaysBehindGameplayAndFitsCameraChanges()
    {
        GameObject cameraObject = new GameObject("Area 1 background test camera");
        try
        {
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            camera.aspect = 16f / 9f;
            Sprite sprite = Resources.Load<Sprite>(Area1BackgroundController.ResourcePath);
            Assert.That(sprite, Is.Not.Null);

            Area1BackgroundController controller =
                cameraObject.AddComponent<Area1BackgroundController>();
            controller.Initialize(camera, sprite);
            controller.Initialize(camera, sprite);

            Transform visual = cameraObject.transform.Find("Area1CoastBackground");
            Assert.That(visual, Is.Not.Null);
            Assert.That(cameraObject.transform.Find("Area1SurfaceFlow"), Is.Null);
            Assert.That(cameraObject.transform.childCount, Is.EqualTo(1));
            SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
            Assert.That(renderer.sortingOrder,
                Is.EqualTo(Area1BackgroundController.BackgroundSortingOrder));
            Assert.That(renderer.sortingOrder, Is.LessThan(-1));
            Assert.That(visual.GetComponent<Collider2D>(), Is.Null);
            Assert.That(visual.GetComponent<Collider>(), Is.Null);
            Assert.That(visual.GetComponent<Canvas>(), Is.Null);
            Assert.That(visual.localScale.y * sprite.bounds.size.y,
                Is.EqualTo(13f).Within(0.001f));
            Assert.That(visual.localScale.x * sprite.bounds.size.x,
                Is.EqualTo(13f * 16f / 9f).Within(0.001f));
            controller.Initialize(camera, sprite);
            Assert.That(cameraObject.transform.childCount, Is.EqualTo(1));

            camera.orthographicSize = 5f;
            controller.FitToCamera();
            Assert.That(visual.localScale.y * sprite.bounds.size.y,
                Is.EqualTo(10f).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(cameraObject);
        }
    }
}
#endif
