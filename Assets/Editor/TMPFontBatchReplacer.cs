using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TMPFontBatchReplacer
{
    private const string TargetFontName =
        "NanumGothic-Bold SDF";

    [MenuItem(
        "Tools/NETBREAK/Apply NanumGothic Bold To Scene TMP"
    )]
    private static void ApplyFontToScene()
    {
        TMP_FontAsset targetFont =
            FindTargetFont();

        if (targetFont == null)
        {
            Debug.LogError(
                $"TMPFontBatchReplacer: " +
                $"'{TargetFontName}' Font Asset을 찾을 수 없습니다."
            );

            return;
        }

        TMP_Text[] texts =
            Object.FindObjectsByType<TMP_Text>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        int changedCount = 0;

        foreach (TMP_Text text in texts)
        {
            if (text == null)
            {
                continue;
            }

            // Project Asset / Prefab Asset 등은 제외하고
            // 현재 열린 Scene의 오브젝트만 처리한다.
            if (!text.gameObject.scene.IsValid() ||
                !text.gameObject.scene.isLoaded)
            {
                continue;
            }

            if (text.font == targetFont)
            {
                continue;
            }

            Undo.RecordObject(
                text,
                "Apply NETBREAK TMP Font"
            );

            text.font =
                targetFont;

            EditorUtility.SetDirty(
                text
            );

            changedCount++;
        }

        if (changedCount > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        Debug.Log(
            $"TMPFontBatchReplacer: " +
            $"{changedCount}개의 TMP Text를 " +
            $"'{TargetFontName}'으로 변경했습니다."
        );
    }

    private static TMP_FontAsset FindTargetFont()
    {
        string[] guids =
            AssetDatabase.FindAssets(
                $"{TargetFontName} t:TMP_FontAsset"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            TMP_FontAsset font =
                AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    path
                );

            if (font != null &&
                font.name == TargetFontName)
            {
                return font;
            }
        }

        return null;
    }
}