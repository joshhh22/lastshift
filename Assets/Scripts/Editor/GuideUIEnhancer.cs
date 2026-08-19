#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GuideUIEnhancer
{
    [MenuItem("Tools/Last Shift/Percantik GuidePanel & Halaman Kontrol (Cool Sci-Fi UI)")]
    public static void EnhanceAllGuidePanels()
    {
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "MainMenu")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan di scene MainMenu!");
            return;
        }

        MainMenuManager menuMgr = Object.FindObjectOfType<MainMenuManager>();
        if (menuMgr == null)
        {
            Debug.LogError("MainMenuManager tidak ditemukan di scene!");
            return;
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font == null)
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in fonts)
            {
                if (f.name.Contains("HomeVideo")) { font = f; break; }
            }
            if (font == null && fonts.Length > 0) font = fonts[0];
        }

        // =========================================================================
        // 1. PERCANTIK GUIDEPANEL (HUB PEMILIHAN PANDUAN)
        // =========================================================================
        if (menuMgr.guidePanel != null)
        {
            GameObject guideObj = menuMgr.guidePanel;
            Undo.RegisterCompleteObjectUndo(guideObj, "Enhance GuidePanel");

            RectTransform rt = guideObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(880, 540);

            // Background
            Image bg = guideObj.GetComponent<Image>();
            if (bg == null) bg = guideObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            // Bersihkan child lama
            for (int i = guideObj.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(guideObj.transform.GetChild(i).gameObject);
            }

            // --- Header ---
            GameObject header = CreateUIObject("Header", guideObj.transform);
            RectTransform hRt = header.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0, 1);
            hRt.anchorMax = new Vector2(1, 1);
            hRt.pivot = new Vector2(0.5f, 1);
            hRt.anchoredPosition = new Vector2(0, -25);
            hRt.sizeDelta = new Vector2(-40, 75);

            CreateTMPText("Badge", header.transform, new Vector2(0, 0), new Vector2(0, 20), "TERMINAL ARSIP STASIUN // PUSAT PANDUAN", 14, font, FontStyles.Bold, new Color(0f, 1f, 0.85f, 1f));
            CreateTMPText("Title", header.transform, new Vector2(0, -24), new Vector2(0, 32), "PANDUAN OPERASIONAL PETUGAS", 24, font, FontStyles.Bold, Color.white);
            CreateTMPText("Subtitle", header.transform, new Vector2(0, -56), new Vector2(0, 20), "Pilih topik panduan di bawah ini untuk melihat tata cara bertugas:", 15, font, FontStyles.Normal, new Color(0.7f, 0.8f, 0.9f, 1f));

            // --- 2 Menu Cards (Hints & Swipe) ---
            GameObject cardsContainer = CreateUIObject("CardsContainer", guideObj.transform);
            RectTransform cRt = cardsContainer.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0.5f, 0.5f);
            cRt.anchorMax = new Vector2(0.5f, 0.5f);
            cRt.pivot = new Vector2(0.5f, 0.5f);
            cRt.anchoredPosition = new Vector2(0, -10);
            cRt.sizeDelta = new Vector2(800, 280);

            // Card 1: KONTROL (Hints)
            Button hintsBtn = CreateGuideCard("HintsCard", cardsContainer.transform, new Vector2(-205, 0), new Vector2(370, 260),
                "// KONTROL & NAVIGASI",
                "Tata Cara Kontrol Karakter",
                "Panduan lengkap tombol gerak [WASD], interaksi loket [E], tombol mouse, dan navigasi di stasiun.",
                font,
                () => { if (MainMenuManager.Instance != null) MainMenuManager.Instance.OpenHints(); });

            // Card 2: SWIPE (Swipe Tutorial)
            Button swipeBtn = CreateGuideCard("SwipeCard", cardsContainer.transform, new Vector2(205, 0), new Vector2(370, 260),
                "// PEMINDAIAN TIKET",
                "Tutorial Gesek Kartu (7 Langkah)",
                "Panduan visual bergambar langkah demi langkah: ambil kartu, snap di mesin scanner, hingga gesek ke kanan.",
                font,
                () => { if (MainMenuManager.Instance != null) MainMenuManager.Instance.OpenSwipeMechanic(); });

            // Close Button
            Button backBtn = CreateButton("BackButton", guideObj.transform, new Vector2(0, -225), new Vector2(200, 44), "KEMBALI (ESC)", font);
            backBtn.onClick.AddListener(() => { if (MainMenuManager.Instance != null) MainMenuManager.Instance.CloseSubPanels(); });

            // Link ke MainMenuManager
            menuMgr.hintsButton = hintsBtn;
            menuMgr.swipeMechanicButton = swipeBtn;
        }

        // =========================================================================
        // 2. PERCANTIK HINTSUBPANEL / BUTTON (HALAMAN KONTROL)
        // =========================================================================
        if (menuMgr.hintsSubPanel != null)
        {
            GameObject hintsObj = menuMgr.hintsSubPanel;
            Undo.RegisterCompleteObjectUndo(hintsObj, "Enhance HintsSubPanel");

            RectTransform rt = hintsObj.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(920, 600);

            // Background
            Image bg = hintsObj.GetComponent<Image>();
            if (bg == null) bg = hintsObj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            // Bersihkan child lama
            for (int i = hintsObj.transform.childCount - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(hintsObj.transform.GetChild(i).gameObject);
            }

            // --- Header ---
            GameObject header = CreateUIObject("Header", hintsObj.transform);
            RectTransform hRt = header.GetComponent<RectTransform>();
            hRt.anchorMin = new Vector2(0, 1);
            hRt.anchorMax = new Vector2(1, 1);
            hRt.pivot = new Vector2(0.5f, 1);
            hRt.anchoredPosition = new Vector2(0, -20);
            hRt.sizeDelta = new Vector2(-40, 65);

            CreateTMPText("Badge", header.transform, new Vector2(0, 0), new Vector2(0, 20), "SISTEM KONTROL PETUGAS // LAST SHIFT", 14, font, FontStyles.Bold, new Color(0f, 1f, 0.85f, 1f));
            CreateTMPText("Title", header.transform, new Vector2(0, -22), new Vector2(0, 32), "PANDUAN KONTROL & NAVIGASI", 24, font, FontStyles.Bold, Color.white);

            // Top-right Back Button
            Button topBackBtn = CreateButton("BackButton", header.transform, new Vector2(240, -10), new Vector2(170, 38), "KEMBALI (ESC)", font);
            topBackBtn.onClick.AddListener(() => { if (MainMenuManager.Instance != null) MainMenuManager.Instance.OpenGuide(); });

            // --- Grid Kontrol ---
            GameObject gridObj = CreateUIObject("ControlsGrid", hintsObj.transform);
            RectTransform gRt = gridObj.GetComponent<RectTransform>();
            gRt.anchorMin = new Vector2(0.5f, 0.5f);
            gRt.anchorMax = new Vector2(0.5f, 0.5f);
            gRt.pivot = new Vector2(0.5f, 0.5f);
            gRt.anchoredPosition = new Vector2(0, 25);
            gRt.sizeDelta = new Vector2(840, 340);

            // 6 Baris Kontrol Rapi & Keren
            CreateControlRow(gridObj.transform, new Vector2(0, 125), "[ W ] [ A ] [ S ] [ D ]  /  [ PANAH ]", "Navigasi & Berjalan Menjelajahi Area Stasiun", font);
            CreateControlRow(gridObj.transform, new Vector2(0, 75), "[ KURSOR MOUSE ]", "Mengarahkan Sudut Pandang Kamera (First Person Look)", font);
            CreateControlRow(gridObj.transform, new Vector2(0, 25), "[ E ]", "Melayani Penumpang di Meja Loket & Interaksi Objek", font);
            CreateControlRow(gridObj.transform, new Vector2(0, -25), "[ KLIK KIRI MOUSE ]", "Memegang Tiket Penumpang & Menekan Tombol Komputer", font);
            CreateControlRow(gridObj.transform, new Vector2(0, -75), "[ DRAG & SWIPE MOUSE ]", "Memindahkan Kartu & Menggesek Kartu di Celah Scanner", font);
            CreateControlRow(gridObj.transform, new Vector2(0, -125), "[ ESC ]", "Membuka Pause Menu / Kembali ke Menu Sebelumnya", font);

            // --- Tips Box (Bawah) ---
            GameObject tipBox = CreateUIObject("TipBox", hintsObj.transform);
            RectTransform tipRt = tipBox.GetComponent<RectTransform>();
            tipRt.anchorMin = new Vector2(0.5f, 0);
            tipRt.anchorMax = new Vector2(0.5f, 0);
            tipRt.pivot = new Vector2(0.5f, 0);
            tipRt.anchoredPosition = new Vector2(0, 75);
            tipRt.sizeDelta = new Vector2(840, 50);

            Image tipBg = tipBox.AddComponent<Image>();
            tipBg.color = new Color(0.12f, 0.2f, 0.28f, 0.6f);

            var tipTxt = CreateTMPText("TipText", tipBox.transform, Vector2.zero, new Vector2(-20, 0),
                "<b>[TIPS PETUGAS]</b> Selalu periksa data tiket penumpang di log komputer loket sebelum memutuskan menerima atau menolak!",
                14, font, FontStyles.Normal, new Color(0.85f, 0.95f, 1f, 1f));
            tipTxt.alignment = TextAlignmentOptions.Center;
        }

        // Simpan Scene
        EditorUtility.SetDirty(menuMgr);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=green>[GuideUIEnhancer]</color> GuidePanel dan Halaman Kontrol berhasil dipercantik dengan font HomeVideo dan pure ASCII.");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static TMP_Text CreateTMPText(string name, Transform parent, Vector2 pos, Vector2 size, string text, float fontSize, TMP_FontAsset font, FontStyles style, Color color)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        TMP_Text t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.color = color;
        t.text = text;
        t.raycastTarget = false;
        return t;
    }

    private static void CreateControlRow(Transform parent, Vector2 pos, string keyText, string descText, TMP_FontAsset font)
    {
        GameObject row = CreateUIObject("ControlRow", parent);
        RectTransform rt = row.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(800, 42);

        Image rowBg = row.AddComponent<Image>();
        rowBg.color = new Color(0.1f, 0.15f, 0.22f, 0.7f);
        rowBg.raycastTarget = false;

        // Keycap Badge (Kiri)
        GameObject keyObj = CreateUIObject("Keycap", row.transform);
        RectTransform keyRt = keyObj.GetComponent<RectTransform>();
        keyRt.anchorMin = new Vector2(0, 0.5f);
        keyRt.anchorMax = new Vector2(0, 0.5f);
        keyRt.pivot = new Vector2(0, 0.5f);
        keyRt.anchoredPosition = new Vector2(15, 0);
        keyRt.sizeDelta = new Vector2(300, 30);

        TMP_Text kt = keyObj.AddComponent<TextMeshProUGUI>();
        if (font != null) kt.font = font;
        kt.fontSize = 15;
        kt.fontStyle = FontStyles.Bold;
        kt.alignment = TextAlignmentOptions.Left;
        kt.color = new Color(0f, 1f, 0.85f, 1f);
        kt.text = keyText;
        kt.raycastTarget = false;

        // Description (Kanan)
        GameObject descObj = CreateUIObject("Desc", row.transform);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 0.5f);
        descRt.anchorMax = new Vector2(1, 0.5f);
        descRt.pivot = new Vector2(0, 0.5f);
        descRt.anchoredPosition = new Vector2(320, 0);
        descRt.sizeDelta = new Vector2(-335, 30);

        TMP_Text dt = descObj.AddComponent<TextMeshProUGUI>();
        if (font != null) dt.font = font;
        dt.fontSize = 14;
        dt.alignment = TextAlignmentOptions.Left;
        dt.color = Color.white;
        dt.text = descText;
        dt.raycastTarget = false;
    }

    private static Button CreateGuideCard(string name, Transform parent, Vector2 pos, Vector2 size, string tag, string title, string desc, TMP_FontAsset font, UnityEngine.Events.UnityAction onClick)
    {
        GameObject cardObj = CreateUIObject(name, parent);
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image bg = cardObj.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.14f, 0.20f, 0.9f);
        bg.raycastTarget = true;

        Button btn = cardObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.7f, 0.85f, 1f);
        cb.pressedColor = new Color(0f, 0.45f, 0.6f, 1f);
        btn.colors = cb;

        // Tag Badge
        GameObject tagObj = CreateUIObject("Tag", cardObj.transform);
        RectTransform tagRt = tagObj.GetComponent<RectTransform>();
        tagRt.anchorMin = new Vector2(0.5f, 1);
        tagRt.anchorMax = new Vector2(0.5f, 1);
        tagRt.pivot = new Vector2(0.5f, 1);
        tagRt.anchoredPosition = new Vector2(0, -18);
        tagRt.sizeDelta = new Vector2(300, 24);
        TMP_Text tagText = tagObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tagText.font = font;
        tagText.fontSize = 14;
        tagText.fontStyle = FontStyles.Bold;
        tagText.alignment = TextAlignmentOptions.Center;
        tagText.color = new Color(0f, 1f, 0.85f, 1f);
        tagText.text = tag;
        tagText.raycastTarget = false;

        // Title
        GameObject titleObj = CreateUIObject("Title", cardObj.transform);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1);
        titleRt.anchorMax = new Vector2(0.5f, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.anchoredPosition = new Vector2(0, -48);
        titleRt.sizeDelta = new Vector2(330, 40);
        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) titleText.font = font;
        titleText.fontSize = 17;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.text = title;
        titleText.raycastTarget = false;

        // Divider
        GameObject divObj = CreateUIObject("Divider", cardObj.transform);
        RectTransform divRt = divObj.GetComponent<RectTransform>();
        divRt.anchorMin = new Vector2(0.5f, 1);
        divRt.anchorMax = new Vector2(0.5f, 1);
        divRt.pivot = new Vector2(0.5f, 1);
        divRt.anchoredPosition = new Vector2(0, -95);
        divRt.sizeDelta = new Vector2(300, 2);
        Image divImg = divObj.AddComponent<Image>();
        divImg.color = new Color(0.2f, 0.35f, 0.45f, 0.5f);
        divImg.raycastTarget = false;

        // Description
        GameObject descObj = CreateUIObject("Desc", cardObj.transform);
        RectTransform descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0, 0);
        descRt.anchorMax = new Vector2(1, 1);
        descRt.pivot = new Vector2(0.5f, 0.5f);
        descRt.anchoredPosition = new Vector2(0, -35);
        descRt.sizeDelta = new Vector2(-30, -120);
        TMP_Text descText = descObj.AddComponent<TextMeshProUGUI>();
        if (font != null) descText.font = font;
        descText.fontSize = 13;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = new Color(0.8f, 0.9f, 1f, 1f);
        descText.text = desc;
        descText.raycastTarget = false;

        // Button Click CTA
        GameObject ctaObj = CreateUIObject("CTA", cardObj.transform);
        RectTransform ctaRt = ctaObj.GetComponent<RectTransform>();
        ctaRt.anchorMin = new Vector2(0, 0);
        ctaRt.anchorMax = new Vector2(1, 0);
        ctaRt.pivot = new Vector2(0.5f, 0);
        ctaRt.anchoredPosition = new Vector2(0, 15);
        ctaRt.sizeDelta = new Vector2(-40, 30);
        TMP_Text ctaText = ctaObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ctaText.font = font;
        ctaText.fontSize = 13;
        ctaText.fontStyle = FontStyles.Bold;
        ctaText.alignment = TextAlignmentOptions.Center;
        ctaText.color = new Color(0f, 0.9f, 1f, 1f);
        ctaText.text = "[ KLIK UNTUK MEMBUKA ]";
        ctaText.raycastTarget = false;

        if (onClick != null) btn.onClick.AddListener(onClick);
        return btn;
    }

    private static Button CreateButton(string name, Transform parent, Vector2 pos, Vector2 size, string text, TMP_FontAsset font)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.18f, 0.28f, 0.38f, 0.95f);
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.85f, 0.75f, 1f);
        cb.pressedColor = new Color(0f, 0.5f, 0.45f, 1f);
        btn.colors = cb;

        GameObject txtObj = CreateUIObject("Text", btnObj.transform);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TMP_Text t = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 14;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.text = text;
        t.raycastTarget = false;

        return btn;
    }
}
#endif