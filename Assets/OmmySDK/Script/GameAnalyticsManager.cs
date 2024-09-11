using System;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Setup;
using UnityEditor;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;

public class GameAnalyticsManager : MonoBehaviour
{
    private static GameAnalyticsManager _instance = null;

    static public GameAnalyticsManager Agent
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindObjectOfType(typeof(GameAnalyticsManager)) as GameAnalyticsManager;
                if (_instance == null)
                {
                    GameObject obj = new GameObject("GameAnalyticsManager");
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<GameAnalyticsManager>();
                }
            }
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this.gameObject.GetComponent<GameAnalyticsManager>();
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }
    public static UnityEvent<bool> OnInitialize;
    public static void Initialize()
    {
        GameAnalytics.onInitialize += (object s, bool b) => OnInitialize?.Invoke(b);
        GameAnalytics.Initialize();
        GameAnalyticsILRD.SubscribeMaxImpressions();
    }

    public static void GameStartAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_Start", levelNo.ToString(), levelNo);
    }
    public static void GameFailAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "Level_Fail", levelNo.ToString(), levelNo);
    }
    public static void GameCompleteAnalytics(int levelNo)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "Level_Complete", levelNo.ToString(), levelNo);
    }
    public static void AdEvent(GAAdAction gAAdAction, GAAdType adType, string network = "admob", string _adplacement = "undefine")
    {
        GameAnalytics.NewAdEvent(gAAdAction, adType, network, _adplacement);
    }
    public static void AdEventILDR(string adUnitId, BannerView ad)
    {
#if gameanalytics_admob_enabled
        GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, ad);
#endif
    }
    public static void AdEventILDR(string adUnitId, InterstitialAd ad)
    {
#if gameanalytics_admob_enabled
        GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, ad);
#endif
    }
    public static void AdEventILDR(string adUnitId, RewardedAd ad)
    {
#if gameanalytics_admob_enabled
        GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, ad);
#endif
    }
    public static void AdEventILDR(string adUnitId, RewardedInterstitialAd ad)
    {
#if gameanalytics_admob_enabled
        GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId, ad);
#endif
    }
    public void DesignEvent(string eventData)
    {
        GameAnalytics.NewDesignEvent(eventData);
    }
    #region GAIDs
    public static void SetGAIds()
    {

        for (; 0 < GameAnalytics.SettingsGA.Platforms.Count;)
        {
            GameAnalytics.SettingsGA.RemovePlatformAtIndex(0);
        }
        //if (ArcadiaSdkManager.myGameIds.platform=="Android")
#if UNITY_ANDROID
        GameAnalytics.SettingsGA.AddPlatform(RuntimePlatform.Android);
#elif UNITY_IOS
        GameAnalytics.SettingsGA.AddPlatform(RuntimePlatform.IPhonePlayer);
#endif

        GameAnalytics.SettingsGA.UpdateGameKey(0, OmmyAnalyticsManager.Agent.gameKey);
        GameAnalytics.SettingsGA.UpdateSecretKey(0, OmmyAnalyticsManager.Agent.secretKey);

        GameAnalytics.SettingsGA.SubmitFpsAverage = true;
        GameAnalytics.SettingsGA.SubmitFpsCritical = true;
        GameAnalytics.SettingsGA.NativeErrorReporting = true;
        GameAnalytics.SettingsGA.SubmitErrors = true;
        GameAnalytics.SettingsGA.InfoLogBuild = true;
        GameAnalytics.SettingsGA.InfoLogEditor = true;
        GameAnalytics.SettingsGA.UsePlayerSettingsBuildNumber = true;
        GameAnalytics.SettingsGA.FpsCriticalThreshold = 30;

    }

    #endregion
}
