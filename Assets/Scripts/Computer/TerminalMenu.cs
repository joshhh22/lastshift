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

    [Header("Footer Displays")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

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

        if (mainMenu != null)
        {
            if (FrutigerAeroComputerUI.Instance != null)
                mainMenu.SetActive(false);
            else
                mainMenu.SetActive(true);
        }

        if (assignmentPage != null) assignmentPage.SetActive(false);
        if (cctvPage != null) cctvPage.SetActive(false);
        if (logsPage != null) logsPage.SetActive(false);

        inSubPage = false;
        CurrentPage = TerminalPage.MainMenu;

        if (FrutigerAeroComputerUI.Instance == null)
        {
            RefreshMenu();
        }

        UpdateFooter();
    }

    private void Update()
    {
        if (ComputerUIController.Instance == null || !ComputerUIController.Instance.IsOpen)
            return;

        UpdateFooter();

        // Jika FrutigerAeroComputerUI aktif, serahkan seluruh kontrol input keyboard ke FrutigerAeroComputerUI
        if (FrutigerAeroComputerUI.Instance != null)
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

            if (menuItems != null && currentIndex >= menuItems.Length)
                currentIndex = 0;

            RefreshMenu();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex--;

            if (menuItems != null && currentIndex < 0)
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
                BackToMainMenu();
                break;

            case TerminalPage.Logs:
                // Cek dulu apakah LogsPageController masih di sub-layer
                LogsPageController logsCtrl = logsPage.GetComponent<LogsPageController>();
                if (logsCtrl != null && !logsCtrl.IsAtRoot)
                {
                    // Biarkan LogsPageController yang handle ESC (mundur satu layer)
                    // Update() LogsPageController akan memrosesnya
                    return;
                }
                // Sudah di root Logs → kembali ke Terminal Main Menu
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

    public void RefreshMenu()
    {
        if (menuItems == null) return;

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i] == null) continue;

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
        if (string.IsNullOrEmpty(text)) return "";
        return text.Replace("► ", "");
    }

    void OpenCurrentPage()
    {
        if (FrutigerAeroComputerUI.Instance != null)
        {
            switch (currentIndex)
            {
                case 0: FrutigerAeroComputerUI.Instance.OpenApp(TerminalPage.Assignment); break;
                case 1: FrutigerAeroComputerUI.Instance.OpenApp(TerminalPage.CCTV); break;
                case 2: FrutigerAeroComputerUI.Instance.OpenApp(TerminalPage.Logs); break;
            }
            return;
        }

        if (mainMenu != null) mainMenu.SetActive(false);

        inSubPage = true;

        switch (currentIndex)
        {
            case 0:
                if (assignmentPage != null) assignmentPage.SetActive(true);
                CurrentPage = TerminalPage.Assignment;
                break;

            case 1:
                if (cctvPage != null) cctvPage.SetActive(true);
                CurrentPage = TerminalPage.CCTV;

                if (CCTVManager.Instance != null)
                    CCTVManager.Instance.OpenCCTV();

                break;

            case 2:
                if (logsPage != null) logsPage.SetActive(true);
                CurrentPage = TerminalPage.Logs;
                break;
        }
    }

    void BackToMainMenu()
    {
        if (FrutigerAeroComputerUI.Instance != null)
        {
            FrutigerAeroComputerUI.Instance.CloseAllWindows();
        }

        if (CurrentPage == TerminalPage.CCTV && CCTVManager.Instance != null)
        {
            CCTVManager.Instance.CloseCCTV();
        }

        if (assignmentPage != null) assignmentPage.SetActive(false);
        if (cctvPage != null) cctvPage.SetActive(false);
        if (logsPage != null) logsPage.SetActive(false);

        if (FrutigerAeroComputerUI.Instance == null && mainMenu != null)
        {
            mainMenu.SetActive(true);
        }

        inSubPage = false;
        CurrentPage = TerminalPage.MainMenu;

        UpdateFooter();
    }

    private void UpdateFooter()
    {
        string dayStr = DayManager.Instance != null ? $"DAY {(int)DayManager.Instance.CurrentDay}" : "DAY 1";
        string timeStr = GameTimeManager.Instance != null ? GameTimeManager.Instance.GetCurrentTime() : "22:00";

        if (dayText != null)
            dayText.text = dayStr;

        if (timeText != null)
            timeText.text = timeStr;

        // Auto-update all child TextMeshProUGUI with name "Day" or "Time"
        foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (tmp == null) continue;
            if (tmp.gameObject.name == "Day")
                tmp.text = dayStr;
            else if (tmp.gameObject.name == "Time")
                tmp.text = timeStr;
        }
    }
}