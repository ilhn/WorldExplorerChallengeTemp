using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
public class VideoLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer, trailerPlayer;
    public float delayForTrailer = 7.0f;
    private float trailerElapsedTime = -1.0f;
    private bool isTrailerPlaying = false;
    void Start()
    {
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "HotelTrailer_FinalCut_1080p.mp4");
        videoPlayer.url = videoPath;

        videoPlayer.Play();

        string trailerPath = System.IO.Path.Combine(Application.streamingAssetsPath, "HotelOpeningTrailer.mp4");
        trailerPlayer.url = trailerPath;
    }
    void Update()
    {
        if(isTrailerPlaying)
            trailerElapsedTime += Time.deltaTime;
    }
    void OnDisable()
    {
        ResetAllPlayers();
    }
    public void PlayTrailer(Button skipButton)
    {
        if (skipButton == null)
        {
            Debug.Log("Skip button is NULL...");
            return;
        }
        // Skip butonu başlangıçta devre dışı
        skipButton.interactable = false;

        TextMeshProUGUI skipButtonText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
        skipButtonText.text = delayForTrailer.ToString();

        trailerPlayer.Play();
        isTrailerPlaying = true;
        StartCoroutine(EnableSkipButtonAfterDelay(skipButton, skipButtonText));
    }
    private IEnumerator EnableSkipButtonAfterDelay(Button skipButton, TextMeshProUGUI skipButtonText)
    {
        Debug.Log("BURADAYIZZ");
        float accumulatedTime = 0f;
        delayForTrailer = 7f; // Başlangıçta 7 saniye beklenmesi gerektiğini varsayalım

        // Trailer oynuyor ve süre bitmediği sürece döngü çalışacak
        while (delayForTrailer > 0)
        {
            accumulatedTime += Time.deltaTime;

            if (accumulatedTime >= 1f)  // Her 1 saniyede bir
            {
                delayForTrailer--; // Kalan süreyi 1 azalt
                accumulatedTime = 0f; // Bir sonraki saniye için zaman biriktirmeyi sıfırla

                // Update the text in button
                skipButtonText.text = delayForTrailer.ToString();
                Debug.Log("Kalan süre: " + delayForTrailer);
            }
            yield return null; // Bir sonraki frame'e geç
        }
        // Süre tamamlandığında butonu aktif et
        Image buttonImage = skipButton.GetComponent<Image>(); // Butonun Image bileşenini al
        if (buttonImage != null)    buttonImage.color = Color.white; // Image'in rengini beyaz yap
        else Debug.Log("The image in the skip button is NULL...");
        
        skipButton.interactable = true;
        skipButtonText.text = "";

        Debug.Log("Time is done for waiting skip ad button...");
    }
    public void StopTrailer()
    {
        trailerPlayer.Stop();
        isTrailerPlaying = false;
    }
    public void ResetVideoPlayer(VideoPlayer player)
    {
        if(player == null)
            return;
        else
        {
            player.Stop();
            player.frame = 0;
            player.time = 0;
            player.url = null;
            player.clip = null;
        }
    }
    public void ResetAllPlayers()
    {
        ResetVideoPlayer(videoPlayer);
        ResetVideoPlayer(trailerPlayer);
    }
    public float GetTrailerElapsedTime()
    {
        return trailerElapsedTime;
    }
}
