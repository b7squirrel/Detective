using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class EditPlayScene
{
    static EditPlayScene()
    {
        // Build Setting�� ������ 0������ ���
        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        // AssetDatabase : ������Ʈ�� ���Ե� ���¿� ������ �� ���
        // SceneAsset : �����Ϳ��� �� ��ü�� ������ �� ���
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);

        // �÷��� ����� ���� �� ����
        EditorSceneManager.playModeStartScene = sceneAsset;
    }
}