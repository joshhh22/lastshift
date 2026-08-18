using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu UI")]
    public GameObject mainMenuCanvas;
    public Button playButton;
    public Button guideButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Sub Panels")]
    public GameObject guidePanel;
    public GameObject creditsPanel;

    [Header("Guide Sub-Panels")]
    public Button hintsButton;
    public Button swipeMechanicButton;
    public GameObject hintsSubPanel;
    public GameObject swipeMechanicSubPanel;

    [Header("Scroll Settings (Swipe Mechanic)")]
    public ScrollRect swipeScrollRect;
    public RectTransform swipeTextRect;
    public float keyboardScrollSpeed = 400f;

    [Header("Prologue UI")]
    public GameObject prologueCanvas;
    public TMP_Text prologueText;
    public TMP_Text prologuePromptText; // Petunjuk [E / SPASI] LANJUT

    public TypewriterEffect typewriterEffect;

    [Header("Fade UI")]
    [SerializeField] private Image fadeOverlay;

    [Header("Settings")]
    public string gameSceneName = "Gameplay";
    public float textStayDuration = 3f;
    public float fadeDuration = 0.6f;

    [TextArea(2, 5)]
    public string[] prologueLines;

    public static MainMenuManager Instance { get; private set; }

    private bool isTransitioning = false;

    private void Awake()
    {
        Instance = this;

        CreateFadeOverlayIfMissing();
    }

    private void Start()
    {
        // Pastikan Cursor aktif di MainMenu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Pastikan Menu utama tampil, panel lain sembunyi di awal
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (prologueCanvas != null) prologueCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        // Pasang listener tombol utama
        if (playButton != null) playButton.onClick.AddListener(StartPrologue);
        if (guideButton != null) guideButton.onClick.AddListener(OpenGuide);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        // Pasang listener tombol Guide sub-menu
        if (hintsButton != null) hintsButton.onClick.AddListener(OpenHints);
        if (swipeMechanicButton != null) swipeMechanicButton.onClick.AddListener(OpenSwipeMechanic);

        // Smooth fade-in saat pertama kali masuk Main Menu
        if (fadeOverlay != null)
        {
            StartCoroutine(FadeScreen(1f, 0f, 0.8f));
        }
    }

    private void CreateFadeOverlayIfMissing()
    {
        if (fadeOverlay != null) return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject fadeObj = new GameObject("ScreenFadeOverlay", typeof(RectTransform));
        fadeObj.transform.SetParent(canvas.transform, false);
        fadeObj.transform.SetAsLastSibling();

        RectTransform rt = fadeObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.one;

        Image img = fadeObj.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;
        fadeOverlay = img;
    }

    private void Update()
    {
        // Fitur Scroll Teks panjang Swipe Mechanic (Wheel Mouse / Panah Keyboard)
        if (swipeMechanicSubPanel != null && swipeMechanicSubPanel.activeSelf)
        {
            HandleSwipeScrollInput();
        }

        // Navigasi Bertahap Tombol ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (hintsSubPanel != null && hintsSubPanel.activeSelf)
            {
                OpenGuide(); // Kembali dari Hints ke Menu Guide
            }
            else if (swipeMechanicSubPanel != null && swipeMechanicSubPanel.activeSelf)
            {
                OpenGuide(); // Kembali dari Swipe Mechanic ke Menu Guide
            }
            else if (guidePanel != null && guidePanel.activeSelf)
            {
                CloseSubPanels(); // Kembali dari Guide ke Main Menu
            }
            else if (creditsPanel != null && creditsPanel.activeSelf)
            {
                CloseSubPanels(); // Kembali dari Credits ke Main Menu
            }
        }
    }

    private void HandleSwipeScrollInput()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (swipeScrollRect != null)
        {
            if (scrollDelta != 0f)
            {
                swipeScrollRect.verticalNormalizedPosition += scrollDelta * 0.5f;
                swipeScrollRect.verticalNormalizedPosition = Mathf.Clamp01(swipeScrollRect.verticalNormalizedPosition);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                swipeScrollRect.verticalNormalizedPosition += Time.deltaTime;
                swipeScrollRect.verticalNormalizedPosition = Mathf.Clamp01(swipeScrollRect.verticalNormalizedPosition);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                swipeScrollRect.verticalNormalizedPosition -= Time.deltaTime;
                swipeScrollRect.verticalNormalizedPosition = Mathf.Clamp01(swipeScrollRect.verticalNormalizedPosition);
            }
        }
        else if (swipeTextRect != null)
        {
            Vector2 pos = swipeTextRect.anchoredPosition;

            if (scrollDelta != 0f)
            {
                pos.y -= scrollDelta * keyboardScrollSpeed;
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                pos.y -= keyboardScrollSpeed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                pos.y += keyboardScrollSpeed * Time.deltaTime;
            }

            pos.y = Mathf.Max(0, pos.y);
            swipeTextRect.anchoredPosition = pos;
        }
    }

    public void OpenGuide()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);

        if (guidePanel != null) guidePanel.SetActive(true);
    }

    public void OpenHints()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);

        if (hintsSubPanel != null) hintsSubPanel.SetActive(true);
    }

    public void OpenSwipeMechanic()
    {
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);

        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(true);

        if (swipeScrollRect != null)
        {
            swipeScrollRect.verticalNormalizedPosition = 1f;
        }
        else if (swipeTextRect != null)
        {
            swipeTextRect.anchoredPosition = Vector2.zero;
        }
    }

    public void OpenCredits()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);

        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseSubPanels()
    {
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void StartPrologue()
    {
        if (isTransitioning) return;
        StartCoroutine(StartPrologueWithFadeRoutine());
    }

    private IEnumerator StartPrologueWithFadeRoutine()
    {
        isTransitioning = true;

        // 1. Fade out Main Menu ke hitam
        yield return FadeScreen(0f, 1f, 0.5f);

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (prologueCanvas != null) prologueCanvas.SetActive(true);
        if (prologueText != null) prologueText.text = "";

        // 2. Fade in ke Prologue Screen
        yield return FadeScreen(1f, 0f, 0.5f);

        // 3. Mainkan Prologue baris demi baris
        yield return StartCoroutine(PrologueRoutine());
    }

    private IEnumerator PrologueRoutine()
    {
        if (prologueLines == null || prologueLines.Length == 0)
        {
            yield return StartCoroutine(FinishPrologueAndLoadGame());
            yield break;
        }

        for (int i = 0; i < prologueLines.Length; i++)
        {
            string line = prologueLines[i];

            SetTextAlpha(1);
            if (typewriterEffect != null && prologueText != null)
            {
                typewriterEffect.StartTyping(prologueText, line);
            }
            else if (prologueText != null)
            {
                prologueText.text = line;
            }

            // Tunggu ngetik sambil bisa ditekan [E/Spasi/Enter/Klik] untuk selesaikan ngetik baris ini
            while (typewriterEffect != null && typewriterEffect.IsTyping)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    typewriterEffect.CompleteTyping();
                    break;
                }
                yield return null;
            }

            // Tunggu input pemain untuk lanjut ke kalimat berikutnya ATAU tunggu durasi stay
            float elapsed = 0f;
            yield return null; // Jeda 1 frame agar input ketik tidak tembus ke skip baris

            while (elapsed < textStayDuration)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                {
                    break; // Skip ke baris berikutnya
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Fade Out baris sebelum berganti ke baris selanjutnya
            if (i < prologueLines.Length - 1)
            {
                yield return FadeText(1f, 0f, 0.35f);
                if (prologueText != null) prologueText.text = "";
                yield return new WaitForSeconds(0.2f);
            }
        }

        yield return StartCoroutine(FinishPrologueAndLoadGame());
    }

    private IEnumerator FinishPrologueAndLoadGame()
    {
        // Fade out Prologue ke hitam sebelum loading scene gameplay
        yield return FadeScreen(0f, 1f, 0.7f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene(gameSceneName);
    }

    public IEnumerator FadeScreen(float startAlpha, float endAlpha, float duration)
    {
        if (fadeOverlay == null)
        {
            CreateFadeOverlayIfMissing();
        }

        if (fadeOverlay == null) yield break;

        fadeOverlay.gameObject.SetActive(true);
        Color c = Color.black;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeOverlay.color = c;

        if (endAlpha <= 0f)
        {
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        if (prologueText == null) yield break;

        float time = 0;
        Color c = prologueText.color;
        c.a = startAlpha;
        prologueText.color = c;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            prologueText.color = c;
            yield return null;
        }

        c.a = endAlpha;
        prologueText.color = c;
    }

    private void SetTextAlpha(float alpha)
    {
        if (prologueText == null) return;
        Color c = prologueText.color;
        c.a = alpha;
        prologueText.color = c;
    }
}
