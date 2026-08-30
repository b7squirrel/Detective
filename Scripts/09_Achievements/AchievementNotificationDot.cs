using System.Collections;
using UnityEngine;

/// <summary>
/// 완료했지만 아직 보상을 받지 않은 업적이 있으면 지정된 빨간점(dotVisual)을 켜고,
/// 없으면 끄는 컴포넌트. 로비의 업적 버튼, 패널 안의 영구/일일/주간 탭 버튼 등
/// "항상 활성 상태인 오브젝트"에 붙여서 재사용한다.
/// 주의: dotVisual 자체에는 이 스크립트를 붙이지 말 것 (SetActive(false) 시
/// 이벤트 구독이 끊겨 다시 켜지지 못함)
/// </summary>
public class AchievementNotificationDot : MonoBehaviour
{
    [Tooltip("이 빨간점이 감시할 범위 (버튼 하나=All, 탭별로는 해당 탭 범위)")]
    [SerializeField] private AchievementManager.AchievementScope scope = AchievementManager.AchievementScope.All;

    [Tooltip("실제로 켜고 끌 빨간점 오브젝트 (Animator가 붙어있는 그 오브젝트)")]
    [SerializeField] private GameObject dotVisual;

    private Coroutine waitForManagerCo;

    private void OnEnable()
    {
        if (AchievementManager.Instance != null)
        {
            Subscribe();
            Refresh();
        }
        else
        {
            // 씬 시작 시점에 AchievementManager 초기화보다 먼저 켜질 수 있으므로 대기
            waitForManagerCo = StartCoroutine(WaitForManagerThenSubscribe());
        }
    }

    private void OnDisable()
    {
        if (waitForManagerCo != null)
        {
            StopCoroutine(waitForManagerCo);
            waitForManagerCo = null;
        }
        Unsubscribe();
    }

    private IEnumerator WaitForManagerThenSubscribe()
    {
        yield return new WaitUntil(() => AchievementManager.Instance != null);
        Subscribe();
        Refresh();
        waitForManagerCo = null;
    }

    private void Subscribe()
    {
        AchievementManager.Instance.OnAnyProgressChanged += OnAchievementChanged;
        AchievementManager.Instance.OnAnyCompleted += OnAchievementChanged;
        AchievementManager.Instance.OnAnyRewarded += OnAchievementChanged;
    }

    private void Unsubscribe()
    {
        if (AchievementManager.Instance == null) return;
        AchievementManager.Instance.OnAnyProgressChanged -= OnAchievementChanged;
        AchievementManager.Instance.OnAnyCompleted -= OnAchievementChanged;
        AchievementManager.Instance.OnAnyRewarded -= OnAchievementChanged;
    }

    private void OnAchievementChanged(RuntimeAchievement ra)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (dotVisual == null) return;
        bool hasUnclaimed = AchievementManager.Instance != null
            && AchievementManager.Instance.HasUnclaimedCompleted(scope);
        dotVisual.SetActive(hasUnclaimed);
    }
}