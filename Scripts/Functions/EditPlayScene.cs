using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class EditPlayScene
{
    static EditPlayScene()
    {
        // Build Setting에 설정된 0번씬의 경로
        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        // AssetDatabase : 프로젝트에 포함된 에셋에 접근할 때 사용
        // SceneAsset : 에디터에서 씬 객체를 참조할 때 사용
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);

        // 플레이 모드의 시작 씬 설정
        EditorSceneManager.playModeStartScene = sceneAsset;
    }
}