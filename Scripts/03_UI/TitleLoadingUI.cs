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
    [SerializeField] float minLoadingTime = 2f; // 최소 로딩 시간
    [SerializeField] float fakeProgressStartPoint = 0.7f; // 페이크 진행 시작 지점 (70%)

    [Header("페이크 로딩 설정")]
    [SerializeField] float minStepDelay = 0.05f; // 최소 업데이트 간격
    [SerializeField] float maxStepDelay = 0.2f; // 최대 업데이트 간격
    [SerializeField] float minStepSize = 0.005f; // 최소 증가량 (0.5%)
    [SerializeField] float maxStepSize = 0.03f; // 최대 증가량 (3%)

    [Header("로비 복귀 설정")]
    [SerializeField] float minReturningHomeTime = 0.8f; // "집으로 돌아가는 중..." 최소 표시 시간

    // 첫 실행 여부 추적 (Static으로 게임 세션 동안 유지)
    private static bool isFirstLoad = true;

    void Start()
    {
        if (isFirstLoad)
        {
            // 첫 실행 - 정상적인 초기화 로딩
            isFirstLoad = false;

            if (useSimpleFade)
            {
                StartCoroutine(SimpleLoadingScreen());
            }
            else
            {
                StartCoroutine(ShowLoadingScreen());
            }
        }
        else
        {
            // 로비 복귀 - "집으로 돌아가는 중..." 표시
            StartCoroutine(ReturningHomeScreen());
        }
    }

    // 로비 복귀용 - GameInitializer 완료 대기 + "집으로 돌아가는 중..." 메시지
    IEnumerator ReturningHomeScreen()
    {
        loadingPanel.SetActive(true);
        float startTime = Time.time;

        // 텍스트를 "집으로 돌아가는 중..."으로 변경
        TextMeshProUGUI loadingText = loadingPanel.GetComponentInChildren<TextMeshProUGUI>();
        if (loadingText != null)
        {
            loadingText.text = LocalizationManager.Game.loadingBackToLobby;
        }

        // 슬라이더 초기화
        if (progressBar != null)
        {
            progressBar.value = 0f;
        }

        // GameInitializer 초기화 완료 대기하면서 슬라이더 업데이트
        while (!GameInitializer.IsInitialized)
        {
            // 실제 진행도 표시
            float progress = GameInitializer.InitializationProgress;

            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, progress, Time.deltaTime * 5f);
            }

            if (progressText != null)
            {
                progressText.text = $"{progress * 100:F0}%";
            }

            yield return null;
        }

        // 100% 완료 표시
        if (progressBar != null)
        {
            progressBar.value = 1f;
        }

        if (progressText != null)
        {
            progressText.text = "100%";
        }

        // 최소 표시 시간 보장
        float elapsedTime = Time.time - startTime;
        if (elapsedTime < minReturningHomeTime)
        {
            yield return new WaitForSeconds(minReturningHomeTime - elapsedTime);
        }

        // 페이드 없이 바로 비활성화
        loadingPanel.SetActive(false);
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
        float startTime = Time.time; // 로딩 시작 시간 기록
        bool isRealLoadingComplete = false;
        float realProgress = 0f;
        float displayProgress = 0f;

        // 실제 초기화 대기
        while (!GameInitializer.IsInitialized)
        {
            realProgress = GameInitializer.InitializationProgress;
            float elapsedTime = Time.time - startTime;

            // 실제 로딩이 fakeProgressStartPoint(70%)에 도달했고, 아직 minLoadingTime이 안 지났다면
            if (realProgress >= fakeProgressStartPoint && elapsedTime < minLoadingTime)
            {
                // 페이크 진행: 불규칙하게 증가
                float targetProgress = Mathf.Lerp(fakeProgressStartPoint, 1f, elapsedTime / minLoadingTime);

                // displayProgress가 targetProgress보다 작으면 랜덤하게 증가
                if (displayProgress < targetProgress)
                {
                    float stepSize = Random.Range(minStepSize, maxStepSize);
                    displayProgress = Mathf.Min(displayProgress + stepSize, targetProgress);

                    // 랜덤 지연
                    yield return new WaitForSeconds(Random.Range(minStepDelay, maxStepDelay));
                }

                // 실제 진행도를 넘지 않도록
                displayProgress = Mathf.Min(displayProgress, realProgress);
            }
            else
            {
                // 70% 이전이거나 최소 시간이 지났으면 실제 진행도 표시
                displayProgress = realProgress;
            }

            if (progressBar != null)
                progressBar.value = displayProgress;

            if (progressText != null)
                progressText.text = $"{displayProgress * 100:F0}%";

            // 진행 상태 텍스트
            if (statusText != null)
            {
                if (displayProgress < 0.25f)
                    statusText.text = LocalizationManager.Game.loadingCardData;
                else if (displayProgress < 0.5f)
                    statusText.text = LocalizationManager.Game.loadingPlayerData;
                else if (displayProgress < 0.75f)
                    statusText.text = LocalizationManager.Game.loadingEquipment;
                else
                    statusText.text = LocalizationManager.Game.loadingComplete;
            }

            yield return null;
        }

        isRealLoadingComplete = true;
        float loadingCompleteTime = Time.time - startTime;

        // 실제 로딩이 완료되었지만 최소 시간이 안 지났다면, 페이크 진행 계속
        if (loadingCompleteTime < minLoadingTime)
        {
            while (Time.time - startTime < minLoadingTime && displayProgress < 1f)
            {
                float elapsedTime = Time.time - startTime;
                float targetProgress = Mathf.Lerp(displayProgress, 1f, elapsedTime / minLoadingTime);

                // 불규칙하게 증가
                float stepSize = Random.Range(minStepSize, maxStepSize);
                displayProgress = Mathf.Min(displayProgress + stepSize, targetProgress, 1f);

                if (progressBar != null)
                    progressBar.value = displayProgress;

                if (progressText != null)
                    progressText.text = $"{displayProgress * 100:F0}%";

                // 랜덤 지연
                yield return new WaitForSeconds(Random.Range(minStepDelay, maxStepDelay));
            }
        }

        // 초기화 완료
        if (progressBar != null)
            progressBar.value = 1f;

        if (progressText != null)
            progressText.text = "100%";

        if (statusText != null)
            statusText.text = LocalizationManager.Game.loadingComplete;

        yield return new WaitForSeconds(0.5f);

        loadingPanel.SetActive(false);
    }

    // 게임 종료 시 첫 로드 플래그 리셋 (에디터 테스트 편의성)
    void OnApplicationQuit()
    {
        isFirstLoad = true;
    }
}