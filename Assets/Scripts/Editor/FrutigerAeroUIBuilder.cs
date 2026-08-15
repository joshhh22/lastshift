#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class FrutigerAeroUIBuilder
{
    [MenuItem("Tools/Last Shift/Bangun Frutiger Aero Computer UI (100000% Better)")]
    public static void BuildFrutigerAeroComputerUI()
    {
        // 1. Pastikan Sprite Import Settings untuk seluruh aset Frutiger Aero
        string[] imgPaths = new string[]
        {
            "Assets/Art/UI/FrutigerAero/Wallpaper.jpg",
            "Assets/Art/UI/FrutigerAero/Icon_Assignment.jpg",
            "Assets/Art/UI/FrutigerAero/Icon_CCTV.jpg",
            "Assets/Art/UI/FrutigerAero/Icon_Logs.jpg"
        };

        foreach (string path in imgPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        Sprite wallpaperSpr = LoadOrCreateSprite("Assets/Art/UI/FrutigerAero/Wallpaper.jpg");
        Sprite assignSpr = LoadOrCreateSprite("Assets/Art/UI/FrutigerAero/Icon_Assignment.jpg");
        Sprite cctvSpr = LoadOrCreateSprite("Assets/Art/UI/FrutigerAero/Icon_CCTV.jpg");
        Sprite logsSpr = LoadOrCreateSprite("Assets/Art/UI/FrutigerAero/Icon_Logs.jpg");

        // 2. Buka scene Gameplay
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Gameplay")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        }

        // 3. Cari GameObject ComputerUIController & ComputerSystem
        ComputerUIController compController = Object.FindObjectOfType<ComputerUIController>(true);
        if (compController == null)
        {
            Debug.LogError("ComputerUIController tidak ditemukan di scene Gameplay!");
            return;
        }

        GameObject compUIObj = compController.gameObject;

        // Hapus permanen objek ComputerUI DOS lama agar tidak pernah muncul lagi
        GameObject oldDOSUI = GameObject.Find("ComputerUI");
        if (oldDOSUI != null && oldDOSUI != compUIObj)
        {
            GameObject.DestroyImmediate(oldDOSUI);
            Debug.Log("[FrutigerAeroUIBuilder] Objek ComputerUI DOS lama berhasil dihapus secara permanen.");
        }

        // Matikan objek CCTVPage Canvas fullscreen lama jika ada
        GameObject oldCCTVCanvas = GameObject.Find("CCTVPage");
        if (oldCCTVCanvas != null && oldCCTVCanvas != compUIObj)
        {
            Canvas c = oldCCTVCanvas.GetComponent<Canvas>();
            if (c != null && oldCCTVCanvas.transform.parent != compUIObj.transform)
            {
                c.enabled = false;
            }
        }

        // Pastikan field computerUI di ComputerUIController menunjuk ke ComputerSystem
        SerializedObject cCtrlSO = new SerializedObject(compController);
        var compUIProp = cCtrlSO.FindProperty("computerUI");
        if (compUIProp != null)
        {
            compUIProp.objectReferenceValue = compUIObj;
            cCtrlSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(compController);
        }

        // Update juga di UIManager jika ada
        UIManager uiMgr = Object.FindObjectOfType<UIManager>(true);
        if (uiMgr != null)
        {
            SerializedObject uiSO = new SerializedObject(uiMgr);
            var uicompProp = uiSO.FindProperty("computerUI");
            if (uicompProp != null)
            {
                uicompProp.objectReferenceValue = compUIObj;
                uiSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(uiMgr);
            }
        }

        Undo.RegisterCompleteObjectUndo(compUIObj, "Build Frutiger Aero Computer UI");

        // Ambil Font HomeVideo-Regular SDF (sesuai request user)
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font == null)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Bold SDF.asset");
        }
        if (font == null)
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (fonts.Length > 0) font = fonts[0];
        }

        // Ambil komponen controller
        TerminalMenu terminalMenu = compUIObj.GetComponentInChildren<TerminalMenu>(true);

        // Tambahkan / Ambil FrutigerAeroComputerUI
        FrutigerAeroComputerUI aeroUI = compUIObj.GetComponent<FrutigerAeroComputerUI>();
        if (aeroUI == null) aeroUI = compUIObj.AddComponent<FrutigerAeroComputerUI>();

        // Pastikan Canvas Scaler & GraphicRaycaster ada pada ComputerSystem
        Canvas canvas = compUIObj.GetComponent<Canvas>();
        if (canvas == null) canvas = compUIObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = compUIObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = compUIObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = compUIObj.GetComponent<GraphicRaycaster>();
        if (raycaster == null) compUIObj.AddComponent<GraphicRaycaster>();

        // =========================================================================
        // 1. DESKTOP WALLPAPER
        // =========================================================================
        Transform wallpaperTrans = compUIObj.transform.Find("Aero_Wallpaper");
        GameObject wallpaperObj;
        if (wallpaperTrans == null)
        {
            wallpaperObj = CreateUIObject("Aero_Wallpaper", compUIObj.transform);
            wallpaperObj.transform.SetAsFirstSibling();
        }
        else
        {
            wallpaperObj = wallpaperTrans.gameObject;
        }

        RectTransform wallRect = wallpaperObj.GetComponent<RectTransform>();
        wallRect.anchorMin = Vector2.zero;
        wallRect.anchorMax = Vector2.one;
        wallRect.sizeDelta = Vector2.zero;

        Image wallImg = wallpaperObj.GetComponent<Image>();
        if (wallImg == null) wallImg = wallpaperObj.AddComponent<Image>();
        wallImg.sprite = wallpaperSpr;
        wallImg.color = Color.white;
        aeroUI.wallpaperImage = wallImg;

        // =========================================================================
        // 2. DESKTOP SHORTCUTS CONTAINER (KIRI ATAS)
        // =========================================================================
        Transform shortcutsTrans = compUIObj.transform.Find("Aero_DesktopShortcuts");
        GameObject shortcutsObj;
        if (shortcutsTrans == null) shortcutsObj = CreateUIObject("Aero_DesktopShortcuts", compUIObj.transform);
        else shortcutsObj = shortcutsTrans.gameObject;

        RectTransform shortRect = shortcutsObj.GetComponent<RectTransform>();
        shortRect.anchorMin = new Vector2(0, 1);
        shortRect.anchorMax = new Vector2(0, 1);
        shortRect.pivot = new Vector2(0, 1);
        shortRect.anchoredPosition = new Vector2(40, -40);
        shortRect.sizeDelta = new Vector2(160, 480);
        aeroUI.desktopShortcutsContainer = shortRect;

        // Bersihkan child shortcuts lama
        for (int i = shortcutsObj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(shortcutsObj.transform.GetChild(i).gameObject);
        }

        // Buat 3 Icon Desktop Keren dengan Selection Glow Halo
        GameObject glow1, glow2, glow3;
        Button scAssign = CreateDesktopIcon("Icon_Assignment", shortcutsObj.transform, new Vector2(0, -10), assignSpr, "TUGAS LOKET", font, out glow1);
        Button scCCTV = CreateDesktopIcon("Icon_CCTV", shortcutsObj.transform, new Vector2(0, -150), cctvSpr, "CCTV MONITOR", font, out glow2);
        Button scLogs = CreateDesktopIcon("Icon_Logs", shortcutsObj.transform, new Vector2(0, -290), logsSpr, "DATABASE LOGS", font, out glow3);

        aeroUI.shortcutAssignmentBtn = scAssign;
        aeroUI.shortcutCCTVBtn = scCCTV;
        aeroUI.shortcutLogsBtn = scLogs;
        aeroUI.shortcutSelectionGlows = new GameObject[] { glow1, glow2, glow3 };

        // =========================================================================
        // 3. DESKTOP HINT BAR (BAWAH DI ATAS TASKBAR)
        // =========================================================================
        Transform hintTrans = compUIObj.transform.Find("Aero_DesktopHint");
        GameObject hintObj;
        if (hintTrans == null) hintObj = CreateUIObject("Aero_DesktopHint", compUIObj.transform);
        else hintObj = hintTrans.gameObject;

        RectTransform hintRt = hintObj.GetComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(0.5f, 0);
        hintRt.anchorMax = new Vector2(0.5f, 0);
        hintRt.pivot = new Vector2(0.5f, 0);
        hintRt.anchoredPosition = new Vector2(0, 68);
        hintRt.sizeDelta = new Vector2(740, 36);

        Image hintBg = hintObj.GetComponent<Image>();
        if (hintBg == null) hintBg = hintObj.AddComponent<Image>();
        hintBg.color = new Color(0.04f, 0.16f, 0.28f, 0.85f);

        GameObject hintTxtObj = hintObj.transform.Find("Text") != null ? hintObj.transform.Find("Text").gameObject : CreateUIObject("Text", hintObj.transform);
        RectTransform htRt = hintTxtObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.sizeDelta = Vector2.zero;
        TMP_Text ht = hintTxtObj.GetComponent<TextMeshProUGUI>();
        if (ht == null) ht = hintTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ht.font = font;
        ht.fontSize = 13;
        ht.fontStyle = FontStyles.Bold;
        ht.alignment = TextAlignmentOptions.Center;
        ht.color = new Color(0f, 1f, 0.9f, 1f);
        ht.text = "[!] PETUNJUK: Klik mouse / tombol [PANAH & ENTER] | Tekan [ESC] untuk keluar";

        // =========================================================================
        // 4. TASKBAR (BAWAH)
        // =========================================================================
        Transform taskbarTrans = compUIObj.transform.Find("Aero_Taskbar");
        GameObject taskbarObj;
        if (taskbarTrans == null) taskbarObj = CreateUIObject("Aero_Taskbar", compUIObj.transform);
        else taskbarObj = taskbarTrans.gameObject;

        RectTransform tbRect = taskbarObj.GetComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0, 0);
        tbRect.anchorMax = new Vector2(1, 0);
        tbRect.pivot = new Vector2(0.5f, 0);
        tbRect.anchoredPosition = Vector2.zero;
        tbRect.sizeDelta = new Vector2(0, 54);
        aeroUI.taskbar = tbRect;

        Image tbImg = taskbarObj.GetComponent<Image>();
        if (tbImg == null) tbImg = taskbarObj.AddComponent<Image>();
        tbImg.color = new Color(0.04f, 0.15f, 0.25f, 0.92f);

        // Bersihkan child taskbar lama
        for (int i = taskbarObj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(taskbarObj.transform.GetChild(i).gameObject);
        }

        // Start Orb Button (Kiri)
        Button startOrb = CreateStartOrb(taskbarObj.transform, font);
        aeroUI.startOrbButton = startOrb;

        // Taskbar App Tabs
        GameObject appTabs = CreateUIObject("AppTabs", taskbarObj.transform);
        RectTransform atRect = appTabs.GetComponent<RectTransform>();
        atRect.anchorMin = new Vector2(0, 0.5f);
        atRect.anchorMax = new Vector2(0, 0.5f);
        atRect.pivot = new Vector2(0, 0.5f);
        atRect.anchoredPosition = new Vector2(190, 0);
        atRect.sizeDelta = new Vector2(600, 44);

        GameObject tbGlow1, tbGlow2, tbGlow3;
        Button tbAssign = CreateTaskbarAppButton("TB_Assign", appTabs.transform, new Vector2(0, 0), "TUGAS LOKET", font, out tbGlow1);
        Button tbCCTV = CreateTaskbarAppButton("TB_CCTV", appTabs.transform, new Vector2(185, 0), "CCTV LIVE", font, out tbGlow2);
        Button tbLogs = CreateTaskbarAppButton("TB_Logs", appTabs.transform, new Vector2(370, 0), "SECURITY LOGS", font, out tbGlow3);

        aeroUI.taskbarAssignmentBtn = tbAssign;
        aeroUI.taskbarCCTVBtn = tbCCTV;
        aeroUI.taskbarLogsBtn = tbLogs;
        aeroUI.assignmentActiveGlow = tbGlow1;
        aeroUI.cctvActiveGlow = tbGlow2;
        aeroUI.logsActiveGlow = tbGlow3;

        // System Tray (Kanan)
        GameObject sysTray = CreateUIObject("SystemTray", taskbarObj.transform);
        RectTransform stRect = sysTray.GetComponent<RectTransform>();
        stRect.anchorMin = new Vector2(1, 0.5f);
        stRect.anchorMax = new Vector2(1, 0.5f);
        stRect.pivot = new Vector2(1, 0.5f);
        stRect.anchoredPosition = new Vector2(-15, 0);
        stRect.sizeDelta = new Vector2(380, 44);

        Image trayBg = sysTray.AddComponent<Image>();
        trayBg.color = new Color(0.08f, 0.22f, 0.35f, 0.7f);

        // Day Badge
        GameObject dayObj = CreateUIObject("DayBadge", sysTray.transform);
        RectTransform dRt = dayObj.GetComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0, 0.5f);
        dRt.anchorMax = new Vector2(0, 0.5f);
        dRt.pivot = new Vector2(0, 0.5f);
        dRt.anchoredPosition = new Vector2(15, 0);
        dRt.sizeDelta = new Vector2(110, 30);
        TMP_Text dayTxt = dayObj.AddComponent<TextMeshProUGUI>();
        if (font != null) dayTxt.font = font;
        dayTxt.fontSize = 15;
        dayTxt.fontStyle = FontStyles.Bold;
        dayTxt.alignment = TextAlignmentOptions.Left;
        dayTxt.color = new Color(0f, 1f, 0.85f, 1f);
        dayTxt.text = "DAY 1";
        aeroUI.dayBadgeText = dayTxt;

        // Clock Text
        GameObject clockObj = CreateUIObject("ClockText", sysTray.transform);
        RectTransform cRt = clockObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(1, 0.5f);
        cRt.anchorMax = new Vector2(1, 0.5f);
        cRt.pivot = new Vector2(1, 0.5f);
        cRt.anchoredPosition = new Vector2(-15, 0);
        cRt.sizeDelta = new Vector2(120, 30);
        TMP_Text clkTxt = clockObj.AddComponent<TextMeshProUGUI>();
        if (font != null) clkTxt.font = font;
        clkTxt.fontSize = 16;
        clkTxt.fontStyle = FontStyles.Bold;
        clkTxt.alignment = TextAlignmentOptions.Right;
        clkTxt.color = Color.white;
        clkTxt.text = "22:00";
        aeroUI.clockText = clkTxt;

        // =========================================================================
        // 5. FLOATING AERO GLASS WINDOWS
        // =========================================================================
        Transform winContainerTrans = compUIObj.transform.Find("Aero_WindowsContainer");
        GameObject winContainer;
        if (winContainerTrans == null) winContainer = CreateUIObject("Aero_WindowsContainer", compUIObj.transform);
        else winContainer = winContainerTrans.gameObject;

        RectTransform winContRect = winContainer.GetComponent<RectTransform>();
        winContRect.anchorMin = Vector2.zero;
        winContRect.anchorMax = Vector2.one;
        winContRect.sizeDelta = new Vector2(0, -54);

        // Bersihkan window container lama
        for (int i = winContainer.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(winContainer.transform.GetChild(i).gameObject);
        }

        // -------------------------------------------------------------
        // A. JENDELA ASSIGNMENT (TUGAS LOKET)
        // -------------------------------------------------------------
        GameObject assignWin = CreateAeroWindow("Window_Assignment", winContainer.transform, new Vector2(210, -25), new Vector2(960, 600), "TUGAS & PROTOKOL LOKET STASIUN", font, out Button assignClose, out TMP_Text assignTitle);
        aeroUI.assignmentWindow = assignWin;
        aeroUI.assignmentCloseBtn = assignClose;
        aeroUI.assignmentTitle = assignTitle;

        BuildAssignmentContent(assignWin.transform.Find("ContentArea"), font);

        // -------------------------------------------------------------
        // B. JENDELA CCTV (SURVEILLANCE LIVE)
        // -------------------------------------------------------------
        GameObject cctvWin = CreateAeroWindow("Window_CCTV", winContainer.transform, new Vector2(220, -20), new Vector2(960, 600), "SISTEM SURVEILLANCE CCTV REAL-TIME", font, out Button cctvClose, out TMP_Text cctvTitle);
        aeroUI.cctvWindow = cctvWin;
        aeroUI.cctvCloseBtn = cctvClose;
        aeroUI.cctvTitle = cctvTitle;

        BuildCCTVContent(cctvWin.transform.Find("ContentArea"), font, aeroUI);

        // -------------------------------------------------------------
        // C. JENDELA LOGS (SECURITY DATABASE)
        // -------------------------------------------------------------
        GameObject logsWin = CreateAeroWindow("Window_Logs", winContainer.transform, new Vector2(215, -22), new Vector2(960, 600), "DATABASE LOGS KEAMANAN & ANOMALI TIKET", font, out Button logsClose, out TMP_Text logsTitle);
        aeroUI.logsWindow = logsWin;
        aeroUI.logsCloseBtn = logsClose;
        aeroUI.logsTitle = logsTitle;

        BuildLogsContent(logsWin.transform.Find("ContentArea"), font);

        // Hubungkan Day & Time Text ke TerminalMenu jika ada
        if (terminalMenu != null)
        {
            SerializedObject tmSO = new SerializedObject(terminalMenu);
            var dtProp = tmSO.FindProperty("dayText");
            var ttProp = tmSO.FindProperty("timeText");
            if (dtProp != null) dtProp.objectReferenceValue = dayTxt;
            if (ttProp != null) ttProp.objectReferenceValue = clkTxt;
            tmSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(terminalMenu);
        }

        // Matikan MainMenu DOS lama di dalam compUIObj jika ada
        Transform oldMenu = compUIObj.transform.Find("MainMenu");
        if (oldMenu != null)
        {
            oldMenu.gameObject.SetActive(false);
        }

        // Start Menu Popup
        GameObject startPopup = CreateStartMenuPopup(compUIObj.transform, font);
        aeroUI.startMenuPopup = startPopup;

        // Pastikan di awal semua jendela tertutup rapi
        aeroUI.CloseAllWindows();

        // Simpan Scene
        EditorUtility.SetDirty(compUIObj);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        EditorUtility.DisplayDialog("Sukses!", "Sistem Operasi Frutiger Aero berhasil diperbarui!\n- Font HomeVideo-Regular diterapkan.\n- Simbol kotak hilang 100%.\n- UI DOS lama dihapus bersih.\n- Jam CCTV tersinkronisasi.", "Mantap!");
        Debug.Log("<color=green>[FrutigerAeroUIBuilder]</color> Berhasil membangun sistem operasi Frutiger Aero.");
    }

    private static Sprite LoadOrCreateSprite(string assetPath)
    {
        Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (spr != null) return spr;

        if (File.Exists(assetPath))
        {
            byte[] fileData = File.ReadAllBytes(assetPath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
        return null;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Button CreateDesktopIcon(string name, Transform parent, Vector2 pos, Sprite iconSprite, string labelText, TMP_FontAsset font, out GameObject selectionGlow)
    {
        GameObject iconObj = CreateUIObject(name, parent);
        RectTransform rt = iconObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(130, 125);

        Image bg = iconObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.25f, 0.4f, 0.35f);

        Button btn = iconObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.85f, 1f, 0.55f);
        cb.pressedColor = new Color(0f, 0.5f, 0.8f, 0.8f);
        btn.colors = cb;

        // Selection Glow Outline (Menunjukkan shortcut yang sedang dipilih panah keyboard)
        GameObject glowObj = CreateUIObject("SelectionGlow", iconObj.transform);
        RectTransform gRt = glowObj.GetComponent<RectTransform>();
        gRt.anchorMin = Vector2.zero;
        gRt.anchorMax = Vector2.one;
        gRt.sizeDelta = new Vector2(8, 8);
        Image gImg = glowObj.AddComponent<Image>();
        gImg.color = new Color(0f, 0.95f, 1f, 0.45f);
        glowObj.transform.SetAsFirstSibling();
        glowObj.SetActive(false);
        selectionGlow = glowObj;

        // Icon Graphic
        GameObject imgObj = CreateUIObject("Graphic", iconObj.transform);
        RectTransform imgRt = imgObj.GetComponent<RectTransform>();
        imgRt.anchorMin = new Vector2(0.5f, 1);
        imgRt.anchorMax = new Vector2(0.5f, 1);
        imgRt.pivot = new Vector2(0.5f, 1);
        imgRt.anchoredPosition = new Vector2(0, -8);
        imgRt.sizeDelta = new Vector2(80, 80);

        Image img = imgObj.AddComponent<Image>();
        img.sprite = iconSprite;
        img.color = Color.white;
        img.preserveAspect = true;

        // Label Text
        GameObject lblObj = CreateUIObject("Label", iconObj.transform);
        RectTransform lblRt = lblObj.GetComponent<RectTransform>();
        lblRt.anchorMin = new Vector2(0, 0);
        lblRt.anchorMax = new Vector2(1, 0);
        lblRt.pivot = new Vector2(0.5f, 0);
        lblRt.anchoredPosition = new Vector2(0, 6);
        lblRt.sizeDelta = new Vector2(0, 24);

        TMP_Text t = lblObj.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 12;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.text = labelText;

        return btn;
    }

    private static Button CreateStartOrb(Transform parent, TMP_FontAsset font)
    {
        GameObject orbObj = CreateUIObject("StartOrb", parent);
        RectTransform rt = orbObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = new Vector2(10, 0);
        rt.sizeDelta = new Vector2(165, 42);

        Image img = orbObj.AddComponent<Image>();
        img.color = new Color(0f, 0.65f, 0.85f, 0.95f);

        Button btn = orbObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.95f, 1f, 1f);
        cb.pressedColor = new Color(0f, 0.45f, 0.65f, 1f);
        btn.colors = cb;

        GameObject txtObj = CreateUIObject("Text", orbObj.transform);
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
        t.text = "[METRO OS]";

        return btn;
    }

    private static Button CreateTaskbarAppButton(string name, Transform parent, Vector2 pos, string label, TMP_FontAsset font, out GameObject glow)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(175, 40);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.12f, 0.28f, 0.42f, 0.75f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.8f, 1f, 0.9f);
        cb.pressedColor = new Color(0f, 0.5f, 0.7f, 1f);
        btn.colors = cb;

        GameObject txtObj = CreateUIObject("Text", btnObj.transform);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TMP_Text t = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 13;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.text = label;

        // Active Glow Underline
        GameObject glowObj = CreateUIObject("ActiveGlow", btnObj.transform);
        RectTransform gRt = glowObj.GetComponent<RectTransform>();
        gRt.anchorMin = new Vector2(0, 0);
        gRt.anchorMax = new Vector2(1, 0);
        gRt.pivot = new Vector2(0.5f, 0);
        gRt.sizeDelta = new Vector2(0, 4);

        Image gImg = glowObj.AddComponent<Image>();
        gImg.color = new Color(0f, 1f, 0.9f, 1f);
        glowObj.SetActive(false);
        glow = glowObj;

        return btn;
    }

    private static GameObject CreateAeroWindow(string name, Transform parent, Vector2 pos, Vector2 size, string titleText, TMP_FontAsset font, out Button closeBtn, out TMP_Text titleTMP)
    {
        GameObject winObj = CreateUIObject(name, parent);
        RectTransform rt = winObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        // Frosted Glass Window Background
        Image bg = winObj.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.12f, 0.22f, 0.96f);

        // --- Title Bar ---
        GameObject titleBar = CreateUIObject("TitleBar", winObj.transform);
        RectTransform tbRt = titleBar.GetComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = Vector2.zero;
        tbRt.sizeDelta = new Vector2(0, 42);

        Image tbBg = titleBar.AddComponent<Image>();
        tbBg.color = new Color(0.1f, 0.32f, 0.52f, 0.98f);

        // Title Text
        GameObject titleTxtObj = CreateUIObject("TitleText", titleBar.transform);
        RectTransform ttRt = titleTxtObj.GetComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0, 0);
        ttRt.anchorMax = new Vector2(1, 1);
        ttRt.anchoredPosition = new Vector2(18, 0);
        ttRt.sizeDelta = new Vector2(-70, 0);

        TMP_Text tt = titleTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) tt.font = font;
        tt.fontSize = 14;
        tt.fontStyle = FontStyles.Bold;
        tt.alignment = TextAlignmentOptions.Left;
        tt.color = Color.white;
        tt.text = titleText;
        titleTMP = tt;

        // Close Button (X)
        GameObject closeObj = CreateUIObject("CloseButton", titleBar.transform);
        RectTransform cRt = closeObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(1, 0.5f);
        cRt.anchorMax = new Vector2(1, 0.5f);
        cRt.pivot = new Vector2(1, 0.5f);
        cRt.anchoredPosition = new Vector2(-8, 0);
        cRt.sizeDelta = new Vector2(36, 28);

        Image closeImg = closeObj.AddComponent<Image>();
        closeImg.color = new Color(0.85f, 0.2f, 0.2f, 0.9f);

        closeBtn = closeObj.AddComponent<Button>();
        ColorBlock cb = closeBtn.colors;
        cb.highlightedColor = new Color(1f, 0.35f, 0.35f, 1f);
        cb.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
        closeBtn.colors = cb;

        GameObject closeTxtObj = CreateUIObject("Text", closeObj.transform);
        RectTransform ctRt = closeTxtObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;
        TMP_Text ct = closeTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ct.font = font;
        ct.fontSize = 15;
        ct.fontStyle = FontStyles.Bold;
        ct.alignment = TextAlignmentOptions.Center;
        ct.color = Color.white;
        ct.text = "X";

        // Content Area Container
        GameObject contentArea = CreateUIObject("ContentArea", winObj.transform);
        RectTransform caRt = contentArea.GetComponent<RectTransform>();
        caRt.anchorMin = Vector2.zero;
        caRt.anchorMax = Vector2.one;
        caRt.anchoredPosition = new Vector2(0, -21);
        caRt.sizeDelta = new Vector2(-24, -62);

        return winObj;
    }

    private static void BuildAssignmentContent(Transform contentArea, TMP_FontAsset font)
    {
        // 1. Header Banner
        GameObject header = CreateUIObject("Header", contentArea);
        RectTransform hRt = header.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = new Vector2(0, -8);
        hRt.sizeDelta = new Vector2(0, 46);

        Image hBg = header.AddComponent<Image>();
        hBg.color = new Color(0.08f, 0.22f, 0.36f, 0.8f);

        GameObject hTxtObj = CreateUIObject("Text", header.transform);
        RectTransform htRt = hTxtObj.GetComponent<RectTransform>();
        htRt.anchorMin = Vector2.zero;
        htRt.anchorMax = Vector2.one;
        htRt.anchoredPosition = new Vector2(15, 0);
        htRt.sizeDelta = new Vector2(-30, 0);
        TMP_Text ht = hTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ht.font = font;
        ht.fontSize = 14;
        ht.fontStyle = FontStyles.Bold;
        ht.alignment = TextAlignmentOptions.Left;
        ht.color = new Color(0f, 1f, 0.9f, 1f);
        ht.text = "DAFTAR TUGAS SHIFT MALAM // METRO LINE 2142";

        // 2. Objectives List Card (Kiri)
        GameObject taskCard = CreateUIObject("TaskCard", contentArea);
        RectTransform tcRt = taskCard.GetComponent<RectTransform>();
        tcRt.anchorMin = new Vector2(0, 0);
        tcRt.anchorMax = new Vector2(0.6f, 1);
        tcRt.pivot = new Vector2(0, 1);
        tcRt.anchoredPosition = new Vector2(0, -62);
        tcRt.sizeDelta = new Vector2(-10, -105);

        Image tcBg = taskCard.AddComponent<Image>();
        tcBg.color = new Color(0.06f, 0.16f, 0.26f, 0.75f);

        GameObject txtObj = CreateUIObject("ObjectiveText", taskCard.transform);
        RectTransform tRt = txtObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.anchoredPosition = new Vector2(16, -16);
        tRt.sizeDelta = new Vector2(-32, -32);

        TMP_Text objTxt = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) objTxt.font = font;
        objTxt.fontSize = 13;
        objTxt.lineSpacing = 8;
        objTxt.enableWordWrapping = true;
        objTxt.color = Color.white;
        objTxt.text =
            "<color=#00F0FF><b>> [AKTIF]</b></color>  <color=#FFFFFF><b>Periksa Sistem Komputer & Loket</b></color>\n\n" +
            "<color=#607D8B><b>- [PENDING]</b></color>  <color=#78909C>Periksa CCTV Keamanan Stasiun</color>\n\n" +
            "<color=#607D8B><b>- [PENDING]</b></color>  <color=#78909C>Layani & Saring Penumpang Kereta</color>";

        // Attach AssignmentPage script
        AssignmentPage ap = contentArea.gameObject.GetComponent<AssignmentPage>();
        if (ap == null) ap = contentArea.gameObject.AddComponent<AssignmentPage>();
        SerializedObject apSO = new SerializedObject(ap);
        var otProp = apSO.FindProperty("objectiveText");
        if (otProp != null) otProp.objectReferenceValue = objTxt;
        apSO.ApplyModifiedProperties();

        // 3. Protocol Rules Card (Kanan)
        GameObject ruleCard = CreateUIObject("RuleCard", contentArea);
        RectTransform rcRt = ruleCard.GetComponent<RectTransform>();
        rcRt.anchorMin = new Vector2(0.62f, 0);
        rcRt.anchorMax = new Vector2(1, 1);
        rcRt.pivot = new Vector2(0, 1);
        rcRt.anchoredPosition = new Vector2(0, -62);
        rcRt.sizeDelta = new Vector2(0, -105);

        Image rcBg = ruleCard.AddComponent<Image>();
        rcBg.color = new Color(0.06f, 0.16f, 0.26f, 0.75f);

        GameObject rcTxtObj = CreateUIObject("Text", ruleCard.transform);
        RectTransform rctRt = rcTxtObj.GetComponent<RectTransform>();
        rctRt.anchorMin = Vector2.zero;
        rctRt.anchorMax = Vector2.one;
        rctRt.anchoredPosition = new Vector2(16, -16);
        rctRt.sizeDelta = new Vector2(-32, -32);

        TMP_Text rcTxt = rcTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) rcTxt.font = font;
        rcTxt.fontSize = 12;
        rcTxt.lineSpacing = 6;
        rcTxt.enableWordWrapping = true;
        rcTxt.color = Color.white;
        rcTxt.text =
            "<color=#FFD600><b>[!] PROTOKOL PETUGAS:</b></color>\n\n" +
            "1. <b>Validasi Tiket:</b> Cek stasiun tujuan & tanggal kedaluwarsa.\n\n" +
            "2. <b>Saring Penipu:</b> Tiket expired atau salah rute WAJIB ditolak.\n\n" +
            "3. <b>Waspada Anomali:</b> Perhatikan gelagat aneh & tiket palsu!";

        // 4. Bottom Hint Bar
        CreateSubHintBar(contentArea, "[!] Dekati loket stasiun untuk menjalankan tugas | Tekan [ESC] atau klik [X] untuk menutup", font);
    }

    private static void BuildCCTVContent(Transform contentArea, TMP_FontAsset font, FrutigerAeroComputerUI aeroUI)
    {
        // 1. Header Info Bar
        GameObject header = CreateUIObject("CCTVHeader", contentArea);
        RectTransform hRt = header.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = new Vector2(0, -6);
        hRt.sizeDelta = new Vector2(0, 42);

        Image hBg = header.AddComponent<Image>();
        hBg.color = new Color(0.06f, 0.18f, 0.3f, 0.85f);

        GameObject camLblObj = CreateUIObject("CamLabel", header.transform);
        RectTransform clRt = camLblObj.GetComponent<RectTransform>();
        clRt.anchorMin = Vector2.zero;
        clRt.anchorMax = new Vector2(0.5f, 1);
        clRt.anchoredPosition = new Vector2(15, 0);
        clRt.sizeDelta = new Vector2(-30, 0);
        TMP_Text clTxt = camLblObj.AddComponent<TextMeshProUGUI>();
        if (font != null) clTxt.font = font;
        clTxt.fontSize = 14;
        clTxt.fontStyle = FontStyles.Bold;
        clTxt.alignment = TextAlignmentOptions.Left;
        clTxt.color = new Color(0f, 1f, 0.85f, 1f);
        clTxt.text = "CAM 01: LOBBY COUNTER";
        aeroUI.cctvCameraLabel = clTxt;

        GameObject recLblObj = CreateUIObject("RecLabel", header.transform);
        RectTransform rlRt = recLblObj.GetComponent<RectTransform>();
        rlRt.anchorMin = new Vector2(0.5f, 0);
        rlRt.anchorMax = Vector2.one;
        rlRt.anchoredPosition = new Vector2(-15, 0);
        rlRt.sizeDelta = new Vector2(-30, 0);
        TMP_Text rlTxt = recLblObj.AddComponent<TextMeshProUGUI>();
        if (font != null) rlTxt.font = font;
        rlTxt.fontSize = 14;
        rlTxt.fontStyle = FontStyles.Bold;
        rlTxt.alignment = TextAlignmentOptions.Right;
        rlTxt.color = new Color(1f, 0.2f, 0.2f, 1f);
        rlTxt.text = "REC 22:00";
        aeroUI.cctvRecLabel = rlTxt;

        // 2. Center Viewport Area (Live Feed RenderTexture)
        GameObject vpObj = CreateUIObject("CameraViewport", contentArea);
        RectTransform vpRt = vpObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.anchoredPosition = new Vector2(0, -10);
        vpRt.sizeDelta = new Vector2(-20, -95);

        RawImage vpRawImg = vpObj.AddComponent<RawImage>();
        vpRawImg.color = Color.white;
        if (CCTVManager.Instance != null && CCTVManager.Instance.CctvRenderTexture != null)
        {
            vpRawImg.texture = CCTVManager.Instance.CctvRenderTexture;
        }
        aeroUI.cctvViewportRawImage = vpRawImg;

        // 3. Arrow Switch Buttons (Mouse-clickable)
        GameObject prevBtnObj = CreateUIObject("PrevCamBtn", contentArea);
        RectTransform pbRt = prevBtnObj.GetComponent<RectTransform>();
        pbRt.anchorMin = new Vector2(0, 0.5f);
        pbRt.anchorMax = new Vector2(0, 0.5f);
        pbRt.pivot = new Vector2(0, 0.5f);
        pbRt.anchoredPosition = new Vector2(12, -10);
        pbRt.sizeDelta = new Vector2(145, 46);

        Image pbImg = prevBtnObj.AddComponent<Image>();
        pbImg.color = new Color(0.08f, 0.25f, 0.4f, 0.85f);
        Button prevBtn = prevBtnObj.AddComponent<Button>();
        prevBtn.onClick.AddListener(() => { if (CCTVManager.Instance != null) CCTVManager.Instance.PreviousCamera(); });

        GameObject pbTxtObj = CreateUIObject("Text", prevBtnObj.transform);
        RectTransform pbtRt = pbTxtObj.GetComponent<RectTransform>();
        pbtRt.anchorMin = Vector2.zero;
        pbtRt.anchorMax = Vector2.one;
        pbtRt.sizeDelta = Vector2.zero;
        TMP_Text pbt = pbTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) pbt.font = font;
        pbt.fontSize = 13;
        pbt.fontStyle = FontStyles.Bold;
        pbt.alignment = TextAlignmentOptions.Center;
        pbt.color = Color.white;
        pbt.text = "< PREV CAM";

        GameObject nextBtnObj = CreateUIObject("NextCamBtn", contentArea);
        RectTransform nbRt = nextBtnObj.GetComponent<RectTransform>();
        nbRt.anchorMin = new Vector2(1, 0.5f);
        nbRt.anchorMax = new Vector2(1, 0.5f);
        nbRt.pivot = new Vector2(1, 0.5f);
        nbRt.anchoredPosition = new Vector2(-12, -10);
        nbRt.sizeDelta = new Vector2(145, 46);

        Image nbImg = nextBtnObj.AddComponent<Image>();
        nbImg.color = new Color(0.08f, 0.25f, 0.4f, 0.85f);
        Button nextBtn = nextBtnObj.AddComponent<Button>();
        nextBtn.onClick.AddListener(() => { if (CCTVManager.Instance != null) CCTVManager.Instance.NextCamera(); });

        GameObject nbTxtObj = CreateUIObject("Text", nextBtnObj.transform);
        RectTransform nbtRt = nbTxtObj.GetComponent<RectTransform>();
        nbtRt.anchorMin = Vector2.zero;
        nbtRt.anchorMax = Vector2.one;
        nbtRt.sizeDelta = Vector2.zero;
        TMP_Text nbt = nbTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) nbt.font = font;
        nbt.fontSize = 13;
        nbt.fontStyle = FontStyles.Bold;
        nbt.alignment = TextAlignmentOptions.Center;
        nbt.color = Color.white;
        nbt.text = "NEXT CAM >";

        // 4. Bottom Hint Bar
        CreateSubHintBar(contentArea, "[!] Klik tombol [PREV / NEXT] atau tekan [PANAH KIRI / KANAN] untuk ganti kamera | [ESC] Tutup", font);
    }

    private static void BuildLogsContent(Transform contentArea, TMP_FontAsset font)
    {
        // 1. Sidebar Kiri (Pilihan 4 Kategori)
        GameObject sidebar = CreateUIObject("CategorySidebar", contentArea);
        RectTransform sbRt = sidebar.GetComponent<RectTransform>();
        sbRt.anchorMin = new Vector2(0, 0);
        sbRt.anchorMax = new Vector2(0.35f, 1);
        sbRt.pivot = new Vector2(0, 1);
        sbRt.anchoredPosition = new Vector2(0, -6);
        sbRt.sizeDelta = new Vector2(-8, -48);

        Image sbBg = sidebar.AddComponent<Image>();
        sbBg.color = new Color(0.05f, 0.14f, 0.24f, 0.85f);

        // 2. Detail Viewer Kanan
        GameObject viewer = CreateUIObject("DetailViewer", contentArea);
        RectTransform vRt = viewer.GetComponent<RectTransform>();
        vRt.anchorMin = new Vector2(0.37f, 0);
        vRt.anchorMax = new Vector2(1, 1);
        vRt.pivot = new Vector2(0, 1);
        vRt.anchoredPosition = new Vector2(0, -6);
        vRt.sizeDelta = new Vector2(0, -48);

        Image vBg = viewer.AddComponent<Image>();
        vBg.color = new Color(0.04f, 0.12f, 0.2f, 0.9f);

        GameObject detailTxtObj = CreateUIObject("ContentText", viewer.transform);
        RectTransform dtRt = detailTxtObj.GetComponent<RectTransform>();
        dtRt.anchorMin = Vector2.zero;
        dtRt.anchorMax = Vector2.one;
        dtRt.anchoredPosition = new Vector2(20, -20);
        dtRt.sizeDelta = new Vector2(-40, -40);

        TMP_Text detailTxt = detailTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) detailTxt.font = font;
        detailTxt.fontSize = 13;
        detailTxt.lineSpacing = 8;
        detailTxt.enableWordWrapping = true;
        detailTxt.color = Color.white;

        // 3. Buat 4 Tombol Kategori (Sesuai TicketGenerator.cs & Tanpa Jawaban Jujur)
        string[] cats = new string[]
        {
            "TIKET INVALID",
            "TIKET EXPIRED",
            "SALAH TUJUAN",
            "TIKET PALSU"
        };

        Button[] catButtons = new Button[cats.Length];

        for (int i = 0; i < cats.Length; i++)
        {
            GameObject cBtnObj = CreateUIObject($"CatBtn_{i}", sidebar.transform);
            RectTransform cbRt = cBtnObj.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0, 1);
            cbRt.anchorMax = new Vector2(1, 1);
            cbRt.pivot = new Vector2(0.5f, 1);
            cbRt.anchoredPosition = new Vector2(0, -12 - (i * 60));
            cbRt.sizeDelta = new Vector2(-20, 50);

            Image cbImg = cBtnObj.AddComponent<Image>();
            cbImg.color = (i == 0) ? new Color(0f, 0.45f, 0.75f, 0.95f) : new Color(0.08f, 0.2f, 0.32f, 0.6f);

            Button cBtn = cBtnObj.AddComponent<Button>();
            ColorBlock cb = cBtn.colors;
            cb.highlightedColor = new Color(0f, 0.8f, 1f, 0.8f);
            cb.pressedColor = new Color(0f, 0.5f, 0.7f, 1f);
            cBtn.colors = cb;
            catButtons[i] = cBtn;

            GameObject cbTxtObj = CreateUIObject("Text", cBtnObj.transform);
            RectTransform cbtRt = cbTxtObj.GetComponent<RectTransform>();
            cbtRt.anchorMin = Vector2.zero;
            cbtRt.anchorMax = Vector2.one;
            cbtRt.anchoredPosition = new Vector2(14, 0);
            cbtRt.sizeDelta = new Vector2(-28, 0);
            TMP_Text cbt = cbTxtObj.AddComponent<TextMeshProUGUI>();
            if (font != null) cbt.font = font;
            cbt.fontSize = 13;
            cbt.fontStyle = FontStyles.Bold;
            cbt.alignment = TextAlignmentOptions.Left;
            cbt.color = Color.white;
            cbt.text = cats[i];
        }

        // Attach FrutigerAeroLogsViewer runtime controller
        FrutigerAeroLogsViewer logsViewer = contentArea.gameObject.GetComponent<FrutigerAeroLogsViewer>();
        if (logsViewer == null) logsViewer = contentArea.gameObject.AddComponent<FrutigerAeroLogsViewer>();
        logsViewer.categoryButtons = catButtons;
        logsViewer.detailText = detailTxt;
        logsViewer.SelectCategory(0);

        // 4. Bottom Hint Bar
        CreateSubHintBar(contentArea, "[!] Klik kategori atau gunakan tombol [PANAH ATAS / BAWAH] | Tekan [ESC] untuk kembali", font);
    }

    private static void CreateSubHintBar(Transform parent, string text, TMP_FontAsset font)
    {
        GameObject hint = CreateUIObject("WindowHint", parent);
        RectTransform hRt = hint.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 0);
        hRt.anchorMax = new Vector2(1, 0);
        hRt.pivot = new Vector2(0.5f, 0);
        hRt.anchoredPosition = new Vector2(0, 4);
        hRt.sizeDelta = new Vector2(0, 32);

        Image hBg = hint.AddComponent<Image>();
        hBg.color = new Color(0.04f, 0.12f, 0.22f, 0.85f);

        GameObject tObj = CreateUIObject("Text", hint.transform);
        RectTransform tRt = tObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.anchoredPosition = new Vector2(10, 0);
        tRt.sizeDelta = new Vector2(-20, 0);

        TMP_Text t = tObj.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 12;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = false;
        t.color = new Color(0f, 1f, 0.9f, 1f);
        t.text = text;
    }

    private static GameObject CreateStartMenuPopup(Transform parent, TMP_FontAsset font)
    {
        GameObject popup = CreateUIObject("StartMenuPopup", parent);
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = new Vector2(10, 60);
        rt.sizeDelta = new Vector2(340, 420);

        Image bg = popup.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.15f, 0.25f, 0.97f);

        // Header
        GameObject header = CreateUIObject("Header", popup.transform);
        RectTransform hRt = header.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = new Vector2(0, -15);
        hRt.sizeDelta = new Vector2(-24, 60);

        TMP_Text title = header.AddComponent<TextMeshProUGUI>();
        if (font != null) title.font = font;
        title.fontSize = 16;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Left;
        title.color = new Color(0f, 1f, 0.9f, 1f);
        title.text = "METRO TRANSIT OS // 2142\n<size=12><color=#B0BEC5>Subway Line Station #04</color></size>";

        // Info List
        GameObject info = CreateUIObject("Info", popup.transform);
        RectTransform iRt = info.GetComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0, 0);
        iRt.anchorMax = new Vector2(1, 1);
        iRt.anchoredPosition = new Vector2(0, -40);
        iRt.sizeDelta = new Vector2(-24, -90);

        TMP_Text infoText = info.AddComponent<TextMeshProUGUI>();
        if (font != null) infoText.font = font;
        infoText.fontSize = 13;
        infoText.color = Color.white;
        infoText.text =
            "<b>Status Petugas:</b> <color=#00E676>ONLINE</color>\n" +
            "<b>Otoritas:</b> Tiket & Keamanan Loket\n" +
            "<b>Sistem:</b> AeroCore v4.28\n" +
            "<b>Protokol:</b> Saring Anomali & Penipu\n\n" +
            "<color=#00F0FF>-------------------------</color>\n" +
            "[!] <i>Gunakan mouse atau tombol panah untuk memilih menu aplikasi.</i>";

        popup.SetActive(false);
        return popup;
    }
}
#endif
