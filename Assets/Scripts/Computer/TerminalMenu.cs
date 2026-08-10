using TMPro;
using UnityEngine;

public enum TerminalPage
{
    MainMenu,
    Assignment,
    CCTV,
    Logs
}
public class TerminalMenu : MonoBehaviour
{
    public static TerminalMenu Instance;

    public TerminalPage CurrentPage { get; private set; } = TerminalPage.MainMenu;

    [Header("Menu Items")]
    [SerializeField] private TextMeshProUGUI assignmentText;
    [SerializeField] private TextMeshProUGUI cctvText;
    [SerializeField] private TextMeshProUGUI logsText;

    [Header("Pages")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject assignmentPage;
    [SerializeField] private GameObject cctvPage;
    [SerializeField] private GameObject logsPage;

    private TextMeshProUGUI[] menuItems;

    private int currentIndex = 0;

    private bool inSubPage = false;

    public bool IsInSubPage => inSubPage;

    private void Awake()
    {
        Instance = this;

        menuItems = new TextMeshProUGUI[]
        {
            assignmentText,
            cctvText,
            logsText
        };
    }

    private void OnEnable()
    {
        currentIndex = 0;

        mainMenu.SetActive(true);
        assignmentPage.SetActive(false);
        cctvPage.SetActive(false);
        logsPage.SetActive(false);

        inSubPage = false;
        CurrentPage = TerminalPage.MainMenu;

        RefreshMenu();
    }

    private void Update()
    {
        if (!ComputerUIController.Instance.IsOpen)
            return;

        if (!ComputerUIController.Instance.IsOpen)
        return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
            return;
        }

        if (CurrentPage == TerminalPage.CCTV)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CCTVManager.Instance.PreviousCamera();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                CCTVManager.Instance.NextCamera();
            }

            return;
        }

        if (CurrentPage != TerminalPage.MainMenu)
            return;

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
            OpenCurrentPage();
        }
    }

    private void HandleEscape()
    {
        switch (CurrentPage)
        {
            case TerminalPage.Assignment:
            case TerminalPage.CCTV:
            case TerminalPage.Logs:
                BackToMainMenu();
                break;

            case TerminalPage.MainMenu:
                if (ObjectiveManager.Instance.GetCurrentObjective() == "Open Computer")
                {
                    ObjectiveManager.Instance.CompleteObjective();
                }

                ComputerUIController.Instance.Close();
                break;
        }
    }

    void RefreshMenu()
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (i == currentIndex)
            {
                menuItems[i].text = "► " + RemoveArrow(menuItems[i].text);
            }
            else
            {
                menuItems[i].text = RemoveArrow(menuItems[i].text);
            }
        }
    }

    string RemoveArrow(string text)
    {
        return text.Replace("► ", "");
    }

    void OpenCurrentPage()
    {
        mainMenu.SetActive(false);

        inSubPage = true;

        switch (currentIndex)
        {
            case 0:
                assignmentPage.SetActive(true);
                CurrentPage = TerminalPage.Assignment;
                break;

            case 1:
                cctvPage.SetActive(true);
                CurrentPage = TerminalPage.CCTV;

                CCTVManager.Instance.OpenCCTV();

                break;

            case 2:
                logsPage.SetActive(true);
                CurrentPage = TerminalPage.Logs;
                break;
        }
    }

    void BackToMainMenu()
    {
        if (CurrentPage == TerminalPage.CCTV)
        {
            CCTVManager.Instance.CloseCCTV();
        }

        assignmentPage.SetActive(false);
        cctvPage.SetActive(false);
        logsPage.SetActive(false);

        mainMenu.SetActive(true);

        inSubPage = false;
        CurrentPage = TerminalPage.MainMenu;
    }
}