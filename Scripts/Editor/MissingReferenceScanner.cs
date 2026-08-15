using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 깨진 참조(Missing Reference) 스캐너.
///
/// grade 0 에셋을 삭제한 뒤, 어딘가에서 그 에셋을 여전히 참조하고 있던 프리팹/ScriptableObject를
/// 찾아내기 위한 도구입니다.
///
/// 두 가지를 스캔합니다:
/// 1) 일반 깨진 참조 스캔 — 프로젝트 내 모든 프리팹/ScriptableObject의 모든 Object 참조 필드를 훑어서,
///    "한 번도 할당 안 된 빈 값"이 아니라 "가리키던 대상이 삭제되어 지금은 끊어진" 참조만 골라냅니다.
/// 2) UpgradeData 전용 점검 — weaponData와 item이 둘 다 비어있는데 Coin/Heal 타입이 아닌 UpgradeData를
///    직접 나열합니다 (지금 겪고 있는 증상과 정확히 일치하는 케이스).
///
/// 사용법:
/// 1. 이 파일을 "Assets/Editor/" 폴더 아래에 둡니다.
/// 2. 상단 메뉴 Tools > Missing Reference Scanner 를 엽니다.
/// 3. 대상 폴더를 지정하고 "스캔 실행"을 누릅니다.
/// 4. 결과 목록에서 "선택" 버튼을 누르면 Project 창에서 해당 에셋이 하이라이트됩니다.
/// </summary>
public class MissingReferenceScanner : EditorWindow
{
    string rootFolder = "Assets";
    Vector2 scrollBroken;
    Vector2 scrollUpgrade;

    List<ScanResult> brokenRefResults = new List<ScanResult>();
    List<ScanResult> upgradeDataResults = new List<ScanResult>();

    class ScanResult
    {
        public string assetPath;
        public string objectName;
        public string detail;
        public UnityEngine.Object assetRef;
    }

    [MenuItem("Tools/Missing Reference Scanner")]
    static void ShowWindow()
    {
        var win = GetWindow<MissingReferenceScanner>("Missing Reference Scanner");
        win.minSize = new Vector2(650, 500);
    }

    void OnGUI()
    {
        GUILayout.Label("깨진 참조(Missing Reference) 스캐너", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "삭제된 에셋(예: grade 0으로 지운 WeaponData/Item)을 여전히 참조하고 있는 " +
            "프리팹/ScriptableObject를 찾아냅니다.\n" +
            "'한 번도 할당 안 된 빈 값'과 '과거엔 있었지만 지금은 사라진 깨진 참조'를 구분해서, " +
            "후자만 보고합니다.",
            MessageType.Info);

        EditorGUILayout.Space();
        rootFolder = EditorGUILayout.TextField("대상 폴더", rootFolder);

        EditorGUILayout.Space();
        if (GUILayout.Button("스캔 실행", GUILayout.Height(32)))
        {
            Scan();
        }

        EditorGUILayout.Space();

        // ── 결과 1: 일반 깨진 참조 ──
        GUILayout.Label($"1) 깨진 참조 목록: {brokenRefResults.Count}개", EditorStyles.boldLabel);
        scrollBroken = EditorGUILayout.BeginScrollView(scrollBroken, GUILayout.Height(200));
        foreach (var r in brokenRefResults)
        {
            DrawResultRow(r);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();

        // ── 결과 2: UpgradeData 타입 불일치 ──
        GUILayout.Label($"2) weaponData/item 둘 다 비어있는 UpgradeData: {upgradeDataResults.Count}개", EditorStyles.boldLabel);
        scrollUpgrade = EditorGUILayout.BeginScrollView(scrollUpgrade, GUILayout.Height(200));
        foreach (var r in upgradeDataResults)
        {
            DrawResultRow(r);
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawResultRow(ScanResult r)
    {
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField($"{r.assetPath}\n  └ {r.objectName} / {r.detail}", GUILayout.Height(36));
        if (GUILayout.Button("선택", GUILayout.Width(60), GUILayout.Height(36)))
        {
            Selection.activeObject = r.assetRef;
            EditorGUIUtility.PingObject(r.assetRef);
        }
        EditorGUILayout.EndHorizontal();
    }

    void Scan()
    {
        brokenRefResults.Clear();
        upgradeDataResults.Clear();

        if (!AssetDatabase.IsValidFolder(rootFolder))
        {
            Debug.LogError($"[MissingReferenceScanner] 폴더를 찾을 수 없습니다: {rootFolder}");
            return;
        }

        // ── ScriptableObject 에셋들 (UpgradeData, WeaponData, Item 등) ──
        string[] soGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { rootFolder });
        foreach (var guid in soGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (obj == null) continue;

            ScanSerializedObjectForBrokenRefs(obj, path, obj.name);

            // UpgradeData 전용 점검
            if (obj is UpgradeData upgradeData)
            {
                CheckUpgradeData(upgradeData, path);
            }
        }

        // ── 프리팹들 (Level, WeaponManager 등 컴포넌트를 가진 것들) ──
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { rootFolder });
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            foreach (var comp in go.GetComponentsInChildren<Component>(true))
            {
                if (comp == null)
                {
                    // 컴포넌트 자체가 Missing Script인 경우
                    brokenRefResults.Add(new ScanResult
                    {
                        assetPath = path,
                        objectName = go.name,
                        detail = "(Missing Script)",
                        assetRef = go
                    });
                    continue;
                }

                ScanSerializedObjectForBrokenRefs(comp, path, go.name);

                // 프리팹 안에 UpgradeData 리스트를 직접 들고 있는 컴포넌트(Level 등)도
                // 리스트 요소 자체가 깨졌는지 별도 체크할 필요는 없음 —
                // 리스트 요소가 깨지면 위 ScanSerializedObjectForBrokenRefs에서 이미 잡힘
            }
        }

        Debug.Log($"[MissingReferenceScanner] 스캔 완료 — 깨진 참조 {brokenRefResults.Count}개, " +
                  $"weaponData/item 둘 다 비어있는 UpgradeData {upgradeDataResults.Count}개");
    }

    void ScanSerializedObjectForBrokenRefs(UnityEngine.Object obj, string assetPath, string objectName)
    {
        SerializedObject so = new SerializedObject(obj);
        SerializedProperty prop = so.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = true;

            if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                // objectReferenceInstanceIDValue != 0 인데 objectReferenceValue == null 이면
                // "한 번도 할당 안 된 빈 값"이 아니라 "가리키던 대상이 사라진 깨진 참조"
                if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                {
                    brokenRefResults.Add(new ScanResult
                    {
                        assetPath = assetPath,
                        objectName = objectName,
                        detail = $"{obj.GetType().Name}.{prop.propertyPath}",
                        assetRef = obj
                    });
                }
            }
        }
    }

    void CheckUpgradeData(UpgradeData upgradeData, string path)
    {
        // Coin, Heal 타입은 원래 weaponData/item이 둘 다 비어있는 게 정상이므로 제외
        if (upgradeData.upgradeType == UpgradeType.Coin || upgradeData.upgradeType == UpgradeType.Heal)
            return;

        if (upgradeData.weaponData == null && upgradeData.item == null)
        {
            upgradeDataResults.Add(new ScanResult
            {
                assetPath = path,
                objectName = upgradeData.name,
                detail = $"upgradeType={upgradeData.upgradeType} (weaponData, item 모두 비어있음)",
                assetRef = upgradeData
            });
        }
    }
}