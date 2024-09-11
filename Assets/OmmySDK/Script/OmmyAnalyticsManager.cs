using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using GameAnalyticsSDK.Setup;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.Events;


public class OmmyAnalyticsManager : MonoBehaviour
{
    private static OmmyAnalyticsManager _instance = null;
    
    static public OmmyAnalyticsManager Agent
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindObjectOfType(typeof(OmmyAnalyticsManager)) as OmmyAnalyticsManager;
                if (_instance == null)
                {
                    GameObject obj = new GameObject("OmmyAnalyticsManager");
                    DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<OmmyAnalyticsManager>();
                }
            }
            return _instance;
        }
    }
    void Awake()
    {
        if(_instance == null)
        {	
            _instance = this.gameObject.GetComponent<OmmyAnalyticsManager>();
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
        GameAnalyticsManager.OnInitialize=onInitialized;
        SetGAIds();
        InIt();
    }
    public string gameKey;
    public string secretKey;
    public void InIt()
    {
        GameAnalyticsManager.Initialize();
        FirebaseManager.InitializeFirebase();
        //GameAnalyticsILRD.SubscribeAdMobImpressions();
        //Debug.Log(":* GameAnalytics Initialized!");
    }
    public void CustomEvent(string eventName, string Value)
    {
        FirebaseManager.LogEvent("Custom",eventName,Value);
    }
    public void GameStartAnalytics(int levelNo)
    {
        FirebaseManager.LogLevelStartEvent(levelNo);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start,"Level_Start",levelNo.ToString(),levelNo);
    }
    public void GameFailAnalytics(int levelNo)
    {
        FirebaseManager.LogLevelFailEvent(levelNo);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail,"Level_Fail",levelNo.ToString(),levelNo);
    }
    public void GameCompleteAnalytics(int levelNo)
    {
        FirebaseManager.LogLevelCompleteEvent(levelNo);
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete,"Level_Complete",levelNo.ToString(),levelNo);
    }
    public void AdEvent(GAAdAction gAAdAction,GAAdType adType,string network="admob",string _adplacement="undefine")
    {
        GameAnalyticsManager.AdEvent(gAAdAction,adType,network,_adplacement);
        //GameAnalytics.NewAdEvent(gAAdAction,adType,network,_adplacement);
    }
    public void AdEventILDR(string adUnitId,BannerView ad)
    {
        GameAnalyticsManager.AdEventILDR(adUnitId,ad);
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,InterstitialAd ad)
    {
        GameAnalyticsManager.AdEventILDR(adUnitId,ad);
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,RewardedAd ad)
    {
        GameAnalyticsManager.AdEventILDR(adUnitId,ad);
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void AdEventILDR(string adUnitId,RewardedInterstitialAd ad)
    {
        GameAnalyticsManager.AdEventILDR(adUnitId,ad);
        //GameAnalyticsILRD.SubscribeAdMobImpressions(adUnitId,ad);
    }
    public void DesignEvent(string eventData)
    {
        GameAnalytics.NewDesignEvent(eventData);
    }
    #region Setup
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Ommy/AnalyticsSetup")]
#endif

    public static void SetGAIds()
    {
        GameAnalyticsManager.SetGAIds();
    }

    #endregion
}
