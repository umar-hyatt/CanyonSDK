using System.Drawing;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomEditor(typeof(OmmySDK))]
public class CanyonSDKEditor : Editor
{
    SerializedProperty adLoadingPanel;
    SerializedProperty showBannerInStartProp;
    SerializedProperty useTestIDsProp;
    SerializedProperty loadNextSceneProp;
    SerializedProperty removeAdProp;
    SerializedProperty InternetRequired;
    SerializedProperty tagForChild;
    SerializedProperty Links;

    SerializedProperty adoptiveBannerProp;
    SerializedProperty adoptiveBannerPosition;
    SerializedProperty adoptiveBannerAdIdProp;

    SerializedProperty squareBannerProp;
    SerializedProperty squareBannerPosition;
    SerializedProperty squareBannerAdIdProp;

    SerializedProperty interstitialProp;
    SerializedProperty preCacheInterstitialProp;
    SerializedProperty interstitialAdIdProp;

    SerializedProperty rewardedProp;
    SerializedProperty preCacheRewardedProp;
    SerializedProperty rewardedVideoAdIdProp;

    SerializedProperty rewardedInterstitialProp;
    SerializedProperty preCacheRewardedInterstitialProp;
    SerializedProperty rewardedInterstitialAdIdProp;
    void OnEnable()
    {
        adLoadingPanel = serializedObject.FindProperty(nameof(adLoadingPanel));
        showBannerInStartProp = serializedObject.FindProperty("showBannerInStart");
        useTestIDsProp = serializedObject.FindProperty("useTestIDs");
        loadNextSceneProp = serializedObject.FindProperty("loadNextScene");
        removeAdProp = serializedObject.FindProperty("removeAd");
        InternetRequired = serializedObject.FindProperty(nameof(InternetRequired));
        tagForChild = serializedObject.FindProperty(nameof(tagForChild));
        Links = serializedObject.FindProperty(nameof(Links));

        adoptiveBannerProp = serializedObject.FindProperty("myGameIds.adaptiveBanner");
        adoptiveBannerPosition = serializedObject.FindProperty("myGameIds.adoptiveBannerPosition");
        adoptiveBannerAdIdProp = serializedObject.FindProperty("myGameIds.adoptiveBannerAdId");

        squareBannerProp = serializedObject.FindProperty("myGameIds.squareBanner");
        squareBannerPosition = serializedObject.FindProperty("myGameIds.squareBannerPosition");
        squareBannerAdIdProp = serializedObject.FindProperty("myGameIds.squareBannerAdId");

        interstitialProp = serializedObject.FindProperty("myGameIds.interstitial");
        preCacheInterstitialProp = serializedObject.FindProperty("myGameIds.preCacheInterstitial");
        interstitialAdIdProp = serializedObject.FindProperty("myGameIds.interstitialAdId");

        rewardedProp = serializedObject.FindProperty("myGameIds.rewarded");
        preCacheRewardedProp = serializedObject.FindProperty("myGameIds.preCacheRewarded");
        rewardedVideoAdIdProp = serializedObject.FindProperty("myGameIds.rewardedVideoAdId");

        rewardedInterstitialProp = serializedObject.FindProperty("myGameIds.rewardedInterstitial");
        preCacheRewardedInterstitialProp = serializedObject.FindProperty("myGameIds.preCacheRewardedInterstitial");
        rewardedInterstitialAdIdProp = serializedObject.FindProperty("myGameIds.rewardedInterstitialAdId");

        int selectedAdTypes = EditorPrefs.GetInt("SelectedAdTypes", GetAdTypesMask());

        // Update adTypeSelected array based on selectedAdTypes bitmask
        for (int i = 0; i < adTypeCount; i++)
        {
            adTypeSelected[i] = (selectedAdTypes & (1 << i)) != 0;
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUIStyle gUIStyle = new GUIStyle();
        gUIStyle.alignment = TextAnchor.UpperLeft;
        gUIStyle.fontSize = 10;
        EditorGUILayout.LabelField("Ommy SDK v2024.2.5", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.PropertyField(adLoadingPanel);
        EditorGUILayout.PropertyField(loadNextSceneProp);
        EditorGUILayout.PropertyField(removeAdProp);
        EditorGUILayout.PropertyField(InternetRequired);
        EditorGUILayout.PropertyField(useTestIDsProp);

        EditorGUILayout.Space();

        // Dropdown for Ad Types
        EditorGUILayout.LabelField("Ads Setting", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(tagForChild);
        int selectedAdTypes = EditorGUILayout.MaskField("Select Ad Types", GetAdTypesMask(), GetAdTypeNames()); 
        // Update adTypeSelected array based on selectedAdTypes bitmask
        for (int i = 0; i < adTypeCount; i++)
        {
            adTypeSelected[i] = (selectedAdTypes & (1 << i)) != 0;
        }

        // Enable/Disable Fields based on selected Ad Types
        EditorGUI.indentLevel++;
        for (int i = 0; i < adTypeCount; i++)
        {
            bool adTypeSelected = this.adTypeSelected[i];

            switch (i)
            {
                case 0: // Adoptive Banner
                    if (!adTypeSelected) 
                    {
                    adoptiveBannerProp.boolValue=false;
                    break;
                    }
                    adoptiveBannerProp.boolValue=true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Adoptive Banner", EditorStyles.whiteLabel);
                    EditorGUILayout.PropertyField(showBannerInStartProp, new GUIContent("Show Banner In Start", "Adoptive Banner"));
                    EditorGUILayout.PropertyField(adoptiveBannerPosition, new GUIContent("Banner Position", "Adoptive Banner"));
                    if (adTypeSelected && !useTestIDsProp.boolValue)
                        EditorGUILayout.PropertyField(adoptiveBannerAdIdProp, new GUIContent("Ad ID", "Adoptive Banner"));
                    break;
                case 1: // Square Banner
                    if (!adTypeSelected)
                    { 
                        squareBannerProp.boolValue=false;
                        break;
                    }
                    squareBannerProp.boolValue=true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Square Banner", EditorStyles.whiteLabel);
                    EditorGUILayout.PropertyField(squareBannerPosition, new GUIContent("Banner Position", "Square Banner"));
                    if (adTypeSelected && !useTestIDsProp.boolValue)
                        EditorGUILayout.PropertyField(squareBannerAdIdProp, new GUIContent("Ad ID", "Square Banner"));
                    break;
                case 2: // Interstitial
                    if (!adTypeSelected)
                    { 
                        interstitialProp.boolValue=false;
                        break;
                    }
                    interstitialProp.boolValue=true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Interstitial", EditorStyles.whiteLabel);
                    EditorGUILayout.PropertyField(preCacheInterstitialProp, new GUIContent("Pre-Cache Interstitial", "Interstitial"));
                    if (adTypeSelected && !useTestIDsProp.boolValue)
                        EditorGUILayout.PropertyField(interstitialAdIdProp, new GUIContent("Ad ID", "Interstitial"));
                    break;
                case 3: // Rewarded Video
                    if (!adTypeSelected)
                    { 
                        rewardedProp.boolValue=false;
                        break;
                    }
                    rewardedProp.boolValue=true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Rewarded Video", EditorStyles.whiteLabel);
                    EditorGUILayout.PropertyField(preCacheRewardedProp, new GUIContent("Pre-Cache Rewarded Video", "Rewarded Video"));
                    if (adTypeSelected && !useTestIDsProp.boolValue)
                        EditorGUILayout.PropertyField(rewardedVideoAdIdProp, new GUIContent("Ad ID", "Rewarded Video"));
                    break;
                case 4: // Rewarded Interstitial
                    if (!adTypeSelected)
                    { 
                        rewardedInterstitialProp.boolValue=false;
                        break;
                    }
                    rewardedInterstitialProp.boolValue=true;
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Rewarded Interstitial", EditorStyles.whiteLabel);
                    EditorGUILayout.PropertyField(preCacheRewardedInterstitialProp, new GUIContent("Pre-Cache Rewarded Interstitial", "Rewarded Interstitial"));
                    if (adTypeSelected && !useTestIDsProp.boolValue)
                        EditorGUILayout.PropertyField(rewardedInterstitialAdIdProp, new GUIContent("Ad ID", "Rewarded Interstitial"));
                    break;
                    // Add more cases for additional ad types as needed
            }
        }
        EditorGUI.indentLevel--;

        if (Event.current.type == EventType.Layout)
        {
            int mask = GetAdTypesMask();
            EditorPrefs.SetInt("SelectedAdTypes", mask); 
        }


        EditorGUILayout.PropertyField(Links);
        serializedObject.ApplyModifiedProperties();
    }

    // Helper method to get the bitmask of the currently selected ad types
    private int GetAdTypesMask()
    {
        int mask = 0;
        for (int i = 0; i < adTypeCount; i++)
        {
            if (adTypeSelected[i])
            {
                mask |= (1 << i);
            }
        }
        return mask;
    }

    // Helper method to get the names of ad types for the dropdown
    private string[] GetAdTypeNames()
    {
        return new string[] { "Adoptive Banner", "Square Banner", "Interstitial", "Rewarded Video", "Rewarded Interstitial" };
        // Add more names for additional ad types as needed
    }

    // ...

    private const int adTypeCount = 5; // Update this if you add more ad types
    private bool[] adTypeSelected = new bool[adTypeCount];
}
