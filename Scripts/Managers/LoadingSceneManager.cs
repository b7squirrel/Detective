using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public void LoadScenes()
    {
        StartCoroutine(LoadSceneCo());
    }

    // 적어도 2초간 로딩하도록
    IEnumerator LoadSceneCo()
    {
        yield return null;

        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();

        AsyncOperation op1 = SceneManager.LoadSceneAsync("Essential", LoadSceneMode.Single);
        op1.allowSceneActivation = false;

        float timer = 0f; // 로딩 시간 재기

        while (!op1.isDone)
        {
            yield return null;
            if (op1.progress < 0.9f)
            {
                Debug.Log("OP1 Progress = " + op1.progress);
                // 그래프 등 표시
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                if(timer > 2f)
                {
                    op1.allowSceneActivation = true;
                    SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
                    yield break;
                }
            }
        }
    }
}