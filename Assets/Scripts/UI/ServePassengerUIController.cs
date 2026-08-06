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

    [SerializeField] private TMP_Text dialogueTitleText;
    [SerializeField] private TMP_Text reasonText;

    [SerializeField] private TMP_Text acceptText;
    [SerializeField] private TMP_Text rejectText;

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

        menuItems = new TMP_Text[]
        {
            validateText,
            talkText,
            cancelText
        };

        rootUI.SetActive(false);
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
            "Destination : " + data.ticket.destinationStation;
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

    reasonText.text = npc.passengerData.reason;
}

    void RefreshDialogue()
    {
        acceptText.text = "Accept";
        rejectText.text = "Reject";

        if (dialogueIndex == 0)
            acceptText.text = "> Accept";
        else
            rejectText.text = "> Reject";
    }

    void HandleDialogueInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.DownArrow))
        {
            dialogueIndex = 1 - dialogueIndex;

            RefreshDialogue();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            ConfirmDialogue();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return; // jangan boleh keluar
        }
    }

    void ConfirmDialogue()
    {
        if (dialogueIndex == 0)
        {
            if(currentNPC.passengerData.isReasonTrue)
            {
                // TODO Performance kecil
            }
            else
            {
                // TODO Performance besar
            }

            currentNPC.Serve();

            Close();

            swipeController.ResetCard();
        }
        else
        {
            if(currentNPC.passengerData.isReasonTrue)
            {
                // TODO Humanity turun
                // TODO Performance naik
            }
            else
            {
                // TODO Performance naik besar
            }

            currentNPC.Reject();

            Close();

            swipeController.ResetCard();
        }
    }
}