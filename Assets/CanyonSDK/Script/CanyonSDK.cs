using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Unity.VisualScripting;
using System.Collections;
using GameAnalyticsSDK;
public class CanyonSDK : MonoBehaviour
{
    //============================== Variables_Region ============================== 
    #region Variables_Region
    private static CanyonSDK _instance = null;
    public BannerView squareBannerView;
    public BannerView adaptiveBannerView;
    public InterstitialAd interstitialAd;
    public RewardedAd rewardedAd;
    public RewardedInterstitialAd rewardedInterstitialAd;
    [Header("-------- v2023.12.10 --------")]
    public GameObject adLoadingPanel;
    public bool showBannerInStart;
    public bool useTestIDs;
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
    static public CanyonSDK Agent
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
        InitAdmob();
        if (showBannerInStart)
            ShowAdoptiveBanner();
        Application.lowMemory += () => Resources.UnloadUnusedAssets();
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

        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder().SetTagForChildDirectedTreatment(tagForChild)
            .SetTestDeviceIds(deviceIds).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);
        // Initialize the Google Mobile Ads SDK.
        //MobileAds.Initialize(HandleInitCompleteAction);
        MobileAds.Initialize((initStatus) =>
        {
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
        if (myGameIds.interstitial && myGameIds.preCacheInterstitial) RequestAndLoadInterstitialAd();
    }
    #region BANNER ADS
    public void RequestAdaptiveBannerAd(string[] bannerIds, AdSize adSize, AdPosition adPosition)
    {
        if (!myGameIds.adaptiveBanner) return;
        PrintStatus("Requesting Banner ad.");

        // These ad units are configured to always serve test ads.
        string adUnitId = bannerIds[0];
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
        if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.Banner);

        // Create a 320x50 banner at top of the screen
        adaptiveBannerView = new BannerView(adUnitId, adSize, adPosition);

        // Add Event Handlers
        adaptiveBannerView.OnBannerAdLoaded += () =>
        {
#if gameanalytics_admob_enabled
            if (GameAnalytics.Initialized)
            {
                AA_AnalyticsManager.Agent.AdEventILDR(adUnitId, adaptiveBannerView);
                AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.Banner);
            }
#endif
            PrintStatus("Banner ad loaded.");
        };
        adaptiveBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Banner);
            PrintStatus("<color=red> Error: Banner ad failed to load with error: " + error.GetMessage());
        };
        adaptiveBannerView.OnAdFullScreenContentOpened += () =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.Banner);
            PrintStatus("Banner ad opening.");
        };
        adaptiveBannerView.OnAdFullScreenContentClosed += () =>
        {
            PrintStatus("Banner ad closed.");
        };
        adaptiveBannerView.OnAdClicked += () =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.Banner);
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
    public void RequestSquareBannerAd(string[] bannerIds, AdSize adSize, AdPosition adPosition)
    {
        if (!myGameIds.squareBanner) return;
        PrintStatus("Requesting Banner ad.");

        // These ad units are configured to always serve test ads.
        string adUnitId = bannerIds[0];
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
#if gameanalytics_admob_enabled
        if (GameAnalytics.Initialized)
        {
            AA_AnalyticsManager.Agent.AdEventILDR(adUnitId, squareBannerView);
            AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.OfferWall);
        }
#endif        
// Add Event Handlers
        squareBannerView.OnBannerAdLoaded += () =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.OfferWall);

            PrintStatus("Banner ad loaded.");
        };
        squareBannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.OfferWall);

            PrintStatus("<color=red> Error: Banner ad failed to load with error: " + error.GetMessage());
        };
        squareBannerView.OnAdFullScreenContentOpened += () =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.OfferWall);
            PrintStatus("Banner ad opening.");
        };
        squareBannerView.OnAdFullScreenContentClosed += () =>
        {
            PrintStatus("Banner ad closed.");
        };
        squareBannerView.OnAdClicked += () =>
        {
            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.OfferWall);
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
    public void ShowSquareBanner()
    {
        if (removeAd)
        {
            return;
        }
        RequestSquareBannerAd(myGameIds.bannerAdId, AdSize.MediumRectangle, myGameIds.bannerPosition);
    }
    public void ShowAdoptiveBanner()
    {

        if (removeAd)
        {
            return;
        }
        var _adSize = AdSize.GetLandscapeAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        RequestAdaptiveBannerAd(myGameIds.adoptiveBannerAdIds, _adSize, myGameIds.adoptiveBannerPosition);
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

        string adUnitId = myGameIds.interstitialAdIds[0];
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

        AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.Interstitial);
        InterstitialAd.Load(adUnitId, CreateAdRequest(),
            (InterstitialAd ad, LoadAdError loadError) =>
            {
                if (loadError != null)
                {
                    PrintStatus("<color=red> Error: Interstitial ad failed to load with error: " +
                        loadError.GetMessage());
                    OnLoad?.Invoke(false);
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Interstitial);

                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("<color=red> Error: Interstitial ad failed to load.");
                    OnLoad?.Invoke(false);
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.Interstitial);

                    return;
                }

                PrintStatus("Interstitial ad loaded.");
                interstitialAd = ad;
                OnLoad?.Invoke(true);
#if gameanalytics_admob_enabled
                if (GameAnalytics.Initialized)
                {
                    AA_AnalyticsManager.Agent.AdEventILDR(adUnitId, interstitialAd);
                    AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.Interstitial);
                } 
#endif
                ad.OnAdFullScreenContentOpened += () =>
                {
                    interstitialCallBack?.Invoke();
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.Interstitial);

                    PrintStatus("Interstitial ad opening.");
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
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
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.Interstitial);
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
    public void ShowInterstitialAd(Action _interstitialCallBack = null)
    {
        if (removeAd) return;

        if (!myGameIds.preCacheInterstitial)
        {
            adLoadingPanel.SetActive(true);
            _interstitialCallBack += () => adLoadingPanel.SetActive(false);
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
                    adLoadingPanel.SetActive(false);
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
        string adUnitId = myGameIds.rewardedVideoAdIds[0];
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
        if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.RewardedVideo);
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
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);

                    return;
                }
                else if (ad == null)
                {
                    PrintStatus("Rewarded ad failed to load.");
                    OnLoad?.Invoke(false);
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
                    return;
                }
                PrintStatus("Rewarded ad loaded.");
                rewardedAd = ad;
                OnLoad?.Invoke(true);
#if gameanalytics_admob_enabled
                if (GameAnalytics.Initialized)
                {
                    AA_AnalyticsManager.Agent.AdEventILDR(adUnitId, rewardedAd);
                    AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.RewardedVideo);
                }
#endif
                ad.OnAdFullScreenContentOpened += () =>
                {
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.RewardedVideo);
                    PrintStatus("Rewarded ad opening.");
                };
                ad.OnAdFullScreenContentClosed += () =>
                {
                    PrintStatus("Rewarded ad closed.");
                    RequestAndLoadRewardedAd();
                };
                ad.OnAdImpressionRecorded += () =>
                {
                    PrintStatus("Rewarded ad recorded an impression.");
                };
                ad.OnAdClicked += () =>
                {
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.RewardedVideo);

                    PrintStatus("Rewarded ad recorded a click.");
                };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
                {
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
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
    public void ShowRewardedAd(Action rewardSuccess = null, Action noVideoAvailable = null)
    {

        if (!myGameIds.preCacheRewarded)
        {
            adLoadingPanel.SetActive(true);
            rewardSuccess += () => adLoadingPanel.SetActive(false);
        }

        rewardedCallBack = rewardSuccess;
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            PrintStatus("Reward not null");
            rewardedAd.Show((Reward reward) =>
            {
                if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo);
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
                    adLoadingPanel.SetActive(false);
                }
            });
        }
    }
    public void ShowRewardedInterstitialAd(Action rewardSuccess = null, Action rewardFail = null)
    {
        if (!myGameIds.preCacheRewardedInterstitial)
        {
            adLoadingPanel.SetActive(true);
            rewardSuccess += () => adLoadingPanel.SetActive(false);
        }


        const string rewardMsg =
            "Rewarded interstitial ad rewarded the user. Type: {0}, amount: {1}.";
        rewardedInterstitialCallBack = rewardSuccess;
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
                            if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo);
                            PrintStatus(String.Format(rewardMsg, reward.Type, reward.Amount));
                        });
                    }
                }
                else
                {
                    adLoadingPanel.SetActive(false);
                }
            });
        }
    }
    public void LoadRewardedInterstitialAd(Action<bool> OnLoad = null)
    {
        if (!myGameIds.rewardedInterstitial) return;
        PrintStatus("Requesting RewardedInterstitial ad");
        string adUnitId = myGameIds.rewardedInterstitialAdIds[0];
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
        if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Request, GAAdType.RewardedVideo);
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
                    if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);
                    return;
                }

                PrintStatus("RewardedInterstitial ad loaded. index is ");
                rewardedInterstitialAd = ad;
                OnLoad?.Invoke(true);
#if gameanalytics_admob_enabled
                if (GameAnalytics.Initialized)
                {
                    AA_AnalyticsManager.Agent.AdEventILDR(adUnitId, rewardedInterstitialAd);
                    AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Loaded, GAAdType.RewardedVideo);
                }
#endif
                ad.OnAdFullScreenContentOpened += () =>
               {
                   if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Show, GAAdType.RewardedVideo);

                   PrintStatus("RewardedInterstitial ad opening.");
               };
                ad.OnAdFullScreenContentClosed += () =>
              {
                  PrintStatus("RewardedInterstitial ad closed.");
                  LoadRewardedInterstitialAd();
              };
                ad.OnAdImpressionRecorded += () =>
              {
                  PrintStatus("RewardedInterstitial ad recorded an impression.");
              };
                ad.OnAdClicked += () =>
              {
                  if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.Clicked, GAAdType.RewardedVideo);
                  PrintStatus("RewardedInterstitial ad recorded a click.");
              };
                ad.OnAdFullScreenContentFailed += (AdError error) =>
              {
                  if (GameAnalytics.Initialized) AA_AnalyticsManager.Agent.AdEvent(GAAdAction.FailedShow, GAAdType.RewardedVideo);

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

        DestroyAdaptiveBannerAd();
        DestroySquareBannerAd();
        DestroyInterstitialAd();
        //DestroyMediumRectangleAd();
        //  this.appOpenAd.Destroy();

    }
    #region AD INSPECTOR

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
    public AdPosition bannerPosition;
    public string[] bannerAdId;
    public AdPosition adoptiveBannerPosition;
    public string[] adoptiveBannerAdIds;
    public bool preCacheInterstitial = true;
    public string[] interstitialAdIds;
    public bool preCacheRewarded = true;
    public string[] rewardedVideoAdIds;
    public bool preCacheRewardedInterstitial = true;
    public string[] rewardedInterstitialAdIds;
}
[Serializable]
public class GameLinks
{
    public string RateUsLink;
    public string MoreGamesLink;
    public string PrivacyLink;
}