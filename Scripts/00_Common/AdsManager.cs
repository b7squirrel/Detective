using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Ump.Api;

public class AdsManager : SingletonBehaviour<AdsManager>
{
    protected override void Init()
    {
        base.Init();
        Logger.Log("[AdsManager] Init() 시작");  // ⭐ 추가

        // ★ 테스트 기기 등록 - 반드시 InitAdsService()보다 먼저!
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TestDeviceIds = new List<string> {
                "BE85D1491E3B0ACC8E8996B7C3BC6C0F",
                "B40200ED0A5B5557E8BE2910D0A87FB2"
                }
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        Logger.Log("[AdsManager] InitConsent 호출 직전");  // ⭐ 추가

        // ★ 동의 절차 먼저 진행 후, 완료되면 광고 SDK 초기화
        InitConsent(() =>
        {
            Logger.Log("[AdsManager] InitConsent 콜백 도착 - 광고 초기화 시작");  // ⭐ 추가
            InitAdsService();
            InitRewardedAds();
        });
    }

    private void InitAdsService()
    {
        MobileAds.Initialize(initStatus =>
        {
            Logger.Log("[AdsManager] Google Ads 초기화 시작...");

            var isInitSuccess = true;
            var statusMap = initStatus.getAdapterStatusMap();
            foreach (var status in statusMap)
            {
                var className = status.Key;
                var adapterStatus = status.Value;
                Logger.Log($"Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if (adapterStatus.InitializationState != AdapterState.Ready)
                {
                    isInitSuccess = false;
                }
            }

            if (isInitSuccess)
                Logger.Log($"Google Ads initialization successful.");
            else
                Logger.LogError($"Google Ads initialization failed.");
        });
    }

    #region BannerAds
    private BannerView m_TopBannerView;
    private string m_TopBannerAdId = string.Empty;
    private const string AOS_BANNER_TEST_AD_ID = "ca-app-pub-3940256099942544/6300978111";
    private const string IOS_BANNER_TEST_AD_ID = "ca-app-pub-3940256099942544/2934735716";
    private const string AOS_TOP_BANNER_AD_ID = "";
    private const string IOS_TOP_BANNER_AD_ID = "";

    private void InitBannerAds()
    {
        SetTopBannerAdId();
    }

    private void SetTopBannerAdId()
    {
#if DEV_VER
#if UNITY_ANDROID
        m_TopBannerAdId = AOS_BANNER_TEST_AD_ID;
#elif UNITY_IOS
        m_TopBannerAdId = IOS_BANNER_TEST_AD_ID;
#endif
#else
#if UNITY_ANDROID
        m_TopBannerAdId = AOS_TOP_BANNER_AD_ID;
#elif UNITY_IOS
        m_TopBannerAdId = IOS_TOP_BANNER_AD_ID;
#endif
#endif
    }

    public void EnableTopBannerAd(bool value)
    {
        Logger.Log($"EnableTopBannerAd value : {value}");

        if (value)
        {
            if (m_TopBannerView == null)
            {
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                m_TopBannerView = new BannerView(m_TopBannerAdId, adaptiveSize, AdPosition.Top);

                AdRequest request = new AdRequest();
                m_TopBannerView.LoadAd(request);
                ListenToTopBannerAdEvents();
            }
            else
            {
                m_TopBannerView.Show();
            }
        }
        else
        {
            if (m_TopBannerView != null)
            {
                m_TopBannerView.Hide();
            }
        }
    }

    private void ListenToTopBannerAdEvents()
    {
        if (m_TopBannerView == null)
        {
            Logger.LogError("m_TopBannerView is null.");
            return;
        }

        m_TopBannerView.OnBannerAdLoaded += () =>
            Logger.Log($"m_TopBannerView loaded an ad with response : {m_TopBannerView.GetResponseInfo()}");

        m_TopBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            Logger.LogError($"m_TopBannerView failed to load an ad with error : {error}");

        m_TopBannerView.OnAdPaid += (AdValue adValue) =>
            Logger.Log($"m_TopBannerView paid {adValue.Value}{adValue.CurrencyCode}.");

        m_TopBannerView.OnAdImpressionRecorded += () =>
            Logger.Log($"m_TopBannerView recorded an impression.");

        m_TopBannerView.OnAdClicked += () =>
            Logger.Log($"m_TopBannerView was clicked.");

        m_TopBannerView.OnAdFullScreenContentOpened += () =>
            Logger.Log($"m_TopBannerView full screen content opened.");

        m_TopBannerView.OnAdFullScreenContentClosed += () =>
            Logger.Log($"m_TopBannerView full screen content closed.");
    }
    #endregion

    #region InterstitialAd
    private InterstitialAd m_StageClearInterstitial;
    private string m_StageClearInterstitialAdId = string.Empty;
    private const string AOS_INTERSTITIAL_TEST_AD_ID = "ca-app-pub-3940256099942544/1033173712";
    private const string IOS_INTERSTITIAL_TEST_AD_ID = "ca-app-pub-3940256099942544/4411468910";
    private const string AOS_STAGE_CLEAR_INTERSTITIAL_AD_ID = "";
    private const string IOS_STAGE_CLEAR_INTERSTITIAL_AD_ID = "";
    private Action m_OnFinishStageClearInterstitialAd = null;

    private void InitInterstitialAds()
    {
        SetStageClearInterstitialAdId();
        LoadStageClearInterstitialAd();
    }

    private void SetStageClearInterstitialAdId()
    {
#if DEV_VER
#if UNITY_ANDROID
        m_StageClearInterstitialAdId = AOS_INTERSTITIAL_TEST_AD_ID;
#elif UNITY_IOS
        m_StageClearInterstitialAdId = IOS_INTERSTITIAL_TEST_AD_ID;
#endif
#else
#if UNITY_ANDROID
        m_StageClearInterstitialAdId = AOS_STAGE_CLEAR_INTERSTITIAL_AD_ID;
#elif UNITY_IOS
        m_StageClearInterstitialAdId = IOS_STAGE_CLEAR_INTERSTITIAL_AD_ID;
#endif
#endif
    }

    private void LoadStageClearInterstitialAd()
    {
        var adRequest = new AdRequest();

        InterstitialAd.Load(m_StageClearInterstitialAdId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Logger.LogError($"Interstitial ad failed to load. Error: {error}");
                    FirebaseManager.LogEvent("ad_load_fail", "ad_type", "interstitial"); // ⭐ 추가
                    return;
                }

                Logger.Log($"Interstitial ad loaded successfully. Response: {ad.GetResponseInfo()}");
                m_StageClearInterstitial = ad;
                FirebaseManager.LogEvent("ad_load_success", "ad_type", "interstitial"); // ⭐ 추가
                ListenToStageClearInterstitialAdEvents();
            });
    }

    private void ListenToStageClearInterstitialAdEvents()
    {
        if (m_StageClearInterstitial == null)
        {
            Logger.LogError($"m_StageClearInterstitial is null");
            return;
        }

        m_StageClearInterstitial.OnAdPaid += (AdValue adValue) =>
            Logger.Log($"m_StageClearInterstitial ad paid {adValue.Value}{adValue.CurrencyCode}.");

        m_StageClearInterstitial.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad recorded an impression.");
            FirebaseManager.LogEvent("ad_impression_recorded", "placement", "stage_clear"); // ⭐ 추가
        };

        m_StageClearInterstitial.OnAdClicked += () =>
            Logger.Log($"m_StageClearInterstitial ad was clicked.");

        m_StageClearInterstitial.OnAdFullScreenContentOpened += () =>
            Logger.Log($"m_StageClearInterstitial ad full screen content opened.");

        m_StageClearInterstitial.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad full screen content closed.");
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };

        m_StageClearInterstitial.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"m_StageClearInterstitial ad failed to open full screen content. Error: {error}");
            FirebaseManager.LogEvent("ad_show_fail_content_error", "placement", "stage_clear"); // ⭐ 추가
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };
    }

    public void ShowStageClearInterstitialAd(Action onFinishStageClearInterstitialAd = null)
    {
        if (m_StageClearInterstitial != null && m_StageClearInterstitial.CanShowAd())
        {
            Logger.Log($"Show stage clear interstitial ad.");
            FirebaseManager.LogEvent("ad_show_attempt", "placement", "stage_clear"); // ⭐ 추가
            m_StageClearInterstitial.Show();
            m_OnFinishStageClearInterstitialAd = onFinishStageClearInterstitialAd;
        }
        else
        {
            Logger.LogError($"Stage clear interstitial ad is not ready yet.");
            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", "stage_clear"); // ⭐ 추가
        }
    }
    #endregion

    #region RewardedAd
    public static bool IsRewardedAdReady { get; private set; } = false;
    public static event Action<bool> OnRewardedAdReadyChanged; // ⭐ 추가: 상태 변경 알림

    private RewardedAd m_DailyFreeGemRewardedAd;
    private string m_DailyFreeGemRewardedAdId = string.Empty;
    private const string AOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/5224354917";
    private const string IOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/1712485313";
    private const string AOS_DAILY_FREE_GEM_REWARDED_AD_ID = "ca-app-pub-2314769566037824/2782705229";
    private const string IOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";

    // ★ 광고창이 완전히 닫힐 때 호출되는 콜백
    private Action m_OnDailyFreeGemRewardedAdClosed = null;

    // ⭐ 추가: 현재 진행 중인 리워드 광고가 어느 위치(부활/상자 등)에서 시작됐는지 기억해서
    //         OnAdImpressionRecorded 콜백에서도 placement를 함께 로깅하기 위함
    private string m_CurrentRewardedAdPlacement = "unknown";

    // ⭐ 추가: 로드 실패 시 재시도 관리
    private Coroutine m_RewardedAdRetryCoroutine;
    private int m_RewardedAdRetryCount = 0;
    private const int REWARDED_AD_MAX_RETRY = 5;
    private const float REWARDED_AD_BASE_RETRY_DELAY = 2f;

    private void InitRewardedAds()
    {
        SetDailyFreeGemRewardedAdId();
        LoadDailyFreeGemRewardedAd();
    }

    // ⭐ 추가: IsRewardedAdReady 값을 바꾸는 유일한 통로. 값이 실제로 달라질 때만 이벤트 발행.
    private static void SetRewardedAdReady(bool value)
    {
        if (IsRewardedAdReady == value) return;
        IsRewardedAdReady = value;
        OnRewardedAdReadyChanged?.Invoke(value);
    }

    private void SetDailyFreeGemRewardedAdId()
    {
#if DEV_VER
#if UNITY_ANDROID
        m_DailyFreeGemRewardedAdId = AOS_REWARDED_AD_TEST_AD_ID;
#elif UNITY_IOS
        m_DailyFreeGemRewardedAdId = IOS_REWARDED_AD_TEST_AD_ID;
#endif
#else
#if UNITY_ANDROID
        m_DailyFreeGemRewardedAdId = AOS_DAILY_FREE_GEM_REWARDED_AD_ID;
#elif UNITY_IOS
        m_DailyFreeGemRewardedAdId = IOS_DAILY_FREE_GEM_REWARDED_AD_ID;
#endif
#endif
    }

    private void LoadDailyFreeGemRewardedAd()
    {
        Logger.Log("[AdsManager] 보상형 광고 로드 시작...");
        var adRequest = new AdRequest();

        RewardedAd.Load(m_DailyFreeGemRewardedAdId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Logger.LogError($"[AdsManager] 보상형 광고 로드 실패. Error: {error}");
                    SetRewardedAdReady(false); // ⭐ 변경
                    FirebaseManager.LogEvent("ad_load_fail", "ad_type", "rewarded");
                    ScheduleRewardedAdRetry(); // ⭐ 추가: 실패 시 자동 재시도 예약
                    return;
                }

                Logger.Log($"[AdsManager] 보상형 광고 로드 성공! Response: {ad.GetResponseInfo()}");
                m_DailyFreeGemRewardedAd = ad;
                SetRewardedAdReady(true); // ⭐ 변경
                m_RewardedAdRetryCount = 0; // ⭐ 추가: 성공했으니 재시도 카운트 초기화
                FirebaseManager.LogEvent("ad_load_success", "ad_type", "rewarded");
                ListenToDailyFreeGemRewardedAdEvents();
            });
    }

    // ⭐ 추가: 지수 백오프로 재로드 예약
    private void ScheduleRewardedAdRetry()
    {
        if (m_RewardedAdRetryCoroutine != null)
            return; // 이미 예약된 재시도가 있으면 중복 예약 방지

        if (m_RewardedAdRetryCount >= REWARDED_AD_MAX_RETRY)
        {
            Logger.LogError($"[AdsManager] 보상형 광고 재시도 {REWARDED_AD_MAX_RETRY}회 모두 실패. 재시도를 중단합니다.");
            FirebaseManager.LogEvent("ad_retry_exhausted", "ad_type", "rewarded"); // ⭐ 추가: 분석용
            return;
        }

        float delay = REWARDED_AD_BASE_RETRY_DELAY * Mathf.Pow(2, m_RewardedAdRetryCount);
        m_RewardedAdRetryCount++;

        Logger.Log($"[AdsManager] {delay}초 후 보상형 광고 재로드 시도 ({m_RewardedAdRetryCount}/{REWARDED_AD_MAX_RETRY})");
        m_RewardedAdRetryCoroutine = StartCoroutine(RewardedAdRetryCo(delay));
    }

    private IEnumerator RewardedAdRetryCo(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        m_RewardedAdRetryCoroutine = null;
        LoadDailyFreeGemRewardedAd();
    }

    private void ListenToDailyFreeGemRewardedAdEvents()
    {
        if (m_DailyFreeGemRewardedAd == null)
        {
            Logger.LogError("m_DailyFreeGemRewardedAd is null.");
            return;
        }

        m_DailyFreeGemRewardedAd.OnAdPaid += (AdValue adValue) =>
            Logger.Log($"m_DailyFreeGemRewardedAd paid {adValue.Value}{adValue.CurrencyCode}.");

        // ⭐ 3단계: 실제 노출 기록 — Show() 호출 성공 여부와 무관하게, 화면에 실제로 떴을 때만 발생
        m_DailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd recorded an impression.");
            FirebaseManager.LogEvent("ad_impression_recorded", "placement", m_CurrentRewardedAdPlacement); // ⭐ 추가
        };

        m_DailyFreeGemRewardedAd.OnAdClicked += () =>
            Logger.Log($"m_DailyFreeGemRewardedAd was clicked.");

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content opened.");

        // ★ 광고창이 완전히 닫힌 시점 — 여기서 onClosed 콜백 호출
        m_DailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content closed.");
            SetRewardedAdReady(false); // ⭐ 변경
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"m_DailyFreeGemRewardedAd failed to open full screen content with error: {error}");
            SetRewardedAdReady(false); // ⭐ 변경
            FirebaseManager.LogEvent("ad_show_fail_content_error", "placement", m_CurrentRewardedAdPlacement); // ⭐ 추가
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };
    }

    // ★ 부활 전용 — 업적 카운트 없음
    // ★ onRewarded: 리워드 지급 시점 (광고 닫기 전)
    // ★ onClosed:   광고창이 완전히 닫힌 시점 ← UnPause, 부활 등은 여기서
    // ⭐ placement: 이 광고가 어느 화면/기능에서 호출됐는지 (예: "revival", "daily_free_gem")
    public void ShowDailyFreeGemRewardedAd(Action onRewarded = null, Action onClosed = null, string placement = "daily_free_gem")
    {
        Logger.Log($"[AdsManager] Show DailyFreeGemRewardedAd 호출");
        Logger.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
            m_CurrentRewardedAdPlacement = placement; // ⭐ 추가
            FirebaseManager.LogEvent("ad_show_attempt", "placement", placement); // ⭐ 추가

            m_OnDailyFreeGemRewardedAdClosed = onClosed;
            m_DailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Logger.Log("Rewarded DailyFreeGem");
                onRewarded?.Invoke();
                // ⭐ 업적 카운트 없음 — 부활에도 사용되므로
            });
        }
        else
        {
            Logger.LogError($"m_DailyFreeGemRewardedAd is not ready yet.");
            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", placement); // ⭐ 추가
            ScheduleRewardedAdRetry(); // ⭐ 추가
        }
    }

    // ⭐ 상자 전용 보상형 광고 — AD_DRAW 업적 카운트 포함
    // ⭐ placement: 어느 상자/기능에서 호출됐는지 구분하고 싶으면 인자로 넘겨도 됨 (기본값 "box")
    public void ShowBoxRewardedAd(Action onRewarded = null, Action onClosed = null, string placement = "box")
    {
        Logger.Log($"[AdsManager] ShowBoxRewardedAd 호출");
        Logger.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
            m_CurrentRewardedAdPlacement = placement; // ⭐ 추가
            FirebaseManager.LogEvent("ad_show_attempt", "placement", placement); // ⭐ 추가

            m_OnDailyFreeGemRewardedAdClosed = onClosed;
            m_DailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Logger.Log("Rewarded BoxAd");
                onRewarded?.Invoke();

                // ⭐ 상자 광고만 업적 카운트
                if (AchievementManager.Instance != null)
                    AchievementManager.Instance.AddProgress(AchievementType.AD_DRAW);
            });
        }
        else
        {
            Logger.LogError($"[AdsManager] 광고가 준비되지 않았습니다.");
            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", placement); // ⭐ 추가
            ScheduleRewardedAdRetry(); // ⭐ 추가
        }
    }
    #endregion

    protected override void Dispose()
    {
        if (m_RewardedAdRetryCoroutine != null) // ⭐ 추가
        {
            StopCoroutine(m_RewardedAdRetryCoroutine);
            m_RewardedAdRetryCoroutine = null;
        }

        if (m_TopBannerView != null)
        {
            m_TopBannerView.Destroy();
            m_TopBannerView = null;
        }

        if (m_StageClearInterstitial != null)
        {
            m_StageClearInterstitial.Destroy();
            m_StageClearInterstitial = null;
        }

        if (m_DailyFreeGemRewardedAd != null)
        {
            m_DailyFreeGemRewardedAd.Destroy();
            m_DailyFreeGemRewardedAd = null;
        }

        base.Dispose();
    }

    #region ConsentManagement
    public void InitConsent(Action onConsentReady)
    {
        Logger.Log("[AdsManager] ConsentInformation.Update 호출 시작");  // ⭐ 추가
        var request = new ConsentRequestParameters();
        // 필요하다면 테스트 기기용 디버그 설정 추가 가능

        ConsentInformation.Update(request, (FormError updateError) =>
        {
            Logger.Log($"[AdsManager] ConsentInformation.Update 콜백 도착. error={updateError}");  // ⭐ 추가
            if (updateError != null)
            {
                Logger.LogError($"[AdsManager] 동의 정보 업데이트 실패: {updateError}");
                onConsentReady?.Invoke(); // 실패해도 게임 진행은 막지 않음
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                Logger.Log($"[AdsManager] LoadAndShowConsentFormIfRequired 콜백 도착. error={formError}");  // ⭐ 추가
                if (formError != null)
                {
                    Logger.LogError($"[AdsManager] 동의 폼 표시 실패: {formError}");
                }
                else
                {
                    Logger.Log("[AdsManager] 동의 절차 완료");
                }
                onConsentReady?.Invoke();
            });
        });
    }

    // 설정 화면의 "개인정보 보호 설정" 버튼에서 호출할 함수
    public void ShowPrivacyOptionsForm()
    {
        ConsentForm.ShowPrivacyOptionsForm((FormError formError) =>
        {
            if (formError != null)
            {
                Logger.LogError($"[AdsManager] 개인정보 설정 폼 표시 실패: {formError}");
            }
            else
            {
                Logger.Log("[AdsManager] 개인정보 설정 폼 닫힘");
            }
        });
    }

    // ★ 추가: 개인정보 설정 폼이 현재 필요한 상태인지 확인 (EEA/영국/스위스 사용자에게만 true)
    public bool IsPrivacyOptionsRequired()
    {
        return ConsentInformation.PrivacyOptionsRequirementStatus
            == PrivacyOptionsRequirementStatus.Required;
    }
    #endregion
}