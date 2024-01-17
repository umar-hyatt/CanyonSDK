using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Setup;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class AA_AnalyticsManager : MonoBehaviour
{
    private static AA_AnalyticsManager _instance = null;
    
    static public AA_AnalyticsManager Agent
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindObjectOfType(typeof(AA_AnalyticsManager)) as AA_AnalyticsManager;
                if (_instance == null)
                {
                    GameObject obj = new GameObject("AA_AnalyticsManager");
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<AA_AnalyticsManager>();
                }
            }
            return _instance;
        }
    }
    void Awake()
    {
        if(_instance == null)
        {	
            _instance = this.gameObject.GetComponent<AA_AnalyticsManager>();
            DontDestroyOnLoad(this);
            Startup();
        }
        else
        {
            if(this != _instance)
                Destroy(this.gameObject);
        }
    }

    public UnityEvent<bool> onInitialized;
    private void Start() {
        
    }
    private void Startup() 
    {
        GameAnalytics.onInitialize+=(object s, bool b)=>{onInitialized.Invoke(b);};
        SetGAIds();
        InIt();
    }
    public string gameKey;
    public string secretKey;
    public void InIt()
    {
        GameAnalytics.Initialize();
        //GameAnalyticsILRD.SubscribeAdMobImpressions();
        //Debug.Log(":* GameAnalytics Initialized!");
    }

    public void GameStartAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start,"Level_Start",levelNo.ToString(),levelNo);
    }
    public void GameFailAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail,"Level_Fail",levelNo.ToString(),levelNo);
    }
    public void GameCompleteAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete,"Level_Complete",levelNo.ToString(),levelNo);
    }
    public void AdEvent(GAAdAction gAAdAction,GAAdType adType,string network="admob",string _adplacement="undefine")
    {
        GameAnalytics.NewAdEvent(gAAdAction,adType,network,_adplacement);
    }
    public void AdEventILDR(string adUnitId,BannerView ad)
    {
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,InterstitialAd ad)
    {
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,RewardedAd ad)
    {
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,RewardedInterstitialAd ad)
    {
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }


    #region GAIDs

    public static void SetGAIds()
    {
#if UNITY_EDITOR
        for (; 0< GameAnalytics.SettingsGA.Platforms.Count; )
        {
            GameAnalytics.SettingsGA.RemovePlatformAtIndex(0);
        }
        GameAnalytics.SettingsGA.AddPlatform(RuntimePlatform.Android);
        GameAnalytics.SettingsGA.UpdateGameKey(0,AA_AnalyticsManager.Agent.gameKey);
        GameAnalytics.SettingsGA.UpdateSecretKey(0,AA_AnalyticsManager.Agent.secretKey);
        
        GameAnalytics.SettingsGA.SubmitFpsAverage = true;
        GameAnalytics.SettingsGA.SubmitFpsCritical = true;
        GameAnalytics.SettingsGA.NativeErrorReporting = true;
        GameAnalytics.SettingsGA.SubmitErrors = true;
        GameAnalytics.SettingsGA.InfoLogBuild = true;
        GameAnalytics.SettingsGA.InfoLogEditor = true;
        GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber = true;
        GameAnalytics.SettingsGA.FpsCriticalThreshold = 30;
#endif
    }

    #endregion
    
    
    
}
