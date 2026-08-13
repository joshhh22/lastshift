using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sistem Pause Game yang dipanggil dengan tombol ESC, menghentikan waktu (Time.timeScale = 0),
/// dan menyediakan tombol/navigasi untuk Resume dan Exit ke Main Menu.
/// </summary>
public class PauseUIController : MonoBehaviour
{
    public static PauseUIController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button exitButton;

    [Header("Keyboard Navigation (Opsional)")]
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private TMP_Text exitText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private int currentIndex = 0;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupRectTransform();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void SetupRectTransform()
    {
        if (pausePanel != null)
        {
            pausePanel.transform.SetAsLastSibling();

            Canvas canvasComp = pausePanel.GetComponent<Canvas>();
            if (canvasComp != null)
            {
                canvasComp.overrideSorting = true;
                canvasComp.sortingOrder = 100;
            }

            pausePanel.transform.localScale = Vector3.one;

            RectTransform rect = pausePanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.one;
                rect.anchoredPosition = Vector2.zero;
                rect.localPosition = Vector3.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.localScale = Vector3.one;
            }

            foreach (Transform child in pausePanel.transform)
            {
                child.gameObject.SetActive(true);
                child.localScale = Vector3.one;
            }
        }
    }

    private void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToMainMenu);

        // Auto-assign tombol & teks jika belum terhubung di Inspector
        if (pausePanel != null)
        {
            if (resumeButton == null)
            {
                Button[] btns = pausePanel.GetComponentsInChildren<Button>(true);
                if (btns.Length > 0) resumeButton = btns[0];
                if (btns.Length > 1) exitButton = btns[1];
            }
        }
    }

    private void Update()
    {
        // Tekan Escape untuk Toggle Pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Jangan pause jika UI dialog / serve passenger sedang terbuka (sudah dihandle oleh UI tersebut)
            if (ServePassengerUIController.Instance != null && ServePassengerUIController.Instance.IsOpen)
                return;

            // Jangan pause jika Computer UI sedang terbuka
            ComputerUIController computer = FindFirstObjectByType<ComputerUIController>();
            if (computer != null && computer.IsOpen)
                return;

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (isPaused)
        {
            HandleNavigationInput();
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);

            SetupRectTransform();

            foreach (Transform child in pausePanel.transform)
            {
                child.gameObject.SetActive(true);
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentIndex = 0;
        RefreshMenu();
    }

    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitToMainMenu()
    {
        StartCoroutine(ExitToMainMenuRoutine());
    }

    private IEnumerator ExitToMainMenuRoutine()
    {
        Time.timeScale = 1f;

        if (FadeController.Instance != null)
        {
            yield return FadeController.Instance.FadeOut();
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        isPaused = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void HandleNavigationInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex = 1 - currentIndex;
            RefreshMenu();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentIndex == 0)
                ResumeGame();
            else
                ExitToMainMenu();
        }
    }

    private void RefreshMenu()
    {
        if (resumeText != null)
            resumeText.text = (currentIndex == 0) ? "> RESUME" : "RESUME";

        if (exitText != null)
            exitText.text = (currentIndex == 1) ? "> EXIT TO MAIN MENU" : "EXIT TO MAIN MENU";

        if (hintText != null)
            hintText.text = "[↑/↓] MOVE   [ENTER] CONFIRM";
    }
}
