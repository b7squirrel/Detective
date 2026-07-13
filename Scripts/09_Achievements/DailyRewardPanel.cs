using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DailyRewardPanel : MonoBehaviour
{
    [Header("보상 데이터")]
    DailyRewardData rewardData;

    [SerializeField] DailyRewardButton[] dailyButtons;
    PlayerDataManager playerDataManager;

    [Header("UI 요소")]
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] GameObject alreadyClaimedMessagePanel;
    [SerializeField] GameObject claimRewardMessagePanel;
    [SerializeField] GameObject closeButton;
    [SerializeField] RectTransform hand;
    Animator anim;

    [Header("보상 이펙트")]
    [SerializeField] private GemCollectFX gemCollectFX;
    RectTransform rewardEffectPos;

    [Header("사운드")]
    [SerializeField] private AudioClip clipPanelUp;
    [SerializeField] private AudioClip clipClaimReward;
    [SerializeField] private AudioClip clipPanelDown; // 패널 닫힐 때 사운드

    // ⭐ 디버그용 플래그
    private bool hasSubscribed = false;
    private bool hasGameInitialized = false;

    // ⭐ 추가: 보상 수령 코루틴 중복 실행 방지 플래그
    // 빠른 연타로 ClaimReward가 여러 번 호출되어도 코루틴이 중복 실행되지 않도록 막음
    // try-finally로 감싸서 코루틴이 어떤 경로(정상 종료/조기 종료/에러)로 빠져나가든 반드시 해제됨
    private bool isClaiming = false;

    void OnEnable()
    {
        Debug.Log("[DailyRewardPanel] OnEnable 시작");

        // rewardData가 없으면 런타임에서 생성
        if (rewardData == null)
        {
            Debug.LogWarning("[DailyRewardPanel] rewardData가 null, 런타임에서 생성");
            rewardData = ScriptableObject.CreateInstance<DailyRewardData>();

            // 7일 보상 데이터 직접 설정
            rewardData.rewards = new DailyRewardItem[7]
            {
                new DailyRewardItem { day = 1, coinReward = 1000, gemReward = 0, isSpecial = false },
                new DailyRewardItem { day = 2, coinReward = 0, gemReward = 10, isSpecial = false },
                new DailyRewardItem { day = 3, coinReward = 4000, gemReward = 0, isSpecial = false },
                new DailyRewardItem { day = 4, coinReward = 0, gemReward = 20, isSpecial = false },
                new DailyRewardItem { day = 5, coinReward = 10000, gemReward = 0, isSpecial = false },
                new DailyRewardItem { day = 6, coinReward = 0, gemReward = 30, isSpecial = false },
                new DailyRewardItem { day = 7, coinReward = 30000, gemReward = 80, isSpecial = true }
            };

            Debug.Log("[DailyRewardPanel] v 런타임 rewardData 생성 완료");
        }

        if (!hasSubscribed)
        {
            GameInitializer.OnGameInitialized += OnGameReady;
            hasSubscribed = true;
            Debug.Log("[DailyRewardPanel] 이벤트 구독 완료");
        }

        // ⭐ 이미 초기화되었다면 바로 UpdateUI
        if (GameInitializer.IsInitialized && !hasGameInitialized)
        {
            Debug.Log("[DailyRewardPanel] GameInitializer 이미 초기화됨, 바로 UpdateUI 호출");
            OnGameReady();
        }

        if (clipPanelUp != null)
            SoundManager.instance?.Play(clipPanelUp);

        // ⭐ 패널이 활성화될 때마다 닫기 버튼 숨김
        if (closeButton != null)
            closeButton.SetActive(false);
    }

    void OnDisable()
    {
        Debug.Log("[DailyRewardPanel] OnDisable");
        GameInitializer.OnGameInitialized -= OnGameReady;
        hasSubscribed = false;
    }

    void Start()
    {
        Debug.Log("[DailyRewardPanel] Start");

        // ⭐ Animator 초기화
        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("[DailyRewardPanel] Animator 컴포넌트가 없습니다!");
        }

        if (gemCollectFX == null)
            gemCollectFX = FindObjectOfType<GemCollectFX>();

        if (closeButton != null)
            closeButton.SetActive(false);
    }

    void OnGameReady()
    {
        Debug.Log("[DailyRewardPanel]  OnGameReady 호출됨!");
        hasGameInitialized = true;

        if (playerDataManager == null)
        {
            playerDataManager = PlayerDataManager.Instance;
            Debug.Log($"[DailyRewardPanel] PlayerDataManager 연결: {playerDataManager != null}");
        }

        Debug.Log($"[DailyRewardPanel] rewardData: {rewardData != null}");
        Debug.Log($"[DailyRewardPanel] dailyButtons: {dailyButtons?.Length ?? 0}개");
        Debug.Log($"[DailyRewardPanel] dayText: {dayText != null}");

        UpdateUI();
    }

    void UpdateUI()
    {
        Debug.Log("[DailyRewardPanel] UpdateUI 시작");

        if (playerDataManager == null)
        {
            Debug.LogError("[DailyRewardPanel] X playerDataManager가 null!");
            return;
        }

        if (rewardData == null)
        {
            Debug.LogError("[DailyRewardPanel] X rewardData가 null!");
            return;
        }

        int currentDay = playerDataManager.GetConsecutiveDays();
        bool hasClaimed = playerDataManager.HasTakenDailyReward();

        Debug.Log($"[DailyRewardPanel] 현재 날짜: {currentDay}일차");
        Debug.Log($"[DailyRewardPanel] 수령 여부: {hasClaimed}");

        // 보상 데이터 가져오기
        DailyRewardItem reward = rewardData.GetReward(currentDay);
        if (reward == null)
        {
            Debug.LogError($"[DailyRewardPanel] X {currentDay}일차 보상 데이터가 null!");
            return;
        }

        Debug.Log($"[DailyRewardPanel] 보상: 코인 {reward.coinReward}, 크리스탈 {reward.gemReward}");

        // 날짜 표시
        if (dayText != null)
        {
            string text = LocalizationManager.Game?.GetDayText(currentDay) ?? $"{currentDay}일차";
            dayText.text = text;
            Debug.Log($"[DailyRewardPanel] dayText 설정: '{text}'");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X dayText가 null!");
        }

        // 날짜 버튼 업데이트
        if (dailyButtons == null || dailyButtons.Length == 0)
        {
            Debug.LogError("[DailyRewardPanel] X dailyButtons가 비어있음!");
            return;
        }

        Debug.Log($"[DailyRewardPanel] 버튼 업데이트 시작 (총 {dailyButtons.Length}개)");

        for (int i = 0; i < dailyButtons.Length; i++)
        {
            if (dailyButtons[i] == null)
            {
                Debug.LogError($"[DailyRewardPanel] X dailyButtons[{i}]가 null!");
                continue;
            }

            DailyRewardItem item = rewardData.GetReward(i + 1);
            int buttonDay = i + 1;

            Debug.Log($"[DailyRewardPanel] 버튼 {buttonDay} 업데이트: 코인 {item.coinReward}, 크리스탈 {item.gemReward}");

            dailyButtons[i].UpdateDailyRewardButton(
                item.coinReward,
                item.gemReward,
                currentDay,
                buttonDay,
                hasClaimed
            );

            // 해당 날짜의 버튼이고 아직 수령하지 않았다면 버튼 이벤트 등록
            if (currentDay == buttonDay && hasClaimed == false)
            {
                Button button = dailyButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(ClaimReward);
                    Debug.Log($"[DailyRewardPanel] v {buttonDay}일차 버튼 클릭 이벤트 등록!");
                }
                else
                {
                    Debug.LogError($"[DailyRewardPanel] X {buttonDay}일차 Button 컴포넌트 없음!");
                }
            }
        }

        // 메시지 패널
        if (alreadyClaimedMessagePanel != null)
        {
            alreadyClaimedMessagePanel.SetActive(hasClaimed);
            Debug.Log($"[DailyRewardPanel] alreadyClaimedMessagePanel: {hasClaimed}");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X alreadyClaimedMessagePanel이 null!");
        }

        if (claimRewardMessagePanel != null)
        {
            claimRewardMessagePanel.SetActive(!hasClaimed);
            Debug.Log($"[DailyRewardPanel] claimRewardMessagePanel: {!hasClaimed}");
        }
        else
        {
            Debug.LogError("[DailyRewardPanel] X claimRewardMessagePanel이 null!");
        }

        // ⭐ hand 위치 설정 (아직 수령하지 않은 경우에만)
        if (hand != null)
        {
            if (!hasClaimed && currentDay >= 1 && currentDay <= dailyButtons.Length)
            {
                // 현재 날짜의 버튼
                DailyRewardButton currentButton = dailyButtons[currentDay - 1];
                RectTransform currentButtonRect = currentButton.GetComponent<RectTransform>();

                if (currentButtonRect != null)
                {
                    // ⭐ hand를 버튼의 직접적인 자식으로 설정
                    hand.SetParent(currentButtonRect, false);

                    // ⭐ 로컬 위치를 0,0,0으로 (부모 버튼의 중앙)
                    hand.anchoredPosition = Vector2.zero;
                    hand.localPosition = Vector3.zero;

                    // ⭐ 버튼보다 앞에 보이도록
                    hand.SetAsLastSibling();

                    hand.gameObject.SetActive(true);
                    Debug.Log($"[DailyRewardPanel] hand를 {currentDay}일차 버튼의 자식으로 설정");
                }
            }
            else
            {
                // 이미 수령했으면 hand 숨김
                hand.gameObject.SetActive(false);
                Debug.Log("[DailyRewardPanel] 보상 수령 완료, hand 숨김");
            }
        }
        Debug.Log("[DailyRewardPanel] v UpdateUI 완료!");
    }

    // ⭐ 수정: 중복 클릭 방지 플래그 체크 후 코루틴 시작
    void ClaimReward()
    {
        // 이미 보상 처리 코루틴이 진행 중이면 무시 (빠른 연타로 인한 중복 지급 방지)
        if (isClaiming)
        {
            Debug.LogWarning("[DailyRewardPanel] 이미 보상 처리 중입니다. 클릭 무시.");
            return;
        }

        isClaiming = true;
        StartCoroutine(ClaimRewardCoroutine());
    }

    // ⭐ 수정: try-finally로 감싸서 코루틴이 어떤 경로로 종료되든 isClaiming이 반드시 해제되도록 함
    IEnumerator ClaimRewardCoroutine()
    {
        try
        {
            Debug.Log("[DailyRewardPanel] ⭐ ClaimReward 호출!");

            // ⭐ 클릭 즉시 hand 숨김
            if (hand != null)
            {
                hand.gameObject.SetActive(false);
                Debug.Log("[DailyRewardPanel] hand 즉시 숨김");
            }

            if (playerDataManager == null || rewardData == null)
            {
                Debug.LogError("[DailyRewardPanel] X ClaimReward: manager가 null!");
                yield break;
            }

            if (playerDataManager.HasTakenDailyReward())
            {
                Debug.LogWarning("[DailyRewardPanel] 이미 오늘 출석 보상을 받았습니다.");
                yield break;
            }

            int currentDay = playerDataManager.GetConsecutiveDays();
            DailyRewardItem reward = rewardData.GetReward(currentDay);

            if (reward == null)
            {
                Debug.LogError("[DailyRewardPanel] X ClaimReward: reward가 null!");
                yield break;
            }

            Debug.Log($"[DailyRewardPanel] {currentDay}일차 보상 수령 시작");

            // ⭐ 1. 현재 버튼 가져오기 및 이펙트 위치 설정
            DailyRewardButton currentButton = dailyButtons[currentDay - 1];
            if (currentButton != null)
            {
                // ⭐ 버튼의 RectTransform을 이펙트 위치로 설정
                RectTransform buttonRect = currentButton.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    rewardEffectPos = buttonRect;
                    Debug.Log($"[DailyRewardPanel] 이펙트 위치를 {currentDay}일차 버튼으로 설정");
                }

                // 포스트잇 떨어지는 애니메이션 재생
                yield return StartCoroutine(currentButton.PlayClaimAnimation());
            }

            // ⭐ 2. 애니메이션 후 사운드 재생
            if (clipClaimReward != null)
                SoundManager.instance?.Play(clipClaimReward);

            // 3. 코인 보상
            if (reward.coinReward > 0)
            {
                int currentCoin = playerDataManager.GetCurrentCoinNumber();
                playerDataManager.SetCoinNumberAsSilent(currentCoin + reward.coinReward);
                Debug.Log($"[DailyRewardPanel] 코인 {reward.coinReward} 지급");

                if (gemCollectFX != null && rewardEffectPos != null)
                    gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.coinReward, false);
            }

            // 4. 크리스탈 보상
            if (reward.gemReward > 0)
            {
                int currentGem = playerDataManager.GetCurrentCristalNumber();
                playerDataManager.SetCristalNumberAsSilent(currentGem + reward.gemReward);
                Debug.Log($"[DailyRewardPanel] 크리스탈 {reward.gemReward} 지급");

                if (gemCollectFX != null && rewardEffectPos != null)
                    gemCollectFX.PlayGemCollectFX(rewardEffectPos, reward.gemReward, true);
            }

            // 5. 보상 수령 기록
            playerDataManager.SetHasTakenDailyReward(true);
            Debug.Log("[DailyRewardPanel] 보상 수령 기록 저장");

            // 6. UI 갱신
            UpdateUI();

            // 7. 론치 패널 뱃지 갱신
            LaunchManager launchManager = FindObjectOfType<LaunchManager>();
            if (launchManager != null)
            {
                launchManager.UpdateDailyRewardBadge();
            }
        }
        finally
        {
            // ⭐ try 블록이 정상 종료되든, yield break로 조기 종료되든 항상 실행됨
            // 단, 아래의 "8. 닫기 버튼 활성화" 대기 코드는 finally보다 먼저 끝나야 하므로
            // 닫기 버튼 로직은 finally 바깥, try 블록 마지막 줄로 옮기지 않고
            // 여기서 isClaiming만 해제한다 (닫기 버튼 활성화는 아래 별도 처리)
            isClaiming = false;
        }

        // ⭐ 8. 잠시 대기 후 닫기 버튼 활성화 (보상 이펙트를 볼 시간 확보)
        // finally 이후에 실행되어도 안전함: isClaiming은 이미 해제되어 다음 클릭이 가능한 상태이고,
        // 이 시점부터는 playerDataManager.HasTakenDailyReward()가 true이므로 중복 지급되지 않음
        yield return new WaitForSeconds(0.5f);

        if (closeButton != null)
            closeButton.SetActive(true);

        Debug.Log("[DailyRewardPanel] ✓ 출석 보상 수령 완료!");
    }

    // ⭐ 패널 닫기 (버튼에 연결)
    public void ClosePanel()
    {
        // ⭐ 이미 비활성이면 코루틴 없이 바로 종료
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(ClosePanelCo());
    }

    IEnumerator ClosePanelCo()
    {
        Debug.Log("[DailyRewardPanel] 패널 닫기 시작");

        // ⭐ 닫기 버튼 비활성화 (중복 클릭 방지)
        if (closeButton != null)
            closeButton.SetActive(false);

        // ⭐ 사운드 재생 (선택사항)
        if (clipPanelDown != null)
            SoundManager.instance?.Play(clipPanelDown);

        // ⭐ Down 애니메이션 트리거
        if (anim != null)
        {
            anim.SetTrigger("Down");

            // 애니메이션 길이 가져오기
            float animLength = GetAnimationLength("Down");

            Debug.Log($"[DailyRewardPanel] Down 애니메이션 재생 ({animLength}초)");

            // 애니메이션이 끝날 때까지 대기
            yield return new WaitForSeconds(animLength);
        }
        else
        {
            Debug.LogWarning("[DailyRewardPanel] Animator가 없어서 바로 비활성화");
        }

        // ⭐ 패널 비활성화
        gameObject.SetActive(false);

        Debug.Log("[DailyRewardPanel] ✓ 패널 닫기 완료");
    }

    // ⭐ 애니메이션 길이 가져오기 헬퍼 메서드
    private float GetAnimationLength(string triggerName)
    {
        if (anim == null) return 0f;

        // 현재 애니메이터의 애니메이션 클립들을 검색
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            // 트리거 이름과 일치하는 클립 찾기 (대소문자 무시)
            if (clip.name.ToLower().Contains(triggerName.ToLower()))
            {
                return clip.length;
            }
        }

        // 찾지 못했을 경우 기본값 (1초)
        Debug.LogWarning($"[DailyRewardPanel] '{triggerName}' 애니메이션을 찾지 못했습니다. 기본값 1초 사용");
        return 1f;
    }

    private string GetDayKey(int day)
    {
        return $"day_{day}";
    }
}