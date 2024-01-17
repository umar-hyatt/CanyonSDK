using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
public class TestSDK : MonoBehaviour
{
    public Dropdown dropdown;
    public void CallButton()
    {
        switch (dropdown.value)
        {
            case 0:
                CanyonSDK.Agent.ShowRewardedAd();
                break;
            case 1:
                CanyonSDK.Agent.ShowInterstitialAd();
                break;
            case 2:
                CanyonSDK.Agent.ShowRewardedInterstitialAd();
                break;
            case 3:
                CanyonSDK.Agent.ShowAdoptiveBanner();
                break;
            case 4:
                CanyonSDK.Agent.HideAdaptiveBanner();
                break;
            case 5:
                CanyonSDK.Agent.DestroyAdaptiveBannerAd();
                break;
            case 6:
                CanyonSDK.Agent.ShowSquareBanner();
                break;
            case 7:
                CanyonSDK.Agent.HideSquareBanner();
                break;
            case 8:
                CanyonSDK.Agent.DestroySquareBannerAd();
                break;
            case 9:
                //GDPRManager.Agent.ResetConsentState();
                break;
        }
    }
    public void SetPreCache(bool value)
    {
        CanyonSDK.Agent.myGameIds.preCacheInterstitial = value;
        CanyonSDK.Agent.myGameIds.preCacheRewarded = value;
        CanyonSDK.Agent.myGameIds.preCacheRewardedInterstitial = value;
    }
    public void OnClickShowInspector()
    {
        CanyonSDK.Agent.OpenAdInspector();
    }
    public void OnChangeInterstitialAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(CanyonSDK.Agent.myGameIds.interstitialAdIds[0]);
        }
        CanyonSDK.Agent.myGameIds.interstitialAdIds[0] = value;
    }
    public void OnChangeSquareBannerAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(CanyonSDK.Agent.myGameIds.bannerAdId[0]);
        }
        CanyonSDK.Agent.myGameIds.bannerAdId[0] = value;
    }
    public void OnChangeRewardedAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(CanyonSDK.Agent.myGameIds.rewardedVideoAdIds[0]);
        }
        CanyonSDK.Agent.myGameIds.rewardedVideoAdIds[0] = value;
    }
    public void OnChangeRewardedInterstitialAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(CanyonSDK.Agent.myGameIds.rewardedInterstitialAdIds[0]);
        }
        CanyonSDK.Agent.myGameIds.rewardedInterstitialAdIds[0] = value;
    }
    public void OnChangeAdoptiveBannerAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(CanyonSDK.Agent.myGameIds.adoptiveBannerAdIds[0]);
        }
        CanyonSDK.Agent.myGameIds.adoptiveBannerAdIds[0] = value;
    }

    void CopyTextToClipboard(string text)
    {
        // Set the systemCopyBuffer property to the text you want to copy
        GUIUtility.systemCopyBuffer = text;

        // Print a message to the console to confirm that the text has been copied
        Debug.Log("Text copied to clipboard: " + text);
    }
}
