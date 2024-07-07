using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class StageTitleGroundUI : MonoBehaviour
{
    [SerializeField] GameObject stageTitleObject;
    [SerializeField] GameObject title;
    [SerializeField] GameObject bossName;
    [SerializeField] float titleDuration;
    [SerializeField] float fadeDuration;

    TMPro.TextMeshPro titleText;
    TMPro.TextMeshPro bossNameText;


    private void Start()
    {
        Init();
    }
    public void Init()
    {
        PlayerDataManager playerData = FindObjectOfType<PlayerDataManager>();
        StageInfo stageInfo = FindObjectOfType<StageInfo>();
        int index = playerData.GetCurrentStageNumber();

        titleText = title.GetComponentInChildren<TMPro.TextMeshPro>();
        bossNameText = bossName.GetComponentInChildren<TMPro.TextMeshPro>();

        titleText.text = "Stage " + index.ToString();
        bossNameText.text = stageInfo.GetStageInfo(index).Title;

        StartCoroutine(StageTitleUpCo());
    }

    IEnumerator StageTitleUpCo()
    {
        yield return new WaitForSeconds(titleDuration);

        FadeOut();
    }
    void FadeOut()
    {
        StartCoroutine(FadeOutCo());
    }

    IEnumerator FadeOutCo()
    {
        Color titleInitialColor = titleText.color;
        Color NameInitialColor = bossNameText.color;
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);

            titleText.color = new Color(titleInitialColor.r, titleInitialColor.g, titleInitialColor.b, alpha);
            bossNameText.color = new Color(NameInitialColor.r, NameInitialColor.g, NameInitialColor.b, alpha);
            yield return null;
        }
    }
}