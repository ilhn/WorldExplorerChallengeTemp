using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour
{
    public Color correctChoiceColor = Color.green, wrongChoiceColor = Color.red;
    public int answerValue;
    void Start()
    {  
    }
    public IEnumerator MainModeTriggerButtonEffect(Button button,bool isCorrect, System.Action loadNext)
    {
        Debug.Log("Girdim");
        if (isCorrect)
        {
            //Debug.Log("answer value : 1");
            answerValue = 1;
        }
        else
        {
            answerValue = -1;
        }
        EventSystem.current.SetSelectedGameObject(null);
        button.GetComponent<Animator>().SetInteger("question", answerValue);
        yield return new WaitForSeconds(2f);
        //closePanelFun.Invoke();
        //yield return new WaitForSeconds(2f);
        button.GetComponent<Animator>().SetInteger("question", 0);
        loadNext.Invoke();
    }
    public IEnumerator MainModeTriggerButtonEffect2(Button button, bool isCorrect)   //Eğer cevap yanlışsa doğru cevabı gösterir
    {
        Debug.Log("Girdim");
        if (isCorrect)
        {
            answerValue = 2;
        }

        yield return new WaitForSeconds(0.75f);
        button.GetComponent<Animator>().SetInteger("question", answerValue);

        yield return new WaitForSeconds(1.25f);
        button.GetComponent<Animator>().SetInteger("question", 0);
    }
    public IEnumerator AnswerQuestion(Button button, System.Action closePanelFun)
    {
        EventSystem.current.SetSelectedGameObject(null);
        button.GetComponent<Animator>().SetInteger("question", answerValue);
        yield return new WaitForSeconds(2f);
        closePanelFun.Invoke();
        yield return new WaitForSeconds(2f);
        button.GetComponent<Animator>().SetInteger("question", 0);
    }
    public IEnumerator AnswerQuestion2(Button button)
    {
        yield return new WaitForSeconds(0.5f);
        button.GetComponent<Animator>().SetInteger("question", 2);
        
        yield return new WaitForSeconds(2.25f);
        button.GetComponent<Animator>().SetInteger("question", 0);
    }
}
