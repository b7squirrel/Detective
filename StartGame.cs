using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartGamePlay()
    {
        StartCoroutine(LoadScenes());
    }

    IEnumerator LoadScenes()
    {
        yield return new WaitForSeconds(.15f); // 스타트 버튼의 Pressed 애니메이션의 재생시간 0.15초
        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();
        SceneManager.LoadScene("Essential", LoadSceneMode.Single);
        SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
    }
}
