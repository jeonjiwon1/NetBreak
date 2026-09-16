using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TacticalSkillSetup
{
    [MenuItem("NETBREAK/Growth/Setup Tactical Skill Manager")]
    public static void Setup()
    {
        RunManager[] runManagers = Object.FindObjectsByType<RunManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (runManagers.Length != 1)
        {
            Debug.LogError(
                $"Tactical Skill setup requires exactly one RunManager in the active Scene. Found: {runManagers.Length}");
            return;
        }

        RunManager runManager = runManagers[0];
        TacticalSkillManager[] managers = Object.FindObjectsByType<TacticalSkillManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (managers.Length > 1 ||
            (managers.Length == 1 && managers[0].gameObject != runManager.gameObject))
        {
            Debug.LogError(
                "A TacticalSkillManager already exists outside the RunManager object. No objects were changed.");
            return;
        }

        SignatureSkillManager[] signatureManagers =
            Object.FindObjectsByType<SignatureSkillManager>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        if (signatureManagers.Length > 1 ||
            (signatureManagers.Length == 1 &&
             signatureManagers[0].gameObject != runManager.gameObject))
        {
            Debug.LogError(
                "A SignatureSkillManager already exists outside the RunManager object. No objects were changed.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Setup Tactical Skill Manager");

        TacticalSkillManager manager = runManager.GetComponent<TacticalSkillManager>();
        if (manager == null)
        {
            manager = Undo.AddComponent<TacticalSkillManager>(runManager.gameObject);
        }

        SerializedObject serialized = new SerializedObject(manager);
        AssignIfEmpty(
            serialized,
            "fishingRodPlacement",
            Object.FindFirstObjectByType<FishingRodPlacementController>(FindObjectsInactive.Include));
        AssignIfEmpty(
            serialized,
            "netPlacement",
            Object.FindFirstObjectByType<NetPlacementController>(FindObjectsInactive.Include));
        AssignIfEmpty(
            serialized,
            "castNet",
            Object.FindFirstObjectByType<CastNetController>(FindObjectsInactive.Include));
        AssignIfEmpty(
            serialized,
            "bait",
            Object.FindFirstObjectByType<BaitController>(FindObjectsInactive.Include));
        serialized.ApplyModifiedProperties();

        SignatureSkillManager signature = runManager.GetComponent<SignatureSkillManager>();
        if (signature == null)
        {
            signature = Undo.AddComponent<SignatureSkillManager>(runManager.gameObject);
        }

        SerializedObject signatureSerialized = new SerializedObject(signature);
        AssignIfEmpty(
            signatureSerialized,
            "castNet",
            Object.FindFirstObjectByType<CastNetController>(FindObjectsInactive.Include));
        signatureSerialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(signature);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeObject = manager;

        Debug.Log(
            "TacticalSkillManager and SignatureSkillManager are ready on RunManager. Existing UI and user-created objects were preserved.");
    }

    private static void AssignIfEmpty(
        SerializedObject serialized,
        string propertyName,
        Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null && property.objectReferenceValue == null && value != null)
        {
            property.objectReferenceValue = value;
        }
    }
}
