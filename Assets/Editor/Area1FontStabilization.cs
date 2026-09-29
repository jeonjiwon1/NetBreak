using TMPro;
using UnityEditor;
using UnityEngine;

internal static class Area1FontStabilization
{
    private static readonly string[] FontPaths =
    {
        "Assets/Resources/UI/Fonts/Galmuri11 SDF.asset",
        "Assets/Resources/UI/Fonts/Galmuri11 Bold SDF.asset",
        "Assets/UI/Fonts/NanumGothic-Regular SDF.asset",
        "Assets/UI/Fonts/NanumGothic-Bold SDF.asset"
    };

    [MenuItem("Tools/NETBREAK/Stabilize TMP Font Assets")]
    private static void Stabilize()
    {
        int changed = 0;
        foreach (string path in FontPaths)
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font == null)
            {
                Debug.LogError("Missing TMP font asset: " + path);
                continue;
            }

            SerializedObject serializedFont = new SerializedObject(font);
            SerializedProperty clearOnBuild = serializedFont.FindProperty("m_ClearDynamicDataOnBuild");
            if (clearOnBuild == null)
            {
                Debug.LogError("TMP clear setting unavailable: " + path);
                continue;
            }

            if (!clearOnBuild.boolValue) continue;
            clearOnBuild.boolValue = false;
            serializedFont.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(font);
            changed++;
        }

        if (changed > 0) AssetDatabase.SaveAssets();
        Debug.Log($"TMP font stabilization: {changed} asset(s) updated.");
    }
}
