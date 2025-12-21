using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdsManager : SingletonBehaviour<AdsManager>
{
    protected override void Init()
    {
        base.Init();
        InitAdsServices();
    }

    void InitAdsServices()
    {
        MobileAds.Initialize(initstatus =>
        {
            // 초기화 성공 여부 확인
            var isInitSuccess = true;
            var statusMap = initstatus.getAdapterStatusMap();
            foreach (var item in statusMap)
            {
                var className = item.Key;
                var adapterStatus = item.Value;
                Logger.Log($"Adapter: {className}, State: {adapterStatus.InitializationState}, Description: {adapterStatus.Description}");
                if(adapterStatus.InitializationState != AdapterState.Ready)
                {
                    isInitSuccess = false;
                }
            }

            if(isInitSuccess)
            {
                Logger.Log($"구글 애즈 초기화 성공");
            }
            else
            {
                Logger.LogError($"구글 애즈 초기화 실패");
            }
        });
        
    }

}
