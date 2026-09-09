using UnityEngine;

public class PrototypeHUD : MonoBehaviour
{
    private GUIStyle style;

    private void Awake()
    {
        style = new GUIStyle();

        style.fontSize = 24;
        style.normal.textColor = Color.white;
    }

    private void OnGUI()
    {
        if (RunManager.Instance == null)
        {
            return;
        }

        RunManager run = RunManager.Instance;

        GUI.Label(
            new Rect(20, 20, 400, 40),
            $"Gold: {run.CurrentGold}",
            style
        );

        GUI.Label(
            new Rect(20, 55, 400, 40),
            $"Caught: {run.CapturedFishCount}",
            style
        );

        GUI.Label(
            new Rect(20, 90, 400, 40),
            $"Catch Rate: {run.CatchRate * 100f:F1}%",
            style
        );

        GUI.Label(
             new Rect(20, 125, 400, 40),
             $"Level: {run.CurrentLevel}",
             style
);

        GUI.Label(
            new Rect(20, 160, 400, 40),
            $"EXP: {run.CurrentExp} / {run.ExpToNextLevel}",
            style
        );
    }
}