using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// 로컬라이제이션 ScriptableObject(.asset) 안의 모든 string / string[] / List<string>
/// 필드를 리플렉션으로 스캔하여, 실제 게임에서 사용되는 모든 글자를 모아
/// TMP Font Asset Creator의 "Characters from File"용 .txt 파일로 저장합니다.
///
/// 사용법:
///   메뉴 Tools > Font > Generate Character Set From Localization
///   - 대상 폴더(로컬라이제이션 .asset들이 들어있는 폴더)와 출력 경로를 Inspector에서 지정
///   - "Generate" 버튼 클릭
///
/// 새 필드/새 텍스트가 추가되어도 코드 수정 없이 자동으로 포함됩니다.
/// </summary>
public class FontCharacterSetGenerator : EditorWindow
{
    private DefaultAsset targetFolder;
    private string outputPath = "Assets/z_assets/TextMesh Pro/CharacterSet.txt";
    private bool includeAsciiRange = true;

    [MenuItem("Tools/Font/Generate Character Set From Localization")]
    public static void ShowWindow()
    {
        GetWindow<FontCharacterSetGenerator>("Font Character Set");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("로컬라이제이션 글자 추출 → TMP Character File 생성", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
            "로컬라이제이션 폴더", targetFolder, typeof(DefaultAsset), false);

        outputPath = EditorGUILayout.TextField("출력 파일 경로", outputPath);

        includeAsciiRange = EditorGUILayout.Toggle(
            "기본 ASCII(영문/숫자/기호) 포함", includeAsciiRange);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate"))
        {
            if (targetFolder == null)
            {
                EditorUtility.DisplayDialog("오류", "로컬라이제이션 폴더를 지정해주세요.", "확인");
                return;
            }
            Generate();
        }
    }

    private void Generate()
    {
        string folderPath = AssetDatabase.GetAssetPath(targetFolder);
        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath });

        HashSet<char> chars = new HashSet<char>();

        if (includeAsciiRange)
        {
            // 공백(32) ~ 물결표(126) : 영문, 숫자, 기본 기호 전부 포함
            for (int code = 32; code <= 126; code++)
                chars.Add((char)code);
        }

        int assetCount = 0;
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null) continue;

            assetCount++;
            CollectCharsFromObject(asset, chars, new HashSet<object>());
        }

        // 정렬: ASCII 먼저, 그 다음 코드포인트 순
        var sorted = chars.OrderBy(c => c).ToArray();
        string result = new string(sorted);

        string fullOutputPath = outputPath;
        string dir = Path.GetDirectoryName(fullOutputPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(fullOutputPath, result, new UTF8Encoding(false));
        AssetDatabase.Refresh();

        Debug.Log($"[FontCharacterSetGenerator] {assetCount}개 에셋에서 총 {result.Length}개의 고유 글자를 추출하여 저장했습니다: {fullOutputPath}");
        EditorUtility.DisplayDialog("완료",
            $"{assetCount}개 에셋 스캔\n총 {result.Length}개 고유 글자\n저장 위치: {fullOutputPath}\n\n" +
            "이제 Font Asset Creator의 Character File에 이 .txt 파일을 지정하고 Generate Font Atlas를 다시 실행하세요.",
            "확인");
    }

    /// <summary>
    /// 객체의 모든 public/private 필드를 재귀적으로 순회하며
    /// string, string[], List<string> 값에서 문자를 수집합니다.
    /// </summary>
    private void CollectCharsFromObject(object obj, HashSet<char> chars, HashSet<object> visited)
    {
        if (obj == null) return;
        if (visited.Contains(obj)) return; // 순환 참조 방지
        visited.Add(obj);

        var type = obj.GetType();

        // Unity/시스템 타입은 재귀 진입하지 않음
        if (type.Namespace != null &&
            (type.Namespace.StartsWith("UnityEngine") || type.Namespace.StartsWith("System")) &&
            !(obj is string))
        {
            // ScriptableObject 자신은 통과시켜야 하므로 별도 처리 (아래에서 필드 순회)
            if (!(obj is ScriptableObject)) return;
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            // 컴파일러 생성/static 백킹 필드 등은 스킵
            if (field.IsStatic) continue;

            object value;
            try { value = field.GetValue(obj); }
            catch { continue; }

            if (value == null) continue;

            switch (value)
            {
                case string s:
                    foreach (char c in s)
                        if (c == ' ' || c >= 32) chars.Add(c);
                    break;

                case System.Collections.IEnumerable enumerable when !(value is string):
                    foreach (var item in enumerable)
                    {
                        if (item is string itemStr)
                        {
                            foreach (char c in itemStr)
                                if (c == ' ' || c >= 32) chars.Add(c);
                        }
                        else if (item != null && IsCollectableType(item.GetType()))
                        {
                            CollectCharsFromObject(item, chars, visited);
                        }
                    }
                    break;

                default:
                    if (IsCollectableType(field.FieldType))
                        CollectCharsFromObject(value, chars, visited);
                    break;
            }
        }
    }

    private bool IsCollectableType(System.Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum) return false;
        if (type.Namespace != null && type.Namespace.StartsWith("UnityEngine")) return false;
        // 사용자 정의 class/struct (직렬화 가능한 데이터 클래스) 또는 ScriptableObject만 재귀
        return type.IsClass || (type.IsValueType && !type.IsPrimitive);
    }
}