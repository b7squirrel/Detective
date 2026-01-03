using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    public enum LoadingType
    {
        ToStage,      // 스테이지로 이동
        ToLobby       // 로비로 귀환
    }

    GameMode currentMode = GameMode.Regular;
    LoadingType currentLoadingType = LoadingType.ToStage;

    [SerializeField] Slider progressBar;
    [SerializeField] TMPro.TextMeshProUGUI progressText;

    // 레귤러 모드용
    public void LoadScenes()
    {
        LoadScenes(GameMode.Regular);
    }

    // 게임 모드 선택 가능
    public void LoadScenes(GameMode mode = GameMode.Regular)
    {
        currentMode = mode;
        currentLoadingType = LoadingType.ToStage;
        progressBar.value = 0;
        SetProgressText();
        StartCoroutine(LoadSceneCo());
    }

    // 로비로 돌아가기 로딩
    public void LoadLobby()
    {
        currentLoadingType = LoadingType.ToLobby;
        progressBar.value = 0;
        SetProgressText();
        StartCoroutine(LoadLobbyCo());
    }

    // 최소한 3초간은 로딩되도록
    IEnumerator LoadSceneCo()
    {
        string stageToPlay = currentMode == GameMode.Infinite ? "InfiniteStage" : "Stage";

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
                    yield break;
                }
            }
            yield return null;
        }
    }

    // 로비 로딩
    IEnumerator LoadLobbyCo()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Single);
        op.allowSceneActivation = false;
        float timer = 0f;
        
        while (!op.isDone)
        {
            if (op.progress < 0.9f)
            {
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
                
                if (timer > 2f) // 로비는 좀 더 빠르게 (2초)
                {
                    op.allowSceneActivation = true;
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