using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GrowthItemSlotRecovery
{
    private const string MenuPath =
        "NETBREAK/UI/Repair G6-C1 Owned Item Slot Hover Components";
    private const string MainScenePath = "Assets/Scenes/Main.unity";
    private const string SlotScriptPath =
        "Assets/Scripts/UI/GrowthItemSlotHover.cs";
    private const string SlotScriptGuid = "6d889fba3f9e4890a193a11d4332ca86";
    private const string OwnedItemsPath =
        "GameCanvas/SkillTreeUI/TreePanel/ItemPage/OwnedItems";

    [MenuItem(MenuPath)]
    private static void Repair()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Stop("Edit Mode에서만 G6-C1 아이템 슬롯을 복구할 수 있습니다.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded || scene.path != MainScenePath)
        {
            Stop($"활성 Scene이 {MainScenePath}가 아니므로 복구하지 않았습니다.");
            return;
        }

        if (!ValidateIntendedScriptAsset(
                out MonoScript expectedScript,
                out long expectedScriptLocalId))
        {
            return;
        }

        if (!TryFindExactPath(scene, OwnedItemsPath, out Transform ownedItems))
        {
            Stop($"예상 Hierarchy를 정확히 찾을 수 없습니다: {OwnedItemsPath}");
            return;
        }

        Transform itemPageTransform = ownedItems.parent;
        GrowthItemPage itemPage = itemPageTransform != null
            ? itemPageTransform.GetComponent<GrowthItemPage>()
            : null;
        if (itemPage == null || ownedItems.childCount != 5 ||
            !HasExactlyOneDirectChild(ownedItems, "Title"))
        {
            Stop("OwnedItems 또는 GrowthItemPage 구조가 생성기의 예상과 달라 복구하지 않았습니다.");
            return;
        }

        var slotsToRepair = new List<GameObject>();
        var loadedHovers = new List<GrowthItemSlotHover>();
        for (int slotIndex = 0; slotIndex < RunItemInventory.Capacity; slotIndex++)
        {
            string slotName = $"OwnedItemSlot_{slotIndex + 1}";
            if (!TryGetUniqueDirectChild(ownedItems, slotName, out Transform slotTransform) ||
                !ValidateSlot(
                    slotTransform.gameObject,
                    itemPage,
                    slotIndex,
                    expectedScript,
                    expectedScriptLocalId,
                    slotsToRepair,
                    loadedHovers))
            {
                return;
            }
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Repair G6-C1 Owned Item Slot Hover Components");
        try
        {
            foreach (GrowthItemSlotHover hover in loadedHovers)
            {
                NormalizeScriptReference(hover, expectedScript, expectedScriptLocalId);
            }

            foreach (GameObject slot in slotsToRepair)
            {
                int slotIndex = ParseSlotIndex(slot.name);
                Undo.RegisterFullObjectHierarchyUndo(
                    slot,
                    "Remove Missing Item Slot Hover");
                int removedCount =
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(slot);
                if (removedCount != 1)
                {
                    throw new InvalidOperationException(
                        $"{slot.name}의 Missing Script 제거 수가 예상과 다릅니다: {removedCount}");
                }

                GrowthItemSlotHover hover =
                    Undo.AddComponent<GrowthItemSlotHover>(slot);
                Undo.RecordObject(hover, "Configure Item Slot Hover");
                hover.Configure(itemPage, slotIndex);
                NormalizeScriptReference(hover, expectedScript, expectedScriptLocalId);
                EditorUtility.SetDirty(hover);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = ownedItems.gameObject;
            Debug.Log(
                "G6-C1 OwnedItemSlot Hover 4개의 스크립트 참조를 " +
                $"GUID {SlotScriptGuid}로 정규화했습니다. " +
                $"Missing Script 복구: {slotsToRepair.Count}개. Scene을 저장하세요.",
                ownedItems);
        }
        catch (Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Stop($"복구 중 오류가 발생해 변경을 되돌렸습니다: {exception.Message}");
        }
    }

    private static bool ValidateIntendedScriptAsset(
        out MonoScript script,
        out long localId)
    {
        script = AssetDatabase.LoadAssetAtPath<MonoScript>(SlotScriptPath);
        localId = 0;
        if (script == null || script.GetClass() != typeof(GrowthItemSlotHover) ||
            typeof(GrowthItemSlotHover).Namespace != null ||
            typeof(GrowthItemSlotHover).Assembly.GetName().Name != "Assembly-CSharp" ||
            AssetDatabase.AssetPathToGUID(SlotScriptPath) != SlotScriptGuid ||
            !AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                script,
                out string guid,
                out localId) ||
            guid != SlotScriptGuid)
        {
            Stop("GrowthItemSlotHover의 타입, 네임스페이스, Assembly 또는 GUID가 예상과 달라 복구하지 않았습니다.");
            return false;
        }

        return true;
    }

    private static bool ValidateSlot(
        GameObject slot,
        GrowthItemPage itemPage,
        int slotIndex,
        MonoScript expectedScript,
        long expectedScriptLocalId,
        ICollection<GameObject> slotsToRepair,
        ICollection<GrowthItemSlotHover> loadedHovers)
    {
        Component[] components = slot.GetComponents<Component>();
        if (components.Length != 4 ||
            components[0] is not RectTransform ||
            components[1] is not CanvasRenderer ||
            components[2] is not Image)
        {
            Stop($"{slot.name}의 컴포넌트 구조가 예상과 달라 복구하지 않았습니다.");
            return false;
        }

        GrowthItemSlotHover hover = components[3] as GrowthItemSlotHover;
        if (hover != null)
        {
            var serializedHover = new SerializedObject(hover);
            SerializedProperty owner = serializedHover.FindProperty("owner");
            SerializedProperty index = serializedHover.FindProperty("slotIndex");
            if (owner == null || index == null ||
                owner.objectReferenceValue != itemPage || index.intValue != slotIndex)
            {
                Stop($"{slot.name}의 정상 Hover 설정이 예상과 달라 수정하지 않았습니다.");
                return false;
            }

            SerializedProperty script = serializedHover.FindProperty("m_Script");
            if (script == null || script.objectReferenceValue is not MonoScript actualScript)
            {
                Stop($"{slot.name}의 직렬화된 스크립트 참조를 확인할 수 없습니다.");
                return false;
            }

            if (!IsExpectedScriptReference(
                    actualScript,
                    expectedScript,
                    expectedScriptLocalId))
            {
                Debug.LogWarning(
                    $"{slot.name}이 Scene 내부 또는 잘못된 GrowthItemSlotHover 스크립트를 " +
                    "참조합니다. 정확한 스크립트 자산으로 다시 저장합니다.",
                    slot);
            }

            loadedHovers.Add(hover);
            return true;
        }

        int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(slot);
        if (components[3] != null || missingCount != 1)
        {
            Stop($"{slot.name}의 Component Index 3이 의도된 단일 Missing Script 상태가 아닙니다.");
            return false;
        }

        slotsToRepair.Add(slot);
        return true;
    }

    private static void NormalizeScriptReference(
        GrowthItemSlotHover hover,
        MonoScript expectedScript,
        long expectedScriptLocalId)
    {
        Undo.RecordObject(hover, "Normalize Item Slot Hover Script Reference");
        var serializedHover = new SerializedObject(hover);
        serializedHover.Update();
        SerializedProperty script = serializedHover.FindProperty("m_Script");
        if (script == null)
        {
            throw new InvalidOperationException(
                $"{hover.gameObject.name}의 m_Script를 찾을 수 없습니다.");
        }

        script.objectReferenceValue = expectedScript;
        serializedHover.ApplyModifiedProperties();
        EditorUtility.SetDirty(hover);

        serializedHover.Update();
        if (script.objectReferenceValue is not MonoScript actualScript ||
            !IsExpectedScriptReference(
                actualScript,
                expectedScript,
                expectedScriptLocalId))
        {
            throw new InvalidOperationException(
                $"{hover.gameObject.name}의 스크립트 자산 참조를 정규화하지 못했습니다.");
        }
    }

    private static bool IsExpectedScriptReference(
        MonoScript actualScript,
        MonoScript expectedScript,
        long expectedScriptLocalId) =>
        actualScript == expectedScript &&
        AssetDatabase.GetAssetPath(actualScript) == SlotScriptPath &&
        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
            actualScript,
            out string guid,
            out long localId) &&
        guid == SlotScriptGuid && localId == expectedScriptLocalId;

    private static bool TryFindExactPath(
        Scene scene,
        string hierarchyPath,
        out Transform result)
    {
        result = null;
        string[] names = hierarchyPath.Split('/');
        GameObject root = null;
        foreach (GameObject candidate in scene.GetRootGameObjects())
        {
            if (candidate.name != names[0])
            {
                continue;
            }

            if (root != null)
            {
                return false;
            }

            root = candidate;
        }

        if (root == null)
        {
            return false;
        }

        Transform current = root.transform;
        for (int i = 1; i < names.Length; i++)
        {
            if (!TryGetUniqueDirectChild(current, names[i], out current))
            {
                return false;
            }
        }

        result = current;
        return true;
    }

    private static bool HasExactlyOneDirectChild(Transform parent, string name) =>
        TryGetUniqueDirectChild(parent, name, out _);

    private static bool TryGetUniqueDirectChild(
        Transform parent,
        string name,
        out Transform result)
    {
        result = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name != name)
            {
                continue;
            }

            if (result != null)
            {
                result = null;
                return false;
            }

            result = child;
        }

        return result != null;
    }

    private static int ParseSlotIndex(string slotName) =>
        slotName[slotName.Length - 1] - '1';

    private static void Stop(string message) =>
        Debug.LogError($"G6-C1 OwnedItemSlot 복구 중단: {message}");
}
