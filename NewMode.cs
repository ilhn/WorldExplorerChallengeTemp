using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;
using UnityEngine.EventSystems;
using TMPro;
public class NewMode : MonoBehaviour
{
    float transitionDuration = 0.1f;  // Geçiş süresi
    private string selectedCountry;

    List<string> asianCountries, northAmericanCountries, southAmericanCountries, africanCountries;
    List<string> centralEU, northernEU, southernEU;
    List<string> countryNames;
    List<string> fakeCountryNames;
    List<string> leftSideCountries;
    List<string> rightSideCountries;

    [SerializeField] private GameObject leftSideButtonContainer;
    [SerializeField] private GameObject rightSideButtonContainer;
    [SerializeField] private GameObject leftSidePanel, rightSidePanel;
    [SerializeField] private Button[] leftSideButtons, rightSideButtons;

    private RectTransform leftSideRectTransform;
    private RectTransform rightSideRectTransform;
    [SerializeField] private Image worldMapImage;
    [SerializeField] private Image mainMenuImage;
    private Button trueButton;
    private int questionCount;
    private int trueQuestionCount;
    private int falseQuestionCount;

    // class referance
    private ManageLoop ManageLoop;
    private ButtonEffect ButtonEffect;
    private ManageAdvertising ManageAdvertising;

    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TextMeshProUGUI endGameText;
    void Awake()
    {
        // init script referans
        ManageLoop = GetComponent<ManageLoop>();
        ButtonEffect = GetComponent<ButtonEffect>();
        ManageAdvertising = GetComponent<ManageAdvertising>();

        leftSideRectTransform = leftSidePanel.GetComponent<RectTransform>();
        rightSideRectTransform = rightSidePanel.GetComponent<RectTransform>();
        ClosePanels();

        countryNames = new List<string>();
        fakeCountryNames = new List<string>();
        leftSideCountries = new List<string>();
        rightSideCountries = new List<string>();

        // countryNames indices 0 - 7
        centralEU = new List<string>{"Germany", "Austria", "Croatia", "United Kingdom", "Hungary", "France", "Portugal", "Spain"};

        // countryNames indices 8 - 13
        southernEU = new List<string>{"Serbia", "Italy", "Türkiye", "Bulgaria", "Greece", "Romania"};

        // countryNames indices 14 - 21
        northernEU = new List<string>{"Iceland", "Norway", "Poland", "Ukraine", "Belarus", "Finland", "Russia", "Sweden"};

        // countryNames indices 22 - 30
        asianCountries = new List<string>{"Australia", "China", "India", "Indonesia", "Iran", "Iraq", "Japan", "Syria", "Thailand"};

        // countryNames indices 31 - 34
        northAmericanCountries = new List<string>{"Canada", "Greenland", "USA", "Mexico"};

        // countryNames indices 35 - 42
        southAmericanCountries = new List<string>{"Argentina", "Bolivia", "Brazil", "Colombia", "Paraguay", "Peru", "Uruguay", "Venezuela"};

        // coutnryNames indices 43 - 53
        africanCountries = new List<string>{"Algeria", "Cameroon", "Egypt", "Ghana", "Libya", "Madagascar", "Mali", "Nigeria", "Senegal", "South Africa", "Zambia"};
    }
    void Start()
    {
        leftSideCountries = northAmericanCountries.Concat(southAmericanCountries).ToList();
        rightSideCountries = centralEU.Concat(southernEU).Concat(northernEU).Concat(asianCountries).Concat(africanCountries).ToList();

        countryNames = centralEU
            .Concat(southernEU)
            .Concat(northernEU)
            .Concat(asianCountries)
            .Concat(northAmericanCountries)
            .Concat(southAmericanCountries)
            .Concat(africanCountries)
            .ToList();

        fakeCountryNames = countryNames;
        AddButtonListeners();
    }
    public void Run(Image worldMapImage)
    {
        Debug.Log("Kosuyom");
        mainMenuImage.enabled = false;
        if (worldMapImage == null)
            Debug.Log("image is null in Run method");

        // Varsayılan harita görüntüsünü yükle
        Texture2D textureEmptyMap = Resources.Load<Texture2D>("newModeDefaultImage/Empty");
        Sprite newModeStartSprite = Sprite.Create(textureEmptyMap, new Rect(0, 0, textureEmptyMap.width, textureEmptyMap.height), new Vector2(0.5f, 0.5f));
        worldMapImage.sprite = newModeStartSprite;
        StartAnimation(worldMapImage);
    }
    void StartAnimation(Image worldMapImage)
    {
        StartCoroutine(AnimateWorldMap(worldMapImage, fakeCountryNames));
    }
    IEnumerator AnimateWorldMap(Image worldMapImage, List<string> fakeCountryNames)
    {
        ManageLoop.PlayFlagAnimSound();
        // Kalan ülkeleri tutan liste
        List<string> remainingCountries = new List<string>(fakeCountryNames);
        int selectCount = 0;

        while (selectCount < 18)
        {
            // Rastgele bir ülke ismi seç
            int randomIndex = Random.Range(0, remainingCountries.Count);
            string countryName = remainingCountries[randomIndex];

            // Seçilen ülkenin görselini yükle
            Texture2D texture = Resources.Load<Texture2D>($"newModeImages/{countryName}");
            if (texture != null)
            {
                Sprite newModeSprite = Sprite.Create(texture, 
                                                     new Rect(0, 0, texture.width, texture.height), 
                                                     new Vector2(0.5f, 0.5f));
                worldMapImage.sprite = newModeSprite;

                // Geçiş süresini bekle
                yield return new WaitForSeconds(transitionDuration);
            }
            else
            {
                Debug.LogError($"Görsel bulunamadı: {countryName}");
            }
            // Seçilen ülkeyi listeden çıkar (tekrar seçilmesin diye)
            selectCount++;
            remainingCountries.RemoveAt(randomIndex);
            selectedCountry = countryName;
        }
        int countryIndex = GetCountryIndex(selectedCountry); // secilen ulkenin CountryNames icerisindeki indexini tutar
        GenerateWrongChoices(GetContinentByIndex(countryIndex)); // secilen ulkenin hangi kitada ya da bolgede oldugunu bulur ve butonlarin textini atar
        ManageLoop.StopFlagAnimSound();

        if (leftSideCountries.Contains(selectedCountry))
        {
            OpenRightSidePanel();
        }
        else if (rightSideCountries.Contains(selectedCountry))
        {
            OpenLeftSidePanel();
        }
        fakeCountryNames.Remove(selectedCountry);
    }
    int GetCountryIndex(string country)
    {
        return countryNames.IndexOf(country);
    }
    void SetButtonsInteractable(Button[] buttons, bool value)
    {
        if(buttons == null)
        {
            return;
        }
        else
        {    
            foreach(Button b in buttons)
            {
                if(b == null)
                    Debug.Log("Button b is NULL");
                else
                    b.interactable = value;
            }
        }
    }
    void GenerateWrongChoices(List<string> continent)
    {
        // Tüm butonları geçici olarak pasif hale getirin
        SetButtonsInteractable(leftSideButtons, false);
        SetButtonsInteractable(rightSideButtons, false);
        
        if(continent == null)
        {
            Debug.Log("secilen ulkenin ait oldugu kita listi NULL olarak secildi");
            return;
        }
        // Seçilen ülkeyi çıkar (tekrar seçeneklerde görünmesin)
        List<string> filteredCountries = new List<string>(continent);
        filteredCountries.Remove(selectedCountry);

        // Eğer yeterli sayıda ülke yoksa
        if (filteredCountries.Count < 3)
        {
            Debug.LogError("Yeterli sayıda yanlış seçenek yok.");
            return;
        }

        // Rastgele 3 yanlış seçenek seç
        List<string> wrongChoices = new List<string>();
        while (wrongChoices.Count < 3)
        {
            int randomIndex = Random.Range(0, filteredCountries.Count);
            string wrongChoice = filteredCountries[randomIndex];

            // Seçilen ülke zaten eklenmiş mi diye kontrol et
            if (!wrongChoices.Contains(wrongChoice))
            {
                wrongChoices.Add(wrongChoice);
                filteredCountries.RemoveAt(randomIndex);  // Eklenen seçeneği listeden çıkar
            }
        }

        // Seçenekleri butonlara ekle (seçilen ülkeyi de dahil ederek)
        List<string> allChoices = new List<string>(wrongChoices);
        allChoices.Add(selectedCountry);  // Doğru cevap eklenir

        // Seçenekleri karıştır
        allChoices = allChoices.OrderBy(x => Random.value).ToList();

        // Left ve right panel butonlarına seçenekleri ekle
        for (int i = 0; i < allChoices.Count; i++)
        {
            if (i < leftSideButtons.Length)
            {
                leftSideButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = allChoices[i];
            }
            if (i < rightSideButtons.Length)
            {
                rightSideButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = allChoices[i];
            }
        }
        // Set Buttons Active
        SetButtonsInteractable(leftSideButtons, true);
        SetButtonsInteractable(rightSideButtons, true);

        //Debug.Log("Seçenekler eklendi: " + string.Join(", ", allChoices));
    }

    List<string> GetContinentByIndex(int index)
    {
        if (index >= 0 && index <= 7) return centralEU;
        if (index >= 8 && index <= 13) return southernEU;
        if (index >= 14 && index <= 21) return northernEU;
        if (index >= 22 && index <= 30) return asianCountries;
        if (index >= 31 && index <= 34) return northAmericanCountries;
        if (index >= 35 && index <= 42) return southAmericanCountries;
        if (index >= 43 && index <= 53) return africanCountries;

        return null;
    }

    void AddButtonListeners()
    {
        foreach (Button b in leftSideButtons)
        {
            b.onClick.AddListener(() =>
            {
                string inputStringLeft = EventSystem.current.currentSelectedGameObject.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
                SetButtonsInteractable(leftSideButtons, false);
                SetAnswerValue(inputStringLeft);
                StartCoroutine(ButtonEffect.AnswerQuestion(b, CloseLeftSidePanel));
            });           
        }

        foreach (Button c in rightSideButtons)
        {
            c.onClick.AddListener(() =>
            {
                string inputStringRight = EventSystem.current.currentSelectedGameObject.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
                SetButtonsInteractable(rightSideButtons, false);
                SetAnswerValue(inputStringRight);
                StartCoroutine(ButtonEffect.AnswerQuestion(c, CloseRightSidePanel));
            });         
        }
    }
    void ShowEndGamePanel()
    {
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            endGamePanel.transform.localScale = Vector3.zero;
            endGamePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        else
            Debug.Log("endGamePanel is NULL..");
    }
    // Panels Hiding and Showing Methods
    //
    // Hiding
    void CloseLeftSidePanel()
    {
        leftSideRectTransform.DOAnchorPosY(Screen.height*3, 0.75f).SetEase(Ease.InBack).OnComplete(() =>
        {          
            selectedCountry = null;
            if (questionCount < 10)
                Run(worldMapImage);
            else
            {
                ManageLoop.correctAnswerCount = trueQuestionCount;
                ManageLoop.wrongAnswerCount = falseQuestionCount;
                ManageLoop.GetReadyForEndgame(true);
            }
        }); 
    }
    void CloseRightSidePanel()
    {
        rightSideRectTransform.DOAnchorPosY(Screen.height*3, 0.75f).SetEase(Ease.InBack).OnComplete(() =>
        {            
            selectedCountry = null;
            if (questionCount < 10)
                Run(worldMapImage);
            else
            {
                ManageLoop.correctAnswerCount = trueQuestionCount;
                ManageLoop.wrongAnswerCount = falseQuestionCount;
                ManageLoop.GetReadyForEndgame(true);
            }
        });
    }
    void SetAnswerValue(string inputText)
    {
        if(inputText == null)
            Debug.Log("inputText is NULL");
        else
        {    
            if (inputText.Equals(selectedCountry))
            {
                ButtonEffect.answerValue = 1;
                trueQuestionCount++;
                ManageLoop.PlayCorrectAnswerSound();
            }
            else
            {
                ButtonEffect.answerValue = -1;
                falseQuestionCount++;
                ManageLoop.PlayWrongAnswerSound();
                
                TrueButtonCheck();
                StartCoroutine(ButtonEffect.AnswerQuestion2(trueButton));
                trueButton = null;
            }
            questionCount++;
        }
    }
    void TrueButtonCheck()
    {
        if (leftSideCountries.Contains(selectedCountry))
        {
            foreach (var button in rightSideButtons)
            {
                if (button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == selectedCountry)
                {
                    trueButton = button;
                }
            }
        }

        else if (rightSideCountries.Contains(selectedCountry))
        {
            foreach (var button in leftSideButtons)
            {
                if (button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == selectedCountry)
                {
                    trueButton = button;
                }
            }
        }
    }
    void ClosePanels()
    {
        leftSidePanel.SetActive(false);
        rightSidePanel.SetActive(false);
    }
    //
    // Opening
    void OpenLeftSidePanel()
    {
        if(leftSidePanel == null)
            Debug.Log("Left Side Panel is NULL");
        else
        {
            leftSidePanel.SetActive(true);
            SetButtonsInteractable(leftSideButtons, true);

            leftSideRectTransform.anchoredPosition = new Vector2(leftSideRectTransform.anchoredPosition.x, Screen.height);

            // Paneli aşağıya doğru hedef pozisyona taşıyoruz (örneğin, y = 0 pozisyonuna)
            leftSideRectTransform.DOAnchorPosY(0, 1.5f).SetEase(Ease.OutQuad); // 1 saniyede, bounce etkisiyle aşağıya düşüyor

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    void OpenRightSidePanel()
    {
        if(rightSidePanel == null)
            Debug.Log("Right Side Panel is NULL");
        else
        {
            rightSidePanel.SetActive(true);
            SetButtonsInteractable(rightSideButtons, true);

            rightSideRectTransform.anchoredPosition = new Vector2(rightSideRectTransform.anchoredPosition.x, Screen.height);

            // Paneli aşağıya doğru hedef pozisyona taşıyoruz (örneğin, y = 0 pozisyonuna)
            rightSideRectTransform.DOAnchorPosY(0, 1.5f).SetEase(Ease.OutQuad); // 1 saniyede, bounce etkisiyle aşağıya düşüyor

            EventSystem.current.SetSelectedGameObject(null);  
        }
    }
}


