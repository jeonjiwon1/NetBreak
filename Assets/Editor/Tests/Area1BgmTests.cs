#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class Area1BgmTests
{
    [Test]
    public void ProfileReferencesThreeOriginalLoopClips()
    {
        Area1BgmProfile profile = Resources.Load<Area1BgmProfile>("Area1BgmProfile");
        Assert.That(profile, Is.Not.Null);
        AssertClip(profile.NormalClip, "Area1_Normal.wav", 153.6f);
        AssertClip(profile.MiniBossClip, "Area1_MiniBoss.wav", 24f);
        AssertClip(profile.BossClip, "Area1_Boss.wav", 82.2857f);
        Assert.That(profile.Volume, Is.EqualTo(0.22f).Within(0.001f));
        Assert.That(profile.FadeDuration, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void DedicatedSourceIsLoopingTwoDimensionalAndUnscaledPitch()
    {
        GameObject host = new GameObject("BgmTestHost");
        try
        {
            Area1BgmController controller = host.AddComponent<Area1BgmController>();
            AudioSource source = controller.Source;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.gameObject, Is.Not.SameAs(host));
            Assert.That(source.loop, Is.True);
            Assert.That(source.playOnAwake, Is.False);
            Assert.That(source.spatialBlend, Is.EqualTo(0f));
            Assert.That(source.pitch, Is.EqualTo(1f));
            Assert.That(host.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }

    private static void AssertClip(AudioClip clip, string file, float seconds)
    {
        Assert.That(clip, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(clip),
            Is.EqualTo("Assets/Audio/BGM/Area1/" + file));
        Assert.That(clip.length, Is.EqualTo(seconds).Within(0.02f));
        Assert.That(clip.channels, Is.EqualTo(1));
        Assert.That(clip.frequency, Is.EqualTo(22050));
        AudioImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip)) as AudioImporter;
        Assert.That(importer, Is.Not.Null);
        Assert.That(importer.defaultSampleSettings.loadType, Is.EqualTo(AudioClipLoadType.Streaming));
    }
}
#endif
