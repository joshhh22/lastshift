using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrutigerAeroComputerUI : MonoBehaviour
{
    public static FrutigerAeroComputerUI Instance;

    [Header("Desktop Components")]
    public Image wallpaperImage;
    public RectTransform desktopShortcutsContainer;

    [Header("Taskbar & System Tray")]
    public RectTransform taskbar;
    public Button startOrbButton;
    public GameObject startMenuPopup;
    public TMP_Text clockText;
    public TMP_Text dayBadgeText;

    [Header("Taskbar App Buttons (Running Indicators)")]
    public Button taskbarAssignmentBtn;
    public Button taskbarCCTVBtn;
    public Button taskbarLogsBtn;
    public GameObject assignmentActiveGlow;
    public GameObject cctvActiveGlow;
    public GameObject logsActiveGlow;

    [Header("Aero Windows")]
    public GameObject assignmentWindow;
    public GameObject cctvWindow;
    public GameObject logsWindow;

    [Header("Window Title Text Displays")]
    public TMP_Text assignmentTitle;
    public TMP_Text cctvTitle;
    public TMP_Text logsTitle;

    [Header("Window Close Buttons")]
    public Button assignmentCloseBtn;
    public Button cctvCloseBtn;
    public Button logsCloseBtn;

    [Header("Desktop Shortcut Buttons")]
    public Button shortcutAssignmentBtn;
    public Button shortcutCCTVBtn;
    public Button shortcutLogsBtn;
    public GameObject[] shortcutSelectionGlows;

    [Header("CCTV Live Controls")]
    public TMP_Text cctvCameraLabel;
    public TMP_Text cctvRecLabel;
    public RawImage cctvViewportRawImage;

    private TerminalPage currentActivePage = TerminalPage.MainMenu;
    private int selectedShortcutIndex = 0;

    private void Awake()
    {
        Instance = this;

        if (startOrbButton != null)
            startOrbButton.onClick.AddListener(ToggleStartMenu);

        if (shortcutAssignmentBtn != null)
            shortcutAssignmentBtn.onClick.AddListener(() => { selectedShortcutIndex = 0; UpdateShortcutSelection(); OpenApp(TerminalPage.Assignment); });
        if (shortcutCCTVBtn != null)
            shortcutCCTVBtn.onClick.AddListener(() => { selectedShortcutIndex = 1; UpdateShortcutSelection(); OpenApp(TerminalPage.CCTV); });
        if (shortcutLogsBtn != null)
            shortcutLogsBtn.onClick.AddListener(() => { selectedShortcutIndex = 2; UpdateShortcutSelection(); OpenApp(TerminalPage.Logs); });

        if (taskbarAssignmentBtn != null)
            taskbarAssignmentBtn.onClick.AddListener(() => ToggleApp(TerminalPage.Assignment));
        if (taskbarCCTVBtn != null)
            taskbarCCTVBtn.onClick.AddListener(() => ToggleApp(TerminalPage.CCTV));
        if (taskbarLogsBtn != null)
            taskbarLogsBtn.onClick.AddListener(() => ToggleApp(TerminalPage.Logs));

        if (assignmentCloseBtn != null)
            assignmentCloseBtn.onClick.AddListener(() => CloseApp(TerminalPage.Assignment));
        if (cctvCloseBtn != null)
            cctvCloseBtn.onClick.AddListener(() => CloseApp(TerminalPage.CCTV));
        if (logsCloseBtn != null)
            logsCloseBtn.onClick.AddListener(() => CloseApp(TerminalPage.Logs));
    }

    private void OnEnable()
    {
        if (startMenuPopup != null)
            startMenuPopup.SetActive(false);

        CloseAllWindows();
        UpdateSystemTray();
        UpdateShortcutSelection();
    }

    private void Update()
    {
        if (ComputerUIController.Instance == null || !ComputerUIController.Instance.IsOpen)
            return;

        UpdateSystemTray();

        // 1. ESC Key Handling (Close active window first, or exit computer if on desktop)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
            return;
        }

        // Toggle Start Menu Popup dengan tombol TAB
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleStartMenu();
        }

        // 2. Keyboard Navigation on Desktop (When no window is open)
        if (currentActivePage == TerminalPage.MainMenu)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedShortcutIndex = (selectedShortcutIndex + 1) % 3;
                UpdateShortcutSelection();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedShortcutIndex = (selectedShortcutIndex - 1 + 3) % 3;
                UpdateShortcutSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                switch (selectedShortcutIndex)
                {
                    case 0: OpenApp(TerminalPage.Assignment); break;
                    case 1: OpenApp(TerminalPage.CCTV); break;
                    case 2: OpenApp(TerminalPage.Logs); break;
                }
            }
        }
        else if (currentActivePage == TerminalPage.CCTV)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (CCTVManager.Instance != null) CCTVManager.Instance.PreviousCamera();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (CCTVManager.Instance != null) CCTVManager.Instance.NextCamera();
            }
        }
    }

    public void HandleEscapeKey()
    {
        if (startMenuPopup != null && startMenuPopup.activeSelf)
        {
            startMenuPopup.SetActive(false);
            return;
        }

        if (currentActivePage != TerminalPage.MainMenu)
        {
            // Close current window and return to Desktop
            CloseAllWindows();
        }
        else
        {
            // Already on Desktop -> Exit Computer
            if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentObjective() == "Open Computer")
            {
                ObjectiveManager.Instance.CompleteObjective();
            }

            if (ComputerUIController.Instance != null)
            {
                ComputerUIController.Instance.Close();
            }
        }
    }

    public void UpdateShortcutSelection()
    {
        if (shortcutSelectionGlows != null)
        {
            for (int i = 0; i < shortcutSelectionGlows.Length; i++)
            {
                if (shortcutSelectionGlows[i] != null)
                {
                    shortcutSelectionGlows[i].SetActive(i == selectedShortcutIndex);
                }
            }
        }
    }

    public void UpdateSystemTray()
    {
        string timeStr = GameTimeManager.Instance != null ? GameTimeManager.Instance.GetCurrentTime() : "22:00";
        string dayStr = DayManager.Instance != null ? $"DAY {(int)DayManager.Instance.CurrentDay}" : "DAY 1";

        if (clockText != null)
            clockText.text = timeStr;

        if (dayBadgeText != null)
            dayBadgeText.text = dayStr;

        if (cctvRecLabel != null)
            cctvRecLabel.text = $"REC {timeStr}";
    }

    public void ToggleStartMenu()
    {
        if (startMenuPopup != null)
        {
            startMenuPopup.SetActive(!startMenuPopup.activeSelf);
        }
    }

    public void OpenApp(TerminalPage page)
    {
        if (startMenuPopup != null)
            startMenuPopup.SetActive(false);

        CloseAllWindows();

        currentActivePage = page;
        selectedShortcutIndex = (int)page - 1;
        UpdateShortcutSelection();

        switch (page)
        {
            case TerminalPage.Assignment:
                if (assignmentWindow != null)
                {
                    assignmentWindow.SetActive(true);
                    StartCoroutine(AnimateWindowOpen(assignmentWindow.GetComponent<RectTransform>()));
                }
                if (assignmentActiveGlow != null) assignmentActiveGlow.SetActive(true);
                break;

            case TerminalPage.CCTV:
                if (cctvWindow != null)
                {
                    cctvWindow.SetActive(true);
                    StartCoroutine(AnimateWindowOpen(cctvWindow.GetComponent<RectTransform>()));
                }
                if (cctvActiveGlow != null) cctvActiveGlow.SetActive(true);
                if (CCTVManager.Instance != null) CCTVManager.Instance.OpenCCTV();
                break;

            case TerminalPage.Logs:
                if (logsWindow != null)
                {
                    logsWindow.SetActive(true);
                    StartCoroutine(AnimateWindowOpen(logsWindow.GetComponent<RectTransform>()));
                }
                if (logsActiveGlow != null) logsActiveGlow.SetActive(true);
                break;
        }
    }

    public void ToggleApp(TerminalPage page)
    {
        if (currentActivePage == page)
        {
            CloseApp(page);
        }
        else
        {
            OpenApp(page);
        }
    }

    public void CloseApp(TerminalPage page)
    {
        if (page == TerminalPage.CCTV && CCTVManager.Instance != null)
        {
            CCTVManager.Instance.CloseCCTV();
        }

        switch (page)
        {
            case TerminalPage.Assignment:
                if (assignmentWindow != null) assignmentWindow.SetActive(false);
                if (assignmentActiveGlow != null) assignmentActiveGlow.SetActive(false);
                break;

            case TerminalPage.CCTV:
                if (cctvWindow != null) cctvWindow.SetActive(false);
                if (cctvActiveGlow != null) cctvActiveGlow.SetActive(false);
                break;

            case TerminalPage.Logs:
                if (logsWindow != null) logsWindow.SetActive(false);
                if (logsActiveGlow != null) logsActiveGlow.SetActive(false);
                break;
        }

        if (currentActivePage == page)
        {
            currentActivePage = TerminalPage.MainMenu;
        }
    }

    public void CloseAllWindows()
    {
        if (CCTVManager.Instance != null) CCTVManager.Instance.CloseCCTV();

        if (assignmentWindow != null) assignmentWindow.SetActive(false);
        if (cctvWindow != null) cctvWindow.SetActive(false);
        if (logsWindow != null) logsWindow.SetActive(false);

        if (assignmentActiveGlow != null) assignmentActiveGlow.SetActive(false);
        if (cctvActiveGlow != null) cctvActiveGlow.SetActive(false);
        if (logsActiveGlow != null) logsActiveGlow.SetActive(false);

        currentActivePage = TerminalPage.MainMenu;
    }

    private IEnumerator AnimateWindowOpen(RectTransform rt)
    {
        if (rt == null) yield break;

        float duration = 0.18f;
        float elapsed = 0f;

        Vector3 startScale = new Vector3(0.85f, 0.85f, 1f);
        Vector3 targetScale = Vector3.one;

        rt.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Overshoot bounce curve for authentic Aero Glass feel
            float smooth = 1f + 0.1f * Mathf.Sin(t * Mathf.PI);
            rt.localScale = Vector3.LerpUnclamped(startScale, targetScale, smooth * t);
            yield return null;
        }

        rt.localScale = targetScale;
    }
}
