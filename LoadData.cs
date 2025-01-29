using System.Collections.Generic;
using UnityEngine;
public class LoadData : MonoBehaviour
{   
    public TextAsset jsonFile;
    public List<Question> easyQuestions = new List<Question>();
    public List<Question> mediumQuestions = new List<Question>();
    public List<Question> hardQuestions = new List<Question>();

    private List<string> allCountries = new List<string>();
    
    void Awake()
    {
        LoadQuestions();
    }
    public List<string> GetAllCountries()
    {
        return allCountries;
    }
    void LoadQuestions()
    {
        string jsonText = jsonFile.text;
        // questions listesine sorulari parse et
        QuestionsWrapper wrapper = JsonUtility.FromJson<QuestionsWrapper>(jsonText);

        HashSet<string> countrySet = new HashSet<string>();
        
        foreach(var question in wrapper.questions)
        {
            if(question.difficulty.Equals("easy"))
                easyQuestions.Add(question);
            else if(question.difficulty.Equals("medium"))
                mediumQuestions.Add(question);
            else if(question.difficulty.Equals("hard"))
                hardQuestions.Add(question);
            else
                Debug.Log("difficulty bilgisi dogru okunamadi");
        
            if(countrySet.Add(question.country))
            {
                allCountries.Add(question.country);
            } 
        }    
    }
    public List<Question> GetQuestionsByDifficulty(string difficulty)
    {
        if (difficulty == null){
            Debug.Log("difficulty degeri null olarak geliyor");
            return null;
        }
        else if (difficulty.Equals("easy"))
            return easyQuestions;
        else if (difficulty.Equals("medium"))
            return mediumQuestions;
        else if (difficulty.Equals("hard"))
            return hardQuestions;
        else{
            Debug.Log("dogru zorluk listesi secilemedi..");
            return null;
        }
    }
}

// helper wrapper class
public class QuestionsWrapper
{
    public Question[] questions;
}













