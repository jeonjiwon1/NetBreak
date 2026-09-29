using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class Area1TutorialTests
{
    private static readonly string[] Keys =
    {
        "Goal", "CoreTool", "PartnerTool", "TacticalSkill", "BossRule"
    };
    private readonly int[] previous = new int[Keys.Length];
    private readonly bool[] existed = new bool[Keys.Length];
    private GameObject owner;
    private Area1TutorialController tutorial;

    [SetUp]
    public void SetUp()
    {
        for (int i = 0; i < Keys.Length; i++)
        {
            string key = Key(i);
            existed[i] = PlayerPrefs.HasKey(key);
            previous[i] = PlayerPrefs.GetInt(key);
            PlayerPrefs.DeleteKey(key);
        }
        owner = new GameObject("TutorialTest");
        tutorial = owner.AddComponent<Area1TutorialController>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(owner);
        for (int i = 0; i < Keys.Length; i++)
        {
            if (existed[i]) PlayerPrefs.SetInt(Key(i), previous[i]);
            else PlayerPrefs.DeleteKey(Key(i));
        }
        PlayerPrefs.Save();
    }

    [Test]
    public void DuplicateStepIsQueuedOnceAndOtherStepStillQueues()
    {
        tutorial.NotifyFishingStarted();
        tutorial.NotifyFishingStarted();
        tutorial.NotifyToolAcquired(GrowthToolRole.Core, ToolId.Net);
        Assert.That(PendingCount(), Is.EqualTo(2));
    }

    [Test]
    public void SeenStepIsSkippedAndResetAllowsItAgain()
    {
        PlayerPrefs.SetInt(Key(0), 1);
        tutorial.NotifyFishingStarted();
        Assert.That(PendingCount(), Is.Zero);
        Area1TutorialController.ResetProgress();
        tutorial.NotifyFishingStarted();
        Assert.That(PendingCount(), Is.EqualTo(1));
    }

    [Test]
    public void EndCleanupDropsPendingAndMissingUiDoesNotThrow()
    {
        tutorial.NotifyFishingStarted();
        Assert.DoesNotThrow(() => tutorial.ClearPending());
        Assert.That(PendingCount(), Is.Zero);
    }

    private int PendingCount()
    {
        FieldInfo field = typeof(Area1TutorialController).GetField(
            "pending", BindingFlags.Instance | BindingFlags.NonPublic);
        object queue = field.GetValue(tutorial);
        return (int)queue.GetType().GetProperty("Count").GetValue(queue);
    }

    private static string Key(int index) => "NetBreak.Area1Tutorial.v1." + Keys[index];
}
