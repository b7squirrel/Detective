using System.Collections;
using UnityEngine;

public class TutorialMove : MonoBehaviour
{
    [SerializeField] float fadeTime = 0.5f; // 페이드 인/아웃 시간
    [SerializeField] float duration = 2f; // 보여주는 시간
    [SerializeField] AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    CanvasGroup canvasGroup;

    void OnEnable()
    {
        SetObject();
        StartCoroutine(ShowInstructionSequence());
    }

    void SetObject()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }
    IEnumerator ShowInstructionSequence()
    {
        // GameManager.instance.pauseManager.PauseGame();

        // 페이드 인
        yield return StartCoroutine(FadeAlpha(0f, 1f, fadeTime));

        // 지정된 시간 동안 대기 (완전히 보이는 상태)
        yield return StartCoroutine(WaitForUnscaledSeconds(duration));

        // 페이드 아웃
        yield return StartCoroutine(FadeAlpha(1f, 0f, fadeTime));

        // 오브젝트 파괴
        Destroy(gameObject);
        // GameManager.instance.pauseManager.UnPauseGame();
    }
    // timeScale이 0이 되어도 진행될 수 있도록 unscaled delta time
    IEnumerator FadeAlpha(float startAlpha, float endAlpha, float fadeTime)
    {
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime; // timeScale의 영향을 받지 않음
            float progress = elapsed / fadeTime;
            float curveValue = fadeCurve.Evaluate(progress);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);

            yield return null;
        }

        // 정확한 최종값 설정
        canvasGroup.alpha = endAlpha;
    }
    // timeScale의 영향을 받지 않는 대기 함수
    IEnumerator WaitForUnscaledSeconds(float seconds)
    {
        float elapsed = 0f;
        
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
