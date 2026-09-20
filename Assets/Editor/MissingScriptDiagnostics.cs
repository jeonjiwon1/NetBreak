using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptDiagnostics
{
    private const string MenuPath = "NETBREAK/Diagnostics/Scan Loaded Scenes for Missing Scripts";

    [MenuItem(MenuPath)]
    private static void ScanLoadedScenes()
    {
        int inspectedSceneCount = 0;
        int inspectedObjectCount = 0;
        int missingScriptCount = 0;

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            inspectedSceneCount++;

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                ScanGameObjectHierarchy(
                    scene,
                    rootObject.transform,
                    ref inspectedObjectCount,
                    ref missingScriptCount);
            }
        }

        Debug.Log(
            $"[Missing Script 검사 완료] Scene {inspectedSceneCount}개, " +
            $"GameObject {inspectedObjectCount}개, Missing Script {missingScriptCount}개");
    }

    private static void ScanGameObjectHierarchy(
        Scene scene,
        Transform current,
        ref int inspectedObjectCount,
        ref int missingScriptCount)
    {
        inspectedObjectCount++;

        Component[] components = current.gameObject.GetComponents<Component>();
        for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
        {
            if (components[componentIndex] != null)
            {
                continue;
            }

            missingScriptCount++;
            Debug.LogWarning(
                $"[Missing Script] Scene: {scene.name} | " +
                $"GameObject: {GetHierarchyPath(current)} | " +
                $"Component Index: {componentIndex} (0-based)",
                current.gameObject);
        }

        for (int childIndex = 0; childIndex < current.childCount; childIndex++)
        {
            ScanGameObjectHierarchy(
                scene,
                current.GetChild(childIndex),
                ref inspectedObjectCount,
                ref missingScriptCount);
        }
    }

    private static string GetHierarchyPath(Transform target)
    {
        var names = new Stack<string>();

        for (Transform current = target; current != null; current = current.parent)
        {
            names.Push(current.name);
        }

        return string.Join("/", names);
    }
}
