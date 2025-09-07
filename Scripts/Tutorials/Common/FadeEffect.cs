using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FadeEffect : MonoBehaviour
{
    // 페이드 효과가 끝났을 때 호출하고 싶은 메소드를 등록, 호출하는 이벤트 클래스
    [System.Serializable]
    class FadeEvent : UnityEvent { }
    FadeEvent onFadeEvent = new FadeEvent();
    
    [SerializeField]
    [Range(0.01f, 10f)]
    float fadeTime; // 페이드 되는 시간
    
    [SerializeField]
    AnimationCurve fadeCurve; // 페이드 효과가 적용되는 알파 값을 곡선의 값으로 설정
    
    CanvasGroup canvasGroup; // 페이드를 시킬 오브젝트의 캔버스 그룹. 알파 조정
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    
    public void FadeIn(UnityAction action)
    {
        StartCoroutine(Fade(action, 1, 0));
    }
    
    public void FadeOut(UnityAction action)
    {
        StartCoroutine(Fade(action, 0, 1));
    }
    
    IEnumerator Fade(UnityAction action, float start, float end)
    {
        // action 메소드를 이벤트에 등록
        onFadeEvent.AddListener(action);
        
        float current = 0.0f;
        float percent = 0.0f;
        
        while (percent < 1)
        {
            // Time.unscaledDeltaTime 사용으로 timeScale의 영향을 받지 않음
            current += Time.unscaledDeltaTime;
            percent = current / fadeTime;
            
            canvasGroup.alpha = Mathf.Lerp(start, end, fadeCurve.Evaluate(percent));
            
            // WaitForEndOfFrame을 사용하여 timeScale과 무관하게 다음 프레임까지 대기
            yield return new WaitForEndOfFrame();
        }
        
        // 최종 알파 값을 정확히 설정
        canvasGroup.alpha = end;
        
        // action 메소드를 실행
        onFadeEvent.Invoke();
        
        // action 메소드를 이벤트에서 제거
        onFadeEvent.RemoveListener(action);
    }
}