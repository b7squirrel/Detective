using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

// GameTexts의 gameHints 배열만 CSV로 내보내고/가져오는 커스텀 에디터
// 반드시 "Assets/Editor/" 폴더 안에 넣어야 합니다.
[CustomEditor(typeof(GameTexts))]
public class GameTextsHintsEditor : Editor
{
    // 임시 편집용 CSV 경로 (gameHints 전용)
    private const string HintsCsvPath = "Assets/Data/Localization/#GameHintsTemp.csv";

    public override void OnInspectorGUI()
    {
        // 기존 인스펙터를 그대로 그림
        DrawDefaultInspector();

        GameTexts gameTexts = (GameTexts)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("게임 힌트 CSV 도구", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("경로: " + HintsCsvPath, MessageType.None);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("힌트 CSV로 내보내기"))
        {
            ExportHintsToCsv(gameTexts);
        }
        if (GUILayout.Button("힌트 CSV에서 가져오기"))
        {
            ImportHintsFromCsv(gameTexts);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void ExportHintsToCsv(GameTexts gameTexts)
    {
        string fullPath = GetFullPath(HintsCsvPath);
        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string[] hints = gameTexts.gameHints ?? new string[0];

        StringBuilder sb = new StringBuilder();
        sb.Append("Hint\n");
        foreach (string hint in hints)
        {
            sb.Append(EscapeCsvField(hint));
            sb.Append('\n');
        }

        File.WriteAllText(fullPath, sb.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();

        Debug.Log("[GameTextsEditor] 힌트 " + hints.Length + "개를 내보냈습니다: " + HintsCsvPath);

        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(HintsCsvPath);
        if (asset != null)
        {
            EditorGUIUtility.PingObject(asset);
        }
    }

    private void ImportHintsFromCsv(GameTexts gameTexts)
    {
        string fullPath = GetFullPath(HintsCsvPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("[GameTextsEditor] 파일을 찾을 수 없습니다: " + HintsCsvPath);
            return;
        }

        string content = File.ReadAllText(fullPath, Encoding.UTF8);
        List<string> rows = ParseSingleColumnCsv(content);

        // 첫 줄이 헤더("Hint")면 제외
        if (rows.Count > 0 && rows[0].Trim().Equals("Hint", StringComparison.OrdinalIgnoreCase))
        {
            rows.RemoveAt(0);
        }

        if (rows.Count == 0)
        {
            Debug.LogWarning("[GameTextsEditor] CSV에서 읽은 힌트가 없습니다.");
            return;
        }

        Undo.RecordObject(gameTexts, "Import Game Hints");
        gameTexts.gameHints = rows.ToArray();
        EditorUtility.SetDirty(gameTexts);
        AssetDatabase.SaveAssets();

        serializedObject.Update();
        Repaint();

        Debug.Log("[GameTextsEditor] 힌트 " + rows.Count + "개를 가져왔습니다.");
    }

    // "Assets/..." 형태의 경로를 실제 디스크 절대 경로로 변환
    private static string GetFullPath(string assetRelativePath)
    {
        const string prefix = "Assets/";
        string relative = assetRelativePath.StartsWith(prefix)
            ? assetRelativePath.Substring(prefix.Length)
            : assetRelativePath;
        return Path.Combine(Application.dataPath, relative);
    }

    // 필드 하나를 CSV 규칙에 맞게 이스케이프 (쉼표/줄바꿈/따옴표가 있으면 따옴표로 감싸기)
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";

        bool needsQuotes = field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r");
        string escaped = field.Replace("\"", "\"\"");
        return needsQuotes ? "\"" + escaped + "\"" : escaped;
    }

    // 한 컬럼짜리 CSV 파싱 (따옴표로 감싼 필드 안의 줄바꿈도 올바르게 처리)
    private static List<string> ParseSingleColumnCsv(string content)
    {
        List<string> rows = new List<string>();
        int i = 0;
        int len = content.Length;

        while (i < len)
        {
            // 빈 줄(파일 끝 개행 등)은 건너뜀
            if (content[i] == '\r') { i++; continue; }
            if (content[i] == '\n') { i++; continue; }

            StringBuilder field = new StringBuilder();

            if (content[i] == '"')
            {
                i++; // 여는 따옴표 건너뛰기
                while (i < len)
                {
                    if (content[i] == '"')
                    {
                        if (i + 1 < len && content[i + 1] == '"')
                        {
                            field.Append('"');
                            i += 2;
                        }
                        else
                        {
                            i++; // 닫는 따옴표
                            break;
                        }
                    }
                    else
                    {
                        field.Append(content[i]);
                        i++;
                    }
                }
                if (i < len && content[i] == '\r') i++;
                if (i < len && content[i] == '\n') i++;
            }
            else
            {
                while (i < len && content[i] != '\n' && content[i] != '\r')
                {
                    field.Append(content[i]);
                    i++;
                }
                if (i < len && content[i] == '\r') i++;
                if (i < len && content[i] == '\n') i++;
            }

            rows.Add(field.ToString());
        }

        return rows;
    }
}