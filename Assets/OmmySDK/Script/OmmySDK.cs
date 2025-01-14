﻿using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using System.Collections;
#if GameAnalytics
using GameAnalyticsSDK;
#endif
public class OmmySDK : MonoBehaviour
{
    //============================== Variables_Region ============================== 
    #region Variables_Region
    private static OmmySDK _instance = null;
    public BannerView squareBannerView;
    public BannerView adaptiveBannerView;
    public InterstitialAd interstitialAd;
    public RewardedAd rewardedAd;
    public RewardedInterstitialAd rewardedInterstitialAd;
    public GameObject adLoadingPanel, noAdPanel;
    public bool showBannerInStart;
    public bool useTestIDs;
    public bool InternetRequired;
    public bool loadNextScene = true;
    public bool removeAd = false;
    public TagForChildDirectedTreatment tagForChild;
    [Space(10)]
    [SerializeField]
    public IDs myGameIds = new IDs();
    public GameLinks Links;
    private Action interstitialCallBack, rewardedCallBack, rewardedInterstitialCallBack;
    #endregion

    //============================== Singleton_Region ============================== 
    #region Singleton_Region
    static public OmmySDK Agent
    {
        get
        {
            return _instance;
        }
    }
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    //================================ Start_Region ================================
    #region Start_Region

    void Start()
    {
        InternetCheckerInit();
        InitAdmob();
        removeAd = PlayerPrefs.GetInt(nameof(removeAd)) == 1;
        Application.lowMemory += () => Resources.UnloadUnusedAssets();
    }
    public void InternetCheckerInit()
    {
#if UNITY_EDITOR
        //	return;
#endif
        if (InternetRequired)
        {
            InternetManager obj = FindObjectOfType<InternetManager>();
            if (obj == null)
            {
                var net = new GameObject();
                net.name = "InternetManager";
                net.AddComponent<InternetManager>();
                DontDestroyOnLoad(net);
            }
        }
    }
    #region Stores
    public void ShowInAppRateUs()
    {
        StoreReviewManager obj = FindObjectOfType<StoreReviewManager>();
        if (obj == null)
        {
            var rate = new GameObject();
            obj = rate.AddComponent<StoreReviewManager>();
            obj.RateUs();
        }
        else
        {
            obj.RateUs();
        }
    }
    public void ShowInAppRateUs(float dely)
    {
        Invoke(nameof(ShowInAppRateUs), dely);
    }
    #endregion
    #region Links 
    public void RateUs()
    {
        if (InternetStatus())
            Application.OpenURL(Links.RateUsLink);
    }
    public void MoreGames()
    {
        if (InternetStatus())
            Application.OpenURL(Links.MoreGamesLink);

    }
    public void PrivacyPolicy()
    {
        if (InternetStatus())
            Application.OpenURL(Links.PrivacyLink);

    }
    public void ExitGame()
    {
        Application.Quit();
    }

    #endregion
    public bool InternetStatus()
    {
        if (SystemInfo.systemMemorySize < 1024)
            return false;
        else if (Application.internetReachability != NetworkReachability.NotReachable)
            return true;
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            return true;
        else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            return true;
        else
            return false;
    }
    public void InitAdmob()
    {

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        MobileAds.SetiOSAppPauseOnBackground(true);
        //deviceIds.Add("D8E71788-08AE-4095-ACE6-F35B24D77298");
        

#elif UNITY_ANDROID
        // this is our office device GAID of Oppo device
        deviceIds.Add("2e57d086-b900-40e3-98a1-9b3220f33eb5");
#endif
        // Configure TagForChildDirectedTreatment and test device IDs.

        // RequestConfiguration requestConfiguration =
        //     new RequestConfiguration.Builder().SetTagForChildDirectedTreatment(tagForChild)
        //     .SetTestDeviceIds(deviceIds).build();
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        RequestConfiguration requestConfiguration = new RequestConfiguration();
        requestConfiguration.TagForChildDirectedTreatment = tagForChild;
        requestConfiguration.TestDeviceIds = deviceIds;
        MobileAds.SetRequestConfiguration(requestConfiguration);
        // Initialize the Google Mobile Ads SDK.
        //MobileAds.Initialize(HandleInitCompleteAction);
        MobileAds.Initialize((initStatus) =>
        {
            Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
            foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        MonoBehaviour.print("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        MonoBehaviour.print("Adapter: " + className + " is initialized.");
                        break;
                }
            }
            LoadAds();
            if (loadNextScene)
                SceneManager.LoadScene(1);
        });

        // Listen to application foreground / background events.
        //LoadAds();
    }
    private AdRequest CreateAdRequest()
    {
        return new AdRequest();
    }

    public void LoadAds()
    {
        if (myGameIds.rewarded && myGameIds.preCacheRewarded) RequestAndLoadRewardedAd();
        if (myGameIds.rewardedInterstitial && myGameIds.preCacheRewardedInterstitial) LoadRewardedInterstitialAd();
        if (removeAd) return;
        if (showBannerInStart) ShowAdoptiveBanner();
        if (myGameIds.interstitial && myGameIds.preCacheInterstitial) RequestAndLoadInterstitialAd();
    }
    #region BANNER ADS
    public void RequestAdaptiveBannerAd(string bannerIds, AdSize adSize, AdPosition adPosition)
    {
        if (!myGameIds.adaptiveBanner) return;
        PrintStatus("Requesting Banner ad.");

        // These ad units are configured to always serve test ads.
        string adUnitId = bannerIds;
        if (useTestIDs)
        {

#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
         adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
            adUnitId = "unexpected_platform";
#endif
        }
        // Clean up banner before reusing
        if (adaptiveBannerView != null)
        {
            adaptiveBannerView.Destroy();
        }
#if GameAnalytics
        if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.Banner);
#endif

        // Create a 320x50 banner at top of the screen
        adaptiveBannerView = new BannerView(adUnitId, adSize, adPosition);

        // Add Event Handlers
        adaptiveBannerView.OnBannerAdLoaded += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized)
            {
                OmmyAnalyticsManager.Agent.AdEventILDR(adUnitId, adaptiveBannerView);
                OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.Banner);
            }
#endif
            PrintStatus("Banner ad loaded.");
        };
        adaptiveBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Banner);
#endif
            PrintStatus("<color=red> Error: Banner ad failed to load with error: " + error.GetMessage());
        };
        adaptiveBannerView.OnAdFullScreenContentOpened += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.Banner);
#endif
            PrintStatus("Banner ad opening.");
        };
        adaptiveBannerView.OnAdFullScreenContentClosed += () =>
        {
            PrintStatus("Banner ad closed.");
        };
        adaptiveBannerView.OnAdClicked += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.Banner);
#endif
            PrintStatus("Banner ad clicked.");
        };
        adaptiveBannerView.OnAdPaid += (AdValue adValue) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Banner ad received a paid event.",
                                        adValue.CurrencyCode,
                                        adValue.Value);
            PrintStatus(msg);
        };
        // Load a banner ad
        adaptiveBannerView.LoadAd(CreateAdRequest());
    }
    public void RequestSquareBannerAd(string bannerIds, AdSize adSize, AdPosition adPosition)
    {
        if (!myGameIds.squareBanner) return;
        PrintStatus("Requesting Banner ad.");

        // These ad units are configured to always serve test ads.
        string adUnitId = bannerIds;
        if (useTestIDs)
        {

#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
         adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
            adUnitId = "unexpected_platform";
#endif
        }
        // Clean up banner before reusing
        if (squareBannerView != null)
        {
            squareBannerView.Destroy();
        }

        // Create a 320x50 banner at top of the screen
        squareBannerView = new BannerView(adUnitId, adSize, adPosition);
#if GameAnalytics
        if (GameAnalytics.Initialized)
        {
            OmmyAnalyticsManager.Agent.AdEventILDR(adUnitId, squareBannerView);
            OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.OfferWall);
        }
#endif
        // Add Event Handlers
        squareBannerView.OnBannerAdLoaded += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.OfferWall);
#endif

            PrintStatus("Banner ad loaded.");
        };
        squareBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.OfferWall);
#endif

            PrintStatus("<color=red> Error: Banner ad failed to load with error: " + error.GetMessage());
        };
        squareBannerView.OnAdFullScreenContentOpened += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.OfferWall);
#endif
            PrintStatus("Banner ad opening.");
        };
        squareBannerView.OnAdFullScreenContentClosed += () =>
        {
            PrintStatus("Banner ad closed.");
        };
        squareBannerView.OnAdClicked += () =>
        {
#if GameAnalytics
            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.OfferWall);
#endif
            PrintStatus("Banner ad clicked.");
        };
        squareBannerView.OnAdPaid += (AdValue adValue) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Banner ad received a paid event.",
                                        adValue.CurrencyCode,
                                        adValue.Value);
            PrintStatus(msg);
        };
        // Load a banner ad
        squareBannerView.LoadAd(CreateAdRequest());
    }
    public void ShowSquareBanner(bool showNew=false)
    {
        if (removeAd)
        {
            return;
        }
        if(squareBannerView!=null&&!squareBannerView.IsDestroyed&&!showNew)
        {
            squareBannerView.Show();
        }
        else
        {
        RequestSquareBannerAd(myGameIds.squareBannerAdId, AdSize.MediumRectangle, myGameIds.squareBannerPosition);
        }
    }
    public void ShowAdoptiveBanner(bool showNew=false)
    {
        if (removeAd)
        {
            return;
        }
        if (adaptiveBannerView!=null&&!adaptiveBannerView.IsDestroyed&&!showNew)
        {
            adaptiveBannerView.Show();
        }
        else
        {
            var _adSize = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            RequestAdaptiveBannerAd(myGameIds.adoptiveBannerAdId, _adSize, myGameIds.adoptiveBannerPosition);
        }
    }
    public void HideAdaptiveBanner()
    {
        if (adaptiveBannerView != null)
        {
            adaptiveBannerView.Hide();
        }
    }
    public void HideSquareBanner()
    {
            if (squareBannerView != null)
            {
                squareBannerView.Hide();
            }
    }
    public void DestroyAdaptiveBannerAd()
    {
        if (adaptiveBannerView != null)
        {
            adaptiveBannerView.Destroy();
        }
    }
    public void DestroySquareBannerAd()
    {
        if (squareBannerView != null)
        {
            squareBannerView.Destroy();
        }
    }

    #endregion

    #region INTERSTITIAL ADS
    public void RequestAndLoadInterstitialAd(Action<bool> OnLoad = null)
    {
        if (!myGameIds.interstitial) return;

        PrintStatus("Requesting Interstitial ad.");

        string adUnitId = myGameIds.interstitialAdId;
        if (useTestIDs)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
         adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
            adUnitId = "unexpected_platform";
#endif
        }
        // Clean up interstitial before using it
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        // Load an interstitial ad
#if GameAnalytics
        OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.Interstitial);
#endif
        InterstitialAd.Load(adUnitId, CreateAdRequest(),
            (InterstitialAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("<color=red> Error: Interstitial ad failed to load with error: " +
                        loadError.GetMessage());
                    OnLoad?.Invoke(false);
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Interstitial);
#endif

                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("<color=red> Error: Interstitial ad failed to load.");
                    OnLoad?.Invoke(false);
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Interstitial);
#endif

                    return;
                }

                PrintStatus("Interstitial ad loaded.");
                interstitialAd = ad;
                OnLoad?.Invoke(true);
#if GameAnalytics
                if (GameAnalytics.Initialized)
                {
                    OmmyAnalyticsManager.Agent.AdEventILDR(adUnitId, interstitialAd);
                    OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.Interstitial);
                }
#endif
                ad.OnAdFullScreenContentOpened += () =>
                {
                    interstitialCallBack?.Invoke();
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.Interstitial);
#endif

                    PrintStatus("Interstitial ad opening.");
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
                    ShowLoading(false);
                    PrintStatus("Interstitial ad closed.");
                    if (myGameIds.preCacheInterstitial)
                        RequestAndLoadInterstitialAd();
                };
                ad.OnAdImpressionRecorded += () =>
                {
                    PrintStatus("Interstitial ad recorded an impression.");
                };
                ad.OnAdClicked += () =>
                {
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.Interstitial);
#endif
                    PrintStatus("Interstitial ad recorded a click.");
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    interstitialCallBack?.Invoke();
                    PrintStatus("Interstitial ad failed to show with error: " +
                                error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    string msg = string.Format("{0} (currency: {1}, value: {2}",
                                               "Interstitial ad received a paid event.",
                                               adValue.CurrencyCode,
                                               adValue.Value);
                    PrintStatus(msg);
                };
            });
    }
    public void ShowInterstitialAd(int timeBeforeAd, Action action = null)
    {
        if (interstitialTimerCorotine != null)
        {
            StopCoroutine(interstitialTimerCorotine);
        }
        interstitialTimerCorotine = StartCoroutine(StartInterstitialAdTimerDely(action, timeBeforeAd));
    }
    Coroutine interstitialTimerCorotine;
    IEnumerator StartInterstitialAdTimerDely(Action action, int dely)
    {
        ShowLoading(true);
        yield return new WaitForSecondsRealtime(dely);
        ShowInterstitialAd(action);
    }
    public void ShowInterstitialAd(Action _interstitialCallBack = null)
    {

        if (removeAd) return;

        if (!myGameIds.preCacheInterstitial)
        {
            ShowLoading(true);
            _interstitialCallBack += () => ShowLoading(false);
        }

        interstitialCallBack = _interstitialCallBack;
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
        }
        else
        {
            //interstitialCallBack?.Invoke();
            PrintStatus("Interstitial ad is not ready yet.");
            RequestAndLoadInterstitialAd((isLoaded) =>
            {
                if (isLoaded && !myGameIds.preCacheInterstitial)
                {
                    if (interstitialAd != null && interstitialAd.CanShowAd())
                    {
                        interstitialAd.Show();
                    }
                }
                else
                {
                    ShowLoading(false);
                }
            });
        }
    }
    public void DestroyInterstitialAd()
    {
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }
    }

    #endregion
    #region REWARDED ADS
    public void RequestAndLoadRewardedAd(Action<bool> OnLoad = null)
    {
        if (!myGameIds.rewarded) return;
        PrintStatus("Requesting Rewarded ad.");
        string adUnitId = myGameIds.rewardedVideoAdId;
        if (useTestIDs)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            adUnitId = "unexpected_platform";
#endif
        }
#if GameAnalytics
        if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.RewardedVideo);
#endif
        // Clean up the old ad before loading a new one.
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        // Create our request used to load the ad.
        var adRequest = new AdRequest();
        // create new rewarded ad instance
        RewardedAd.Load(adUnitId, adRequest,
            (RewardedAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("Rewarded ad failed to load with error: " +
                                loadError.GetMessage());
                    OnLoad?.Invoke(false);
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
#endif

                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("Rewarded ad failed to load.");
                    OnLoad?.Invoke(false);
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
#endif
                    return;
                }
                PrintStatus("Rewarded ad loaded.");
                rewardedAd = ad;
                OnLoad?.Invoke(true);
#if GameAnalytics
                if (GameAnalytics.Initialized)
                {
                    OmmyAnalyticsManager.Agent.AdEventILDR(adUnitId, rewardedAd);
                    OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.RewardedVideo);
                }
#endif
                ad.OnAdFullScreenContentOpened += () =>
                {
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.RewardedVideo);
#endif
                    PrintStatus("Rewarded ad opening.");
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
                    ShowLoading(false);
                    PrintStatus("Rewarded ad closed.");
                    if (myGameIds.preCacheRewarded)
                        RequestAndLoadRewardedAd();
                };
                ad.OnAdImpressionRecorded += () =>
                {
                    PrintStatus("Rewarded ad recorded an impression.");
                };
                ad.OnAdClicked += () =>
                {
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.RewardedVideo);
#endif

                    PrintStatus("Rewarded ad recorded a click.");
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
#endif
                    rewardedCallBack?.Invoke();
                    PrintStatus("Rewarded ad failed to show with error: " +
                               error.GetMessage());
                };
                ad.OnAdPaid += (AdValue adValue) =>
                {
                    //rewardedCallBack?.Invoke();
                    string msg = string.Format("{0} (currency: {1}, value: {2}",
                                               "Rewarded ad received a paid event.",
                                               adValue.CurrencyCode,
                                               adValue.Value);
                    PrintStatus(msg);
                };
            });
    }
    public void ShowRewardedAd(int timeBeforeAd, Action success = null, Action fail = null)
    {
        if (rewardedTimerCorotine != null)
        {
            StopCoroutine(rewardedTimerCorotine);
        }
        rewardedTimerCorotine = StartCoroutine(StartRewardedAdTimer(success, fail, timeBeforeAd));
    }
    Coroutine rewardedTimerCorotine;
    IEnumerator StartRewardedAdTimer(Action success, Action fail, int time)
    {
        ShowLoading(true);
        yield return new WaitForSecondsRealtime(time);
        ShowRewardedAd(success, fail);
    }
    public void ShowRewardedAd(Action rewardSuccess = null, Action noVideoAvailable = null)
    {
        if (!myGameIds.preCacheRewarded)
        {
            ShowLoading(true);
            rewardSuccess += () => ShowLoading(false);
        }

        rewardedCallBack = rewardSuccess;
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            PrintStatus("Reward not null");
            rewardedAd.Show((Reward reward) =>
            {
#if GameAnalytics
                if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo);
#endif
                rewardedCallBack?.Invoke();
                PrintStatus("Rewarded ad granted a reward: " + reward.Amount);
            });
            //  RequestAndLoadRewardedAd();
        }
        else
        {
            noVideoAvailable?.Invoke();
            PrintStatus("Rewarded ad is not ready yet.");
            RequestAndLoadRewardedAd((isLoaded) =>
            {
                if (isLoaded && !myGameIds.preCacheRewarded)
                {
                    if (rewardedAd != null && rewardedAd.CanShowAd())
                    {
                        rewardedAd.Show((Reward reward) =>
                        {
                            rewardedCallBack?.Invoke();
                            PrintStatus("Rewarded ad granted a reward: " + reward.Amount);
                        });
                    }
                }
                else
                {
                    ShowLoading(false);
                }
            });
        }
    }
    public void ShowRewardedInterstitialAd(Action rewardSuccess = null, Action rewardFail = null)
    {
        rewardedInterstitialCallBack = rewardSuccess;
        rewardFail += () => { ShowNoAd(true); };
        if (!myGameIds.preCacheRewardedInterstitial)
        {
            ShowLoading(true);
            rewardSuccess += () => ShowLoading(false);
        }


        const string rewardMsg =
            "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";
        if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
        {
            rewardedInterstitialAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                rewardedInterstitialCallBack?.Invoke();
                PrintStatus(String.Format(rewardMsg, reward.Type, reward.Amount));
            });
        }
        else
        {
            rewardFail?.Invoke();
            PrintStatus("RewardedInterstitial ad is not ready yet");
            LoadRewardedInterstitialAd((isLoaded) =>
            {
                if (isLoaded && !myGameIds.preCacheRewardedInterstitial)
                {
                    if (rewardedInterstitialAd != null && rewardedInterstitialAd.CanShowAd())
                    {
                        rewardedInterstitialAd.Show((Reward reward) =>
                        {
                            // TODO: Reward the user.
                            rewardedInterstitialCallBack?.Invoke();
#if GameAnalytics
                            if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo);
#endif
                            PrintStatus(String.Format(rewardMsg, reward.Type, reward.Amount));
                        });
                    }
                }
                else
                {
                    ShowLoading(false);
                }
            });
        }
    }
    public void LoadRewardedInterstitialAd(Action<bool> OnLoad = null)
    {
        if (!myGameIds.rewardedInterstitial) return;
        PrintStatus("Requesting RewardedInterstitial ad");
        string adUnitId = myGameIds.rewardedInterstitialAdId;
        if (useTestIDs)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/5354046379";
#elif UNITY_IPHONE
   adUnitId = "ca-app-pub-3940256099942544/6978759866";
#else
   adUnitId = "unused";
#endif
        }
        // Clean up the old ad before loading a new one.
        if (rewardedInterstitialAd != null)
        {
            rewardedInterstitialAd.Destroy();
            rewardedInterstitialAd = null;
        }

        PrintStatus("Loading the rewarded interstitial ad.");
#if GameAnalytics
        if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.RewardedVideo);
#endif
        // create our request used to load the ad.
        //   var adRequest = new AdRequest();
        //   adRequest.Keywords.Add("unity-admob-sample");
        // send the request to load the ad.
        RewardedInterstitialAd.Load(adUnitId, CreateAdRequest(),
            (RewardedInterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    PrintStatus("rewardedInterstitial ad failed to load with error : " + error);
                    OnLoad?.Invoke(false);
#if GameAnalytics
                    if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
#endif
                    return;
                }

                PrintStatus("RewardedInterstitial ad loaded. index is ");
                rewardedInterstitialAd = ad;
                OnLoad?.Invoke(true);
#if GameAnalytics
                if (GameAnalytics.Initialized)
                {
                    OmmyAnalyticsManager.Agent.AdEventILDR(adUnitId, rewardedInterstitialAd);
                    OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.RewardedVideo);
                }
#endif
                ad.OnAdFullScreenContentOpened += () =>
               {
#if GameAnalytics
                   if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.RewardedVideo);
#endif

                   PrintStatus("RewardedInterstitial ad opening.");
               };
                ad.OnAdFullScreenContentClosed += () =>
              {
                  PrintStatus("RewardedInterstitial ad closed.");
                  ShowLoading(false);
                  if (myGameIds.preCacheRewardedInterstitial)
                      LoadRewardedInterstitialAd();
              };
                ad.OnAdImpressionRecorded += () =>
              {
                  PrintStatus("RewardedInterstitial ad recorded an impression.");
              };
                ad.OnAdClicked += () =>
              {
#if GameAnalytics
                  if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.RewardedVideo);
#endif
                  PrintStatus("RewardedInterstitial ad recorded a click.");
              };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
              {
#if GameAnalytics
                  if (GameAnalytics.Initialized) OmmyAnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
#endif

                  rewardedInterstitialCallBack?.Invoke();
                  PrintStatus("RewardedInterstitial ad failed to show with error: " +
                             error.GetMessage());
              };
                ad.OnAdPaid += (AdValue adValue) =>
              {
                  //rewardedInterstitialCallBack?.Invoke();
                  string msg = string.Format("{0} (currency: {1}, value: {2}",
                                             "RewardedInterstitial ad received a paid event.",
                                             adValue.CurrencyCode,
                                             adValue.Value);
                  PrintStatus(msg);
              };
            });
    }
    public void OnRewardComplete(Reward reward)
    {
        //RequestAndLoadRewardedAd();
        PrintStatus("get reward is " + reward.Amount);
    }

    #endregion
    public void RemoveAllAds()
    {
        PlayerPrefs.SetInt(nameof(removeAd), 1);
        removeAd = true;
        DestroyAdaptiveBannerAd();
        DestroySquareBannerAd();
        DestroyInterstitialAd();
        //DestroyMediumRectangleAd();
        //  this.appOpenAd.Destroy();

    }
    #region AD INSPECTOR
    public void ShowNoAd(bool show)
    {
        if (!show)
        {
            noAdPanel.SetActive(false);
            return;
        }
        if (noAdCoroutine != null)
        {
            StopCoroutine(ShowNoAdTask());
        }
        noAdCoroutine = StartCoroutine(ShowLoadingTask());
    }
    Coroutine noAdCoroutine;
    IEnumerator ShowNoAdTask()
    {
        noAdPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(2);
        noAdPanel.SetActive(false);
    }
    void ShowLoading(bool show)
    {
        if (!show)
        {
            adLoadingPanel.SetActive(false);
            return;
        }
        if (loadingCoroutine != null)
        {
            StopCoroutine(ShowLoadingTask());
        }
        loadingCoroutine = StartCoroutine(ShowLoadingTask());
    }
    Coroutine loadingCoroutine;
    IEnumerator ShowLoadingTask()
    {
        adLoadingPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(5);
        adLoadingPanel.SetActive(false);
    }
    public void OpenAdInspector()
    {
        PrintStatus("Opening Ad inspector.");

        MobileAds.OpenAdInspector((error) =>
        {
            if (error != null)
            {
                PrintStatus("<color=red> Error: Ad inspector failed to open with error: " + error);
            }
            else
            {
                PrintStatus("Ad inspector opened successfully.");
            }
        });
    }

    #endregion
    private void PrintStatus(string message)
    {
        Debug.Log(message);
    }
    #endregion
}
[Serializable]
public class GameIDs
{
    public List<IDs> id = new List<IDs>();
}
[Serializable]
public class IDs
{
    [Header("Enabled ads")]
    public bool adaptiveBanner = true;
    public bool squareBanner = true;
    public bool interstitial = true, rewarded = true, rewardedInterstitial = true;
    [Header("Set IDs High, Medium, All Prices")]
    public AdPosition squareBannerPosition;
    public string squareBannerAdId;
    public AdPosition adoptiveBannerPosition;
    public string adoptiveBannerAdId;
    public bool preCacheInterstitial = true;
    public string interstitialAdId;
    public bool preCacheRewarded = true;
    public string rewardedVideoAdId;
    public bool preCacheRewardedInterstitial = true;
    public string rewardedInterstitialAdId;
}
[Serializable]
public class GameLinks
{
    public string RateUsLink;
    public string MoreGamesLink;
    public string PrivacyLink;
}