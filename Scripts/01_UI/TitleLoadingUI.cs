using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class TitleLoadingUI : MonoBehaviour
{
    [Header("로딩 패널")]
    [SerializeField] GameObject loadingPanel;
    
    [Header("기존 방식 - 간단한 페이드")]
    [SerializeField] bool useSimpleFade = true; // true면 기존 방식, false면 진행바 방식
    [SerializeField] float blackOutTime = 2f; // 로딩 화면 표시 시간
    [SerializeField] float fadeOutDuration = 0.5f; // 페이드 아웃 시간
    
    [Header("진행바 방식 (선택사항)")]
    [SerializeField] Slider progressBar;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI statusText;
    
    void Start()
    {
        if (useSimpleFade)
        {
            StartCoroutine(SimpleLoadingScreen());
        }
        else
        {
            StartCoroutine(ShowLoadingScreen());
        }
    }
    
    // 기존 MainMenuManager 방식 - 간단한 페이드 인/아웃
    IEnumerator SimpleLoadingScreen()
    {
        loadingPanel.SetActive(true);
        
        // blackOutTime만큼 대기
        yield return new WaitForSeconds(blackOutTime);
        
        // ImageBouncerManager가 있으면 점프 효과 실행
        ImageBouncerManager bouncer = FindObjectOfType<ImageBouncerManager>();
        if (bouncer != null)
        {
            bouncer.Jump(150);
        }
        
        yield return new WaitForSeconds(1f);
        
        // 페이드 아웃 애니메이션
        Image loadingImage = loadingPanel.GetComponentInChildren<Image>();
        TextMeshProUGUI loadingText = loadingPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        if (loadingImage != null)
        {
            loadingImage.DOFade(0, fadeOutDuration)
                .OnComplete(() => { loadingPanel.SetActive(false); });
        }
        
        if (loadingText != null)
        {
            loadingText.DOFade(0, fadeOutDuration);
        }
        
        // 이미지나 텍스트가 없으면 그냥 비활성화
        if (loadingImage == null && loadingText == null)
        {
            yield return new WaitForSeconds(fadeOutDuration);
            loadingPanel.SetActive(false);
        }
    }
    
    // 진행바 방식 - GameInitializer와 연동
    IEnumerator ShowLoadingScreen()
    {
        loadingPanel.SetActive(true);
        
        // // GameInitializer가 없으면 간단한 페이드로 전환
        // if (GameInitializer.Instance == null)
        // {
        //     Debug.LogWarning("GameInitializer를 찾을 수 없습니다. 간단한 로딩 화면으로 전환합니다.");
        //     yield return StartCoroutine(SimpleLoadingScreen());
        //     yield break;
        // }
        
        while (!GameInitializer.IsInitialized)
        {
            float progress = GameInitializer.InitializationProgress;
            
            if (progressBar != null)
                progressBar.value = progress;
            
            if (progressText != null)
                progressText.text = $"{progress * 100:F0}%";
            
            // 진행 상태 텍스트
            if (statusText != null)
            {
                if (progress < 0.25f)
                    statusText.text = "카드 데이터 로딩 중...";
                else if (progress < 0.5f)
                    statusText.text = "플레이어 데이터 로딩 중...";
                else if (progress < 0.75f)
                    statusText.text = "장비 데이터 로딩 중...";
                else
                    statusText.text = "초기화 완료 중...";
            }
            
            yield return null;
        }
        
        // 초기화 완료
        if (progressBar != null)
            progressBar.value = 1f;
        
        if (progressText != null)
            progressText.text = "100%";
        
        if (statusText != null)
            statusText.text = "준비 완료!";
        
        yield return new WaitForSeconds(0.5f);
        
        loadingPanel.SetActive(false);
    }
}