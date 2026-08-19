#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class SwipeTutorialBuilder
{
    [MenuItem("Tools/Last Shift/Bangun UI Panduan Swipe Visual (7 Langkah)")]
    public static void BuildSwipeTutorialUI()
    {
        // 1. Pastikan Sprite Import Settings untuk semua gambar tutorial
        string[] imgPaths = new string[]
        {
            "Assets/Art/UI/Tutorial/Step1_Serve.jpg",
            "Assets/Art/UI/Tutorial/Step2_Validate.png",
            "Assets/Art/UI/Tutorial/Step3_TakeCard.png",
            "Assets/Art/UI/Tutorial/Step4_DragToSlot.png"
        };

        foreach (string path in imgPaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
            }
        }

        Sprite spr1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Tutorial/Step1_Serve.jpg");
        Sprite spr2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Tutorial/Step2_Validate.png");
        Sprite spr3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Tutorial/Step3_TakeCard.png");
        Sprite spr4 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Tutorial/Step4_DragToSlot.png");

        // 2. Buka scene MainMenu jika belum terbuka
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "MainMenu")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        // 3. Cari Panel 'swipe' di Canvas
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan di scene MainMenu!");
            return;
        }

        Transform swipeTrans = canvas.transform.Find("swipe");
        if (swipeTrans == null)
        {
            // Cari rekursif
            var allTrans = canvas.GetComponentsInChildren<Transform>(true);
            foreach (var t in allTrans)
            {
                if (t.name == "swipe" && t != canvas.transform)
                {
                    swipeTrans = t;
                    break;
                }
            }
        }

        if (swipeTrans == null)
        {
            Debug.LogError("Panel 'swipe' tidak ditemukan di Canvas!");
            return;
        }

        GameObject swipeObj = swipeTrans.gameObject;
        Undo.RegisterCompleteObjectUndo(swipeObj, "Build Swipe Tutorial UI");

        // Reset Transform swipe panel
        RectTransform swipeRect = swipeObj.GetComponent<RectTransform>();
        if (swipeRect != null)
        {
            swipeRect.localScale = Vector3.one;
            swipeRect.anchorMin = new Vector2(0.5f, 0.5f);
            swipeRect.anchorMax = new Vector2(0.5f, 0.5f);
            swipeRect.pivot = new Vector2(0.5f, 0.5f);
            swipeRect.anchoredPosition = Vector2.zero;
            swipeRect.sizeDelta = new Vector2(960, 620);
        }

        // Bersihkan child lama yang tidak terpakai
        for (int i = swipeObj.transform.childCount - 1; i >= 0; i--)
        {
            GameObject.DestroyImmediate(swipeObj.transform.GetChild(i).gameObject);
        }

        // Background Image Panel
        Image bgImg = swipeObj.GetComponent<Image>();
        if (bgImg == null) bgImg = swipeObj.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);

        // Tambahkan komponen SwipeTutorialUI
        SwipeTutorialUI tutorialUI = swipeObj.GetComponent<SwipeTutorialUI>();
        if (tutorialUI == null) tutorialUI = swipeObj.AddComponent<SwipeTutorialUI>();

        // Ambil Font TMP default yang ada di project
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (defaultFont == null)
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            if (fonts.Length > 0) defaultFont = fonts[0];
        }

        // ========================================================
        // 1. HEADER (Step Number & Step Title)
        // ========================================================
        GameObject headerObj = CreateUIObject("Header", swipeObj.transform);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.pivot = new Vector2(0.5f, 1);
        headerRect.anchoredPosition = new Vector2(0, -20);
        headerRect.sizeDelta = new Vector2(-40, 70);

        // Step Number (LANGKAH 1 / 7)
        GameObject stepNumObj = CreateUIObject("StepNumberText", headerObj.transform);
        RectTransform stepNumRect = stepNumObj.GetComponent<RectTransform>();
        stepNumRect.anchorMin = new Vector2(0, 1);
        stepNumRect.anchorMax = new Vector2(1, 1);
        stepNumRect.anchoredPosition = new Vector2(0, 0);
        stepNumRect.sizeDelta = new Vector2(0, 24);
        TMP_Text stepNumText = stepNumObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) stepNumText.font = defaultFont;
        stepNumText.fontSize = 18;
        stepNumText.fontStyle = FontStyles.Bold;
        stepNumText.alignment = TextAlignmentOptions.Center;
        stepNumText.color = new Color(0f, 1f, 0.8f, 1f); // Neon Cyan
        stepNumText.text = "LANGKAH 1 / 7";

        // Step Title
        GameObject stepTitleObj = CreateUIObject("StepTitleText", headerObj.transform);
        RectTransform stepTitleRect = stepTitleObj.GetComponent<RectTransform>();
        stepTitleRect.anchorMin = new Vector2(0, 0);
        stepTitleRect.anchorMax = new Vector2(1, 1);
        stepTitleRect.anchoredPosition = new Vector2(0, -15);
        stepTitleRect.sizeDelta = new Vector2(0, -24);
        TMP_Text stepTitleText = stepTitleObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) stepTitleText.font = defaultFont;
        stepTitleText.fontSize = 24;
        stepTitleText.fontStyle = FontStyles.Bold;
        stepTitleText.alignment = TextAlignmentOptions.Center;
        stepTitleText.color = Color.white;
        stepTitleText.text = "1. SERVE PASSENGER (LAYANI PENUMPANG)";

        // ========================================================
        // 2. IMAGE CONTAINER
        // ========================================================
        GameObject imgContainer = CreateUIObject("ImageContainer", swipeObj.transform);
        RectTransform imgContRect = imgContainer.GetComponent<RectTransform>();
        imgContRect.anchorMin = new Vector2(0.5f, 0.5f);
        imgContRect.anchorMax = new Vector2(0.5f, 0.5f);
        imgContRect.pivot = new Vector2(0.5f, 0.5f);
        imgContRect.anchoredPosition = new Vector2(0, 30);
        imgContRect.sizeDelta = new Vector2(580, 290);

        // Border Frame
        Image frameImg = imgContainer.AddComponent<Image>();
        frameImg.color = new Color(0.15f, 0.22f, 0.3f, 0.8f);

        // Image Display
        GameObject displayObj = CreateUIObject("ImageDisplay", imgContainer.transform);
        RectTransform dispRect = displayObj.GetComponent<RectTransform>();
        dispRect.anchorMin = new Vector2(0, 0);
        dispRect.anchorMax = new Vector2(1, 1);
        dispRect.sizeDelta = new Vector2(-8, -8); // 4px padding
        Image stepImg = displayObj.AddComponent<Image>();
        stepImg.preserveAspect = true;
        stepImg.sprite = spr1;

        // ========================================================
        // 3. DESCRIPTION TEXT
        // ========================================================
        GameObject descObj = CreateUIObject("DescriptionText", swipeObj.transform);
        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 0);
        descRect.anchorMax = new Vector2(0.5f, 0);
        descRect.pivot = new Vector2(0.5f, 0);
        descRect.anchoredPosition = new Vector2(0, 95);
        descRect.sizeDelta = new Vector2(860, 65);
        TMP_Text descText = descObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) descText.font = defaultFont;
        descText.fontSize = 19;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = new Color(0.9f, 0.95f, 1f, 1f);
        descText.text = "Dekati meja loket saat penumpang tiba, lalu tekan tombol <b>[E]</b> untuk membuka menu pelayanan.";

        // ========================================================
        // 4. FOOTER NAVIGATION (Prev, Dots, Next, Back)
        // ========================================================
        GameObject footerObj = CreateUIObject("Footer", swipeObj.transform);
        RectTransform footerRect = footerObj.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0, 0);
        footerRect.anchorMax = new Vector2(1, 0);
        footerRect.pivot = new Vector2(0.5f, 0);
        footerRect.anchoredPosition = new Vector2(0, 15);
        footerRect.sizeDelta = new Vector2(-40, 55);

        // Prev Button
        Button prevBtn = CreateButton("PrevButton", footerObj.transform, new Vector2(-280, 0), new Vector2(160, 42), "â—€ SEBELUMNYA", defaultFont);

        // Page Indicator Dots
        GameObject dotsObj = CreateUIObject("PageDots", footerObj.transform);
        RectTransform dotsRect = dotsObj.GetComponent<RectTransform>();
        dotsRect.anchorMin = new Vector2(0.5f, 0.5f);
        dotsRect.anchorMax = new Vector2(0.5f, 0.5f);
        dotsRect.anchoredPosition = new Vector2(0, 0);
        dotsRect.sizeDelta = new Vector2(250, 30);
        TMP_Text dotsText = dotsObj.AddComponent<TextMeshProUGUI>();
        if (defaultFont != null) dotsText.font = defaultFont;
        dotsText.fontSize = 22;
        dotsText.alignment = TextAlignmentOptions.Center;
        dotsText.color = Color.white;
        dotsText.text = "â— â—‹ â—‹ â—‹ â—‹ â—‹ â—‹";

        // Next Button
        Button nextBtn = CreateButton("NextButton", footerObj.transform, new Vector2(280, 0), new Vector2(160, 42), "SELANJUTNYA â–¶", defaultFont);

        // Back / Close Button (Pojok Kanan Atas)
        Button backBtn = CreateButton("BackButton", swipeObj.transform, new Vector2(435, 270), new Vector2(65, 40), "âœ• ESC", defaultFont);
        ColorBlock cb = backBtn.colors;
        cb.normalColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        backBtn.colors = cb;

        // ========================================================
        // 5. ISI 7 DATA LANGKAH LENGKAP
        // ========================================================
        tutorialUI.stepNumberText = stepNumText;
        tutorialUI.stepTitleText = stepTitleText;
        tutorialUI.stepDescriptionText = descText;
        tutorialUI.stepImageDisplay = stepImg;
        tutorialUI.pageIndicatorText = dotsText;
        tutorialUI.prevButton = prevBtn;
        tutorialUI.nextButton = nextBtn;
        tutorialUI.backButton = backBtn;

        tutorialUI.steps = new SwipeTutorialUI.TutorialStep[]
        {
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "1. SERVE PASSENGER (LAYANI PENUMPANG)",
                stepDescription = "Dekati meja loket saat penumpang tiba di loket, lalu tekan tombol <b>[E]</b> untuk membuka menu pelayanan tiket.",
                stepImage = spr1
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "2. PILIH VALIDATE TICKET",
                stepDescription = "Gunakan tombol panah atau <b>[W][S]</b>, lalu tekan <b>[ENTER]</b> untuk memilih opsi <b>> VALIDATE TICKET</b>.",
                stepImage = spr2
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "3. AMBIL KARTU TIKET DENGAN MOUSE",
                stepDescription = "Arahkan kursor mouse ke kartu penumpang di sebelah kanan meja, lalu <b>KLIK KIRI & TAHAN</b> untuk mengangkat kartu.",
                stepImage = spr3
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "4. GESER (DRAG) KARTU KE CELAH SCANNER",
                stepDescription = "Sambil tetap menahan klik kiri mouse, <b>tarik (drag)</b> kartu mendekati celah/lubang mesin scanner di sebelah kiri.",
                stepImage = spr4
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "5. KARTU OTOMATIS TERKUNCI (SNAP)",
                stepDescription = "Ketika kartu didekatkan ke ujung kiri celah scanner, lepaskan klik. Kartu akan <b>otomatis menempel (Snap)</b> pada posisi siap gesek.",
                stepImage = spr4
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "6. GESEK (SWIPE) KARTU KE ARAH KANAN",
                stepDescription = "Klik & tahan kembali pada kartu di celah, lalu <b>geser lurus ke arah KANAN</b> dengan kecepatan sedang dan stabil hingga ke ujung kanan.",
                stepImage = spr4
            },
            new SwipeTutorialUI.TutorialStep
            {
                stepTitle = "7. VERIFIKASI LOG & ULANGI JIKA PERLU",
                stepDescription = "Jika kartu ditolak karena salah gesek, atau ingin mencocokkan data di log komputer loket, pilih <b>CANCEL</b> untuk mengulang pemindaian.",
                stepImage = spr2
            }
        };

        tutorialUI.UpdateUI();

        // Tandai scene dirty dan simpan
        EditorUtility.SetDirty(swipeObj);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        EditorUtility.DisplayDialog("Sukses!", "Panel Swipe Tutorial Visual (7 Langkah Bergambar) berhasil dibangun dan dipasang ke Main Menu!\n\nSilakan klik Play di Main Menu untuk mencobanya.", "Keren!");
        Debug.Log("<color=green>[SwipeTutorialBuilder]</color> Berhasil membangun UI Panduan 7 Langkah!");
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
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
        img.color = new Color(0.18f, 0.28f, 0.38f, 0.9f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0f, 0.8f, 0.7f, 1f);
        cb.pressedColor = new Color(0f, 0.5f, 0.45f, 1f);
        cb.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        btn.colors = cb;

        GameObject txtObj = CreateUIObject("Text", btnObj.transform);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        TMP_Text t = txtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 16;
        t.fontStyle = FontStyles.Bold;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.text = text;

        return btn;
    }
}
#endif
