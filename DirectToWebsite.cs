using UnityEngine;

public class DirectToWebsite : MonoBehaviour
{
    private string urlSteam = "https://store.steampowered.com/app/3158480"; // direct to steam page of hotel owner simulator
    private string urlYoutube = "https://youtu.be/gNQYfp3aMP4?si=DxhBoLqLpls2j0Q0"; // hotel owner simulator youtube trailer link

    public void OpenURL()
    {
        Application.OpenURL(urlSteam);
    }
    public void OpenClip()
    {
        Application.OpenURL(urlYoutube);
    }
}
