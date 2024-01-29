using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public void LoadScenes()
    {
        StartCoroutine(LoadSceneCo());
    }

    // ��� 2�ʰ� �ε��ϵ���
    IEnumerator LoadSceneCo()
    {
        yield return null;

        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();

        AsyncOperation op1 = SceneManager.LoadSceneAsync("Essential", LoadSceneMode.Single);
        op1.allowSceneActivation = false;

        float timer = 0f; // �ε� �ð� ���

        while (!op1.isDone)
        {
            yield return null;
            if (op1.progress < 0.9f)
            {
                Debug.Log("OP1 Progress = " + op1.progress);
                // �׷��� �� ǥ��
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