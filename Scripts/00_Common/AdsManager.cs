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

        // ★ 테스트 기기 등록 - 반드시 InitAdsService()보다 먼저!
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TestDeviceIds = new List<string> { "BE85D1491E3B0ACC8E8996B7C3BC6C0F" }
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // ★ 동의 절차 먼저 진행 후, 완료되면 광고 SDK 초기화
        InitConsent(() =>
        {
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
                    return;
                }

                Logger.Log($"Interstitial ad loaded successfully. Response: {ad.GetResponseInfo()}");
                m_StageClearInterstitial = ad;
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
            Logger.Log($"m_StageClearInterstitial ad recorded an impression.");

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
            m_StageClearInterstitial.Show();
            m_OnFinishStageClearInterstitialAd = onFinishStageClearInterstitialAd;
        }
        else
        {
            Logger.LogError($"Stage clear interstitial ad is not ready yet.");
        }
    }
    #endregion

    #region RewardedAd
    public static bool IsRewardedAdReady { get; private set; } = false;

    private RewardedAd m_DailyFreeGemRewardedAd;
    private string m_DailyFreeGemRewardedAdId = string.Empty;
    private const string AOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/5224354917";
    private const string IOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/1712485313";
    private const string AOS_DAILY_FREE_GEM_REWARDED_AD_ID = "ca-app-pub-2314769566037824/2782705229";
    private const string IOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";

    // ★ 광고창이 완전히 닫힐 때 호출되는 콜백
    private Action m_OnDailyFreeGemRewardedAdClosed = null;

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
                    IsRewardedAdReady = false;
                    return;
                }

                Logger.Log($"[AdsManager] 보상형 광고 로드 성공! Response: {ad.GetResponseInfo()}");
                m_DailyFreeGemRewardedAd = ad;
                IsRewardedAdReady = true;
                ListenToDailyFreeGemRewardedAdEvents();
            });
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

        m_DailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
            Logger.Log($"m_DailyFreeGemRewardedAd recorded an impression.");

        m_DailyFreeGemRewardedAd.OnAdClicked += () =>
            Logger.Log($"m_DailyFreeGemRewardedAd was clicked.");

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content opened.");

        // ★ 광고창이 완전히 닫힌 시점 — 여기서 onClosed 콜백 호출
        m_DailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content closed.");
            IsRewardedAdReady = false;
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"m_DailyFreeGemRewardedAd failed to open full screen content with error: {error}");
            IsRewardedAdReady = false;
            m_OnDailyFreeGemRewardedAdClosed?.Invoke();
            m_OnDailyFreeGemRewardedAdClosed = null;
            LoadDailyFreeGemRewardedAd();
        };
    }

    // ★ 부활 전용 — 업적 카운트 없음
    // ★ onRewarded: 리워드 지급 시점 (광고 닫기 전)
    // ★ onClosed:   광고창이 완전히 닫힌 시점 ← UnPause, 부활 등은 여기서
    public void ShowDailyFreeGemRewardedAd(Action onRewarded = null, Action onClosed = null)
    {
        Logger.Log($"[AdsManager] Show DailyFreeGemRewardedAd 호출");
        Logger.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
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
        }
    }

    // ⭐ 상자 전용 보상형 광고 — AD_DRAW 업적 카운트 포함
    public void ShowBoxRewardedAd(Action onRewarded = null, Action onClosed = null)
    {
        Logger.Log($"[AdsManager] ShowBoxRewardedAd 호출");
        Logger.Log($"[AdsManager] 광고 준비 상태: {IsRewardedAdReady}");

        if (m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
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
        }
    }
    #endregion

    protected override void Dispose()
    {
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
        var request = new ConsentRequestParameters();
        // 필요하다면 테스트 기기용 디버그 설정 추가 가능

        ConsentInformation.Update(request, (FormError updateError) =>
        {
            if (updateError != null)
            {
                Logger.LogError($"[AdsManager] 동의 정보 업데이트 실패: {updateError}");
                onConsentReady?.Invoke(); // 실패해도 게임 진행은 막지 않음
                return;
            }

            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
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

    #endregion
}