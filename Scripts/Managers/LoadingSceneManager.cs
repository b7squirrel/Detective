using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] Slider progressBar;
    [SerializeField] TMPro.TextMeshProUGUI progressText;

    /// <summary>
    /// canvas의 애니메이션에서 호출
    /// </summary>
    public void LoadScenes()
    {
        Debug.Log("Load Scenes");
        progressBar.value = 0;
        SetProgressText();
        StartCoroutine(LoadSceneCo());
    }

    // 적어도 3초간 로딩하도록
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
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
                if (timer > 3f)
                {
                    op1.allowSceneActivation = true;
                    SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
                    yield break;
                }
            }
        }
    }

    void SetProgressText()
    {
        float progress = progressBar.value * 100f;
        int progressInt = Mathf.FloorToInt(progress);
        progressText.text = progressInt.ToString() + "%";
    }
}