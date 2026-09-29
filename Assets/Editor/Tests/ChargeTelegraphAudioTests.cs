#if UNITY_INCLUDE_TESTS
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class ChargeTelegraphAudioTests
{
    private GameObject fishObject;
    private GameObject audioObject;

    [TearDown]
    public void TearDown()
    {
        if (fishObject != null) Object.DestroyImmediate(fishObject);
        if (audioObject != null) Object.DestroyImmediate(audioObject);
    }

    [Test]
    public void GiantTunaWarningThenChargeStartOncePerDash()
    {
        FishVisualProfile profile = LoadProfile("CoastMiniBoss");
        Assert.That(profile.ChargeWarningClip, Is.Not.Null);
        Assert.That(profile.ChargeStartClip, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(profile.ChargeWarningClip),
            Is.EqualTo("Assets/Audio/SFX/Boss/GiantTuna_ChargeWarning.wav"));
        Assert.That(AssetDatabase.GetAssetPath(profile.ChargeStartClip),
            Is.EqualTo("Assets/Audio/SFX/Boss/GiantTuna_ChargeTelegraph.wav"));
        Assert.That(profile.ChargeWarningClip.length, Is.LessThan(0.65f));
        ItemEffectManager audio = CreateAudio();
        List<AudioClip> played = new List<AudioClip>();
        audio.ChargeSoundPlayed += played.Add;

        FishController fish = CreateFish("CoastMiniBoss");
        MiniBossController miniBoss = fish.GetComponent<MiniBossController>();
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            IEnumerator dash = InvokeCoroutine(miniBoss, "TelegraphAndDash");
            Assert.That(dash.MoveNext(), Is.True);
            Assert.That(miniBoss.IsTelegraphing, Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2 - 1));
            Assert.That(played[played.Count - 1], Is.SameAs(profile.ChargeWarningClip));
            Assert.That(miniBoss.IsDashing, Is.False);
            for (int i = 0; i < 20 && miniBoss.IsTelegraphing; i++)
            {
                Assert.That(dash.MoveNext(), Is.True);
                if (miniBoss.IsTelegraphing)
                    Assert.That(played.Count, Is.EqualTo(attempt * 2 - 1));
            }
            Assert.That(miniBoss.IsDashing, Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2));
            Assert.That(played[played.Count - 1], Is.SameAs(profile.ChargeStartClip));
            Assert.That(dash.MoveNext(), Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2));
        }
    }

    [Test]
    public void SharkWarningThenChargeStartInBothPhasesAndAfterReuse()
    {
        FishVisualProfile profile = LoadProfile("CoastBoss");
        Assert.That(profile.ChargeWarningClip, Is.Not.Null);
        Assert.That(profile.ChargeStartClip, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(profile.ChargeWarningClip),
            Is.EqualTo("Assets/Audio/SFX/Boss/SharkBoss_ChargeWarning.wav"));
        Assert.That(AssetDatabase.GetAssetPath(profile.ChargeStartClip),
            Is.EqualTo("Assets/Audio/SFX/Boss/SharkBoss_ChargeTelegraph.wav"));
        Assert.That(profile.ChargeWarningClip.length, Is.LessThan(0.45f));
        ItemEffectManager audio = CreateAudio();
        List<AudioClip> played = new List<AudioClip>();
        audio.ChargeSoundPlayed += played.Add;

        FishController fish = CreateFish("CoastBoss");
        BossBehaviorController boss = fish.GetComponent<BossBehaviorController>();
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            if (attempt == 3)
            {
                fishObject.SetActive(false);
                fish.Initialize(LoadFish("CoastBoss"));
                fishObject.SetActive(true);
            }
            boss.SetPhase(attempt == 1 ? 2 : 3);
            IEnumerator rush = InvokeCoroutine(boss, "ExecuteRush");
            Assert.That(rush.MoveNext(), Is.True);
            Assert.That(boss.IsTelegraphing, Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2 - 1));
            Assert.That(played[played.Count - 1], Is.SameAs(profile.ChargeWarningClip));
            Assert.That(boss.IsRushing, Is.False);
            for (int i = 0; i < 20 && boss.IsTelegraphing; i++)
            {
                Assert.That(rush.MoveNext(), Is.True);
                if (boss.IsTelegraphing)
                    Assert.That(played.Count, Is.EqualTo(attempt * 2 - 1));
            }
            Assert.That(boss.IsRushing, Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2));
            Assert.That(played[played.Count - 1], Is.SameAs(profile.ChargeStartClip));
            Assert.That(rush.MoveNext(), Is.True);
            Assert.That(played.Count, Is.EqualTo(attempt * 2));
        }
        fishObject.SetActive(false);
        Assert.That(played.Count, Is.EqualTo(6));
    }

    private ItemEffectManager CreateAudio()
    {
        audioObject = new GameObject("Charge audio test");
        return audioObject.AddComponent<ItemEffectManager>();
    }

    private FishController CreateFish(string name)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Fish/Fish.prefab");
        fishObject = Object.Instantiate(prefab);
        fishObject.SetActive(false);
        FishController fish = fishObject.GetComponent<FishController>();
        fish.Initialize(LoadFish(name));
        fishObject.SetActive(true);
        return fish;
    }

    private static IEnumerator InvokeCoroutine(MonoBehaviour behavior, string method) =>
        (IEnumerator)behavior.GetType().GetMethod(method,
            BindingFlags.Instance | BindingFlags.NonPublic).Invoke(behavior, null);

    private static FishVisualProfile LoadProfile(string name) =>
        AssetDatabase.LoadAssetAtPath<FishVisualProfile>(
            "Assets/Art/Fish/" + name + "/" + name + "_VisualProfile.asset");

    private static FishData LoadFish(string name) =>
        AssetDatabase.LoadAssetAtPath<FishData>(
            "Assets/Data/Fish/FishData_" + name + ".asset");
}
#endif
