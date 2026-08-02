using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private float typingSpeed = 0.03f;

    private Coroutine typingRoutine;

    private TMP_Text targetText;

    private string currentSentence;

    public bool IsTyping { get; private set; }

    public void StartTyping(TMP_Text textUI, string sentence)
    {
        Debug.Log("Typing : " + sentence);

        targetText = textUI;
        currentSentence = sentence;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeRoutine());
    }

    IEnumerator TypeRoutine()
    {
        IsTyping = true;

        targetText.text = "";

        foreach (char c in currentSentence)
        {
            targetText.text += c;

            yield return new WaitForSeconds(typingSpeed);
        }

        IsTyping = false;
    }

    public void CompleteTyping()
    {
        if (!IsTyping)
            return;

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        targetText.text = currentSentence;

        IsTyping = false;
    }
}