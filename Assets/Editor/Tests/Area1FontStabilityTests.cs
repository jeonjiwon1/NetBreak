#if UNITY_INCLUDE_TESTS
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

public sealed class Area1FontStabilityTests
{
    [TestCase("Assets/Resources/UI/Fonts/Galmuri11 SDF.asset")]
    [TestCase("Assets/Resources/UI/Fonts/Galmuri11 Bold SDF.asset")]
    [TestCase("Assets/UI/Fonts/NanumGothic-Regular SDF.asset")]
    [TestCase("Assets/UI/Fonts/NanumGothic-Bold SDF.asset")]
    public void ConsecutiveImportsPreserveSerializedFont(string path)
    {
        byte[] before = File.ReadAllBytes(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        byte[] afterFirst = File.ReadAllBytes(path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        byte[] afterSecond = File.ReadAllBytes(path);

        Assert.That(afterFirst, Is.EqualTo(before));
        Assert.That(afterSecond, Is.EqualTo(afterFirst));
    }

    [TestCase("Galmuri11 SDF")]
    [TestCase("Galmuri11 Bold SDF")]
    public void RuntimeGlyphsDoNotModifyPersistentFont(string name)
    {
        string path = "Assets/Resources/UI/Fonts/" + name + ".asset";
        TMP_FontAsset source = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        Assert.That(source, Is.Not.Null);
        byte[] before = File.ReadAllBytes(path);
        int sourceGlyphCount = source.glyphTable.Count;

        System.Type typography = typeof(PrototypeHUDCanvas).Assembly.GetType("Area1Typography");
        Assert.That(typography, Is.Not.Null);
        MethodInfo create = typography.GetMethod("CreateRuntimeFont",
            BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo reset = typography.GetMethod("ResetRuntimeFonts",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(create, Is.Not.Null);
        Assert.That(reset, Is.Not.Null);

        TMP_FontAsset runtime = (TMP_FontAsset)create.Invoke(null, new object[] { source });
        Assert.That(runtime, Is.Not.Null);
        try
        {
            Assert.That(EditorUtility.IsPersistent(runtime), Is.False);
            Assert.That(EditorUtility.IsPersistent(runtime.material), Is.False);
            Assert.That(EditorUtility.IsPersistent(runtime.atlasTexture), Is.False);
            Assert.That(runtime.sourceFontFile, Is.EqualTo(source.sourceFontFile));
            Assert.That(runtime.TryAddCharacters("한글 QWER LMB TAB"), Is.True);
            Assert.That(runtime.glyphTable.Count, Is.GreaterThan(0));
            Assert.That(source.glyphTable.Count, Is.EqualTo(sourceGlyphCount));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(before));
        }
        finally
        {
            reset.Invoke(null, null);
            foreach (TMP_FontAsset fallback in runtime.fallbackFontAssetTable)
                DestroyRuntimeFont(fallback);
            DestroyRuntimeFont(runtime);
        }
    }

    private static void DestroyRuntimeFont(TMP_FontAsset font)
    {
        if (font == null) return;
        foreach (Texture2D texture in font.atlasTextures)
            if (texture != null) Object.DestroyImmediate(texture);
        if (font.material != null) Object.DestroyImmediate(font.material);
        Object.DestroyImmediate(font);
    }
}
#endif
