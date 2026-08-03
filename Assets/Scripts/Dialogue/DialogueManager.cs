using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Events")]
    public UnityEvent onDialogueFinished;

    [SerializeField] private TypewriterEffect typewriter;

    private DialogueData currentDialogue;
    private int currentIndex;

    private bool isPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        UIManager.Instance.HideDialogue();
    }


    private void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (typewriter.IsTyping)
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
        if (dialogue == null)
            return;

        Debug.Log("Start Dialogue");

        PlayerLockManager.Instance.LockPlayer();

        currentDialogue = dialogue;
        currentIndex = 0;
        isPlaying = true;

        UIManager.Instance.ShowDialogue();

        ShowLine();
    }

    // =========================
    // SHOW
    // =========================

    private void ShowLine()
    {
        DialogueLine line = currentDialogue.lines[currentIndex];

        speakerText.text = line.speaker;

        Debug.Log(currentDialogue.lines[currentIndex].text);
        typewriter.StartTyping(dialogueText, line.text);
    }

    // =========================
    // NEXT
    // =========================

    private void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentDialogue.lines.Count)
        {
            EndDialogue();
            return;
        }

        ShowLine();
    }

    // =========================
    // END
    // =========================

    private void EndDialogue()
    {
        isPlaying = false;

        UIManager.Instance.HideDialogue();

        currentDialogue = null;

        PlayerLockManager.Instance.UnlockPlayer();

        onDialogueFinished?.Invoke();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}