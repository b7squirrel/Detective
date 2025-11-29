using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EquipmentImageManager))]
public class UpdateEquipmentImage : Editor
{
    public override void OnInspectorGUI()
    {
        EquipmentImageManager equipImgManager = (EquipmentImageManager)target;

        // Inspector에서 변경된 값을 가져옴
        serializedObject.Update();

        // 기본 Inspector UI 그리기
        base.OnInspectorGUI();

        // 변경사항을 적용
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Update Equipment Image"))
        {
            // 강제로 변경 사항을 감지하고 업데이트
            EditorUtility.SetDirty(equipImgManager);
            
            // 스프라이트 업데이트 실행
            equipImgManager.UpdateImages();
        }
    }
}