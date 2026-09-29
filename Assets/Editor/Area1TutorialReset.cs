using UnityEditor;

internal static class Area1TutorialReset
{
    [MenuItem("Tools/NETBREAK/Reset Area 1 Tutorial Progress")]
    private static void Reset()
    {
        Area1TutorialController.ResetProgress();
    }
}
