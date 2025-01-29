using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class ManageAdvertising : MonoBehaviour
{

//ca-app-pub-2244252108812173/4460801443
//kullanacagım gercek reklam idsi

//test reklam idsi
//"ca-app-pub-3940256099942544/1033173712"

// These ad units are configured to always serve test ads.
    #if UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-2244252108812173/4460801443";
    #else
    private string _adUnitId = "unused";
    #endif

    private InterstitialAd _interstitialAd;
    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            Debug.Log("AdMob SDK initialized.");
        });

        LoadInterstitialAd();
    }
    void OnDisable()
    {
        _interstitialAd.Destroy();
    }

    // Load the interstitial ad
    public void LoadInterstitialAd()
    {
        // Clean up the old ad before loading a new one.
        if (_interstitialAd != null)
        {
                _interstitialAd.Destroy();
                _interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        InterstitialAd.Load(_adUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                    "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                            + ad.GetResponseInfo());

                _interstitialAd = ad;
            });
    }

    ///
    /// Shows the interstitial ad.
    ///
    public void ShowInterstitialAd(Action onAdClosedCallback)
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            Debug.Log("Showing interstitial ad.");
            
            _interstitialAd.OnAdFullScreenContentClosed += () => onAdClosedCallback?.Invoke();
            
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("Interstitial ad is not ready yet.");
        }
    }
}
