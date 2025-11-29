using UnityEngine;
using UnityEditor;
using TMPro;

public class SelectedTextEditor : EditorWindow
{
    public TextListData textData; // ScriptableObject 참조
    private Vector2 scrollPos;    // 스크롤 위치 저장

    [MenuItem("Tools/Selected Text Editor")]
    static void OpenWindow()
    {
        GetWindow<SelectedTextEditor>("Selected Text Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("TextMeshProUGUI 텍스트 수정", EditorStyles.boldLabel);

        // ScriptableObject 연결
        textData = (TextListData)EditorGUILayout.ObjectField("Text Data", textData, typeof(TextListData), false);

        if (textData == null)
        {
            EditorGUILayout.HelpBox("TextListData를 연결하세요.", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // 씬 전체 자동 등록 버튼
        if (GUILayout.Button("씬의 모든 TextMeshProUGUI 자동 등록"))
        {
            AutoFillTextList();
        }

        EditorGUILayout.Space();

        if (textData.textList.Count == 0)
        {
            EditorGUILayout.HelpBox("TextMeshProUGUI가 없습니다.", MessageType.Info);
            return;
        }

        // 스크롤 시작
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(400)); // 높이 조정 가능

        // 개별 수정
        GUILayout.Label("각 텍스트를 개별 수정", EditorStyles.boldLabel);
        for (int i = 0; i < textData.textList.Count; i++)
        {
            TextMeshProUGUI t = textData.textList[i];
            if (t == null) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(t.name, GUILayout.Width(150));
            string newText = EditorGUILayout.TextField(t.text);
            if (newText != t.text)
            {
                Undo.RecordObject(t, "Change Text");
                t.text = newText;
                EditorUtility.SetDirty(t);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView(); // 스크롤 종료

        EditorGUILayout.Space();

        // 전체 텍스트 일괄 변경
        if (GUILayout.Button("모든 텍스트를 동일하게 변경"))
        {
            string commonText = EditorGUILayout.TextField("New Text", "");
            foreach (var t in textData.textList)
            {
                if (t == null) continue;
                Undo.RecordObject(t, "Change Text");
                t.text = commonText;
                EditorUtility.SetDirty(t);
            }
        }
    }

    private void AutoFillTextList()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true); // 비활성화 포함
        textData.textList.Clear();
        foreach (var t in allTexts)
        {
            textData.textList.Add(t);
        }
        EditorUtility.SetDirty(textData);
        Debug.Log($"씬의 TextMeshProUGUI {allTexts.Length}개를 자동 등록했습니다.");
    }
}