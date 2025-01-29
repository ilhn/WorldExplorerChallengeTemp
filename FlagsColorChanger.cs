using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagColorChanger : MonoBehaviour
{
    public List<GameObject> flags; // Array to store your flag objects
    private float changeInterval = 0.075f; // Time interval for each flag to change color  //0.075f
    private float intervalIncreaseRate = 0.03f;    //0.030f
    private Vector3 currentScale; // flags current x, y scales
    private Vector3 enlargedScale; // flags enlarged x, y scales (as a default, current both x,y multiple by 2) 

    private Color colorAlphaZero = new Color(0f,0f,0f,0f);

    private HashSet<GameObject> usedFlags;

    public string randomCountry;
    private GameObject selectedFlag;
    public IEnumerator StartFlagAnimation(float duration, List<string> countries, Image flagImage)
    {
        float elapsedTime = 0.0f;
        float tempInterval = changeInterval;
        usedFlags = new HashSet<GameObject>();

        if (selectedFlag != null)
        {
            flags.Remove(selectedFlag);
        }

        while (elapsedTime < duration)
        {
            int randomIndex = Random.Range(0, flags.Count);
            selectedFlag = flags[randomIndex];

            while (usedFlags.Contains(selectedFlag))
            {
                if(usedFlags.Count == countries.Count)
                    break;
                randomIndex = Random.Range(0, flags.Count);
                selectedFlag = flags[randomIndex];
            }

            usedFlags.Add(selectedFlag);

            randomCountry = selectedFlag.name;
            LoadCountryFlag(randomCountry, flagImage); 

            // Başlangıç değerlerini ayarla
            Image flagRenderer = selectedFlag.GetComponent<Image>();
            currentScale = selectedFlag.transform.localScale;
            enlargedScale = new Vector3(currentScale.x * 1.5f, currentScale.y * 1.5f, currentScale.z);

            // Renk ve ölçek değişimi için zamanlayıcı
            float timer = 0f;

            // Değişim süresi boyunca çalışacak döngü
            while (timer < tempInterval)
            {
                // Zaman ilerlemesi
                timer += Time.deltaTime;

                // Renk ve ölçek değişimlerini hesapla
                float t_value = timer / tempInterval; // 0 ile 1 arasında bir değer

                flagRenderer.color = Color.white;
                selectedFlag.transform.localScale = Vector3.Lerp(currentScale, enlargedScale, t_value); // Ölçek değişimi

                yield return null; // Bir sonraki frame'i bekle
            }
            flagRenderer.color = Color.white;
            selectedFlag.transform.localScale = currentScale;

            // Bekleme süresi
            yield return new WaitForSeconds(changeInterval);

            elapsedTime += tempInterval;
            tempInterval += intervalIncreaseRate;
        }
    }
    public void StopFlagAnimation()
    {
        // Tüm bayrakları tekrar kirmizi yap
        foreach (GameObject flag in flags)
        {
            flag.GetComponent<Image>().color = colorAlphaZero;
        }
    }
    public string GetRandomCountry(List<string> allCountries) // also removes selected country from list
    {
        if (allCountries == null || allCountries.Count == 0)
        {
            Debug.LogWarning("allCountries listesi boş veya null!");
            return null;
        }

        int randomIndex = Random.Range(0, allCountries.Count);
        string selectedCountry = allCountries[randomIndex];
        
        Debug.Log("SECILEN ULKE");
        Debug.Log(selectedCountry);

        // remove selected country from list
        allCountries.RemoveAt(randomIndex);
        
        return selectedCountry;
    }
    public void LoadCountryFlag(string countryName, Image flagImage)
    {
        // Resources klasöründen bayrağı yükle
        Texture2D flagTexture = Resources.Load<Texture2D>($"countryFlags/{countryName}");
        
        if (flagTexture != null)
        {
            // Bayrak görselini Sprite'e çevir
            Sprite flagSprite = Sprite.Create(flagTexture, new Rect(0, 0, flagTexture.width, flagTexture.height), new Vector2(0.5f, 0.5f));
            flagImage.sprite = flagSprite; // Image bileşenine bayrağı ata
        }
        else
        {
            Debug.LogError($"Flag not found for country: {countryName}");
        }
    }
}