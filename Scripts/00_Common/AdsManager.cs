using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : SingletonBehaviour<AdsManager>
{
    protected override void Init()
    {
        base.Init();

        InitAdsService();
        InitBannerAds();
        InitInterstitialAds();
        InitRewardedAds();
    }

    private void InitAdsService()
    {
        MobileAds.Initialize(initStatus =>
        {
            //Check if initialization was successful
            var isInitSuccess = true;
            var statusMap = initStatus.getAdapterStatusMap();
            foreach (var status in statusMap)
            {
                var className = status.Key;
                var adapterStatus = status.Value;
                Logger.Log($"Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if(adapterStatus.InitializationState != AdapterState.Ready)
                {
                    isInitSuccess = false;
                }
            }

            if(isInitSuccess)
            {
                Logger.Log($"Google Ads initialization successful.");
            }
            else
            {
                Logger.LogError($"Google Ads initialization failed.");
            }
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

        if(value)
        {
            if(m_TopBannerView == null)
            {
                AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
                m_TopBannerView = new BannerView(m_TopBannerAdId, adaptiveSize, AdPosition.Top);

                // create ad request
                AdRequest request = new AdRequest();
                // load the banner with the request
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
            if(m_TopBannerView != null)
            {
                m_TopBannerView.Hide();
            }
        }
    }

    private void ListenToTopBannerAdEvents()
    {
        if(m_TopBannerView == null)
        {
            Logger.LogError("m_TopBannerView is null.");
            return;
        }

        m_TopBannerView.OnBannerAdLoaded += () =>
        {
            Logger.Log($"m_TopBannerView loaded an ad with response : {m_TopBannerView.GetResponseInfo()}");
        };

        m_TopBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Logger.LogError($"m_TopBannerView failed to load an ad with error : {error}");
        };

        m_TopBannerView.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"m_TopBannerView paid {adValue.Value}{adValue.CurrencyCode}.");
        };

        m_TopBannerView.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"m_TopBannerView recorded an impression.");
        };

        m_TopBannerView.OnAdClicked += () =>
        {
            Logger.Log($"m_TopBannerView was clicked.");
        };

        m_TopBannerView.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"m_TopBannerView full screen content opened.");
        };

        m_TopBannerView.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_TopBannerView full screen content closed.");
        };
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
        // create ad request
        var adRequest = new AdRequest();

        // send request to load ad
        InterstitialAd.Load(m_StageClearInterstitialAdId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if(error != null || ad == null)
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
        if(m_StageClearInterstitial == null)
        {
            Logger.LogError($"m_StageClearInterstitial is null");
            return;
        }

        m_StageClearInterstitial.OnAdPaid += (AdValue adValue) =>
        {
            Logger.Log($"m_StageClearInterstitial ad paid {adValue.Value}{adValue.CurrencyCode}.");
        };

        m_StageClearInterstitial.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad recorded an impression.");
        };

        m_StageClearInterstitial.OnAdClicked += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad was clicked.");
        };

        m_StageClearInterstitial.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad full screen content opened.");
        };

        m_StageClearInterstitial.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_StageClearInterstitial ad full screen content closed.");
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };

        m_StageClearInterstitial.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"m_StageClearInterstitial ad failed to open full screnn content. Error: {error}");
            LoadStageClearInterstitialAd();
            m_OnFinishStageClearInterstitialAd?.Invoke();
            m_OnFinishStageClearInterstitialAd = null;
        };
    }

	//예시
	//AdsManager.Instance.ShowStageClearInterstitialAd(() =>
    //{
    //    StartNextStage();
    //});
    public void ShowStageClearInterstitialAd(Action onFinishStageClearInterstitialAd = null)
    {
        if(m_StageClearInterstitial != null && m_StageClearInterstitial.CanShowAd())
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
    private RewardedAd m_DailyFreeGemRewardedAd;
    private string m_DailyFreeGemRewardedAdId = string.Empty;
    private const string AOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/5224354917";
    private const string IOS_REWARDED_AD_TEST_AD_ID = "ca-app-pub-3940256099942544/1712485313";
    private const string AOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";
    private const string IOS_DAILY_FREE_GEM_REWARDED_AD_ID = "";

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
        var adRequest = new AdRequest();

        RewardedAd.Load(m_DailyFreeGemRewardedAdId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if(error != null || ad == null)
                {
                    Logger.LogError($"Rewarded ad failed to load. Error: {error}");
                    return;
                }

                Logger.Log($"Rewarded ad loaded successfully. Response: {ad.GetResponseInfo()}");
                m_DailyFreeGemRewardedAd = ad;
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
        {
            Logger.Log($"m_DailyFreeGemRewardedAd paid {adValue.Value}{adValue.CurrencyCode}.");
        };

        m_DailyFreeGemRewardedAd.OnAdImpressionRecorded += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd recoreded an impression.");
        };

        m_DailyFreeGemRewardedAd.OnAdClicked += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd was clicked.");
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentOpened += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content opened.");
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentClosed += () =>
        {
            Logger.Log($"m_DailyFreeGemRewardedAd full screen content closed.");
            LoadDailyFreeGemRewardedAd();
        };

        m_DailyFreeGemRewardedAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Logger.LogError($"m_DailyFreeGemRewardedAd failed to open full screen content with error: {error}");
            LoadDailyFreeGemRewardedAd();
        };
    }

    public void ShowDailyFreeGemRewardedAd(Action onRewardDailyFreeGemAd = null)
    {
        Logger.Log($"Show DailyFreeGemRewardedAd");

        if(m_DailyFreeGemRewardedAd != null && m_DailyFreeGemRewardedAd.CanShowAd())
        {
            m_DailyFreeGemRewardedAd.Show((Reward reward) =>
            {
                Logger.Log("Rewarded DailyFreeGem");
                onRewardDailyFreeGemAd?.Invoke();
            });
        }
        else
        {
            Logger.LogError($"m_DailyFreeGemRewardedAd is not ready yet.");
        }
    }
    #endregion

    protected override void Dispose()
    {
        if(m_TopBannerView != null)
        {
            m_TopBannerView.Destroy();
            m_TopBannerView = null;
        }

        if(m_StageClearInterstitial != null)
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
}
