#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class ItemCSVImporter : EditorWindow
{
    [MenuItem("Tools/Localization/Import Items CSV")]
    public static void ShowWindow()
    {
        GetWindow<ItemCSVImporter>("Items CSV Importer");
    }

    private string itemsCSVPath = "Assets/Localization/CSV/items.csv";
    
    private ItemTexts koreanItemTexts;
    private ItemTexts englishItemTexts;

    void OnGUI()
    {
        GUILayout.Label("Items CSV Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        
        // CSV 파일 경로
        GUILayout.Label("CSV File", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        itemsCSVPath = EditorGUILayout.TextField("Items CSV Path", itemsCSVPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select Items CSV", "Assets/Localization/CSV", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                // 절대 경로를 상대 경로로 변환
                if (path.StartsWith(Application.dataPath))
                {
                    itemsCSVPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // ItemTexts 할당
        GUILayout.Label("Target ScriptableObjects", EditorStyles.boldLabel);
        koreanItemTexts = (ItemTexts)EditorGUILayout.ObjectField(
            "Korean ItemTexts", koreanItemTexts, typeof(ItemTexts), false);
        englishItemTexts = (ItemTexts)EditorGUILayout.ObjectField(
            "English ItemTexts", englishItemTexts, typeof(ItemTexts), false);
        
        EditorGUILayout.Space(10);
        
        // 상태 표시
        if (File.Exists(itemsCSVPath))
        {
            EditorGUILayout.HelpBox($"✓ CSV 파일 찾음: {itemsCSVPath}", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"✗ CSV 파일 없음: {itemsCSVPath}", MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        // Import 버튼
        GUI.enabled = File.Exists(itemsCSVPath) && koreanItemTexts != null && englishItemTexts != null;
        if (GUILayout.Button("Import from CSV", GUILayout.Height(40)))
        {
            ImportCSV();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // Export 버튼
        GUI.enabled = koreanItemTexts != null && englishItemTexts != null;
        if (GUILayout.Button("Export to CSV", GUILayout.Height(30)))
        {
            ExportToCSV();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space(10);
        
        // 도움말
        EditorGUILayout.HelpBox(
            "사용 방법:\n" +
            "1. 구글 시트에서 CSV 다운로드\n" +
            "2. Assets/Localization/CSV/ 폴더에 items.csv 저장\n" +
            "3. Korean/English ItemTexts 할당\n" +
            "4. Import from CSV 버튼 클릭", 
            MessageType.Info
        );
    }

    void ImportCSV()
    {
        if (koreanItemTexts == null || englishItemTexts == null)
        {
            EditorUtility.DisplayDialog("Error", "Korean과 English ItemTexts를 모두 할당해주세요!", "OK");
            return;
        }

        if (!File.Exists(itemsCSVPath))
        {
            EditorUtility.DisplayDialog("Error", $"CSV 파일을 찾을 수 없습니다:\n{itemsCSVPath}", "OK");
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(itemsCSVPath);
            
            if (lines.Length < 2)
            {
                EditorUtility.DisplayDialog("Error", "CSV 파일이 비어있거나 헤더만 있습니다.", "OK");
                return;
            }
            
            // 헤더 제외
            var dataLines = lines.Skip(1).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            
            List<ItemTexts.ItemLocalizedName> koreanItems = new List<ItemTexts.ItemLocalizedName>();
            List<ItemTexts.ItemLocalizedName> englishItems = new List<ItemTexts.ItemLocalizedName>();

            int lineNumber = 2; // 헤더 다음부터
            foreach (string line in dataLines)
            {
                string[] values = ParseCSVLine(line);
                
                if (values.Length < 3)
                {
                    Debug.LogWarning($"Line {lineNumber}: 열이 부족합니다. 건너뜁니다. ({line})");
                    lineNumber++;
                    continue;
                }

                string internalName = values[0].Trim();
                string koreanName = values[1].Trim();
                string englishName = values[2].Trim();

                if (string.IsNullOrEmpty(internalName))
                {
                    Debug.LogWarning($"Line {lineNumber}: Internal name이 비어있습니다. 건너뜁니다.");
                    lineNumber++;
                    continue;
                }

                koreanItems.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName = koreanName
                });

                englishItems.Add(new ItemTexts.ItemLocalizedName
                {
                    itemInternalName = internalName,
                    displayName = englishName
                });
                
                lineNumber++;
            }

            // ItemTexts 업데이트
            koreanItemTexts.itemNames = koreanItems.ToArray();
            englishItemTexts.itemNames = englishItems.ToArray();

            // 저장
            EditorUtility.SetDirty(koreanItemTexts);
            EditorUtility.SetDirty(englishItemTexts);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "Import 완료!", 
                $"성공적으로 {koreanItems.Count}개의 아이템을 임포트했습니다!\n\n" +
                $"Korean: {koreanItems.Count}개\n" +
                $"English: {englishItems.Count}개", 
                "OK"
            );
            
            Debug.Log($"✓ CSV Import 완료: {koreanItems.Count}개의 아이템 임포트됨");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Import 중 오류 발생:\n{e.Message}", "OK");
            Debug.LogError($"CSV Import 오류: {e}");
        }
    }

    void ExportToCSV()
    {
        if (koreanItemTexts == null || englishItemTexts == null)
        {
            EditorUtility.DisplayDialog("Error", "Korean과 English ItemTexts를 모두 할당해주세요!", "OK");
            return;
        }

        try
        {
            List<string> lines = new List<string>();
            
            // 헤더
            lines.Add("itemInternalName,korean_name,english_name");

            // 데이터 개수 확인
            int count = Mathf.Min(koreanItemTexts.itemNames.Length, englishItemTexts.itemNames.Length);
            
            if (count == 0)
            {
                EditorUtility.DisplayDialog("Warning", "Export할 아이템이 없습니다.", "OK");
                return;
            }

            // 데이터
            for (int i = 0; i < count; i++)
            {
                var koreanItem = koreanItemTexts.itemNames[i];
                var englishItem = englishItemTexts.itemNames[i];

                if (koreanItem == null || englishItem == null)
                {
                    Debug.LogWarning($"Index {i}: Null 아이템이 있습니다. 건너뜁니다.");
                    continue;
                }

                // CSV 형식으로 변환 (따옴표로 감싸기)
                string line = $"{EscapeCSV(koreanItem.itemInternalName)}," +
                             $"{EscapeCSV(koreanItem.displayName)}," +
                             $"{EscapeCSV(englishItem.displayName)}";
                lines.Add(line);
            }

            // 디렉토리 생성
            string directory = Path.GetDirectoryName(itemsCSVPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 파일 저장
            File.WriteAllLines(itemsCSVPath, lines);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Export 완료!", 
                $"성공적으로 {count}개의 아이템을 Export했습니다!\n\n" +
                $"저장 위치: {itemsCSVPath}", 
                "OK"
            );
            
            Debug.Log($"✓ CSV Export 완료: {itemsCSVPath}");
            
            // 파일 선택
            EditorUtility.RevealInFinder(itemsCSVPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Export 중 오류 발생:\n{e.Message}", "OK");
            Debug.LogError($"CSV Export 오류: {e}");
        }
    }

    // CSV 파싱 (따옴표 처리)
    string[] ParseCSVLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        string currentValue = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue);
                currentValue = "";
            }
            else
            {
                currentValue += c;
            }
        }
        
        values.Add(currentValue);
        return values.ToArray();
    }
    
    // CSV 이스케이프 (쉼표나 따옴표가 있으면 따옴표로 감싸기)
    string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            // 따옴표는 두 개로 이스케이프
            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
        
        return value;
    }
}
#endif