using System.Collections.Generic;
[System.Serializable]
public class Question{
    public int id;
    public string country;
    public string questionText;
    public List<string> options;
    public int correctAnswerIndex;
    public string difficulty;
}
