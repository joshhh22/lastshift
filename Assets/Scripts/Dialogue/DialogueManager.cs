using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Events")]
    public UnityEvent onDialogueFinished;

    [SerializeField] private TypewriterEffect typewriter;

    private List<DialogueLine> activeLines = new List<DialogueLine>();
    private int currentIndex;
    private bool isPlaying;
    private Action currentOnFinishedCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (UIManager.Instance != null)
            UIManager.Instance.HideDialogue();
        else if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        // Mendukung [E], [Spasi], [Enter], dan Klik Kiri Mouse untuk lanjut
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.CompleteTyping();
            }
            else
            {
                NextLine();
            }
        }
    }

    // =========================
    // START
    // =========================

    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Count == 0)
            return;

        StartDialogue(dialogue.lines, null);
    }

    public void StartDialogue(DialogueData dialogue, Action onFinished)
    {
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        StartDialogue(dialogue.lines, onFinished);
    }

    public void StartDialogue(List<DialogueLine> lines, Action onFinished = null)
    {
        if (lines == null || lines.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        Debug.Log("<color=cyan>[DialogueManager]</color> Start Dialogue (" + lines.Count + " lines)");

        if (PlayerLockManager.Instance != null)
            PlayerLockManager.Instance.LockPlayer();

        activeLines = new List<DialogueLine>(lines);
        currentIndex = 0;
        isPlaying = true;
        currentOnFinishedCallback = onFinished;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowDialogue();
        else if (dialogueUI != null)
            dialogueUI.SetActive(true);

        ShowLine();
    }

    // =========================
    // SHOW
    // =========================

    private void ShowLine()
    {
        if (currentIndex < 0 || currentIndex >= activeLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = activeLines[currentIndex];

        if (speakerText != null)
            speakerText.text = line.speaker;

        if (typewriter != null && dialogueText != null)
        {
            typewriter.StartTyping(dialogueText, line.text);
        }
        else if (dialogueText != null)
        {
            dialogueText.text = line.text;
        }
    }

    // =========================
    // NEXT
    // =========================

    private void NextLine()
    {
        currentIndex++;

        if (currentIndex >= activeLines.Count)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    // =========================
    // END
    // =========================

    public void EndDialogue()
    {
        isPlaying = false;

        if (UIManager.Instance != null)
            UIManager.Instance.HideDialogue();
        else if (dialogueUI != null)
            dialogueUI.SetActive(false);

        activeLines.Clear();

        if (PlayerLockManager.Instance != null)
            PlayerLockManager.Instance.UnlockPlayer();

        Action callback = currentOnFinishedCallback;
        currentOnFinishedCallback = null;

        if (callback != null)
        {
            callback.Invoke();
        }
        else
        {
            onDialogueFinished?.Invoke();
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}