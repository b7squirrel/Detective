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
        Debug.Log("[AdsManager] Init() 시작");

        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TestDeviceIds = new List<string> {
                "BE85D1491E3B0ACC8E8996B7C3BC6C0F",
                "B40200ED0A5B5557E8BE2910D0A87FB2"
                }
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        Debug.Log("[AdsManager] InitConsent 호출 직전");
        Debug.Log($"[AdsManager] 네트워크 연결 상태: {Application.internetReachability}"); // ⭐ 추가: 진단용

        InitConsent(() =>
        {
            Debug.Log("[AdsManager] InitConsent 콜백 도착 - 광고 초기화 시작");
            InitAdsService();
            InitRewardedAds();
        });
    }

    private void InitAdsService()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("[AdsManager] Google Ads 초기화 시작...");

            var isInitSuccess = true;
            var statusMap = initStatus.getAdapterStatusMap();
            foreach (var status in statusMap)
            {
                var className = status.Key;
                var adapterStatus = status.Value;
                Debug.Log($"[AdsManager] Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if (adapterStatus.InitializationState != AdapterState.Ready)
                {
                    isInitSuccess = false;
                }
            }

            if (isInitSuccess)
                Debug.Log($"[AdsManager] Google Ads initialization successful.");
            else
                Debug.LogError($"[AdsManager] Google Ads initialization failed. (일부 어댑터가 Ready 상태가 아님)");
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
        Debug.Log($"[AdsManager] EnableTopBannerAd value : {value}");

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
            Debug.LogError("[AdsManager] m_TopBannerView is null.");
            return;
        }

        m_TopBannerView.OnBannerAdLoaded += () =>
            Debug.Log($"[AdsManager] m_TopBannerView loaded an ad with response : {m_TopBannerView.GetResponseInfo()}");

        m_TopBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            Debug.LogError($"[AdsManager] m_TopBannerView failed to load an ad with error : {error}");

        m_TopBannerView.OnAdPaid += (AdValue adValue) =>
            Debug.Log($"[AdsManager] m_TopBannerView paid {adValue.Value}{adValue.CurrencyCode}.");

        m_TopBannerView.OnAdImpressionRecorded += () =>
            Debug.Log($"[AdsManager] m_TopBannerView recorded an impression.");

        m_TopBannerView.OnAdClicked += () =>
            Debug.Log($"[AdsManager] m_TopBannerView was clicked.");

        m_TopBannerView.OnAdFullScreenContentOpened += () =>
            Debug.Log($"[AdsManager] m_TopBannerView full screen content opened.");

        m_TopBannerView.OnAdFullScreenContentClosed += () =>
            Debug.Log($"[AdsManager] m_TopBannerView full screen content closed.");
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
                    Debug.LogError($"[AdsManager] Interstitial ad failed to load. Error: {error}");
                    FirebaseManager.LogEvent("ad_load_fail", "ad_type", "interstitial");
                    return;
                }

                Debug.Log($"[AdsManager] Interstitial ad loaded successfully. Response: {ad.GetResponseInfo()}");
                m_StageClearInterstitial = ad;
                FirebaseManager.LogEvent("ad_load_success", "ad_type", "interstitial");
                ListenToStageClearInterstitialAdEvents();
            });
    }

    private void ListenToStageClearInterstitialAdEvents()
    {
        if (m_StageClearInterstitial == null)
        {
            Debug.LogError($"[AdsManager] m_StageClearInterstitial is null");
            return;
        }

        m_StageClearInterstitial.OnAdPaid += (AdValue adValue) =>
            Debug.Log($"[AdsManager] m_StageClearInterstitial ad paid {adValue.Value}{adValue.CurrencyCode}.");

        m_StageClearInterstitial.OnAdImpressionRecorded += () =>
        {
            Debug.Log($"[AdsManager] m_StageClearInterstitial ad recorded an impression.");
            FirebaseManager.LogEvent("ad_impression_recorded", "placement", "stage_clear");
        };

        m_StageClearInterstitial.OnAdClicked += () =>
            Debug.Log($"[AdsManager] m_StageClearInterstitial ad was clicked.");

        m_StageClearInterstitial.OnAdFullScreenContentOpened += () =>
            Debug.Log($"[AdsManager] m_StageClearInterstitial ad full screen content opened.");

        m_StageClearInterstitial.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log($"[AdsManager] m_StageClearInterstitial ad full screen content closed.");
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };

        m_StageClearInterstitial.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"[AdsManager] m_StageClearInterstitial ad failed to open full screen content. Error: {error}");
            FirebaseManager.LogEvent("ad_show_fail_content_error", "placement", "stage_clear");
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };
    }

    public void ShowStageClearInterstitialAd(Action onFinishStageClearInterstitialAd = null)
    {
        if (m_StageClearInterstitial != null && m_StageClearInterstitial.CanShowAd())
        {
            Debug.Log($"[AdsManager] Show stage clear interstitial ad.");
            FirebaseManager.LogEvent("ad_show_attempt", "placement", "stage_clear");
            m_StageClearInterstitial.Show();
            m_OnFinishStageClearInterstitialAd = onFinishStageClearInterstitialAd;
        }
        else
        {
            Debug.LogError($"[AdsManager] Stage clear interstitial ad is not ready yet.");
            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", "stage_clear");
        }
    }
    #endregion

    #region RewardedAd
    public static bool IsRewardedAdReady { get; private set; } = false;
    public static event Action<bool> OnRewardedAdReadyChanged; // ⭐ 광고 준비 상태 변경 알림

    private RewardedAd m_DailyFreeGemRewardedAd;
    private string m_DailyFreeGemRewardedAdId = string.Empty;
    private const string AOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/5224354917";
    private const string IOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/1712485313";
    private const string AOS_DAILY_FREE_GEM_REWARDED_AD_ID = "ca-app-pub-2314769566037824/2782705229";
    private const string IOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";

    private Action m_OnDailyFreeGemRewardedAdClosed = null;
    private string m_CurrentRewardedAdPlacement = "unknown";

    // ⭐ 재시도/중복 방지/최소 간격 관리용 필드
    private Coroutine m_RewardedAdRetryCoroutine;
    private int m_RewardedAdRetryCount = 0;
    private const int REWARDED_AD_MAX_RETRY = 5;
    private const float REWARDED_AD_BASE_RETRY_DELAY = 2f;
    private bool m_IsRewardedAdLoading = false;
    private float m_LastRewardedAdLoadAttemptTime = -999f;
    private const float REWARDED_AD_MIN_LOAD_INTERVAL = 5f;

    private void InitRewardedAds()
    {
        SetDailyFreeGemRewardedAdId();
        LoadDailyFreeGemRewardedAd();
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

    // ⭐ 추가: IsRewardedAdReady를 바꾸는 유일한 통로. 값이 실제로 바뀔 때만 이벤트 발행.
    private static void SetRewardedAdReady(bool value)
    {
        if (IsRewardedAdReady == value) return;
        IsRewardedAdReady = value;
        Debug.Log($"[AdsManager] IsRewardedAdReady 변경 → {value}");
        OnRewardedAdReadyChanged?.Invoke(value);
    }

    private void LoadDailyFreeGemRewardedAd()
    {
        if (m_IsRewardedAdLoading)
        {
            Debug.Log("[AdsManager] 이미 보상형 광고 로드 진행 중 - 중복 요청 무시");
            return;
        }

        Debug.Log("[AdsManager] 보상형 광고 로드 시작...");
        Debug.Log($"[AdsManager] [진단] 네트워크 연결 상태: {Application.internetReachability}, 현재 재시도 횟수: {m_RewardedAdRetryCount}/{REWARDED_AD_MAX_RETRY}"); // ⭐ 추가

        m_IsRewardedAdLoading = true;
        m_LastRewardedAdLoadAttemptTime = Time.unscaledTime;
        var adRequest = new AdRequest();

        RewardedAd.Load(m_DailyFreeGemRewardedAdId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                m_IsRewardedAdLoading = false;

                if (error != null || ad == null)
                {
                    // ⭐ 추가: 실패 원인을 최대한 자세히 로그캣에 남김
                    Debug.LogError($"[AdsManager] 보상형 광고 로드 실패.");
                    if (error != null)
                    {
                        Debug.LogError($"[AdsManager] [진단] Code: {error.GetCode()}, Domain: {error.GetDomain()}, Message: {error.GetMessage()}");
                        Debug.LogError($"[AdsManager] [진단] 상세: {error}");
                    }
                    else
                    {
                        Debug.LogError("[AdsManager] [진단] error는 null이지만 ad 객체도 null - 원인 불명확");
                    }
                    Debug.LogError($"[AdsManager] [진단] 네트워크 상태: {Application.internetReachability}");

                    SetRewardedAdReady(false);
                    FirebaseManager.LogEvent("ad_load_fail", "ad_type", "rewarded");
                    ScheduleRewardedAdRetry();
                    return;
                }

                Debug.Log($"[AdsManager] 보상형 광고 로드 성공! Response: {ad.GetResponseInfo()}");
                m_DailyFreeGemRewardedAd = ad;
                SetRewardedAdReady(true);
                m_RewardedAdRetryCount = 0;
                FirebaseManager.LogEvent("ad_load_success", "ad_type", "rewarded");
                ListenToDailyFreeGemRewardedAdEvents();
            });
    }

    // ⭐ 추가: 백오프 재시도 예약
    private void ScheduleRewardedAdRetry()
    {
        if (m_RewardedAdRetryCoroutine != null)
        {
            Debug.Log("[AdsManager] 이미 예약된 재시도가 있어 중복 예약 생략");
            return;
        }

        if (m_RewardedAdRetryCount >= REWARDED_AD_MAX_RETRY)
        {
            Debug.LogError($"[AdsManager] 보상형 광고 재시도 {REWARDED_AD_MAX_RETRY}회 모두 실패. 재시도를 중단합니다.");
            FirebaseManager.LogEvent("ad_retry_exhausted", "ad_type", "rewarded");
            return;
        }

        float delay = REWARDED_AD_BASE_RETRY_DELAY * Mathf.Pow(2, m_RewardedAdRetryCount);
        m_RewardedAdRetryCount++;

        Debug.Log($"[AdsManager] {delay}초 후 보상형 광고 재로드 시도 ({m_RewardedAdRetryCount}/{REWARDED_AD_MAX_RETRY})");
        m_RewardedAdRetryCoroutine = StartCoroutine(RewardedAdRetryCo(delay));
    }

    private IEnumerator RewardedAdRetryCo(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        m_RewardedAdRetryCoroutine = null;
        LoadDailyFreeGemRewardedAd();
    }

    // ⭐ 추가: 공통 즉시 재로드 로직. resetRetryCountIfExhausted=true면 5회 소진 상태여도 새 기회로 취급해 초기화.
    private void AttemptRewardedAdReload(bool resetRetryCountIfExhausted)
    {
        if (m_IsRewardedAdLoading)
        {
            Debug.Log("[AdsManager] 이미 로드 중이므로 즉시 재시도 생략");
            return;
        }

        float elapsed = Time.unscaledTime - m_LastRewardedAdLoadAttemptTime;
        if (elapsed < REWARDED_AD_MIN_LOAD_INTERVAL)
        {
            Debug.Log($"[AdsManager] 마지막 시도 후 {elapsed:F1}초 경과 - 최소 간격({REWARDED_AD_MIN_LOAD_INTERVAL}초) 미달로 즉시 재시도 생략, 백오프 스케줄로 대체");
            ScheduleRewardedAdRetry();
            return;
        }

        if (m_RewardedAdRetryCoroutine != null)
        {
            StopCoroutine(m_RewardedAdRetryCoroutine);
            m_RewardedAdRetryCoroutine = null;
        }

        if (resetRetryCountIfExhausted && m_RewardedAdRetryCount >= REWARDED_AD_MAX_RETRY)
        {
            Debug.Log("[AdsManager] 재시도 소진 상태 - 새로운 기회로 카운트 초기화 후 재시도");
            m_RewardedAdRetryCount = 0;
        }

        Debug.Log("[AdsManager] 즉시 광고 재로드 시도");
        LoadDailyFreeGemRewardedAd();
    }

    private void TryImmediateRewardedAdReload()
    {
        AttemptRewardedAdReload(resetRetryCountIfExhausted: false);
    }

    // ⭐ 추가: 스테이지 진입, 부활 패널 스킵 등 외부에서 선제적으로 로드를 요청할 때 사용
    public void RequestRewardedAdLoad()
    {
        if (IsRewardedAdReady)
        {
            Debug.Log("[AdsManager] RequestRewardedAdLoad 호출됐지만 이미 준비돼 있어 무시");
            return;
        }
        Debug.Log("[AdsManager] RequestRewardedAdLoad 호출 - 선제적 로드 시도");
        AttemptRewardedAdReload(resetRetryCountIfExhausted: true);
    }

    private void ListenToDailyFreeGemRewardedAdEvents()
    {
        if (m_DailyFreeGemRewardedAd == null)
        {
            Debug.LogError("[AdsManager] m_DailyFreeGemRewardedAd is null.");
            return;
        }

        m_DailyFreeGemRewardedAd.OnAdPaid += (AdValue adValue) =>
            Debug.Log($"[AdsManager] m_DailyFreeGemRewardedAd paid {adValue.Value}{adValue.CurrencyCode}.");

        m_DailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log($"[AdsManager] m_DailyFreeGemRewardedAd recorded an impression.");
            FirebaseManager.LogEvent("ad_impression_recorded", "placement", m_CurrentRewardedAdPlacement);
        };

        m_DailyFreeGemRewardedAd.OnAdClicked += () =>
            Debug.Log($"[AdsManager] m_DailyFreeGemRewardedAd was clicked.");

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
            Debug.Log($"[AdsManager] m_DailyFreeGemRewardedAd full screen content opened.");

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log($"[AdsManager] m_DailyFreeGemRewardedAd full screen content closed.");
            SetRewardedAdReady(false);
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"[AdsManager] m_DailyFreeGemRewardedAd failed to open full screen content with error: {error}");
            Debug.LogError($"[AdsManager] [진단] Code: {error.GetCode()}, Domain: {error.GetDomain()}, Message: {error.GetMessage()}"); // ⭐ 추가
            SetRewardedAdReady(false);
            FirebaseManager.LogEvent("ad_show_fail_content_error", "placement", m_CurrentRewardedAdPlacement);
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };
    }

    // ⭐ onAdNotReady 파라미터 추가: 광고가 준비 안 됐을 때 호출자에게 알려줌
    public void ShowDailyFreeGemRewardedAd(Action onRewarded = null, Action onClosed = null, Action onAdNotReady = null, string placement = "daily_free_gem")
    {
        Debug.Log($"[AdsManager] Show DailyFreeGemRewardedAd 호출 (placement: {placement})");
        Debug.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}, 로드 중 여부: {m_IsRewardedAdLoading}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
            m_CurrentRewardedAdPlacement = placement;
            FirebaseManager.LogEvent("ad_show_attempt", "placement", placement);

            m_OnDailyFreeGemRewardedAdClosed = onClosed;
            m_DailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Debug.Log("[AdsManager] Rewarded DailyFreeGem");
                onRewarded?.Invoke();
            });
        }
        else
        {
            // ⭐ 추가: 왜 못 보여줬는지 진단 로그
            Debug.LogError($"[AdsManager] m_DailyFreeGemRewardedAd is not ready yet.");
            Debug.LogError($"[AdsManager] [진단] ad == null: {m_DailyFreeGemRewardedAd == null}, CanShowAd: {(m_DailyFreeGemRewardedAd != null ? m_DailyFreeGemRewardedAd.CanShowAd().ToString() : "N/A")}");
            Debug.LogError($"[AdsManager] [진단] 마지막 로드 시도 후 경과: {Time.unscaledTime - m_LastRewardedAdLoadAttemptTime:F1}초, 재시도 횟수: {m_RewardedAdRetryCount}/{REWARDED_AD_MAX_RETRY}, 네트워크: {Application.internetReachability}");

            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", placement);
            SetRewardedAdReady(false);
            TryImmediateRewardedAdReload();
            onAdNotReady?.Invoke();
        }
    }

    public void ShowBoxRewardedAd(Action onRewarded = null, Action onClosed = null, Action onAdNotReady = null, string placement = "box")
    {
        Debug.Log($"[AdsManager] ShowBoxRewardedAd 호출 (placement: {placement})");
        Debug.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}, 로드 중 여부: {m_IsRewardedAdLoading}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
            m_CurrentRewardedAdPlacement = placement;
            FirebaseManager.LogEvent("ad_show_attempt", "placement", placement);

            m_OnDailyFreeGemRewardedAdClosed = onClosed;
            m_DailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Debug.Log("[AdsManager] Rewarded BoxAd");
                onRewarded?.Invoke();

                if (AchievementManager.Instance != null)
                    AchievementManager.Instance.AddProgress(AchievementType.AD_DRAW);
            });
        }
        else
        {
            Debug.LogError($"[AdsManager] 광고가 준비되지 않았습니다.");
            Debug.LogError($"[AdsManager] [진단] ad == null: {m_DailyFreeGemRewardedAd == null}, CanShowAd: {(m_DailyFreeGemRewardedAd != null ? m_DailyFreeGemRewardedAd.CanShowAd().ToString() : "N/A")}");
            Debug.LogError($"[AdsManager] [진단] 마지막 로드 시도 후 경과: {Time.unscaledTime - m_LastRewardedAdLoadAttemptTime:F1}초, 재시도 횟수: {m_RewardedAdRetryCount}/{REWARDED_AD_MAX_RETRY}, 네트워크: {Application.internetReachability}");

            FirebaseManager.LogEvent("ad_show_fail_not_ready", "placement", placement);
            SetRewardedAdReady(false);
            TryImmediateRewardedAdReload();
            onAdNotReady?.Invoke();
        }
    }
    #endregion

    protected override void Dispose()
    {
        if (m_RewardedAdRetryCoroutine != null)
        {
            StopCoroutine(m_RewardedAdRetryCoroutine);
            m_RewardedAdRetryCoroutine = null;
        }
        m_IsRewardedAdLoading = false;

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
        Debug.Log("[AdsManager] ConsentInformation.Update 호출 시작");
        var request = new ConsentRequestParameters();

        ConsentInformation.Update(request, (FormError updateError) =>
        {
            Debug.Log($"[AdsManager] ConsentInformation.Update 콜백 도착. error={updateError}");
            if (updateError != null)
            {
                Debug.LogError($"[AdsManager] 동의 정보 업데이트 실패: {updateError}");
                onConsentReady?.Invoke();
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                Debug.Log($"[AdsManager] LoadAndShowConsentFormIfRequired 콜백 도착. error={formError}");
                if (formError != null)
                {
                    Debug.LogError($"[AdsManager] 동의 폼 표시 실패: {formError}");
                }
                else
                {
                    Debug.Log("[AdsManager] 동의 절차 완료");
                }
                onConsentReady?.Invoke();
            });
        });
    }

    public void ShowPrivacyOptionsForm()
    {
        ConsentForm.ShowPrivacyOptionsForm((FormError formError) =>
        {
            if (formError != null)
            {
                Debug.LogError($"[AdsManager] 개인정보 설정 폼 표시 실패: {formError}");
            }
            else
            {
                Debug.Log("[AdsManager] 개인정보 설정 폼 닫힘");
            }
        });
    }

    public bool IsPrivacyOptionsRequired()
    {
        return ConsentInformation.PrivacyOptionsRequirementStatus
            == PrivacyOptionsRequirementStatus.Required;
    }
    #endregion
}