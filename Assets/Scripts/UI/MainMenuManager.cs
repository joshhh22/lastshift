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

    public TypewriterEffect typewriterEffect;

    [Header("Settings")]
    public string gameSceneName = "Gameplay";
    public float textStayDuration = 2f;
    public float fadeDuration = 1.0f;

    [TextArea(2, 5)]
    public string[] prologueLines;

    public static MainMenuManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
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

        // Jika menggunakan ScrollRect standar Unity UI
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
        // Jika menggunakan pergeseran posisi RectTransform teks langsung
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

        // Reset scroll ke paling atas
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
        // Hilangkan menu utama
        mainMenuCanvas.SetActive(false);
        // Tampilkan layar hitam prologue
        prologueCanvas.SetActive(true);
        // Teks awal jadikan putih penuh (alpha 1) karena akan diketik satu per satu
        SetTextAlpha(1);
        prologueText.text = "";

        StartCoroutine(PrologueRoutine());
    }

    private IEnumerator PrologueRoutine()
    {
        // Jeda bentar sebelum teks pertama muncul (biar dramatis)
        yield return new WaitForSeconds(1f);

        // Putar semua kalimat prologue satu per satu
        foreach (string line in prologueLines)
        {
            // Mulai ngetik
            SetTextAlpha(1); 
            typewriterEffect.StartTyping(prologueText, line);

            // Tunggu sampai typewriter selesai ngetik huruf terakhir
            while (typewriterEffect.IsTyping)
            {
                yield return null;
            }

            // Tunggu orang baca setelah selesai ngetik
            yield return new WaitForSeconds(textStayDuration);

            // Fade Out (Menghilang perlahan)
            if (line != prologueLines[prologueLines.Length - 1]) // Kecuali kalimat terakhir
            {
                yield return FadeText(1f, 0f, fadeDuration);
                prologueText.text = ""; // Bersihkan text biar siap ngetik kalimat baru
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Tahan kalimat terakhir agak lama
        yield return new WaitForSeconds(1f);

        // Tunggu transisi masuk ke gameplay perlahan
        prologueText.text = "Loading...";
        
        // Sembunyikan dan kunci cursor sebelum masuk ke dalam game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Pindah Scene
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
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
        Color c = prologueText.color;
        c.a = alpha;
        prologueText.color = c;
    }
}
