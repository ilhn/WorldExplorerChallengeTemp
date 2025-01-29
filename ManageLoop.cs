using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ManageLoop : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    [SerializeField] private AudioSource correctAnswerAudioSource, wrongAnswerAudioSource, buttonClickAudioSource, flagAnimAudioSource, themeAudioSource;
    [SerializeField] private Image flagImage;
    [SerializeField] private Button[] replyButtons, openingMenuButtons;
    [SerializeField] private Button buttonEasy, buttonMedium, buttonHard, buttonGoBack, buttonMenu;
    [SerializeField] private Button buttonMenuLSP, buttonMenuRSP, buttonBackMainMenu, buttonCreditsMainMenu, skipAdButton;
    [SerializeField] private Button buttonConsentAgree, buttonConsentDecline;
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private GameObject openingPanel, difficultyPanel, endGamePanel, subPanel, creditsPanel, videoPanel, trailerPanel, playerConsentPanel;
    [SerializeField] private GameObject timeBar;
    [SerializeField] private Image worldMapImage;
    [SerializeField] private TextMeshProUGUI questionNumberTMP, correctQuestionNumberTMP, wrongQuestionNumberTMP;
    
    private TextMeshProUGUI questionText, endGameText;
    private int currentQuestionIndex;
    public static int wrongAnswerCount, correctAnswerCount;
    private int staticCurrentQuestionNumber;
    private List<Question> currentQuestions;
    private List<string> remainingCountries;

    // class referances
    [SerializeField] private FirebaseHandler firebaseHandlerInstance;
    private LoadData loadData;
    private FlagColorChanger flagColorChanger;
    private ButtonEffect buttonEffect;
    private NewMode newMode;
    private ManageAdvertising manageAdvertising;
    private VideoLoader videoLoader;
    

    private List<string> allCountries;
    //private string removedCountry;
    private Coroutine timerCoroutine;
    //private AudioSource clockAudioSource;
    private bool userAnswered = false, isGameStarting = true, isPopulated = false, isConsentGranted = true;
    private float durationTime = 20.0f; // Fixed duration time for questions
    private float trailerViewTime = 0.0f;
    private const float durationFlagAnimation = 2.5f;
    private const string consentKey = "HasConsent";
    private Image timeBarImage;
    private string selectedDifficulty;
    private int currentRound = 0;

    [SerializeField] private Image mainMenuImage;
    [SerializeField] private Sprite blueButton;

    void Awake()
    {
        PlayThemeSound();
        
        if (PlayerPrefs.GetInt(consentKey, 0) == 1)
        {
            // daha once panel gosterildi, paneli kapat
            ClosePlayerConsentPanel();
        }
        else
        {
            // daha once panel gosterilmedi, paneli göster ve gosterildigini kaydet
            PlayerPrefs.SetInt(consentKey, 1);
            PlayerPrefs.Save();
            ShowPlayerConsentPanel();
        }
        

        remainingCountries = new List<string>();

        // script referances
        loadData = GetComponent<LoadData>();
        flagColorChanger = GetComponent<FlagColorChanger>();
        buttonEffect = GetComponent<ButtonEffect>();
        newMode = GetComponent<NewMode>();
        manageAdvertising = GetComponent<ManageAdvertising>();
        videoLoader = GetComponent<VideoLoader>();

        // find and initialize gameobjects within code
        questionPanel = GameObject.Find("Canvas/QuestionPanel");
        openingPanel = GameObject.Find("Canvas/OpeningMenuPanel");
        timeBar = GameObject.Find("Canvas/QuestionPanel/TimeBarField");
        difficultyPanel = GameObject.Find("Canvas/DifficultyPanel");
        endGamePanel = GameObject.Find("Canvas/EndingPanel");
        subPanel = GameObject.Find("Canvas/SubPanel");
        creditsPanel = GameObject.Find("Canvas/CreditsPanel");
        videoPanel = GameObject.Find("Canvas/VideoPanel");
        trailerPanel = GameObject.Find("Canvas/TrailerPanel");

        questionText = questionPanel.transform.Find("QuestionTextContainer/QuestionText").GetComponent<TextMeshProUGUI>();
        endGameText = endGamePanel.transform.Find("ButtonContainer/EndGameText").GetComponent<TextMeshProUGUI>();
        timeBarImage = timeBar.transform.Find("TimeBar").GetComponent<Image>();

        // get all countries names from database
        allCountries = loadData.GetAllCountries();
        HideQuizField();
    }
    void Start()
    {   
        correctAnswerCount = 0;
        wrongAnswerCount = 0;
        Application.targetFrameRate = 60;
        ShowVideoPanel();
        closePanels();


        if (correctAnswerAudioSource != null)
            correctAnswerAudioSource.enabled = true; // Aktif et

        if (wrongAnswerAudioSource != null)
            wrongAnswerAudioSource.enabled = true;

        if (buttonClickAudioSource != null)
            buttonClickAudioSource.enabled = true;

        if (flagAnimAudioSource != null)
            flagAnimAudioSource.enabled = true;

        if (themeAudioSource != null)
            themeAudioSource.enabled = true;

        buttonEasy.onClick.AddListener(() => GetReadyForQuestionMode("easy"));
    
        buttonMedium.onClick.AddListener(() => GetReadyForQuestionMode("medium"));

        buttonHard.onClick.AddListener(() => GetReadyForQuestionMode("hard"));

        buttonGoBack.onClick.AddListener(() => 
        {
            PlayButtonClickSound();
            SetDifficultyButtonsInteractable(false);
            SetButtonsInteractable(openingMenuButtons,true);

            BackToMenu();
            isGameStarting = true;
            SetDifficultyButtonsInteractable(true);
        });
        buttonBackMainMenu.onClick.AddListener(() => 
        {
            GetReadyToReturn();
            SceneManager.LoadScene("GameScene");
        });
        buttonCreditsMainMenu.onClick.AddListener(() => 
        {
            CloseCreditsPanel();
            ShowOpeningPanel();
        });
        buttonMenu.onClick.AddListener(() => 
        {
            GetReadyToReturn();
            SceneManager.LoadScene("GameScene");
        });
        buttonMenuLSP.onClick.AddListener(() => 
        {
            GetReadyToReturn();
            SceneManager.LoadScene("GameScene");
        });
        buttonMenuRSP.onClick.AddListener(() => 
        {
            GetReadyToReturn();
            SceneManager.LoadScene("GameScene");
        });
        skipAdButton.onClick.AddListener(() =>
        {
            videoLoader.ResetVideoPlayer(videoLoader.trailerPlayer);
            CloseTrailerPanel();
            
            trailerViewTime = videoLoader.GetTrailerElapsedTime();
            if(isConsentGranted)
            {
            // Triggering Firebase Custom Event for Question Mode
            SendFirebaseCustomEvent("New Mode Data", "Correct Ans", correctAnswerCount.ToString(),
                                                        "Trailer View Time", trailerViewTime.ToString());
            }

            ShowEndGamePanel();
            //Debug.Log("Izlenme suresi : " + trailerViewTime);
        });
        buttonConsentAgree.onClick.AddListener(() => 
        {
            PlayButtonClickSound();
            isConsentGranted = true;
            ClosePlayerConsentPanel();
            ShowOpeningPanel();
        });
        buttonConsentDecline.onClick.AddListener(() => 
        {
            PlayButtonClickSound();
            isConsentGranted = false;
            ClosePlayerConsentPanel();
            ShowOpeningPanel();
        });
        
        // Start New Game Button Actions
        openingMenuButtons[0].onClick.AddListener(() =>
        {
            SetButtonsInteractable(openingMenuButtons, false);
            
            if(isGameStarting)
                PlayButtonClickSound();
            StartNewGame();
        });
        
        // Start New Mode Button Actions
        openingMenuButtons[1].onClick.AddListener(() =>
        {
            SetButtonsInteractable(openingMenuButtons, false);
            
            PlayButtonClickSound();
            CloseVideoPanel();
            StartNewMode();
        });

        // Credits Button Actions
        openingMenuButtons[2].onClick.AddListener(() =>
        {
            SetButtonsInteractable(openingMenuButtons, false);
            PlayButtonClickSound();
            ShowCreditsPanel();
        });

        // Exit Button Actions
        openingMenuButtons[3].onClick.AddListener(() =>
        {
            SetButtonsInteractable(openingMenuButtons, false);
            PlayButtonClickSound();
            Application.Quit();
        });
    }
    private void Update()
    {
        correctQuestionNumberTMP.text = "True : " + correctAnswerCount.ToString();
        wrongQuestionNumberTMP.text = "False : " + wrongAnswerCount.ToString();
    }
    void OnDisable()
    {
        if (correctAnswerAudioSource != null && correctAnswerAudioSource.isPlaying)
        {
            correctAnswerAudioSource.Stop();
            correctAnswerAudioSource.enabled = false;
        }
        if (wrongAnswerAudioSource != null && wrongAnswerAudioSource.isPlaying)
        {
            wrongAnswerAudioSource.Stop();
            wrongAnswerAudioSource.enabled = false;
        }
        if (buttonClickAudioSource != null && buttonClickAudioSource.isPlaying)
        {
            buttonClickAudioSource.Stop();
            buttonClickAudioSource.enabled = false;
        }
        if (flagAnimAudioSource != null && flagAnimAudioSource.isPlaying)
        {
            flagAnimAudioSource.Stop();
            flagAnimAudioSource.enabled = false;
        }
        if (themeAudioSource != null && themeAudioSource.isPlaying)
        {
            themeAudioSource.Stop();
            themeAudioSource.enabled = false;
        }
    }
    private void GetReadyToReturn()
    {
        correctAnswerCount = 0; 
        wrongAnswerCount = 0;
        // Clean the DOTween actions on the scene
        DOTween.KillAll();

        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None); // Find all buttons in scene
        foreach(Button tempButton in buttons)
            tempButton.onClick.RemoveAllListeners(); // remove all listeners from buttons on the scene

        StopAllCoroutines(); // stop all the coroutines
    }
    private void GetReadyForQuestionMode(string textDifficulty)
    {
        PlayButtonClickSound();
        CloseVideoPanel();
        SetDifficultyButtonsInteractable(false);
        HandleDifficultySelection(textDifficulty);
    }
    public void GetReadyForEndgame(bool showCustomAd)
    {
        endGameText.text = string.Format("QUIZ COMPLETED\n\nWRONG ANSWERS : {0}\nCORRECT ANSWERS : {1}", wrongAnswerCount, correctAnswerCount);
        CloseSubPanel();
        
        if(showCustomAd) // new mode is encountered
        {
            ShowTrailerPanel();
            videoLoader.PlayTrailer(skipAdButton);
        }
        else // question mode (default mode) encountered
        {
            if(isConsentGranted)
            {
            // Triggering Firebase Custom Event for Selected Mode
            SendFirebaseCustomEvent("Quest Mode Data", "Correct Ans", correctAnswerCount.ToString(),
                                                        "Selected Difficulty", selectedDifficulty);
            }

            manageAdvertising.ShowInterstitialAd(() => ShowEndGamePanel());
        }
        //Debug.Log("Quiz Completed");
    }
    public void StartNewGame()
    {
        //Debug.Log("Starting a new game");
        isGameStarting = false;
        CloseOpeningPanel(() => 
        {
            ShowDifficultyPanel();
        });
    }
    public void StartNewMode()
    {   
        if(worldMapImage == null)
            Debug.Log("image is null");

        //Debug.Log(worldMapImage);
        StopThemeSound();

        CloseOpeningPanel(() => 
        {
            newMode.Run(worldMapImage);
        });
    }
    void HandleDifficultySelection(string difficulty)
    {
        if(difficulty != null && difficulty.Equals("medium"))
            durationTime = 25.0f;
        else if(difficulty != null && difficulty.Equals("hard"))
            durationTime = 30.0f;

        selectedDifficulty = difficulty;
        CloseDifficultyPanel();
        StopThemeSound();
        mainMenuImage.enabled = false;
        StartCoroutine(FlagAnimationAndQuiz());
    }
    IEnumerator StartTimer()
    {
        float remainingTime = durationTime; // Use a local variable for countdown

        // ensure that timebar is filled at the beginning
        timeBarImage.fillAmount = 1.0f;

        while (remainingTime >= 0)
        {
            // Geçen süreyi çıkar
            remainingTime -= Time.deltaTime;

            // Zaman çubuğunu güncelle
            timeBarImage.fillAmount = Mathf.Clamp01(remainingTime / durationTime);

            // Bir sonraki kareyi bekle
            yield return null;
        }
        HandleTimeUp(); // Call to handle when time is up
    }
    void HandleTimeUp()
    {
        //Debug.Log("HandleTimeUp called"); // Burayı ekleyin
        userAnswered = true; // Mark as answered
        currentQuestionIndex++;

        //Debug.Log($"Current Question Index: {currentQuestionIndex}, Total Questions: {currentQuestions.Count}");

        if (currentQuestionIndex < currentQuestions.Count)
        {
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            //Debug.Log("All questions answered in this round!");
            HideQuizField(); // hide questionpanel at the end of answering 3rd question for each country
            
            if(currentRound < 10)
            {
                if(staticCurrentQuestionNumber >= 30)
                {
                    GetReadyForEndgame(false);
                }
                else
                    StartCoroutine(FlagAnimationAndQuiz());
            }
            else
            {
                GetReadyForEndgame(false);
            }
        }
    }
    void PopulateRemainingCountries()
    {
        remainingCountries.Clear();
        List<Question> questions = loadData.GetQuestionsByDifficulty(selectedDifficulty);

        foreach(var question in questions)
            if(!remainingCountries.Contains(question.country))
                remainingCountries.Add(question.country);
    }
    IEnumerator FlagAnimationAndQuiz()
    {
        HideQuizField();
        // start flag anim sound
        PlayFlagAnimSound();

        yield return StartCoroutine(flagColorChanger.StartFlagAnimation(durationFlagAnimation, allCountries, flagImage));

        flagColorChanger.StopFlagAnimation();
        //stop flag anim sound
        StopFlagAnimSound();

        ShowQuizField();
        
        if(!isPopulated)
        {
            PopulateRemainingCountries();
            isPopulated = true;
        }
        string selectedCountry = flagColorChanger.randomCountry;
        currentQuestions = LoadQuestionsFromCountry(selectedCountry);
        currentQuestionIndex = 0;

        LoadCountryFlag(selectedCountry);
        LoadQuestion(currentQuestionIndex);
    }
    void LoadCountryFlag(string countryName)
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
    string SelectCountry()
    {
        if (remainingCountries.Count == 0)
        {
            Debug.LogWarning("No remaining countries to select from!");
            return null; // veya uygun bir değer döndürün
        }

        int randomIndex = UnityEngine.Random.Range(0, remainingCountries.Count);
        string selectedCountry = remainingCountries[randomIndex];
        remainingCountries.RemoveAt(randomIndex);

        return selectedCountry;
    }
    List<Question> LoadQuestionsFromCountry(string country)
    {
        List<Question> questionsFromCountry = loadData.GetQuestionsByDifficulty(selectedDifficulty);
        questionsFromCountry = questionsFromCountry.FindAll(q => q.country == country);

        if (questionsFromCountry.Count >= 3)
        {
            // Shuffle the questions and select the first 3
            List<Question> selectedQuestions = new List<Question>();
            for (int i = 0; i < 3; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, questionsFromCountry.Count);
                selectedQuestions.Add(questionsFromCountry[randomIndex]);
                questionsFromCountry.RemoveAt(randomIndex); // Remove the selected question
            }
            return selectedQuestions;
        }
        else
        {
            Debug.LogError("Not enough questions for the selected country.");
            return new List<Question>();
        }
    }
    void LoadQuestion(int questionIndex)
    {
        
        SetButtonsInteractable(replyButtons, true);

        if (questionIndex < currentQuestions.Count)
        {
            staticCurrentQuestionNumber++;
            questionNumberTMP.text = "QUESTION : " + staticCurrentQuestionNumber.ToString() + "/30";

            Question currentQuestion = currentQuestions[questionIndex];
            questionText.text = currentQuestion.questionText;

            RectTransform rectTransformQuestion = questionText.GetComponent<RectTransform>();
            rectTransformQuestion.localScale = Vector3.zero;
            rectTransformQuestion.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);

            //clockAnimator.SetBool("isAnimating", false);
            //clockAudioSource.Stop();


            for (int i = 0; i < replyButtons.Length; i++)
            {
                replyButtons[i].GetComponent<Image>().sprite = blueButton;
                RectTransform rectTransform = replyButtons[i].GetComponent<RectTransform>();
                rectTransform.localScale = Vector3.zero;
                rectTransform.DOScale(Vector3.one, 1f).SetEase(Ease.OutBack);

                replyButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.options[i];
                int answerIndex = i; // Capture the current index for the button's listener
                replyButtons[i].onClick.RemoveAllListeners();
                replyButtons[i].onClick.AddListener(() => 
                {
                SetButtonsInteractable(replyButtons, false);
                CheckAnswer(answerIndex);
                });
            }
            for (int j = 0; j < openingMenuButtons.Length; j++)
            {
                openingMenuButtons[j].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.options[j];
                int answerIndex = j; // Capture the current index for the button's listener
                openingMenuButtons[j].onClick.RemoveAllListeners();
                openingMenuButtons[j].onClick.AddListener(() => StartNewGame());
            }
            userAnswered = false;

            // set first button selected
            //EventSystem.current.SetSelectedGameObject(replyButtons[0].gameObject);

            // Start the timer coroutine only if it is not already running
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine); // Stop any existing timer
            }
            timerCoroutine = StartCoroutine(StartTimer()); // Start a new timer
        }
    }
    void CheckAnswer(int answerIndex)
    {
        if (userAnswered) return; // Prevent multiple answers

        userAnswered = true; // Mark that an answer has been given
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine); // Stop the timer on answer
            timerCoroutine = null; // Reset the coroutine reference
        }

        bool isCorrect = answerIndex == currentQuestions[currentQuestionIndex].correctAnswerIndex;       

        Button clickedButton = replyButtons[answerIndex]; // button that user clicked
        //buttonEffect.TriggerButtonEffect(clickedButton, isCorrect, LoadNextQuestion);
        StartCoroutine(buttonEffect.MainModeTriggerButtonEffect(clickedButton, isCorrect, LoadNextQuestion));
        

        if (!isCorrect)   //Yanlışsa doğru şıkkı yakıyor
        {
            int trueIndex = currentQuestions[currentQuestionIndex].correctAnswerIndex;
            Button trueButton = replyButtons[trueIndex];
 
            StartCoroutine(buttonEffect.MainModeTriggerButtonEffect2(trueButton, true));
        }

        if(isCorrect){
            PlayCorrectAnswerSound();
            correctAnswerCount++;
        }
        else
        {
            PlayWrongAnswerSound();
            wrongAnswerCount++;
        }
    }
    private void LoadNextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < currentQuestions.Count)
            LoadQuestion(currentQuestionIndex);
        else
        {
            //Debug.Log("All questions answered in current round!");
            currentRound++;
            HandleTimeUp();
        }
    }
    void SetDifficultyButtonsInteractable(bool value)
    {
        buttonEasy.interactable = value;
        buttonMedium.interactable = value;
        buttonHard.interactable = value;
        buttonGoBack.interactable = value;
    }
    void SetButtonsInteractable(Button[] buttons, bool value)
    {
        foreach(Button b in buttons)
            b.interactable = value;
    }
    void SendFirebaseCustomEvent(string customEventName, string customParameterName1, string customParamValue1,
                                                        string customParameterName2, string customParamValue2)
                                                        
    {
        if(firebaseHandlerInstance != null)
        {
            firebaseHandlerInstance.LogCustomEvent(customEventName, customParameterName1, customParamValue1,
                                                                    customParameterName2, customParamValue2);
            //Debug.Log("Custom Event Instance is being sent...");
        }
        else
            Debug.Log("Firebase Handler Instance is NULL...");
    }

    //
    // Show - Hide Panels
    //
    void closePanels()
    {
        CloseTrailerPanel();
        CloseCreditsPanel();
        CloseEndGamePanel();
        CloseSubPanel();
        difficultyPanel.SetActive(false);
    }

    void ShowSkipAdButton() // close skip ad button method not implemented
    {
        if(skipAdButton != null)
        {
            RectTransform rectTransform = skipAdButton.GetComponent<RectTransform>();
            skipAdButton.transform.localScale = Vector3.zero;
            skipAdButton.transform.DOScale(Vector3.one, 1.2f).SetEase(Ease.OutQuint);
        }
        else
            Debug.Log("subPanel is NULL..");
    }
    void HideQuizField()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    void ShowQuizField()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1; // Show the quiz field
            canvasGroup.interactable = true; // Enable interaction
            canvasGroup.blocksRaycasts = true; // Allow raycasts

            questionPanel.transform.localScale = Vector3.zero;

            questionPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
    }
    void ShowDifficultyPanel()
    {       
        if(difficultyPanel != null)
        {
            SetDifficultyButtonsInteractable(true);

            RectTransform rectTransform = difficultyPanel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(2000, rectTransform.anchoredPosition.y);

            difficultyPanel.SetActive(true);
            rectTransform.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() => 
            {
                //EventSystem.current.SetSelectedGameObject([0].gameObject);
                //buttonEasy.Select();
            });
        }
    }
    void CloseDifficultyPanel()
    {        
        if(difficultyPanel != null)
        {
            RectTransform rectTransform = difficultyPanel.GetComponent<RectTransform>();

            rectTransform.DOAnchorPosX(-2000, 1.0f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                difficultyPanel.SetActive(false);
                ShowSubPanel();
            });

        }
    }
    void BackToMenu()
    {
        if (difficultyPanel != null)
        {
            RectTransform rectTransform = difficultyPanel.GetComponent<RectTransform>();

            rectTransform.DOAnchorPosX(-2000, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                difficultyPanel.SetActive(false);
                ShowOpeningPanel();
                //showSubPanel();
            });

        }
    }

    void ShowEndGamePanel()
    {
        if(endGamePanel != null)
        {
            endGamePanel.SetActive(true);
            endGamePanel.transform.localScale = Vector3.zero;
            endGamePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        else
            Debug.Log("endGamePanel is NULL..");
    }
    void CloseEndGamePanel()
    {
        if(endGamePanel != null)
            endGamePanel.SetActive(false);
        else
            Debug.Log("endGamePanel is NULL..");
    }
    void ShowCreditsPanel()
    {
        if(creditsPanel != null)
        {
            CloseOpeningPanel(() => {});

            creditsPanel.SetActive(true);
            creditsPanel.transform.localScale = Vector3.zero;
            creditsPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }
        else
            Debug.Log("creditsPanel is NULL..");
    }
    void CloseCreditsPanel()
    {
        if(creditsPanel != null)
            creditsPanel.SetActive(false);
        else
            Debug.Log("creditsPanel is NULL..");
    }
    void ShowVideoPanel()
    {
        if(videoPanel != null)
        {
            RectTransform rectTransform = videoPanel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -100);
            videoPanel.SetActive(true);
            rectTransform.DOAnchorPosY(30, 1.5f).SetEase(Ease.OutBack);

        }
        else
            Debug.Log("videoPanel is NULL..");
    }
    void CloseVideoPanel()
    {
        if(videoPanel != null)
        {
            videoPanel.SetActive(false);
            videoLoader.ResetVideoPlayer(videoLoader.videoPlayer);
        }
        else
            Debug.Log("videoPanel is NULL..");
    }
    void ShowSubPanel()
    {    
        if(subPanel != null)
        {
            RectTransform rectTransform = subPanel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -100);
            subPanel.SetActive(true);
            rectTransform.DOAnchorPosY(30, 1.5f).SetEase(Ease.OutBack);

        }
        else
            Debug.Log("subPanel is NULL..");
    }


    void CloseSubPanel()
    {
        if(subPanel != null)
            subPanel.SetActive(false);
        else
            Debug.Log("subPanel is NULL..");
    }

    void ShowOpeningPanel()
    {
        SetButtonsInteractable(openingMenuButtons, true);
        
        if(openingPanel != null)
        {
            RectTransform rectTransform = openingPanel.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(2000, rectTransform.anchoredPosition.y);

            openingPanel.SetActive(true);
            rectTransform.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                // Butonu seçili yap
                //EventSystem.current.SetSelectedGameObject(openingMenuButtons[0].gameObject);
            });
        }
    }
    void CloseOpeningPanel(Action onComplete)
    {
        if(openingPanel != null)
        {
            RectTransform rectTransform = openingPanel.GetComponent<RectTransform>();

            rectTransform.DOAnchorPosX(-1500, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                openingPanel.SetActive(false);
                onComplete?.Invoke();
            });
        }
    }
    void ShowTrailerPanel()
    {
        if(trailerPanel != null)
        {
            trailerPanel.SetActive(true);
            ShowSkipAdButton();
        }
        else
            Debug.Log("trailerPanel is NULL..");
    }
    void CloseTrailerPanel()
    {
        if(trailerPanel != null)
            trailerPanel.SetActive(false);
        else
            Debug.Log("trailerPanel is NULL..");
    }
    void ShowPlayerConsentPanel()
    {
        if(playerConsentPanel != null )
        {
            playerConsentPanel.SetActive(true);
            playerConsentPanel.transform.localScale = Vector3.zero;

            playerConsentPanel.transform.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBack);
        }
        else
            Debug.Log("Consent Panel is NULL..");
    }
    void ClosePlayerConsentPanel()
    {
        if(playerConsentPanel != null)
            playerConsentPanel.SetActive(false);
        else
            Debug.Log("PlayerConsentPanel is NULL..");
    }
    //
    // Methods for Sounds
    //
    public void PlayCorrectAnswerSound()
    {
        if(correctAnswerAudioSource != null)    correctAnswerAudioSource.Play();
        else Debug.Log("Correct Answer Audio Source is NULL");
    }
    public void PlayWrongAnswerSound()
    {
        if(wrongAnswerAudioSource != null)  wrongAnswerAudioSource.Play();
        else Debug.Log("Wrong Answer Audio Source is NULL");
    }
    private void PlayButtonClickSound()
    {
        if(buttonClickAudioSource != null)  buttonClickAudioSource.Play();
        else Debug.Log("Button Click Audio Source is NULL");
    }
    public void PlayFlagAnimSound()
    {
        if(flagAnimAudioSource != null) flagAnimAudioSource.Play();
        else Debug.Log("Flag Anim Audio Source is NULL");
    }
    public void StopFlagAnimSound()
    {
        if(flagAnimAudioSource != null) flagAnimAudioSource.Stop();
        else Debug.Log("Flag Anim Audio Source is NULL");
    }
    private void PlayThemeSound()
    {
        if(themeAudioSource != null)    themeAudioSource.Play();
        else Debug.Log("Theme Audio Source is NULL");
    }
    public void StopThemeSound()
    {
        if(themeAudioSource != null)    themeAudioSource.Stop();
        else Debug.Log("Theme Audio Source is NULL");
    }
}