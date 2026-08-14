using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 5단계(0~4) 등급 체계를 4단계(0~3)로 줄이기 위한 에셋 재번호 툴입니다.
/// 사전 조건: 각 무기/아이템 그룹의 grade=0 에셋들은 이미 삭제되어 있어야 합니다.
/// 남은 grade 1,2,3,4 에셋들을 0,1,2,3으로 파일명과 grade 필드를 함께 재번호합니다.
///
/// 사용법:
/// 1. 이 파일을 프로젝트의 "Assets/Editor/" 폴더 아래에 둡니다. (Editor 폴더가 없다면 새로 만드세요)
/// 2. 상단 메뉴 Tools > Weapon Grade Renumberer 를 엽니다.
/// 3. 대상 폴더를 지정합니다 (예: Assets/Data/Weapons_Items/01_Weapon).
/// 4. "Dry Run" 체크된 상태로 먼저 실행해서 Console 로그로 어떤 파일이 어떻게 바뀔지 미리 확인합니다.
/// 5. 문제 없으면 Dry Run 체크를 해제하고 다시 실행합니다.
/// 6. 반드시 실행 전 프로젝트를 백업하거나 버전관리(Git 등)에 커밋해두고 진행하세요.
/// </summary>
public class WeaponGradeRenumberer : EditorWindow
{
    string rootFolder = "Assets/Data/Weapons_Items/01_Weapon";
    bool dryRun = true;
    Vector2 scroll;
    List<string> lastLog = new List<string>();

    [MenuItem("Tools/Weapon Grade Renumberer")]
    static void ShowWindow()
    {
        var win = GetWindow<WeaponGradeRenumberer>("Weapon Grade Renumberer");
        win.minSize = new Vector2(500, 400);
    }

    class AssetInfo
    {
        public string path;
        public string prefix;
        public int grade;
        public string suffix; // "" 이면 본체 에셋 (WeaponData 혹은 Item)
    }

    // 이름_숫자(_접미사) 패턴. prefix는 non-greedy로 가장 먼저 나오는 "_숫자" 앞부분을 잡는다.
    static readonly Regex NamePattern = new Regex(@"^(.+?)_(\d+)(_.*)?$");

    void OnGUI()
    {
        GUILayout.Label("무기/아이템 Grade 재번호 툴", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "grade 0번 에셋을 이미 삭제했다는 전제 하에, 남은 1,2,3,4번을 0,1,2,3으로 재번호합니다.\n" +
            "파일명과, 접미사 없는 본체 에셋의 grade 필드를 함께 갱신합니다.",
            MessageType.Info);

        EditorGUILayout.Space();
        rootFolder = EditorGUILayout.TextField("대상 폴더", rootFolder);
        dryRun = EditorGUILayout.Toggle("Dry Run (미리보기만, 실제 변경 없음)", dryRun);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(!AssetDatabase.IsValidFolder(rootFolder)))
        {
            GUI.backgroundColor = dryRun ? Color.white : new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button(dryRun ? "미리보기 실행" : "⚠ 실제 변경 실행 (되돌릴 수 없음)", GUILayout.Height(32)))
            {
                Run();
            }
            GUI.backgroundColor = Color.white;
        }

        if (!AssetDatabase.IsValidFolder(rootFolder))
        {
            EditorGUILayout.HelpBox("유효한 폴더 경로가 아닙니다.", MessageType.Warning);
        }

        if (lastLog.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label($"결과 로그 ({lastLog.Count}줄) — Console에도 동일하게 출력됩니다", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(250));
            foreach (var line in lastLog)
                EditorGUILayout.LabelField(line, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndScrollView();
        }
    }

    void Run()
    {
        lastLog.Clear();

        if (!AssetDatabase.IsValidFolder(rootFolder))
        {
            Debug.LogError($"[WeaponGradeRenumberer] 폴더를 찾을 수 없습니다: {rootFolder}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { rootFolder });
        List<AssetInfo> infos = new List<AssetInfo>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            Match m = NamePattern.Match(name);
            if (!m.Success) continue;

            infos.Add(new AssetInfo
            {
                path = path,
                prefix = m.Groups[1].Value,
                grade = int.Parse(m.Groups[2].Value),
                suffix = m.Groups[3].Success ? m.Groups[3].Value : ""
            });
        }

        if (infos.Count == 0)
        {
            lastLog.Add("패턴에 맞는(이름_숫자) 에셋을 찾지 못했습니다. 폴더 경로를 확인해주세요.");
            Debug.LogWarning($"[WeaponGradeRenumberer] {rootFolder} 아래에서 대상 에셋을 찾지 못했습니다.");
            return;
        }

        var groups = infos.GroupBy(i => i.prefix).OrderBy(g => g.Key);

        int renamedCount = 0;
        int gradeFieldUpdated = 0;
        int skippedGroups = 0;

        foreach (var group in groups)
        {
            string prefix = group.Key;
            var gradesPresent = group.Select(i => i.grade).Distinct().OrderBy(g => g).ToList();

            if (gradesPresent.Contains(0))
            {
                lastLog.Add($"⚠ [건너뜀] '{prefix}': grade 0이 아직 남아있습니다. 0번을 먼저 삭제한 뒤 다시 실행하세요.");
                skippedGroups++;
                continue;
            }

            if (gradesPresent.Count == 0) continue;

            lastLog.Add($"— '{prefix}' 그룹 처리 (등급 {string.Join(",", gradesPresent)}) —");

            // 오름차순으로 처리해야 이름 충돌이 안 생김 (1→0 먼저 비운 뒤 2→1 진행)
            foreach (int oldGrade in gradesPresent)
            {
                int newGrade = oldGrade - 1;
                if (newGrade < 0)
                {
                    lastLog.Add($"  ⚠ 예상치 못한 grade {oldGrade}, 건너뜀");
                    continue;
                }

                var assetsAtGrade = group.Where(i => i.grade == oldGrade).ToList();

                foreach (var info in assetsAtGrade)
                {
                    string dir = Path.GetDirectoryName(info.path).Replace("\\", "/");
                    string ext = Path.GetExtension(info.path);
                    string newName = $"{prefix}_{newGrade}{info.suffix}";
                    string newPath = $"{dir}/{newName}{ext}";

                    lastLog.Add($"  {info.path}  →  {newPath}");

                    if (!dryRun)
                    {
                        string error = AssetDatabase.RenameAsset(info.path, newName);
                        if (!string.IsNullOrEmpty(error))
                        {
                            Debug.LogError($"[WeaponGradeRenumberer] 이름 변경 실패: {info.path} → {newName} ({error})");
                            lastLog.Add($"    ✗ 실패: {error}");
                            continue;
                        }
                        renamedCount++;

                        // 접미사가 없는 본체 에셋(WeaponData 혹은 Item)이면 grade 필드도 갱신
                        if (string.IsNullOrEmpty(info.suffix))
                        {
                            if (TrySetGradeField(newPath, newGrade))
                                gradeFieldUpdated++;
                        }
                    }
                }
            }
        }

        if (!dryRun)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        string summary = dryRun
            ? $"[미리보기 완료] 총 {infos.Count}개 대상, {skippedGroups}개 그룹 건너뜀. 실제로는 아무것도 변경되지 않았습니다."
            : $"[변경 완료] 이름 변경 {renamedCount}개, grade 필드 갱신 {gradeFieldUpdated}개, 건너뛴 그룹 {skippedGroups}개";

        lastLog.Add("");
        lastLog.Add(summary);
        Debug.Log($"[WeaponGradeRenumberer] {summary}");
    }

    /// <summary>
    /// 리플렉션으로 "grade" 필드(또는 프로퍼티)를 찾아 값을 설정합니다.
    /// WeaponData(grade: int)와 Item(grade: enum 등) 양쪽 모두 대응합니다.
    /// </summary>
    static bool TrySetGradeField(string assetPath, int newGrade)
    {
        UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (asset == null) return false;

        var type = asset.GetType();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        FieldInfo field = type.GetField("grade", flags);
        if (field != null)
        {
            SetValue(field.FieldType, v => field.SetValue(asset, v), newGrade);
            EditorUtility.SetDirty(asset);
            return true;
        }

        PropertyInfo prop = type.GetProperty("grade", flags);
        if (prop != null && prop.CanWrite)
        {
            SetValue(prop.PropertyType, v => prop.SetValue(asset, v), newGrade);
            EditorUtility.SetDirty(asset);
            return true;
        }

        Debug.LogWarning($"[WeaponGradeRenumberer] '{assetPath}' 에서 'grade' 필드/프로퍼티를 찾지 못했습니다. 수동으로 확인해주세요.");
        return false;
    }

    static void SetValue(System.Type fieldType, System.Action<object> setter, int newGrade)
    {
        if (fieldType == typeof(int))
        {
            setter(newGrade);
        }
        else if (fieldType.IsEnum)
        {
            setter(System.Enum.ToObject(fieldType, newGrade));
        }
        else
        {
            Debug.LogWarning($"[WeaponGradeRenumberer] 지원하지 않는 grade 필드 타입: {fieldType}");
        }
    }
}