using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    // 게임을 시작하기 전 사운드, 트랜지션 등을 마무리 하기 위해 딜레이 시키는 시간
    [SerializeField] float startDelay; 
    public void StartGamePlay()
    {
        StartCoroutine(LoadScenes());
    }

    IEnumerator LoadScenes()
    {
        yield return new WaitForSeconds(startDelay); // 스타트 버튼의 Pressed 애니메이션의 재생시간 0.15초
        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();
        SceneManager.LoadScene("Essential", LoadSceneMode.Single);
        SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
    }
}
