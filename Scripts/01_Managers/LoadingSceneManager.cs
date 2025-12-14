using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] Slider progressBar;
    [SerializeField] TMPro.TextMeshProUGUI progressText;

    public void LoadScenes()
    {
        progressBar.value = 0;
        SetProgressText();
        StartCoroutine(LoadSceneCo());
    }

    // 최소한 3초간은 로딩되도록
    IEnumerator LoadSceneCo()
    {
        //yield return null;

        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        //string stageToPlay = "GamePlayStage" + currentStage.ToString();
        string stageToPlay = "Stage";

        AsyncOperation op1 = SceneManager.LoadSceneAsync("Essential", LoadSceneMode.Single);
        op1.allowSceneActivation = false;

        float timer = 0f; // 로딩 시간 측정용

        while (!op1.isDone)
        {
            // 90%까지는 실제 로딩이 진행됨
            if (op1.progress < 0.9f)
            {
                // 그 구간까지만 게이지 표시
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
            }
            // 그 이후부터는 100%까지 인위적으로 채움
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
                if (timer > 3f)
                {
                    op1.allowSceneActivation = true;
                    SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);

                    StageManager stagemanager = FindObjectOfType<StageManager>();
                    yield break;
                }
            }
            yield return null;
        }
    }

    void SetProgressText()
    {
        float progress = progressBar.value * 100f;
        int progressInt = Mathf.FloorToInt(progress);
        progressText.text = progressInt.ToString() + "%";
    }
}