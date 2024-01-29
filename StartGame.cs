using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    // ������ �����ϱ� �� ����, Ʈ������ ���� ������ �ϱ� ���� ������ ��Ű�� �ð�
    [SerializeField] float startDelay; 
    public void StartGamePlay()
    {
        StartCoroutine(LoadScenes());
    }

    IEnumerator LoadScenes()
    {
        yield return new WaitForSeconds(startDelay); // ��ŸƮ ��ư�� Pressed �ִϸ��̼��� ����ð� 0.15��
        int currentStage = FindAnyObjectByType<PlayerDataManager>().GetCurrentStageNumber();
        string stageToPlay = "GamePlayStage" + currentStage.ToString();
        SceneManager.LoadScene("Essential", LoadSceneMode.Single);
        SceneManager.LoadScene(stageToPlay, LoadSceneMode.Additive);
    }
}
