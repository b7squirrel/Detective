using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] Slider progressBar;
    [SerializeField] TMPro.TextMeshProUGUI progressText;

    /// <summary>
    /// canvas�� �ִϸ��̼ǿ��� ȣ��
    /// </summary>
    public void LoadScenes()
    {
        progressBar.value = 0;
        SetProgressText();
        StartCoroutine(LoadSceneCo());
    }

    // ��� 3�ʰ� �ε��ϵ���
    IEnumerator LoadSceneCo()
    {
        //yield return null;

        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();

        AsyncOperation op1 = SceneManager.LoadSceneAsync("Essential", LoadSceneMode.Single);
        op1.allowSceneActivation = false;

        float timer = 0f; // �ε� �ð� ���

        while (!op1.isDone)
        {
            Debug.Log("OP1 Progress = " + op1.progress);

            // 90%������ ���ε��� ���� �ε��� ����
            if (op1.progress < 0.9f)
            {
                // �׷��� �� ǥ��
                progressBar.value = Mathf.MoveTowards(progressBar.value, 1f, Time.deltaTime);
                SetProgressText();
            }
            // �� ���ķδ� 100%���� ����ũ �ε�
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

    void SetProgressText()
    {
        float progress = progressBar.value * 100f;
        int progressInt = Mathf.FloorToInt(progress);
        progressText.text = progressInt.ToString() + "%";
    }
}