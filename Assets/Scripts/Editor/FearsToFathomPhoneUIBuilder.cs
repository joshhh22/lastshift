#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class FearsToFathomPhoneUIBuilder
{
    [MenuItem("Tools/Last Shift/Bangun Fears to Fathom Phone UI (Sistem Chat Interaktif)")]
    public static void BuildPhoneUI()
    {
        // 1. Pastikan folder Art UI Phone ada
        string folder = "Assets/Art/UI/Phone";
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        // 2. Generate Rounded Sprites
        Sprite cardSpr = GenerateRoundedSprite(folder + "/Window_Card.png", 128, 128, 20, Color.white);
        Sprite incomingBubbleSpr = GenerateRoundedSprite(folder + "/Bubble_Incoming.png", 128, 128, 22, new Color(0.91f, 0.91f, 0.93f, 1f)); // #E9E9EB
        Sprite outgoingBubbleSpr = GenerateRoundedSprite(folder + "/Bubble_Outgoing.png", 128, 128, 22, new Color(0f, 0.48f, 1f, 1f));     // #007AFF
        Sprite inputPillSpr = GenerateRoundedSprite(folder + "/Input_Pill.png", 128, 64, 20, new Color(0.95f, 0.95f, 0.97f, 1f));      // #F2F2F7
        Sprite dotSpr = GenerateCircleSprite(folder + "/Dot_Circle.png", 64, Color.white);
        Sprite toastSpr = GenerateRoundedSprite(folder + "/Toast_Pill.png", 128, 64, 24, new Color(0.12f, 0.14f, 0.18f, 0.95f));

        // 3. Buka scene Gameplay
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Gameplay")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        }

        // 4. Cari GameObject PhoneUI & PhoneManager
        PhoneManager phoneMgr = Object.FindObjectOfType<PhoneManager>(true);
        if (phoneMgr == null)
        {
            Debug.LogError("PhoneManager tidak ditemukan di scene!");
            return;
        }

        GameObject phoneUIObj = GameObject.Find("PhoneUI");
        if (phoneUIObj == null)
        {
            phoneUIObj = new GameObject("PhoneUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        }

        Undo.RegisterCompleteObjectUndo(phoneUIObj, "Build Fears to Fathom Phone UI");

        // Ambil Font HomeVideo-Regular SDF
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

        // Canvas Setup
        Canvas canvas = phoneUIObj.GetComponent<Canvas>();
        if (canvas == null) canvas = phoneUIObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = phoneUIObj.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = phoneUIObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = phoneUIObj.GetComponent<GraphicRaycaster>();
        if (raycaster == null) phoneUIObj.AddComponent<GraphicRaycaster>();

        // Bersihkan child lama di dalam PhoneUI
        for (int i = phoneUIObj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(phoneUIObj.transform.GetChild(i).gameObject);
        }

        // Hapus script PhonePage lama jika ada
        PhonePage oldPage = phoneUIObj.GetComponent<PhonePage>();
        if (oldPage != null) GameObject.DestroyImmediate(oldPage);

        // Tambahkan PhoneChatController baru
        PhoneChatController chatCtrl = phoneUIObj.GetComponent<PhoneChatController>();
        if (chatCtrl == null) chatCtrl = phoneUIObj.AddComponent<PhoneChatController>();

        // =========================================================================
        // A. BACKGROUND DIMMER (Semi-transparan saat buka ponsel)
        // =========================================================================
        GameObject dimmer = CreateUIObject("Phone_Dimmer", phoneUIObj.transform);
        RectTransform dRt = dimmer.GetComponent<RectTransform>();
        dRt.anchorMin = Vector2.zero;
        dRt.anchorMax = Vector2.one;
        dRt.sizeDelta = Vector2.zero;

        Image dImg = dimmer.AddComponent<Image>();
        dImg.color = new Color(0f, 0f, 0f, 0.45f);

        Button dimmerBtn = dimmer.AddComponent<Button>();
        dimmerBtn.onClick.AddListener(() => {
            if (PhoneChatController.Instance != null)
                PhoneChatController.Instance.OnPlayerClickedReply();
        });

        // =========================================================================
        // B. MESSAGES WINDOW (macOS / Fears to Fathom Style Window Card)
        // =========================================================================
        GameObject winObj = CreateUIObject("Phone_CardWindow", phoneUIObj.transform);
        RectTransform wRt = winObj.GetComponent<RectTransform>();
        wRt.anchorMin = new Vector2(0.5f, 0.5f);
        wRt.anchorMax = new Vector2(0.5f, 0.5f);
        wRt.pivot = new Vector2(0.5f, 0.5f);
        wRt.anchoredPosition = new Vector2(0, 10);
        wRt.sizeDelta = new Vector2(460, 640);

        Image wImg = winObj.AddComponent<Image>();
        wImg.sprite = cardSpr;
        wImg.type = Image.Type.Sliced;
        wImg.color = new Color(0.98f, 0.98f, 0.99f, 1f);

        // 1. Window Header
        GameObject header = CreateUIObject("Header", winObj.transform);
        RectTransform hRt = header.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0, 1);
        hRt.anchorMax = new Vector2(1, 1);
        hRt.pivot = new Vector2(0.5f, 1);
        hRt.anchoredPosition = Vector2.zero;
        hRt.sizeDelta = new Vector2(0, 48);

        Image hImg = header.AddComponent<Image>();
        hImg.color = new Color(0.94f, 0.94f, 0.96f, 1f);

        // Header Separator Line
        GameObject sep = CreateUIObject("Separator", header.transform);
        RectTransform sepRt = sep.GetComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0, 0);
        sepRt.anchorMax = new Vector2(1, 0);
        sepRt.anchoredPosition = Vector2.zero;
        sepRt.sizeDelta = new Vector2(0, 1);
        Image sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(0.88f, 0.88f, 0.9f, 1f);

        // Window Control Dots (🔴 🟡 🟢)
        GameObject dotsContainer = CreateUIObject("WindowDots", header.transform);
        RectTransform dotsRt = dotsContainer.GetComponent<RectTransform>();
        dotsRt.anchorMin = new Vector2(0, 0.5f);
        dotsRt.anchorMax = new Vector2(0, 0.5f);
        dotsRt.pivot = new Vector2(0, 0.5f);
        dotsRt.anchoredPosition = new Vector2(16, 0);
        dotsRt.sizeDelta = new Vector2(50, 12);

        CreateDot("Dot_Red", dotsContainer.transform, new Vector2(0, 0), new Color(1f, 0.37f, 0.34f, 1f), dotSpr);
        CreateDot("Dot_Yellow", dotsContainer.transform, new Vector2(16, 0), new Color(1f, 0.74f, 0.18f, 1f), dotSpr);
        CreateDot("Dot_Green", dotsContainer.transform, new Vector2(32, 0), new Color(0.15f, 0.79f, 0.25f, 1f), dotSpr);

        // Header Title (Messages • Contact Name)
        GameObject titleObj = CreateUIObject("TitleText", header.transform);
        RectTransform tRt = titleObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.anchoredPosition = Vector2.zero;
        tRt.sizeDelta = new Vector2(-120, 0);

        TMP_Text titleTxt = titleObj.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTxt.font = font;
        titleTxt.fontSize = 14;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        titleTxt.text = "Messages • Ibu";

        // Close Button (X)
        GameObject closeBtnObj = CreateUIObject("CloseButton", header.transform);
        RectTransform cRt = closeBtnObj.GetComponent<RectTransform>();
        cRt.anchorMin = new Vector2(1, 0.5f);
        cRt.anchorMax = new Vector2(1, 0.5f);
        cRt.pivot = new Vector2(1, 0.5f);
        cRt.anchoredPosition = new Vector2(-14, 0);
        cRt.sizeDelta = new Vector2(24, 24);

        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(() => { if (PhoneManager.Instance != null) PhoneManager.Instance.ClosePhone(); });

        GameObject cTxtObj = CreateUIObject("Text", closeBtnObj.transform);
        RectTransform ctRt = cTxtObj.GetComponent<RectTransform>();
        ctRt.anchorMin = Vector2.zero;
        ctRt.anchorMax = Vector2.one;
        ctRt.sizeDelta = Vector2.zero;
        TMP_Text cTxt = cTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) cTxt.font = font;
        cTxt.fontSize = 14;
        cTxt.fontStyle = FontStyles.Bold;
        cTxt.alignment = TextAlignmentOptions.Center;
        cTxt.color = new Color(0.4f, 0.4f, 0.45f, 1f);
        cTxt.text = "X";

        // 2. Chat Scroll View
        GameObject scrollObj = CreateUIObject("ChatScrollView", winObj.transform);
        RectTransform sRt = scrollObj.GetComponent<RectTransform>();
        sRt.anchorMin = Vector2.zero;
        sRt.anchorMax = Vector2.one;
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = new Vector2(0, 8);
        sRt.sizeDelta = new Vector2(-28, -135);

        ScrollRect sRect = scrollObj.AddComponent<ScrollRect>();
        sRect.horizontal = false;
        sRect.vertical = true;
        sRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject vpObj = CreateUIObject("Viewport", scrollObj.transform);
        RectTransform vpRt = vpObj.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;

        Image vpImg = vpObj.AddComponent<Image>();
        vpImg.color = Color.white;
        Mask vpMask = vpObj.AddComponent<Mask>();
        vpMask.showMaskGraphic = false;

        // Content
        GameObject contentObj = CreateUIObject("Content", vpObj.transform);
        RectTransform contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup vlg = contentObj.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10;
        vlg.padding = new RectOffset(10, 10, 12, 12);
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sRect.content = contentRt;
        sRect.viewport = vpRt;

        // 3. Bubble Templates
        GameObject inTemplate = CreateIncomingBubbleTemplate("Template_Incoming", contentObj.transform, incomingBubbleSpr, font);
        GameObject outTemplate = CreateOutgoingBubbleTemplate("Template_Outgoing", contentObj.transform, outgoingBubbleSpr, font);
        GameObject typingTemplate = CreateTypingIndicatorTemplate("Template_Typing", contentObj.transform, incomingBubbleSpr, font);

        // 4. Bottom Input Bar (Clickable Button for Replying)
        GameObject inputBar = CreateUIObject("BottomInputBar", winObj.transform);
        RectTransform ibRt = inputBar.GetComponent<RectTransform>();
        ibRt.anchorMin = new Vector2(0, 0);
        ibRt.anchorMax = new Vector2(1, 0);
        ibRt.pivot = new Vector2(0.5f, 0);
        ibRt.anchoredPosition = new Vector2(0, 36);
        ibRt.sizeDelta = new Vector2(-28, 42);

        Image ibImg = inputBar.AddComponent<Image>();
        ibImg.sprite = inputPillSpr;
        ibImg.type = Image.Type.Sliced;
        ibImg.color = new Color(0.93f, 0.93f, 0.95f, 1f);

        Button inputBtn = inputBar.AddComponent<Button>();

        GameObject ibTxtObj = CreateUIObject("PlaceholderText", inputBar.transform);
        RectTransform ibtRt = ibTxtObj.GetComponent<RectTransform>();
        ibtRt.anchorMin = Vector2.zero;
        ibtRt.anchorMax = Vector2.one;
        ibtRt.anchoredPosition = new Vector2(16, 0);
        ibtRt.sizeDelta = new Vector2(-64, 0);

        TMP_Text ibTxt = ibTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) ibTxt.font = font;
        ibTxt.fontSize = 11.5f;
        ibTxt.color = new Color(0.5f, 0.5f, 0.55f, 1f);
        ibTxt.text = "Message...";

        // Send Icon Arrow (Blue Circle)
        GameObject sendIcon = CreateUIObject("SendIcon", inputBar.transform);
        RectTransform siRt = sendIcon.GetComponent<RectTransform>();
        siRt.anchorMin = new Vector2(1, 0.5f);
        siRt.anchorMax = new Vector2(1, 0.5f);
        siRt.pivot = new Vector2(1, 0.5f);
        siRt.anchoredPosition = new Vector2(-6, 0);
        siRt.sizeDelta = new Vector2(28, 28);

        Image siImg = sendIcon.AddComponent<Image>();
        siImg.sprite = dotSpr;
        siImg.color = new Color(0f, 0.48f, 1f, 1f);

        GameObject siArr = CreateUIObject("Arrow", sendIcon.transform);
        RectTransform saRt = siArr.GetComponent<RectTransform>();
        saRt.anchorMin = Vector2.zero;
        saRt.anchorMax = Vector2.one;
        saRt.sizeDelta = Vector2.zero;
        TMP_Text saTxt = siArr.AddComponent<TextMeshProUGUI>();
        if (font != null) saTxt.font = font;
        saTxt.fontSize = 13;
        saTxt.fontStyle = FontStyles.Bold;
        saTxt.alignment = TextAlignmentOptions.Center;
        saTxt.color = Color.white;
        saTxt.text = "^";

        // 5. Bottom Hint Bar
        GameObject hintObj = CreateUIObject("BottomHint", winObj.transform);
        RectTransform hntRt = hintObj.GetComponent<RectTransform>();
        hntRt.anchorMin = new Vector2(0, 0);
        hntRt.anchorMax = new Vector2(1, 0);
        hntRt.pivot = new Vector2(0.5f, 0);
        hntRt.anchoredPosition = new Vector2(0, 8);
        hntRt.sizeDelta = new Vector2(-20, 22);

        TMP_Text hntTxt = hintObj.AddComponent<TextMeshProUGUI>();
        if (font != null) hntTxt.font = font;
        hntTxt.fontSize = 10.5f;
        hntTxt.fontStyle = FontStyles.Bold;
        hntTxt.alignment = TextAlignmentOptions.Center;
        hntTxt.enableWordWrapping = false;
        hntTxt.color = new Color(0.4f, 0.45f, 0.5f, 1f);
        hntTxt.text = "[!] KLIK LAYAR / ENTER UNTUK MEMBALAS  |  [TAB] TUTUP";

        // =========================================================================
        // C. FLOATING TOAST NOTIFICATION BANNER (SLIDE DOWN FROM TOP)
        // =========================================================================
        GameObject toastObj = CreateUIObject("Phone_ToastNotification", phoneUIObj.transform);
        RectTransform tstRt = toastObj.GetComponent<RectTransform>();
        tstRt.anchorMin = new Vector2(0.5f, 1);
        tstRt.anchorMax = new Vector2(0.5f, 1);
        tstRt.pivot = new Vector2(0.5f, 1);
        tstRt.anchoredPosition = new Vector2(0, 100);
        tstRt.sizeDelta = new Vector2(460, 54);

        Image tstImg = toastObj.AddComponent<Image>();
        tstImg.sprite = toastSpr;
        tstImg.type = Image.Type.Sliced;
        tstImg.color = new Color(0.08f, 0.12f, 0.18f, 0.96f);

        CanvasGroup toastCG = toastObj.AddComponent<CanvasGroup>();
        toastCG.alpha = 0f;

        PhoneToastNotification toastScript = toastObj.AddComponent<PhoneToastNotification>();

        GameObject tstTitle = CreateUIObject("Title", toastObj.transform);
        RectTransform ttRt = tstTitle.GetComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0, 0.5f);
        ttRt.anchorMax = new Vector2(1, 1);
        ttRt.anchoredPosition = new Vector2(18, -4);
        ttRt.sizeDelta = new Vector2(-36, 0);

        TMP_Text tt = tstTitle.AddComponent<TextMeshProUGUI>();
        if (font != null) tt.font = font;
        tt.fontSize = 13;
        tt.fontStyle = FontStyles.Bold;
        tt.alignment = TextAlignmentOptions.Left;
        tt.color = new Color(0f, 1f, 0.9f, 1f);
        tt.text = "[!] Pesan Baru dari Ibu";

        GameObject tstSub = CreateUIObject("Sub", toastObj.transform);
        RectTransform tsRt = tstSub.GetComponent<RectTransform>();
        tsRt.anchorMin = new Vector2(0, 0);
        tsRt.anchorMax = new Vector2(1, 0.5f);
        tsRt.anchoredPosition = new Vector2(18, 4);
        tsRt.sizeDelta = new Vector2(-36, 0);

        TMP_Text ts = tstSub.AddComponent<TextMeshProUGUI>();
        if (font != null) ts.font = font;
        ts.fontSize = 11;
        ts.alignment = TextAlignmentOptions.Left;
        ts.color = new Color(0.8f, 0.85f, 0.9f, 1f);
        ts.text = "Tekan [TAB] untuk membaca pesan";

        // Hubungkan Serialized Fields pada PhoneToastNotification
        SerializedObject toastSO = new SerializedObject(toastScript);
        toastSO.FindProperty("canvasGroup").objectReferenceValue = toastCG;
        toastSO.FindProperty("container").objectReferenceValue = tstRt;
        toastSO.FindProperty("titleText").objectReferenceValue = tt;
        toastSO.FindProperty("hintText").objectReferenceValue = ts;
        toastSO.ApplyModifiedProperties();

        // Hubungkan Serialized Fields pada PhoneChatController
        SerializedObject chatSO = new SerializedObject(chatCtrl);
        chatSO.FindProperty("contactTitleText").objectReferenceValue = titleTxt;
        chatSO.FindProperty("scrollRect").objectReferenceValue = sRect;
        chatSO.FindProperty("chatContainer").objectReferenceValue = contentRt;
        chatSO.FindProperty("incomingBubbleTemplate").objectReferenceValue = inTemplate;
        chatSO.FindProperty("outgoingBubbleTemplate").objectReferenceValue = outTemplate;
        chatSO.FindProperty("typingIndicatorTemplate").objectReferenceValue = typingTemplate;
        chatSO.FindProperty("inputBarButton").objectReferenceValue = inputBtn;
        chatSO.FindProperty("inputPlaceholderText").objectReferenceValue = ibTxt;
        chatSO.FindProperty("bottomHintText").objectReferenceValue = hntTxt;
        chatSO.ApplyModifiedProperties();

        // Pastikan PhoneManager menunjuk ke PhoneUI
        SerializedObject pmSO = new SerializedObject(phoneMgr);
        pmSO.FindProperty("phoneUI").objectReferenceValue = phoneUIObj;
        pmSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(phoneMgr);

        // Matikan PhoneUI di awal
        phoneUIObj.SetActive(false);

        // Simpan Scene
        EditorUtility.SetDirty(phoneUIObj);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        EditorUtility.DisplayDialog("Sukses!", "UI Ponsel Fears to Fathom berhasil diperbarui dengan layout simetris, presisi rapi, dan proporsional!", "Mantap!");
        Debug.Log("<color=green>[FearsToFathomPhoneUIBuilder]</color> Berhasil membangun UI Ponsel Fears to Fathom simetris.");
    }

    private static GameObject CreateIncomingBubbleTemplate(string name, Transform parent, Sprite bubbleSpr, TMP_FontAsset font)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rRt = row.GetComponent<RectTransform>();
        rRt.sizeDelta = new Vector2(0, 36);

        LayoutElement rLe = row.AddComponent<LayoutElement>();
        rLe.flexibleWidth = 1f;

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject bubble = CreateUIObject("Bubble", row.transform);
        Image bImg = bubble.AddComponent<Image>();
        bImg.sprite = bubbleSpr;
        bImg.type = Image.Type.Sliced;
        bImg.color = new Color(0.91f, 0.91f, 0.93f, 1f);

        HorizontalLayoutGroup bHlg = bubble.AddComponent<HorizontalLayoutGroup>();
        bHlg.padding = new RectOffset(16, 16, 10, 10);
        bHlg.childControlWidth = true;
        bHlg.childControlHeight = true;
        bHlg.childForceExpandWidth = false;
        bHlg.childForceExpandHeight = false;

        ContentSizeFitter csf = bubble.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject txtObj = CreateUIObject("Text", bubble.transform);
        TMP_Text txt = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) txt.font = font;
        txt.fontSize = 12.5f;
        txt.lineSpacing = 5;
        txt.enableWordWrapping = true;
        txt.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        txt.text = "Incoming message text here...";

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredWidth = 260;

        return row;
    }

    private static GameObject CreateOutgoingBubbleTemplate(string name, Transform parent, Sprite bubbleSpr, TMP_FontAsset font)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rRt = row.GetComponent<RectTransform>();
        rRt.sizeDelta = new Vector2(0, 36);

        LayoutElement rLe = row.AddComponent<LayoutElement>();
        rLe.flexibleWidth = 1f;

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject bubble = CreateUIObject("Bubble", row.transform);
        Image bImg = bubble.AddComponent<Image>();
        bImg.sprite = bubbleSpr;
        bImg.type = Image.Type.Sliced;
        bImg.color = new Color(0f, 0.48f, 1f, 1f);

        HorizontalLayoutGroup bHlg = bubble.AddComponent<HorizontalLayoutGroup>();
        bHlg.padding = new RectOffset(16, 16, 10, 10);
        bHlg.childControlWidth = true;
        bHlg.childControlHeight = true;
        bHlg.childForceExpandWidth = false;
        bHlg.childForceExpandHeight = false;

        ContentSizeFitter csf = bubble.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject txtObj = CreateUIObject("Text", bubble.transform);
        TMP_Text txt = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) txt.font = font;
        txt.fontSize = 12.5f;
        txt.lineSpacing = 5;
        txt.enableWordWrapping = true;
        txt.color = Color.white;
        txt.text = "Player outgoing reply text here...";

        LayoutElement le = txtObj.AddComponent<LayoutElement>();
        le.preferredWidth = 260;

        return row;
    }

    private static GameObject CreateTypingIndicatorTemplate(string name, Transform parent, Sprite bubbleSpr, TMP_FontAsset font)
    {
        GameObject row = CreateUIObject(name, parent);
        RectTransform rRt = row.GetComponent<RectTransform>();
        rRt.sizeDelta = new Vector2(0, 32);

        LayoutElement rLe = row.AddComponent<LayoutElement>();
        rLe.flexibleWidth = 1f;

        HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;

        GameObject bubble = CreateUIObject("Bubble", row.transform);
        Image bImg = bubble.AddComponent<Image>();
        bImg.sprite = bubbleSpr;
        bImg.type = Image.Type.Sliced;
        bImg.color = new Color(0.91f, 0.91f, 0.93f, 1f);

        HorizontalLayoutGroup bHlg = bubble.AddComponent<HorizontalLayoutGroup>();
        bHlg.padding = new RectOffset(16, 16, 8, 8);
        bHlg.childControlWidth = true;
        bHlg.childControlHeight = true;
        bHlg.childForceExpandWidth = false;
        bHlg.childForceExpandHeight = false;

        ContentSizeFitter csf = bubble.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject txtObj = CreateUIObject("Text", bubble.transform);
        TMP_Text txt = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) txt.font = font;
        txt.fontSize = 14;
        txt.color = new Color(0.55f, 0.55f, 0.6f, 1f);
        txt.text = "*  *  *";

        return row;
    }

    private static void CreateDot(string name, Transform parent, Vector2 pos, Color color, Sprite circleSpr)
    {
        GameObject dot = CreateUIObject(name, parent);
        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(10, 10);

        Image img = dot.AddComponent<Image>();
        img.sprite = circleSpr;
        img.color = color;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Sprite GenerateRoundedSprite(string savePath, int width, int height, int radius, Color fillColor)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int dx = Mathf.Min(x, width - 1 - x);
                int dy = Mathf.Min(y, height - 1 - y);

                if (dx < radius && dy < radius)
                {
                    float dist = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius, radius));
                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, transparent);
                        continue;
                    }
                    else if (dist > radius - 1)
                    {
                        float alpha = 1f - (dist - (radius - 1));
                        tex.SetPixel(x, y, new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha));
                        continue;
                    }
                }
                tex.SetPixel(x, y, fillColor);
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(radius, radius, radius, radius);
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(savePath);
    }

    private static Sprite GenerateCircleSprite(string savePath, int size, Color fillColor)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color transparent = new Color(0, 0, 0, 0);
        float radius = size / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist > radius)
                {
                    tex.SetPixel(x, y, transparent);
                }
                else if (dist > radius - 1f)
                {
                    float alpha = 1f - (dist - (radius - 1f));
                    tex.SetPixel(x, y, new Color(fillColor.r, fillColor.g, fillColor.b, fillColor.a * alpha));
                }
                else
                {
                    tex.SetPixel(x, y, fillColor);
                }
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        AssetDatabase.ImportAsset(savePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(savePath);
    }
}
#endif
