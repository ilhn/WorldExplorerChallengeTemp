using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Analytics;

public class FirebaseHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("FIREBASE BASLADI");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        var dependencyStatus = task.Result;
        
        if (dependencyStatus == Firebase.DependencyStatus.Available) 
        {
            // Create and hold a reference to your FirebaseApp,
            // where app is a Firebase.FirebaseApp property of your application class.
            FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

            // Set a flag here to indicate whether Firebase is ready to use by your app.
            Debug.Log("FIREBASE INITIALIZE EDILDI");
        }
        else
        {
            UnityEngine.Debug.LogError(System.String.Format(
            "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            // Firebase Unity SDK is not safe to use here.
        }
        });
    }

    public void LogCustomEvent(string eventName, string parameterName1, string parameterValue1, string parameterName2, string parameterValue2)
    {
        FirebaseAnalytics.LogEvent(
            eventName,
            new Parameter(parameterName1, parameterValue1), // dogru sayisi
            new Parameter(parameterName2, parameterValue2)  // secilen zorluk seviyesi (question mode) ya da hotel trailer izlenme suresi (new mode) 
        );
        Debug.Log("CUSTOM EVENT IS BEING SENT...");
    }
}
