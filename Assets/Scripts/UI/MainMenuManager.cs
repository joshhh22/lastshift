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

    [Header("Play Choice (Continue / New Game)")]
    public GameObject playChoicePanel;
    public Button continueGameButton;
    public Button newGameButton;
    public Button playChoiceBackButton;
    public TMP_Text saveInfoText;

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

    [Header("Audio")]
    [SerializeField] private AudioClip clickSfx;
    private AudioSource audioSource;

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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

#if UNITY_EDITOR
        if (clickSfx == null)
        {
            clickSfx = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/click.mp3");
        }
#endif

        CreateFadeOverlayIfMissing();
    }

    private void Start()
    {
        // Pastikan Cursor aktif dan bebas di MainMenu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Pastikan Menu utama tampil, panel lain sembunyi di awal
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (prologueCanvas != null) prologueCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        // Pasang listener tombol utama
        if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (guideButton != null) guideButton.onClick.AddListener(OpenGuide);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        // Pasang listener tombol Play Choice (jika sudah ada di scene)
        if (continueGameButton != null) continueGameButton.onClick.AddListener(ContinueGame);
        if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
        if (playChoiceBackButton != null) playChoiceBackButton.onClick.AddListener(ClosePlayChoicePanel);

        // Pasang listener tombol Guide sub-menu
        if (hintsButton != null) hintsButton.onClick.AddListener(OpenHints);
        if (swipeMechanicButton != null) swipeMechanicButton.onClick.AddListener(OpenSwipeMechanic);

        // Pasang listener tombol Back di semua panel secara otomatis & persisten
        HookAllBackButtons();

        // Pasang audio click.mp3 ke SEMUA tombol di scene secara otomatis
        foreach (Button btn in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (btn != null)
            {
                btn.onClick.AddListener(PlayClickSound);
            }
        }

        // Smooth fade-in saat pertama kali masuk Main Menu
        if (fadeOverlay != null)
        {
            StartCoroutine(FadeScreen(1f, 0f, 0.8f));
        }
    }

    public TMP_FontAsset GetHomeVideoFont()
    {
#if UNITY_EDITOR
        TMP_FontAsset font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font != null) return font;
#endif
        var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in fonts)
        {
            if (f != null && f.name.Contains("HomeVideo")) return f;
        }
        if (fonts.Length > 0) return fonts[0];
        return null;
    }

    public void HookAllBackButtons()
    {
        // 1. Guide Panel Back Buttons
        if (guidePanel != null)
        {
            foreach (Button b in guidePanel.GetComponentsInChildren<Button>(true))
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("back") || n.Contains("kembali") || n.Contains("close") || n.Contains("esc"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(CloseSubPanels);
                    b.onClick.AddListener(PlayClickSound);
                }
            }
        }

        // 2. Hints Sub Panel Back Buttons
        if (hintsSubPanel != null)
        {
            foreach (Button b in hintsSubPanel.GetComponentsInChildren<Button>(true))
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("back") || n.Contains("kembali") || n.Contains("close") || n.Contains("esc"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(OpenGuide);
                    b.onClick.AddListener(PlayClickSound);
                }
            }
        }

        // 3. Swipe Mechanic Sub Panel Back Buttons
        if (swipeMechanicSubPanel != null)
        {
            foreach (Button b in swipeMechanicSubPanel.GetComponentsInChildren<Button>(true))
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("back") || n.Contains("kembali") || n.Contains("close") || n.Contains("esc"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(OpenGuide);
                    b.onClick.AddListener(PlayClickSound);
                }
            }
        }

        // 4. Credits Panel Back Buttons
        if (creditsPanel != null)
        {
            foreach (Button b in creditsPanel.GetComponentsInChildren<Button>(true))
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("back") || n.Contains("kembali") || n.Contains("close") || n.Contains("esc"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(CloseSubPanels);
                    b.onClick.AddListener(PlayClickSound);
                }
            }
        }

        // 5. Play Choice Modal Back Buttons
        if (playChoicePanel != null)
        {
            foreach (Button b in playChoicePanel.GetComponentsInChildren<Button>(true))
            {
                string n = b.gameObject.name.ToLower();
                if (n.Contains("back") || n.Contains("kembali") || n.Contains("close") || n.Contains("esc"))
                {
                    b.onClick.RemoveAllListeners();
                    b.onClick.AddListener(ClosePlayChoicePanel);
                    b.onClick.AddListener(PlayClickSound);
                }
            }
        }
    }

    public void PlayClickSound()
    {
        if (audioSource != null && clickSfx != null)
        {
            audioSource.PlayOneShot(clickSfx, 0.85f);
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
            if (playChoicePanel != null && playChoicePanel.activeSelf)
            {
                ClosePlayChoicePanel(); // Kembali dari Play Choice ke Main Menu
            }
            else if (hintsSubPanel != null && hintsSubPanel.activeSelf)
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
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
            guidePanel.transform.SetAsLastSibling();
        }

        HookAllBackButtons();
    }

    public void OpenHints()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        if (hintsSubPanel != null)
        {
            hintsSubPanel.SetActive(true);
            hintsSubPanel.transform.SetAsLastSibling();
        }

        HookAllBackButtons();
    }

    public void OpenSwipeMechanic()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        if (swipeMechanicSubPanel != null)
        {
            swipeMechanicSubPanel.SetActive(true);
            swipeMechanicSubPanel.transform.SetAsLastSibling();
        }

        if (swipeScrollRect != null)
        {
            swipeScrollRect.verticalNormalizedPosition = 1f;
        }
        else if (swipeTextRect != null)
        {
            swipeTextRect.anchoredPosition = Vector2.zero;
        }

        HookAllBackButtons();
    }

    public void OpenCredits()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            creditsPanel.transform.SetAsLastSibling();
        }

        HookAllBackButtons();
    }

    public void CloseSubPanels()
    {
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (playChoicePanel != null) playChoicePanel.SetActive(false);

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;

        if (SaveManager.HasSaveData())
        {
            OpenPlayChoicePanel();
        }
        else
        {
            StartNewGame();
        }
    }

    public void OpenPlayChoicePanel()
    {
        CreatePlayChoicePanelIfMissing();

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (guidePanel != null) guidePanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (hintsSubPanel != null) hintsSubPanel.SetActive(false);
        if (swipeMechanicSubPanel != null) swipeMechanicSubPanel.SetActive(false);

        if (playChoicePanel != null)
        {
            playChoicePanel.SetActive(true);
            playChoicePanel.transform.SetAsLastSibling();

            SaveData data = SaveManager.GetSaveData();
            if (data != null && data.hasSaveData)
            {
                if (saveInfoText != null)
                {
                    string objName = !string.IsNullOrEmpty(data.savedObjectiveTitle) ? data.savedObjectiveTitle : "Shift Standby";
                    saveInfoText.text = $"Last Saved: <color=#00F0FF>DAY {data.currentDay}</color> • {objName}\n<size=11><color=#888888>{data.saveDateFormatted}</color></size>";
                }

                if (continueGameButton != null)
                {
                    TMP_Text btnTxt = continueGameButton.GetComponentInChildren<TMP_Text>();
                    if (btnTxt != null)
                    {
                        btnTxt.text = $"[ > ] CONTINUE (DAY {data.currentDay})";
                    }
                }
            }

            HookAllBackButtons();
        }
    }

    public void ClosePlayChoicePanel()
    {
        if (playChoicePanel != null) playChoicePanel.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }

    public void ContinueGame()
    {
        if (isTransitioning) return;
        StartCoroutine(ContinueGameRoutine());
    }

    private IEnumerator ContinueGameRoutine()
    {
        isTransitioning = true;
        SaveManager.IsContinuingGame = true;

        // Fade out Main Menu ke hitam
        yield return FadeScreen(0f, 1f, 0.6f);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Langsung load Gameplay scene tanpa prologue
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartNewGame()
    {
        if (isTransitioning) return;
        SaveManager.IsContinuingGame = false;
        SaveManager.ClearSaveData();

        StartPrologue();
    }

    private void CreatePlayChoicePanelIfMissing()
    {
        if (playChoicePanel != null) return;

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        TMP_FontAsset font = GetHomeVideoFont();

        // Container Modal
        GameObject panelObj = new GameObject("PlayChoiceModal", typeof(RectTransform));
        panelObj.transform.SetParent(canvas.transform, false);
        panelObj.transform.SetAsLastSibling();

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(480, 340);
        panelRt.anchoredPosition = Vector2.zero;

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.04f, 0.06f, 0.09f, 0.96f);

        // Header Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0, 45);
        titleRt.anchoredPosition = new Vector2(0, -18);
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) titleText.font = font;
        titleText.text = "SELECT MISSION";
        titleText.fontSize = 22;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0f, 0.95f, 1f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;

        // Save Info Text
        GameObject infoObj = new GameObject("SaveInfo", typeof(RectTransform));
        infoObj.transform.SetParent(panelObj.transform, false);
        RectTransform infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.05f, 1f);
        infoRt.anchorMax = new Vector2(0.95f, 1f);
        infoRt.pivot = new Vector2(0.5f, 1f);
        infoRt.sizeDelta = new Vector2(0, 50);
        infoRt.anchoredPosition = new Vector2(0, -65);
        saveInfoText = infoObj.AddComponent<TextMeshProUGUI>();
        if (font != null) saveInfoText.font = font;
        saveInfoText.text = "SAVED PROGRESS FOUND";
        saveInfoText.fontSize = 13;
        saveInfoText.color = new Color(0.8f, 0.85f, 0.9f, 0.9f);
        saveInfoText.alignment = TextAlignmentOptions.Center;

        // Continue Button
        continueGameButton = CreateModalButton(panelObj.transform, "ContinueButton", "[ > ] CONTINUE SHIFT", new Vector2(0, -135), new Color(0.08f, 0.28f, 0.42f, 1f), new Color(0f, 1f, 0.95f, 1f), font);
        continueGameButton.onClick.AddListener(ContinueGame);

        // New Game Button
        newGameButton = CreateModalButton(panelObj.transform, "NewGameButton", "[ * ] START NEW GAME", new Vector2(0, -190), new Color(0.18f, 0.18f, 0.22f, 1f), Color.white, font);
        newGameButton.onClick.AddListener(StartNewGame);

        // Back Button
        playChoiceBackButton = CreateModalButton(panelObj.transform, "BackButton", "KEMBALI (ESC)", new Vector2(0, -245), new Color(0.12f, 0.12f, 0.14f, 0.8f), new Color(0.7f, 0.7f, 0.7f, 1f), font);
        playChoiceBackButton.onClick.AddListener(ClosePlayChoicePanel);

        playChoicePanel = panelObj;
        playChoicePanel.SetActive(false);
    }

    private Button CreateModalButton(Transform parent, string name, string label, Vector2 anchoredPos, Color bgColor, Color textColor, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(400, 44);
        rt.anchoredPosition = anchoredPos;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.3f;
        colors.pressedColor = bgColor * 0.8f;
        colors.selectedColor = bgColor;
        btn.colors = colors;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TMP_Text tmp = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 15;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        btn.onClick.AddListener(PlayClickSound);

        return btn;
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
