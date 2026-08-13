using TMPro;
using UnityEngine;

public class ServePassengerUIController : MonoBehaviour
{
    public static ServePassengerUIController Instance;

    [Header("Panels")]
    [SerializeField] private GameObject rootUI;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject swipePanel;

    [Header("Menu")]
    [SerializeField] private TMP_Text validateText;
    [SerializeField] private TMP_Text talkText;
    [SerializeField] private TMP_Text cancelText;

    [SerializeField] private CardSwipeController swipeController;

    [Header("Ticket Info")]
    [SerializeField] private TMP_Text passengerNameText;
    [SerializeField] private TMP_Text ticketIDText;
    [SerializeField] private TMP_Text originText;
    [SerializeField] private TMP_Text destinationText;

    [Header("Dialogue Panel")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TypewriterEffect typewriter; // Tambahan Typewriter Effect

    [SerializeField] private TMP_Text dialogueTitleText;
    [SerializeField] private TMP_Text reasonText;

    [SerializeField] private TMP_Text acceptText;
    [SerializeField] private TMP_Text rejectText;
    [SerializeField] private TMP_Text cancelDialogueText; // Opsional: Teks UI Cancel

    private TMP_Text[] menuItems;

    private int currentIndex;

    private NPCController currentNPC;

    private bool isOpen;
    private bool inSwipePanel;
    private bool inDialoguePanel;

    private int dialogueIndex;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        Instance = this;

        System.Collections.Generic.List<TMP_Text> items = new System.Collections.Generic.List<TMP_Text>();
        if (validateText != null) items.Add(validateText);
        if (talkText != null) items.Add(talkText);
        if (cancelText != null) items.Add(cancelText);
        menuItems = items.ToArray();

        if (rootUI != null)
            rootUI.SetActive(false);
    }

    void RefreshMenu()
    {
        if (validateText != null) validateText.text = "Validate Ticket";
        if (talkText != null) talkText.text = "Talk";
        if (cancelText != null) cancelText.text = "Cancel";

        switch (currentIndex)
        {
            case 0:
                if (validateText != null) validateText.text = "> Validate Ticket";
                break;

            case 1:
                if (talkText != null)
                    talkText.text = "> Talk";
                else if (cancelText != null)
                    cancelText.text = "> Cancel";
                break;

            case 2:
                if (cancelText != null) cancelText.text = "> Cancel";
                break;
        }
    }

    void SelectCurrent()
    {
        switch (currentIndex)
        {
            case 0:
                OpenSwipePanel();
                break;

            case 1:
                if (talkText != null)
                    Debug.Log("Talk");
                else
                    Close();
                break;

            case 2:
                Close();
                break;
        }
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (inSwipePanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                BackToMenu();
            }

            return;
        }

        if (inDialoguePanel)
        {
            HandleDialogueInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex++;

            if (currentIndex >= menuItems.Length)
                currentIndex = 0;

            RefreshMenu();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;

            if (currentIndex < 0)
                currentIndex = menuItems.Length - 1;

            RefreshMenu();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SelectCurrent();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open(NPCController npc)
    {
        currentNPC = npc;

        isOpen = true;
        inSwipePanel = false;
        inDialoguePanel = false;

        currentIndex = 0;

        rootUI.SetActive(true);

        menuPanel.SetActive(true);
        swipePanel.SetActive(false);
        dialoguePanel.SetActive(false);   // <-- TAMBAHKAN

        RefreshMenu();

        PlayerLockManager.Instance.EnterUIMode();
    }

    public void Close()
    {
        isOpen = false;
        inSwipePanel = false;
        inDialoguePanel = false;

        currentNPC = null;

        menuPanel.SetActive(false);
        swipePanel.SetActive(false);
        dialoguePanel.SetActive(false);

        rootUI.SetActive(false);

        PlayerLockManager.Instance.ExitUIMode();
    }

    void RefreshMenu()
    {
        validateText.text = "Validate Ticket";
        talkText.text = "Talk";
        cancelText.text = "Cancel";

        switch (currentIndex)
        {
            case 0:
                validateText.text = "> Validate Ticket";
                break;

            case 1:
                talkText.text = "> Talk";
                break;

            case 2:
                cancelText.text = "> Cancel";
                break;
        }
    }

    void SelectCurrent()
    {
        switch (currentIndex)
        {
            case 0:
                OpenSwipePanel();
                break;

            case 1:
                Debug.Log("Talk");
                break;

            case 2:
                Close();
                break;
        }
    }

    void OpenSwipePanel()
    {
        inSwipePanel = true;

        menuPanel.SetActive(false);
        swipePanel.SetActive(true);

        PassengerData data = currentNPC.passengerData;

        passengerNameText.text =
            "Name : " + data.passengerName;

        ticketIDText.text =
            "Ticket ID : " + data.ticket.ticketID;

        originText.text =
            "Origin : " + data.ticket.originStation;

        destinationText.text =
            "Dest : " + data.ticket.destinationStation +
            "  |  Class : " + data.ticket.seatClass.ToString();
    }

    void BackToMenu()
    {
        inSwipePanel = false;

        swipePanel.SetActive(false);
        dialoguePanel.SetActive(false);   // <-- TAMBAHKAN
        menuPanel.SetActive(true);
    }

    public void OpenDialoguePanel(NPCController npc)
    {
        menuPanel.SetActive(false);
        swipePanel.SetActive(false);

        dialoguePanel.SetActive(true);

        // Auto-find cancelDialogueText jika belum di-drag ke Inspector
        if (cancelDialogueText == null && dialoguePanel != null)
        {
            foreach (TMP_Text t in dialoguePanel.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t.name.ToLower().Contains("cancel"))
                {
                    cancelDialogueText = t;
                    break;
                }
            }
        }

        inSwipePanel = false;
        inDialoguePanel = true;
        dialogueIndex = 0;

        RefreshDialogue();

        switch (npc.passengerData.ticket.status)
        {
            case TicketStatus.Invalid:
                dialogueTitleText.text = "INVALID TICKET";
                break;

            case TicketStatus.Expired:
                dialogueTitleText.text = "EXPIRED TICKET";
                break;

            case TicketStatus.Fake:
                dialogueTitleText.text = "FAKE TICKET";
                break;

            case TicketStatus.WrongDestination:
                dialogueTitleText.text = "WRONG DESTINATION";
                break;
        }

        if (typewriter != null)
        {
            typewriter.StartTyping(reasonText, npc.passengerData.reason);
        }
        else
        {
            reasonText.text = npc.passengerData.reason;
        }
    }

    void RefreshDialogue()
    {
        if (acceptText != null) acceptText.text = (dialogueIndex == 0) ? "> ACCEPT" : "ACCEPT";
        if (rejectText != null) rejectText.text = (dialogueIndex == 1) ? "> REJECT" : "REJECT";
        if (cancelDialogueText != null) cancelDialogueText.text = (dialogueIndex == 2) ? "> CANCEL" : "CANCEL";
    }

    private bool inReaction;

    void HandleDialogueInput()
    {
        if (inReaction)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (typewriter != null && typewriter.IsTyping)
                {
                    typewriter.CompleteTyping();
                }
            }
            return;
        }

        int maxOptions = (cancelDialogueText != null) ? 3 : 2;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            dialogueIndex = (dialogueIndex + 1) % maxOptions;
            RefreshDialogue();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            dialogueIndex = (dialogueIndex - 1 + maxOptions) % maxOptions;
            RefreshDialogue();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (typewriter != null && typewriter.IsTyping)
            {
                typewriter.CompleteTyping();
            }
            else
            {
                ConfirmDialogue();
            }
        }

        // Bolehkan Escape untuk membatalkan dialog dan keluar cek terminal
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelDialogue();
        }
    }

    void ConfirmDialogue()
    {
        if (dialogueIndex == 2)
        {
            CancelDialogue();
            return;
        }

        bool accepted = (dialogueIndex == 0);

        if (PerformanceManager.Instance != null && currentNPC != null)
        {
            PerformanceManager.Instance.EvaluateDecision(
                accepted,
                currentNPC.passengerData
            );
        }

        StartCoroutine(ShowReactionRoutine(accepted));
    }

    private System.Collections.IEnumerator ShowReactionRoutine(bool accepted)
    {
        inReaction = true;

        if (dialogueTitleText != null)
            dialogueTitleText.text = accepted ? "PASSENGER ACCEPTED" : "PASSENGER REJECTED";

        if (acceptText != null) acceptText.text = "";
        if (rejectText != null) rejectText.text = "";
        if (cancelDialogueText != null) cancelDialogueText.text = "";

        string reaction = TicketGenerator.GetDecisionReaction(currentNPC != null ? currentNPC.passengerData : null, accepted);

        if (typewriter != null)
        {
            typewriter.StartTyping(reasonText, reaction);
            yield return new WaitForSeconds(0.2f);

            while (typewriter.IsTyping)
            {
                yield return null;
            }
        }
        else if (reasonText != null)
        {
            reasonText.text = reaction;
        }

        // Jeda bentar biar player bisa baca reaksi penumpangnya
        yield return new WaitForSeconds(1.5f);

        inReaction = false;

        if (currentNPC != null)
        {
            if (accepted)
                currentNPC.Serve();
            else
                currentNPC.Reject();
        }

        Close();
        if (swipeController != null)
            swipeController.ResetCard();
    }

    public void CancelDialogue()
    {
        inReaction = false;
        Close();
        if (swipeController != null)
            swipeController.ResetCard();
    }
}