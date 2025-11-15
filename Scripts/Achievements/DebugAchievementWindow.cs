using UnityEngine;

public class DebugAchievementWindow : MonoBehaviour
{
    public bool showWindow = false;
    private Vector2 scrollPos;

    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;

    public void ToggleShowWindow()
    {
        showWindow = !showWindow;
    }

    private void OnGUI()
    {
        if (!showWindow) return;

        // 라벨 스타일 초기화
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 40; // 라벨 글자 크기
            labelStyle.normal.textColor = Color.white;
        }

        // 버튼 스타일 초기화
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 36; // 버튼 글자 크기
        }

        // 창 크기 2배
        GUI.Window(999, new Rect(20, 20, 840, 1200), DrawWindow, "Achievement Debug");
    }

    private void DrawWindow(int id)
    {
        var manager = AchievementManager.Instance;
        if (manager == null)
        {
            GUILayout.Label("AchievementManager Instance not found.", labelStyle);
            return;
        }

        GUILayout.Label("Runtime Achievements", labelStyle);
        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true);

        foreach (var ra in manager.runtimeDict.Values)
        {
            GUILayout.BeginVertical("box");

            GUILayout.Label($"ID   : {ra.original.id}", labelStyle);
            GUILayout.Label($"Type : {ra.original.type}", labelStyle);
            GUILayout.Label($"Progress: {ra.progress}/{ra.original.targetValue}", labelStyle);
            GUILayout.Label($"Completed: {ra.isCompleted}", labelStyle);
            GUILayout.Label($"Rewarded : {ra.isRewarded}", labelStyle);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+1", buttonStyle, GUILayout.Width(60))) manager.AddProgressByID(ra.original.id, 1);
            if (GUILayout.Button("+5", buttonStyle, GUILayout.Width(60))) manager.AddProgressByID(ra.original.id, 5);
            if (GUILayout.Button("+10", buttonStyle, GUILayout.Width(60))) manager.AddProgressByID(ra.original.id, 10);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Force Complete", buttonStyle))
            {
                int remain = ra.original.targetValue - ra.progress;
                if (remain > 0) manager.AddProgressByID(ra.original.id, remain);
            }

            if (GUILayout.Button("Give Reward", buttonStyle)) manager.Reward(ra.original.id);

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.EndScrollView();

        GUI.DragWindow();
    }
}