#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class PlayChoiceUIBuilder
{
    [MenuItem("Tools/Last Shift/Build Play Choice UI in MainMenu Scene")]
    public static void BuildPlayChoiceUI()
    {
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "MainMenu")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        }

        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Canvas c = Object.FindObjectOfType<Canvas>(true);
            if (c != null) canvas = c.gameObject;
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas tidak ditemukan di MainMenu scene!");
            return;
        }

        MainMenuManager menuMgr = Object.FindObjectOfType<MainMenuManager>(true);
        if (menuMgr == null)
        {
            Debug.LogError("MainMenuManager tidak ditemukan!");
            return;
        }

        TMP_FontAsset fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (fontRegular == null)
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in fonts)
            {
                if (f.name.Contains("HomeVideo")) { fontRegular = f; break; }
            }
            if (fontRegular == null && fonts.Length > 0) fontRegular = fonts[0];
        }

        // Hapus panel lama jika ada
        Transform existing = canvas.transform.Find("PlayChoicePanel");
        if (existing != null)
        {
            Object.DestroyImmediate(existing.gameObject);
        }

        // =========================================================================
        // 1. CONTAINER MODAL
        // =========================================================================
        GameObject panelObj = new GameObject("PlayChoicePanel", typeof(RectTransform));
        panelObj.transform.SetParent(canvas.transform, false);
        panelObj.transform.SetAsLastSibling();

        RectTransform panelRt = panelObj.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(500, 360);
        panelRt.anchoredPosition = Vector2.zero;

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.04f, 0.06f, 0.09f, 0.96f);

        // Border Glow
        GameObject borderObj = new GameObject("BorderGlow", typeof(RectTransform));
        borderObj.transform.SetParent(panelObj.transform, false);
        RectTransform borderRt = borderObj.GetComponent<RectTransform>();
        borderRt.anchorMin = Vector2.zero;
        borderRt.anchorMax = Vector2.one;
        borderRt.sizeDelta = new Vector2(4, 4);
        borderRt.anchoredPosition = Vector2.zero;
        borderObj.transform.SetAsFirstSibling();
        Image borderImg = borderObj.AddComponent<Image>();
        borderImg.color = new Color(0f, 0.8f, 1f, 0.25f);
        borderImg.raycastTarget = false;

        // Header Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.sizeDelta = new Vector2(0, 50);
        titleRt.anchoredPosition = new Vector2(0, -18);

        TMP_Text titleText = titleObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) titleText.font = fontRegular;
        titleText.text = "SELECT MISSION";
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0f, 0.95f, 1f, 1f);
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;

        // Divider Line
        GameObject divObj = new GameObject("Divider", typeof(RectTransform));
        divObj.transform.SetParent(panelObj.transform, false);
        RectTransform divRt = divObj.GetComponent<RectTransform>();
        divRt.anchorMin = new Vector2(0.1f, 1f);
        divRt.anchorMax = new Vector2(0.9f, 1f);
        divRt.pivot = new Vector2(0.5f, 1f);
        divRt.sizeDelta = new Vector2(0, 2);
        divRt.anchoredPosition = new Vector2(0, -68);
        Image divImg = divObj.AddComponent<Image>();
        divImg.color = new Color(0f, 0.85f, 1f, 0.4f);
        divImg.raycastTarget = false;

        // Save Info Text
        GameObject infoObj = new GameObject("SaveInfoText", typeof(RectTransform));
        infoObj.transform.SetParent(panelObj.transform, false);
        RectTransform infoRt = infoObj.GetComponent<RectTransform>();
        infoRt.anchorMin = new Vector2(0.05f, 1f);
        infoRt.anchorMax = new Vector2(0.95f, 1f);
        infoRt.pivot = new Vector2(0.5f, 1f);
        infoRt.sizeDelta = new Vector2(0, 55);
        infoRt.anchoredPosition = new Vector2(0, -78);

        TMP_Text saveInfoText = infoObj.AddComponent<TextMeshProUGUI>();
        if (fontRegular != null) saveInfoText.font = fontRegular;
        saveInfoText.text = "LAST SAVED PROGRESS:\n<color=#00F0FF>DAY 1 • Shift Standby</color>";
        saveInfoText.fontSize = 14;
        saveInfoText.color = new Color(0.85f, 0.88f, 0.92f, 0.95f);
        saveInfoText.alignment = TextAlignmentOptions.Center;
        saveInfoText.raycastTarget = false;

        // =========================================================================
        // 2. BUTTONS
        // =========================================================================
        Button continueBtn = CreateButton(panelObj.transform, "ContinueButton", "[ > ] CONTINUE SHIFT", new Vector2(0, -150), new Color(0.08f, 0.30f, 0.45f, 1f), new Color(0f, 1f, 0.95f, 1f), fontRegular);
        Button newGameBtn = CreateButton(panelObj.transform, "NewGameButton", "[ * ] START NEW GAME", new Vector2(0, -210), new Color(0.16f, 0.16f, 0.20f, 1f), Color.white, fontRegular);
        Button backBtn = CreateButton(panelObj.transform, "BackButton", "KEMBALI (ESC)", new Vector2(0, -270), new Color(0.10f, 0.10f, 0.12f, 0.9f), new Color(0.7f, 0.7f, 0.7f, 1f), fontRegular);

        // =========================================================================
        // 3. SERIALIZE REFERENCES TO MAINMENUMANAGER
        // =========================================================================
        SerializedObject so = new SerializedObject(menuMgr);
        so.FindProperty("playChoicePanel").objectReferenceValue = panelObj;
        so.FindProperty("continueGameButton").objectReferenceValue = continueBtn;
        so.FindProperty("newGameButton").objectReferenceValue = newGameBtn;
        so.FindProperty("playChoiceBackButton").objectReferenceValue = backBtn;
        so.FindProperty("saveInfoText").objectReferenceValue = saveInfoText;
        so.ApplyModifiedProperties();

        panelObj.SetActive(false); // Sembunyikan secara default

        EditorUtility.SetDirty(menuMgr);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("<color=cyan>[PlayChoiceUIBuilder]</color> PlayChoicePanel berhasil dibuat dengan font HomeVideo dan pure ASCII!");
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Color bgColor, Color textColor, TMP_FontAsset font)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(400, 46);
        rt.anchoredPosition = anchoredPos;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = bgColor * 1.35f;
        colors.pressedColor = bgColor * 0.75f;
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
        tmp.fontSize = 16;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        return btn;
    }
}
#endif