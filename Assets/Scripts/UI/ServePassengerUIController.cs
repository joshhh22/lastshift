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

    private TMP_Text[] menuItems;

    private int currentIndex;

    private NPCController currentNPC;

    private bool isOpen;
    private bool inSwipePanel;

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

        currentIndex = 0;

        rootUI.SetActive(true);

        menuPanel.SetActive(true);
        swipePanel.SetActive(false);

        RefreshMenu();

        PlayerLockManager.Instance.EnterUIMode();
    }

    public void Close()
    {
        isOpen = false;

        currentNPC = null;

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
    }

    void BackToMenu()
    {
        inSwipePanel = false;

        swipePanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}