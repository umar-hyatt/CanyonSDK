using System.Collections;
using System.Collections.Generic;
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
                OmmySDK.Agent.ShowRewardedAd(3);
                break;
            case 1:
                OmmySDK.Agent.ShowInterstitialAd(3);
                break;
            case 2:
                OmmySDK.Agent.ShowRewardedInterstitialAd();
                break;
            case 3:
                OmmySDK.Agent.ShowAdoptiveBanner();
                break;
            case 4:
                OmmySDK.Agent.HideAdaptiveBanner();
                break;
            case 5:
                OmmySDK.Agent.DestroyAdaptiveBannerAd();
                break;
            case 6:
                OmmySDK.Agent.ShowSquareBanner();
                break;
            case 7:
                OmmySDK.Agent.HideSquareBanner();
                break;
            case 8:
                OmmySDK.Agent.DestroySquareBannerAd();
                break;
            case 9:
                //GDPRManager.Agent.ResetConsentState();
                break;
        }
    }
    public void SetPreCache(bool value)
    {
        OmmySDK.Agent.myGameIds.preCacheInterstitial = value;
        OmmySDK.Agent.myGameIds.preCacheRewarded = value;
        OmmySDK.Agent.myGameIds.preCacheRewardedInterstitial = value;
    }
    public void OnClickShowInspector()
    {
        OmmySDK.Agent.OpenAdInspector();
    }
    public void OnChangeInterstitialAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(OmmySDK.Agent.myGameIds.interstitialAdId);
        }
        OmmySDK.Agent.myGameIds.interstitialAdId = value;
    }
    public void OnChangeSquareBannerAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(OmmySDK.Agent.myGameIds.squareBannerAdId);
        }
        OmmySDK.Agent.myGameIds.squareBannerAdId = value;
    }
    public void OnChangeRewardedAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(OmmySDK.Agent.myGameIds.rewardedVideoAdId);
        }
        OmmySDK.Agent.myGameIds.rewardedVideoAdId = value;
    }
    public void OnChangeRewardedInterstitialAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(OmmySDK.Agent.myGameIds.rewardedInterstitialAdId);
        }
        OmmySDK.Agent.myGameIds.rewardedInterstitialAdId = value;
    }
    public void OnChangeAdoptiveBannerAdId(string value)
    {
        if (value.Length <= 1)
        {
            CopyTextToClipboard(OmmySDK.Agent.myGameIds.adoptiveBannerAdId);
        }
        OmmySDK.Agent.myGameIds.adoptiveBannerAdId = value;
    }

    void CopyTextToClipboard(string text)
    {
        // Set the systemCopyBuffer property to the text you want to copy
        GUIUtility.systemCopyBuffer = text;

        // Print a message to the console to confirm that the text has been copied
        Debug.Log("Text copied to clipboard: " + text);
    }
}
