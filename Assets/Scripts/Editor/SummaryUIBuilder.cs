#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SummaryUIBuilder
{
    [MenuItem("Tools/Last Shift/Setup Fears to Fathom Style Summary Panel")]
    public static void BuildFearsToFathomSummaryPanel()
    {
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Gameplay")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        }

        // 1. Ambil Font HomeVideo-Regular (Hanya gunakan Regular, tanpa Bold)
        TMP_FontAsset fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (fontRegular == null)
        {
            fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Bold SDF.asset");
        }

        // 2. Ambil Audio Clips
        AudioClip reportOpenClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Beep(ClockIN,Out,AccessGranted).mp3");
        AudioClip stampClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Access Denied.mp3");

        // 3. Cari SummaryUIController di Scene
        SummaryUIController summaryCtrl = Object.FindObjectOfType<SummaryUIController>(true);
        if (summaryCtrl == null)
        {
            GameObject ctrlObj = new GameObject("SummaryUIController");
            summaryCtrl = ctrlObj.AddComponent<SummaryUIController>();
        }

        // 4. Cari atau Buat Canvas UI utama
        Canvas mainCanvas = null;
        foreach (Canvas c in Object.FindObjectsOfType<Canvas>(true))
        {
            if (c.gameObject.name == "Canvas" || c.gameObject.name == "UI")
            {
                mainCanvas = c;
                break;
            }
        }
        if (mainCanvas == null)
        {
            mainCanvas = Object.FindObjectOfType<Canvas>(true);
        }

        // 5. Cari atau Buat SummaryPanel GameObject
        Transform summaryPanelTr = null;
        if (mainCanvas != null)
        {
            summaryPanelTr = mainCanvas.transform.Find("SummaryPanel");
        }
        if (summaryPanelTr == null)
        {
            GameObject spFind = GameObject.Find("SummaryPanel");
            if (spFind != null) summaryPanelTr = spFind.transform;
        }

        if (summaryPanelTr == null && mainCanvas != null)
        {
            GameObject spObj = CreateUIObject("SummaryPanel", mainCanvas.transform);
            summaryPanelTr = spObj.transform;
        }

        if (summaryPanelTr == null)
        {
            Debug.LogError("[SummaryUIBuilder] Canvas tidak ditemukan!");
            return;
        }

        // Bersihkan child lama di SummaryPanel agar layout baru 100% rapi dan presisi
        while (summaryPanelTr.childCount > 0)
        {
            GameObject.DestroyImmediate(summaryPanelTr.GetChild(0).gameObject);
        }

        // Setup SummaryPanel Root Background (CRT Dark Slate Black)
        RectTransform rootRt = summaryPanelTr.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.sizeDelta = Vector2.zero;
        rootRt.anchoredPosition = Vector2.zero;

        Image rootImg = summaryPanelTr.GetComponent<Image>();
        if (rootImg == null) rootImg = summaryPanelTr.gameObject.AddComponent<Image>();
        rootImg.color = new Color(0.02f, 0.035f, 0.055f, 0.98f);

        // =========================================================================
        // A. CRT VINTAGE FRAME & BORDER CONTAINER
        // =========================================================================
        GameObject frameObj = CreateUIObject("RetroFrameContainer", summaryPanelTr);
        RectTransform frameRt = frameObj.GetComponent<RectTransform>();
        frameRt.anchorMin = new Vector2(0.04f, 0.04f);
        frameRt.anchorMax = new Vector2(0.96f, 0.96f);
        frameRt.sizeDelta = Vector2.zero;

        Image frameImg = frameObj.AddComponent<Image>();
        frameImg.color = new Color(0.04f, 0.07f, 0.1f, 0.85f);

        // =========================================================================
        // B. TOP HEADER BAR
        // =========================================================================
        GameObject headerObj = CreateUIObject("HeaderBar", frameObj.transform);
        RectTransform hdrRt = headerObj.GetComponent<RectTransform>();
        hdrRt.anchorMin = new Vector2(0, 1);
        hdrRt.anchorMax = new Vector2(1, 1);
        hdrRt.pivot = new Vector2(0.5f, 1);
        hdrRt.anchoredPosition = new Vector2(0, -18);
        hdrRt.sizeDelta = new Vector2(-40, 105);

        // Title System
        GameObject sysTxtObj = CreateUIObject("SystemTitleText", headerObj.transform);
        RectTransform stRt = sysTxtObj.GetComponent<RectTransform>();
        stRt.anchorMin = new Vector2(0, 1);
        stRt.anchorMax = new Vector2(1, 1);
        stRt.pivot = new Vector2(0, 1);
        stRt.anchoredPosition = new Vector2(10, 0);
        stRt.sizeDelta = new Vector2(0, 26);

        TMP_Text sysTxt = sysTxtObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) sysTxt.font = fontRegular;
        sysTxt.fontSize = 17;
        sysTxt.fontStyle = FontStyles.Normal;
        sysTxt.color = new Color(0f, 0.95f, 0.85f, 0.95f);
        sysTxt.text = "METRO TRANSIT AUTHORITY // DAILY SHIFT REPORT";

        // Day Text (Besar & Jelas)
        GameObject dayTxtObj = CreateUIObject("DayText", headerObj.transform);
        RectTransform dtRt = dayTxtObj.GetComponent<RectTransform>();
        dtRt.anchorMin = new Vector2(0, 1);
        dtRt.anchorMax = new Vector2(1, 1);
        dtRt.pivot = new Vector2(0, 1);
        dtRt.anchoredPosition = new Vector2(10, -28);
        dtRt.sizeDelta = new Vector2(0, 44);

        TMP_Text dayTxt = dayTxtObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) dayTxt.font = fontRegular;
        dayTxt.fontSize = 34;
        dayTxt.fontStyle = FontStyles.Normal;
        dayTxt.color = Color.white;
        dayTxt.text = "DAY 03 COMPLETE";

        // Station Info Subtitle
        GameObject infoTxtObj = CreateUIObject("StationInfoText", headerObj.transform);
        RectTransform itRt = infoTxtObj.GetComponent<RectTransform>();
        itRt.anchorMin = new Vector2(0, 0);
        itRt.anchorMax = new Vector2(1, 0);
        itRt.pivot = new Vector2(0, 0);
        itRt.anchoredPosition = new Vector2(10, 4);
        itRt.sizeDelta = new Vector2(0, 26);

        TMP_Text infoTxt = infoTxtObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) infoTxt.font = fontRegular;
        infoTxt.fontSize = 15;
        infoTxt.fontStyle = FontStyles.Normal;
        infoTxt.color = new Color(0.55f, 0.75f, 0.88f, 0.95f);
        infoTxt.text = "SECTOR 04 SUBWAY // SHIFT: 00:00 - 04:00 AM // OPERATOR ID: #4092-A";

        // =========================================================================
        // C. EVALUATION BADGE / STAMP (Kanan Atas)
        // =========================================================================
        GameObject evalBadgeObj = CreateUIObject("EvaluationBadgeContainer", headerObj.transform);
        RectTransform ebRt = evalBadgeObj.GetComponent<RectTransform>();
        ebRt.anchorMin = new Vector2(1, 0.5f);
        ebRt.anchorMax = new Vector2(1, 0.5f);
        ebRt.pivot = new Vector2(1, 0.5f);
        ebRt.anchoredPosition = new Vector2(-10, 0);
        ebRt.sizeDelta = new Vector2(400, 60);

        Image ebBg = evalBadgeObj.AddComponent<Image>();
        ebBg.color = new Color(0f, 0.9f, 0.4f, 0.15f);

        GameObject evalTxtObj = CreateUIObject("EvaluationText", evalBadgeObj.transform);
        RectTransform etRt = evalTxtObj.GetComponent<RectTransform>();
        etRt.anchorMin = Vector2.zero;
        etRt.anchorMax = Vector2.one;
        etRt.sizeDelta = Vector2.zero;

        TMP_Text evalTxt = evalTxtObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) evalTxt.font = fontRegular;
        evalTxt.fontSize = 18;
        evalTxt.fontStyle = FontStyles.Normal;
        evalTxt.alignment = TextAlignmentOptions.Center;
        evalTxt.color = new Color(0f, 0.9f, 0.4f, 1f);
        evalTxt.text = "[ EVALUATION: SATISFACTORY ]";

        // =========================================================================
        // D. MAIN CONTENT SPLIT (Kiri: Metrik Performa, Kanan: Log Kegagalan)
        // =========================================================================
        GameObject contentSplitObj = CreateUIObject("ContentSplit", frameObj.transform);
        RectTransform csRt = contentSplitObj.GetComponent<RectTransform>();
        csRt.anchorMin = new Vector2(0, 0.20f);
        csRt.anchorMax = new Vector2(1, 0.81f);
        csRt.pivot = new Vector2(0.5f, 0.5f);
        csRt.anchoredPosition = Vector2.zero;
        csRt.sizeDelta = new Vector2(-40, 0);

        // --- 1. LEFT PANEL: PERFORMANCE METRICS ---
        GameObject leftPanel = CreateUIObject("LeftMetricsPanel", contentSplitObj.transform);
        RectTransform lpRt = leftPanel.GetComponent<RectTransform>();
        lpRt.anchorMin = new Vector2(0, 0);
        lpRt.anchorMax = new Vector2(0.48f, 1);
        lpRt.sizeDelta = Vector2.zero;

        Image lpBg = leftPanel.AddComponent<Image>();
        lpBg.color = new Color(0.03f, 0.05f, 0.08f, 0.8f);

        // Metric Section Header
        GameObject mHdrObj = CreateUIObject("MetricsHeader", leftPanel.transform);
        RectTransform mhRt = mHdrObj.GetComponent<RectTransform>();
        mhRt.anchorMin = new Vector2(0, 1);
        mhRt.anchorMax = new Vector2(1, 1);
        mhRt.pivot = new Vector2(0.5f, 1);
        mhRt.anchoredPosition = new Vector2(0, -14);
        mhRt.sizeDelta = new Vector2(-28, 28);

        TMP_Text mhTxt = mHdrObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) mhTxt.font = fontRegular;
        mhTxt.fontSize = 17;
        mhTxt.fontStyle = FontStyles.Normal;
        mhTxt.color = new Color(0f, 0.85f, 1f, 1f);
        mhTxt.text = "[ STATISTIK EVALUASI SHIFT ]";

        // Performance Score
        GameObject pScoreObj = CreateUIObject("PerformanceText", leftPanel.transform);
        RectTransform psRt = pScoreObj.GetComponent<RectTransform>();
        psRt.anchorMin = new Vector2(0, 1);
        psRt.anchorMax = new Vector2(1, 1);
        psRt.pivot = new Vector2(0, 1);
        psRt.anchoredPosition = new Vector2(14, -50);
        psRt.sizeDelta = new Vector2(-28, 26);

        TMP_Text pScoreTxt = pScoreObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) pScoreTxt.font = fontRegular;
        pScoreTxt.fontSize = 16;
        pScoreTxt.fontStyle = FontStyles.Normal;
        pScoreTxt.color = Color.white;
        pScoreTxt.text = "PERFORMANCE INDEX : 85%";

        // Performance Fill Bar
        GameObject pBarBase = CreateUIObject("PerformanceBarBase", leftPanel.transform);
        RectTransform pbRt = pBarBase.GetComponent<RectTransform>();
        pbRt.anchorMin = new Vector2(0, 1);
        pbRt.anchorMax = new Vector2(1, 1);
        pbRt.pivot = new Vector2(0, 1);
        pbRt.anchoredPosition = new Vector2(14, -78);
        pbRt.sizeDelta = new Vector2(-28, 18);

        Image pbImg = pBarBase.AddComponent<Image>();
        pbImg.color = new Color(0.12f, 0.16f, 0.22f, 1f);

        GameObject pBarFill = CreateUIObject("Fill", pBarBase.transform);
        RectTransform pbfRt = pBarFill.GetComponent<RectTransform>();
        pbfRt.anchorMin = Vector2.zero;
        pbfRt.anchorMax = Vector2.one;
        pbfRt.sizeDelta = Vector2.zero;

        Image pbfImg = pBarFill.AddComponent<Image>();
        pbfImg.color = new Color(0f, 0.9f, 0.45f, 1f);
        pbfImg.type = Image.Type.Filled;
        pbfImg.fillMethod = Image.FillMethod.Horizontal;
        pbfImg.fillAmount = 0.85f;

        // Humanity Score
        GameObject hScoreObj = CreateUIObject("HumanityText", leftPanel.transform);
        RectTransform hsRt = hScoreObj.GetComponent<RectTransform>();
        hsRt.anchorMin = new Vector2(0, 1);
        hsRt.anchorMax = new Vector2(1, 1);
        hsRt.pivot = new Vector2(0, 1);
        hsRt.anchoredPosition = new Vector2(14, -108);
        hsRt.sizeDelta = new Vector2(-28, 26);

        TMP_Text hScoreTxt = hScoreObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) hScoreTxt.font = fontRegular;
        hScoreTxt.fontSize = 16;
        hScoreTxt.fontStyle = FontStyles.Normal;
        hScoreTxt.color = Color.white;
        hScoreTxt.text = "HUMANITY INDEX    : 70%";

        // Humanity Fill Bar
        GameObject hBarBase = CreateUIObject("HumanityBarBase", leftPanel.transform);
        RectTransform hbRt = hBarBase.GetComponent<RectTransform>();
        hbRt.anchorMin = new Vector2(0, 1);
        hbRt.anchorMax = new Vector2(1, 1);
        hbRt.pivot = new Vector2(0, 1);
        hbRt.anchoredPosition = new Vector2(14, -136);
        hbRt.sizeDelta = new Vector2(-28, 18);

        Image hbImg = hBarBase.AddComponent<Image>();
        hbImg.color = new Color(0.12f, 0.16f, 0.22f, 1f);

        GameObject hBarFill = CreateUIObject("Fill", hBarBase.transform);
        RectTransform hbfRt = hBarFill.GetComponent<RectTransform>();
        hbfRt.anchorMin = Vector2.zero;
        hbfRt.anchorMax = Vector2.one;
        hbfRt.sizeDelta = Vector2.zero;

        Image hbfImg = hBarFill.AddComponent<Image>();
        hbfImg.color = new Color(0f, 0.8f, 1f, 1f);
        hbfImg.type = Image.Type.Filled;
        hbfImg.fillMethod = Image.FillMethod.Horizontal;
        hbfImg.fillAmount = 0.7f;

        // Accurate Decisions Text
        GameObject corrObj = CreateUIObject("CorrectText", leftPanel.transform);
        RectTransform cRt = corrObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0, 1);
        cRt.anchorMax = new Vector2(1, 1);
        cRt.pivot = new Vector2(0, 1);
        cRt.anchoredPosition = new Vector2(14, -172);
        cRt.sizeDelta = new Vector2(-28, 26);

        TMP_Text corrTxt = corrObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) corrTxt.font = fontRegular;
        corrTxt.fontSize = 16;
        corrTxt.fontStyle = FontStyles.Normal;
        corrTxt.color = new Color(0.7f, 1f, 0.8f, 1f);
        corrTxt.text = "ACCURATE INSPECTIONS : 12";

        // Protocol Violations Text
        GameObject wrgObj = CreateUIObject("WrongText", leftPanel.transform);
        RectTransform wRt = wrgObj.GetComponent<RectTransform>();
        wRt.anchorMin = new Vector2(0, 1);
        wRt.anchorMax = new Vector2(1, 1);
        wRt.pivot = new Vector2(0, 1);
        wRt.anchoredPosition = new Vector2(14, -204);
        wRt.sizeDelta = new Vector2(-28, 26);

        TMP_Text wrgTxt = wrgObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) wrgTxt.font = fontRegular;
        wrgTxt.fontSize = 16;
        wrgTxt.fontStyle = FontStyles.Normal;
        wrgTxt.color = new Color(1f, 0.4f, 0.4f, 1f);
        wrgTxt.text = "PROTOCOL VIOLATIONS  : 2";

        // Served Passengers Text
        GameObject srvObj = CreateUIObject("ServedText", leftPanel.transform);
        RectTransform sRt = srvObj.GetComponent<RectTransform>();
        srvRtMinMax(sRt, 14, -236, -28, 26);

        TMP_Text srvTxt = srvObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) srvTxt.font = fontRegular;
        srvTxt.fontSize = 16;
        srvTxt.fontStyle = FontStyles.Normal;
        srvTxt.color = new Color(0.85f, 0.9f, 1f, 1f);
        srvTxt.text = "PASSENGERS PROCESSED : 14";

        // --- 2. RIGHT PANEL: FAILURE & INCIDENT VIOLATION LOGS ---
        GameObject rightPanel = CreateUIObject("RightFailureLogsPanel", contentSplitObj.transform);
        RectTransform rpRt = rightPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = new Vector2(0.52f, 0);
        rpRt.anchorMax = new Vector2(1, 1);
        rpRt.sizeDelta = Vector2.zero;

        Image rpBg = rightPanel.AddComponent<Image>();
        rpBg.color = new Color(0.04f, 0.05f, 0.08f, 0.9f);

        // Failures Header
        GameObject fHdrObj = CreateUIObject("FailureHeader", rightPanel.transform);
        RectTransform fhRt = fHdrObj.GetComponent<RectTransform>();
        fhRt.anchorMin = new Vector2(0, 1);
        fhRt.anchorMax = new Vector2(1, 1);
        fhRt.pivot = new Vector2(0.5f, 1);
        fhRt.anchoredPosition = new Vector2(0, -14);
        fhRt.sizeDelta = new Vector2(-28, 28);

        TMP_Text fhTxt = fHdrObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) fhTxt.font = fontRegular;
        fhTxt.fontSize = 17;
        fhTxt.fontStyle = FontStyles.Normal;
        fhTxt.color = new Color(1f, 0.45f, 0.45f, 1f);
        fhTxt.text = "[ LOG PELANGGARAN & CATATAN KEGAGALAN HARI INI ]";

        // Failure Multi-line Content Box
        GameObject fContentObj = CreateUIObject("FailureContentText", rightPanel.transform);
        RectTransform fcRt = fContentObj.GetComponent<RectTransform>();
        fcRt.anchorMin = new Vector2(0, 0);
        fcRt.anchorMax = new Vector2(1, 1);
        fcRt.pivot = new Vector2(0.5f, 0.5f);
        fcRt.anchoredPosition = new Vector2(0, -18);
        fcRt.sizeDelta = new Vector2(-28, -56);

        TMP_Text fcTxt = fContentObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) fcTxt.font = fontRegular;
        fcTxt.fontSize = 15;
        fcTxt.fontStyle = FontStyles.Normal;
        fcTxt.lineSpacing = 3.5f;
        fcTxt.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        fcTxt.text = "• [00:42] Meloloskan Penumpang Berdokumen Palsu: Alex\n• [02:30] KEGAGALAN SISTEM: Terlambat Mengunci Anomali CCTV di CAM 02!";

        // =========================================================================
        // E. SUPERVISOR MEMO PANEL (Bawah)
        // =========================================================================
        GameObject memoBox = CreateUIObject("SupervisorMemoBox", frameObj.transform);
        RectTransform mbRt = memoBox.GetComponent<RectTransform>();
        mbRt.anchorMin = new Vector2(0, 0.07f);
        mbRt.anchorMax = new Vector2(1, 0.18f);
        mbRt.pivot = new Vector2(0.5f, 0.5f);
        mbRt.anchoredPosition = Vector2.zero;
        mbRt.sizeDelta = new Vector2(-40, 0);

        Image mbBg = memoBox.AddComponent<Image>();
        mbBg.color = new Color(0.025f, 0.04f, 0.06f, 0.9f);

        GameObject memoTxtObj = CreateUIObject("SupervisorMemoText", memoBox.transform);
        RectTransform mtRt = memoTxtObj.GetComponent<RectTransform>();
        mtRt.anchorMin = Vector2.zero;
        mtRt.anchorMax = Vector2.one;
        mtRt.sizeDelta = new Vector2(-28, -8);

        TMP_Text mtTxt = memoTxtObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) mtTxt.font = fontRegular;
        mtTxt.fontSize = 15;
        mtTxt.fontStyle = FontStyles.Normal;
        mtTxt.alignment = TextAlignmentOptions.Center;
        mtTxt.color = new Color(0.75f, 0.88f, 0.98f, 0.95f);
        mtTxt.text = "\"Pengawas stasiun mencatat kinerja shift Anda. Tetap waspada di malam berikutnya.\"";

        // =========================================================================
        // F. FOOTER CONTINUE PROMPT (Paling Bawah)
        // =========================================================================
        GameObject footerObj = CreateUIObject("FooterContinueText", frameObj.transform);
        RectTransform ftRt = footerObj.GetComponent<RectTransform>();
        ftRt.anchorMin = new Vector2(0, 0);
        ftRt.anchorMax = new Vector2(1, 0);
        ftRt.pivot = new Vector2(0.5f, 0);
        ftRt.anchoredPosition = new Vector2(0, 8);
        ftRt.sizeDelta = new Vector2(-40, 30);

        TMP_Text ftTxt = footerObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) ftTxt.font = fontRegular;
        ftTxt.fontSize = 18;
        ftTxt.fontStyle = FontStyles.Normal;
        ftTxt.alignment = TextAlignmentOptions.Center;
        ftTxt.color = new Color(0f, 1f, 0.85f, 1f);
        ftTxt.text = ">> TEKAN [ENTER / SPASI] UNTUK MEMULAI SHIFT BERIKUTNYA <<";

        // =========================================================================
        // G. HUBUNGKAN SERIALIZED FIELDS DI SummaryUIController
        // =========================================================================
        SerializedObject ctrlSO = new SerializedObject(summaryCtrl);
        ctrlSO.FindProperty("root").objectReferenceValue = summaryPanelTr.gameObject;
        ctrlSO.FindProperty("shiftReportTitleText").objectReferenceValue = sysTxt;
        ctrlSO.FindProperty("dayText").objectReferenceValue = dayTxt;
        ctrlSO.FindProperty("stationInfoText").objectReferenceValue = infoTxt;
        ctrlSO.FindProperty("evaluationBadgeContainer").objectReferenceValue = evalBadgeObj;
        ctrlSO.FindProperty("evaluationBadgeText").objectReferenceValue = evalTxt;
        ctrlSO.FindProperty("evaluationBadgeBg").objectReferenceValue = ebBg;
        ctrlSO.FindProperty("supervisorMemoText").objectReferenceValue = mtTxt;
        ctrlSO.FindProperty("performanceText").objectReferenceValue = pScoreTxt;
        ctrlSO.FindProperty("performanceFillBar").objectReferenceValue = pbfImg;
        ctrlSO.FindProperty("humanityText").objectReferenceValue = hScoreTxt;
        ctrlSO.FindProperty("humanityFillBar").objectReferenceValue = hbfImg;
        ctrlSO.FindProperty("correctText").objectReferenceValue = corrTxt;
        ctrlSO.FindProperty("wrongText").objectReferenceValue = wrgTxt;
        ctrlSO.FindProperty("servedText").objectReferenceValue = srvTxt;
        ctrlSO.FindProperty("failureLogsContainer").objectReferenceValue = rightPanel;
        ctrlSO.FindProperty("failureLogsTitleText").objectReferenceValue = fhTxt;
        ctrlSO.FindProperty("failureLogsContentText").objectReferenceValue = fcTxt;
        ctrlSO.FindProperty("continueText").objectReferenceValue = ftTxt;
        ctrlSO.FindProperty("reportOpenSfx").objectReferenceValue = reportOpenClip;
        ctrlSO.FindProperty("stampSfx").objectReferenceValue = stampClip;
        ctrlSO.ApplyModifiedProperties();

        // Pastikan SummaryPanel disembunyikan saat awal start scene
        summaryPanelTr.gameObject.SetActive(false);

        // Simpan Scene
        EditorUtility.SetDirty(summaryCtrl);
        EditorUtility.SetDirty(summaryPanelTr.gameObject);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        EditorUtility.DisplayDialog("Sukses!", "Summary Panel Bergaya 'Fears to Fathom' Diperbarui:\n\n- Font diubah menjadi HomeVideo-Regular murni (tanpa Bold).\n- Ukuran teks diperbesar agar sangat jelas & mudah dibaca.\n- Layout bar & padding lebih luas dan nyaman dipandang.", "Sempurna!");
        Debug.Log("<color=green>[SummaryUIBuilder]</color> Berhasil memperbarui ukuran teks dan font HomeVideo-Regular.");
    }

    private static void srvRtMinMax(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
#endif
