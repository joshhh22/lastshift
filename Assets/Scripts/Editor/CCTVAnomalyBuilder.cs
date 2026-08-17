#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class CCTVAnomalyBuilder
{
    [MenuItem("Tools/Last Shift/Setup CCTV Anomaly System (Monster 1 & 2 + QTE + Jumpscare)")]
    public static void SetupCCTVAnomalySystem()
    {
        // 1. Pastikan Scene Gameplay aktif
        var currentScene = EditorSceneManager.GetActiveScene();
        if (currentScene.name != "Gameplay")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        }

        // 2. Ubah Rig FBX Animasi menjadi Humanoid dan Aktifkan Loop Time
        EnsureHumanoidRigAndLoop("Assets/Art/npc/withoutskin/monster/Running Crawl.fbx", true);
        EnsureHumanoidRigAndLoop("Assets/Art/npc/withoutskin/monster/Zombie Crawl.fbx", true);
        EnsureHumanoidRigAndLoop("Assets/Art/npc/withoutskin/monster/Dying.fbx", false);
        EnsureHumanoidRigAndLoop("Assets/Art/npc/withoutskin/monster/Zombie Scream.fbx", true);

        // 3. Setup Animator Controllers untuk Monster 1 & Monster 2
        AnimatorController animCtrl1 = GetOrCreateMonster1Controller();
        AnimatorController animCtrl2 = GetOrCreateMonster2Controller();

        // 4. Pasang Animator & Controller ke Prefabs Monster serta reset offset internal
        GameObject m1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/monster/Character_Monster_05 1.prefab");
        if (m1Prefab != null)
        {
            Animator anim = m1Prefab.GetComponentInChildren<Animator>();
            if (anim != null && animCtrl1 != null)
            {
                anim.runtimeAnimatorController = animCtrl1;
                anim.applyRootMotion = false;
            }
            EditorUtility.SetDirty(m1Prefab);
        }

        GameObject m2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/monster/Character_Monster_04.prefab");
        if (m2Prefab != null)
        {
            Animator anim = m2Prefab.GetComponentInChildren<Animator>();
            if (anim != null && animCtrl2 != null)
            {
                anim.runtimeAnimatorController = animCtrl2;
                anim.applyRootMotion = false;
            }
            EditorUtility.SetDirty(m2Prefab);
        }

        // 5. Ambil Asset Audio & Font
        AudioClip alarmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/alarmcctv.mp3");
        AudioClip jumpscareClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Prefabs/monster/granny_bed_jumpscare_sound_effectmp3converter.mp3");
        AudioClip accessDeniedClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Access Denied.mp3");
        AudioClip beepClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/Beep(ClockIN,Out,AccessGranted).mp3");
        AudioClip gateClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Audio/gate.wav");

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");
        if (font == null)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Bold SDF.asset");
        }

        // 6. Cari GameObject ComputerSystem (Termasuk yang inactive)
        GameObject compSys = null;
        ComputerUIController compUI = Object.FindObjectOfType<ComputerUIController>(true);
        if (compUI != null)
        {
            compSys = compUI.gameObject;
        }
        else
        {
            var roots = currentScene.GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name == "ComputerSystem")
                {
                    compSys = r;
                    break;
                }
            }
        }

        if (compSys == null)
        {
            Debug.LogError("[CCTVAnomalyBuilder] ComputerSystem tidak ditemukan di scene!");
            return;
        }

        Transform windowCCTV = null;
        foreach (Transform t in compSys.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "Window_CCTV")
            {
                windowCCTV = t;
                break;
            }
        }

        if (windowCCTV == null)
        {
            Debug.LogError("[CCTVAnomalyBuilder] Window_CCTV tidak ditemukan di ComputerSystem!");
            return;
        }

        Transform contentArea = windowCCTV.Find("ContentArea");
        if (contentArea == null) contentArea = windowCCTV;

        // Bersihkan Anomaly UI lama jika ada
        Transform oldUI = contentArea.Find("CCTV_Anomaly_Overlay");
        if (oldUI != null) GameObject.DestroyImmediate(oldUI.gameObject);

        // =========================================================================
        // A. BUILD CCTV ANOMALY UI OVERLAY
        // =========================================================================
        GameObject overlayObj = CreateUIObject("CCTV_Anomaly_Overlay", contentArea);
        RectTransform oRt = overlayObj.GetComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.sizeDelta = Vector2.zero;

        CCTVAnomalyUIController uiCtrl = overlayObj.AddComponent<CCTVAnomalyUIController>();

        // 1. Warning Banner (Atas)
        GameObject warnBanner = CreateUIObject("WarningBanner", overlayObj.transform);
        RectTransform wbRt = warnBanner.GetComponent<RectTransform>();
        wbRt.anchorMin = new Vector2(0, 1);
        wbRt.anchorMax = new Vector2(1, 1);
        wbRt.pivot = new Vector2(0.5f, 1);
        wbRt.anchoredPosition = new Vector2(0, -52);
        wbRt.sizeDelta = new Vector2(-40, 36);

        Image wbImg = warnBanner.AddComponent<Image>();
        wbImg.color = new Color(0.8f, 0.1f, 0.1f, 0.9f);

        GameObject wbTxtObj = CreateUIObject("Text", warnBanner.transform);
        RectTransform wbtRt = wbTxtObj.GetComponent<RectTransform>();
        wbtRt.anchorMin = Vector2.zero;
        wbtRt.anchorMax = Vector2.one;
        wbtRt.sizeDelta = Vector2.zero;
        TMP_Text wbTxt = wbTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) wbTxt.font = font;
        wbTxt.fontSize = 13;
        wbTxt.fontStyle = FontStyles.Bold;
        wbTxt.alignment = TextAlignmentOptions.Center;
        wbTxt.color = Color.white;
        wbTxt.text = "⚠️ PERINGATAN: ANOMALI DI CAM 01!";

        // 2. Emergency Lockdown Button (Bawah tengah)
        GameObject lockBtnObj = CreateUIObject("EmergencyLockdownBtn", overlayObj.transform);
        RectTransform lbRt = lockBtnObj.GetComponent<RectTransform>();
        lbRt.anchorMin = new Vector2(0.5f, 0);
        lbRt.anchorMax = new Vector2(0.5f, 0);
        lbRt.pivot = new Vector2(0.5f, 0);
        lbRt.anchoredPosition = new Vector2(0, 56);
        lbRt.sizeDelta = new Vector2(280, 42);

        Image lbImg = lockBtnObj.AddComponent<Image>();
        lbImg.color = new Color(0.9f, 0.15f, 0.15f, 1f);

        Button lbBtn = lockBtnObj.AddComponent<Button>();

        GameObject lbTxtObj = CreateUIObject("Text", lockBtnObj.transform);
        RectTransform lbtRt = lbTxtObj.GetComponent<RectTransform>();
        lbtRt.anchorMin = Vector2.zero;
        lbtRt.anchorMax = Vector2.one;
        lbtRt.sizeDelta = Vector2.zero;
        TMP_Text lbTxt = lbTxtObj.AddComponent<TextMeshProUGUI>();
        if (font != null) lbTxt.font = font;
        lbTxt.fontSize = 13;
        lbTxt.fontStyle = FontStyles.Bold;
        lbTxt.alignment = TextAlignmentOptions.Center;
        lbTxt.color = Color.white;
        lbTxt.text = "🚨 [ TUTUP GERBANG DARURAT ]";

        // 3. QTE Container (Minigame Bar diletakkan rapi di BAWAH agar tidak menutupi tengah layar CCTV)
        GameObject qteObj = CreateUIObject("QTEContainer", overlayObj.transform);
        RectTransform qRt = qteObj.GetComponent<RectTransform>();
        qRt.anchorMin = new Vector2(0.5f, 0);
        qRt.anchorMax = new Vector2(0.5f, 0);
        qRt.pivot = new Vector2(0.5f, 0);
        qRt.anchoredPosition = new Vector2(0, 52);
        qRt.sizeDelta = new Vector2(360, 58);

        Image qBg = qteObj.AddComponent<Image>();
        qBg.color = new Color(0.04f, 0.07f, 0.12f, 0.85f);

        // QTE Status Text
        GameObject qsObj = CreateUIObject("StatusText", qteObj.transform);
        RectTransform qsRt = qsObj.GetComponent<RectTransform>();
        qsRt.anchorMin = new Vector2(0, 1);
        qsRt.anchorMax = new Vector2(1, 1);
        qsRt.pivot = new Vector2(0.5f, 1);
        qsRt.anchoredPosition = new Vector2(0, -4);
        qsRt.sizeDelta = new Vector2(0, 20);

        TMP_Text qsTxt = qsObj.AddComponent<TextMeshProUGUI>();
        if (font != null) qsTxt.font = font;
        qsTxt.fontSize = 11;
        qsTxt.fontStyle = FontStyles.Bold;
        qsTxt.alignment = TextAlignmentOptions.Center;
        qsTxt.color = Color.white;
        qsTxt.text = "TEKAN <b>[SPASI]</b> SAAT JARUM DI ZONA HIJAU!";

        // QTE Bar Base (Black bar)
        GameObject barBase = CreateUIObject("BarBase", qteObj.transform);
        RectTransform bbRt = barBase.GetComponent<RectTransform>();
        bbRt.anchorMin = new Vector2(0.5f, 0);
        bbRt.anchorMax = new Vector2(0.5f, 0);
        bbRt.pivot = new Vector2(0.5f, 0);
        bbRt.anchoredPosition = new Vector2(0, 12);
        bbRt.sizeDelta = new Vector2(280, 18);

        Image bbImg = barBase.AddComponent<Image>();
        bbImg.color = new Color(0.12f, 0.14f, 0.18f, 1f);

        // QTE Green Zone (Zona Hijau Lebih Sempit ~44px untuk Tantangan Presisi)
        GameObject greenZone = CreateUIObject("GreenZone", barBase.transform);
        RectTransform gzRt = greenZone.GetComponent<RectTransform>();
        gzRt.anchorMin = new Vector2(0.5f, 0.5f);
        gzRt.anchorMax = new Vector2(0.5f, 0.5f);
        gzRt.pivot = new Vector2(0.5f, 0.5f);
        gzRt.anchoredPosition = Vector2.zero;
        gzRt.sizeDelta = new Vector2(44, 18);

        Image gzImg = greenZone.AddComponent<Image>();
        gzImg.color = new Color(0f, 0.9f, 0.35f, 1f);

        // QTE Pointer (Moving Arrow below bar)
        GameObject pointer = CreateUIObject("Pointer", barBase.transform);
        RectTransform pRt = pointer.GetComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0);
        pRt.anchorMax = new Vector2(0.5f, 0);
        pRt.pivot = new Vector2(0.5f, 1);
        pRt.anchoredPosition = new Vector2(0, -2);
        pRt.sizeDelta = new Vector2(16, 16);

        TMP_Text pTxt = pointer.AddComponent<TextMeshProUGUI>();
        if (font != null) pTxt.font = font;
        pTxt.fontSize = 16;
        pTxt.fontStyle = FontStyles.Bold;
        pTxt.alignment = TextAlignmentOptions.Center;
        pTxt.color = new Color(1f, 0.35f, 0.1f, 1f);
        pTxt.text = "▲";

        // 4. Focus Meter Container (Monster 2)
        GameObject focusObj = CreateUIObject("FocusContainer", overlayObj.transform);
        RectTransform fRt = focusObj.GetComponent<RectTransform>();
        fRt.anchorMin = new Vector2(0.5f, 0);
        fRt.anchorMax = new Vector2(0.5f, 0);
        fRt.pivot = new Vector2(0.5f, 0);
        fRt.anchoredPosition = new Vector2(0, 56);
        fRt.sizeDelta = new Vector2(340, 50);

        Image fBg = focusObj.AddComponent<Image>();
        fBg.color = new Color(0.06f, 0.1f, 0.15f, 0.92f);

        GameObject fStatusObj = CreateUIObject("StatusText", focusObj.transform);
        RectTransform fsRt = fStatusObj.GetComponent<RectTransform>();
        fsRt.anchorMin = new Vector2(0, 1);
        fsRt.anchorMax = new Vector2(1, 1);
        fsRt.pivot = new Vector2(0.5f, 1);
        fsRt.anchoredPosition = new Vector2(0, -4);
        fsRt.sizeDelta = new Vector2(0, 20);

        TMP_Text fsTxt = fStatusObj.AddComponent<TextMeshProUGUI>();
        if (font != null) fsTxt.font = font;
        fsTxt.fontSize = 12;
        fsTxt.fontStyle = FontStyles.Bold;
        fsTxt.alignment = TextAlignmentOptions.Center;
        fsTxt.color = new Color(0f, 1f, 0.85f, 1f);
        fsTxt.text = "FOKUS PADA ANOMALI: 0%";

        // Progress Bar Base
        GameObject pbBase = CreateUIObject("ProgressBarBase", focusObj.transform);
        RectTransform pbbRt = pbBase.GetComponent<RectTransform>();
        pbbRt.anchorMin = new Vector2(0.5f, 0);
        pbbRt.anchorMax = new Vector2(0.5f, 0);
        pbbRt.pivot = new Vector2(0.5f, 0);
        pbbRt.anchoredPosition = new Vector2(0, 8);
        pbbRt.sizeDelta = new Vector2(300, 14);

        Image pbbImg = pbBase.AddComponent<Image>();
        pbbImg.color = new Color(0.2f, 0.25f, 0.3f, 1f);

        // Progress Bar Fill
        GameObject pbFill = CreateUIObject("Fill", pbBase.transform);
        RectTransform pbfRt = pbFill.GetComponent<RectTransform>();
        pbfRt.anchorMin = Vector2.zero;
        pbfRt.anchorMax = Vector2.one;
        pbfRt.sizeDelta = Vector2.zero;

        Image pbfImg = pbFill.AddComponent<Image>();
        pbfImg.color = new Color(0f, 0.9f, 0.45f, 1f);
        pbfImg.type = Image.Type.Filled;
        pbfImg.fillMethod = Image.FillMethod.Horizontal;
        pbfImg.fillAmount = 0f;

        // 5. Glitch Static Distortion Overlay (Menutupi seluruh viewport kamera)
        GameObject glitchObj = CreateUIObject("GlitchStaticOverlay", overlayObj.transform);
        RectTransform gRt = glitchObj.GetComponent<RectTransform>();
        gRt.anchorMin = Vector2.zero;
        gRt.anchorMax = Vector2.one;
        gRt.sizeDelta = Vector2.zero;
        Image gImg = glitchObj.AddComponent<Image>();
        gImg.color = new Color(1f, 0.1f, 0.1f, 0.25f);
        glitchObj.SetActive(false);

        // 6. Hold Lockdown Container (Muncul setelah Jumpscare Monster 2)
        GameObject holdObj = CreateUIObject("HoldLockdownContainer", overlayObj.transform);
        RectTransform hRt = holdObj.GetComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0.5f, 0);
        hRt.anchorMax = new Vector2(0.5f, 0);
        hRt.pivot = new Vector2(0.5f, 0);
        hRt.anchoredPosition = new Vector2(0, 56);
        hRt.sizeDelta = new Vector2(380, 60);

        Image hBg = holdObj.AddComponent<Image>();
        hBg.color = new Color(0.85f, 0.1f, 0.1f, 0.95f);

        GameObject hStatusObj = CreateUIObject("StatusText", holdObj.transform);
        RectTransform hsRt = hStatusObj.GetComponent<RectTransform>();
        hsRt.anchorMin = new Vector2(0, 1);
        hsRt.anchorMax = new Vector2(1, 1);
        hsRt.pivot = new Vector2(0.5f, 1);
        hsRt.anchoredPosition = new Vector2(0, -6);
        hsRt.sizeDelta = new Vector2(0, 22);

        TMP_Text hsTxt = hStatusObj.AddComponent<TextMeshProUGUI>();
        if (font != null) hsTxt.font = font;
        hsTxt.fontSize = 12;
        hsTxt.fontStyle = FontStyles.Bold;
        hsTxt.alignment = TextAlignmentOptions.Center;
        hsTxt.color = Color.white;
        hsTxt.text = "TAHAN <b>[SPASI / KLIK MOUSE]</b> UNTUK LOCKDOWN!";

        // Hold Progress Bar Base
        GameObject hpbBase = CreateUIObject("ProgressBarBase", holdObj.transform);
        RectTransform hpbbRt = hpbBase.GetComponent<RectTransform>();
        hpbbRt.anchorMin = new Vector2(0.5f, 0);
        hpbbRt.anchorMax = new Vector2(0.5f, 0);
        hpbbRt.pivot = new Vector2(0.5f, 0);
        hpbbRt.anchoredPosition = new Vector2(0, 10);
        hpbbRt.sizeDelta = new Vector2(340, 16);

        Image hpbbImg = hpbBase.AddComponent<Image>();
        hpbbImg.color = new Color(0.2f, 0.05f, 0.05f, 1f);

        // Hold Progress Bar Fill
        GameObject hpbFill = CreateUIObject("Fill", hpbBase.transform);
        RectTransform hpbfRt = hpbFill.GetComponent<RectTransform>();
        hpbfRt.anchorMin = Vector2.zero;
        hpbfRt.anchorMax = Vector2.one;
        hpbfRt.sizeDelta = Vector2.zero;

        Image hpbfImg = hpbFill.AddComponent<Image>();
        hpbfImg.color = new Color(0f, 1f, 0.4f, 1f);
        hpbfImg.type = Image.Type.Filled;
        hpbfImg.fillMethod = Image.FillMethod.Horizontal;
        hpbfImg.fillAmount = 0f;

        // Hubungkan Serialized Properties pada CCTVAnomalyUIController
        SerializedObject uiSO = new SerializedObject(uiCtrl);
        uiSO.FindProperty("warningBanner").objectReferenceValue = warnBanner;
        uiSO.FindProperty("warningBannerText").objectReferenceValue = wbTxt;
        uiSO.FindProperty("emergencyLockdownBtn").objectReferenceValue = lbBtn;
        uiSO.FindProperty("lockdownBtnText").objectReferenceValue = lbTxt;
        uiSO.FindProperty("qteContainer").objectReferenceValue = qteObj;
        uiSO.FindProperty("qteBarBase").objectReferenceValue = bbRt;
        uiSO.FindProperty("qteGreenZone").objectReferenceValue = gzRt;
        uiSO.FindProperty("qtePointer").objectReferenceValue = pRt;
        uiSO.FindProperty("qteStatusText").objectReferenceValue = qsTxt;
        uiSO.FindProperty("glitchStaticOverlay").objectReferenceValue = glitchObj;
        uiSO.FindProperty("focusContainer").objectReferenceValue = focusObj;
        uiSO.FindProperty("focusFillBar").objectReferenceValue = pbfImg;
        uiSO.FindProperty("focusStatusText").objectReferenceValue = fsTxt;
        uiSO.FindProperty("holdLockdownContainer").objectReferenceValue = holdObj;
        uiSO.FindProperty("holdLockdownFillBar").objectReferenceValue = hpbfImg;
        uiSO.FindProperty("holdLockdownStatusText").objectReferenceValue = hsTxt;
        uiSO.FindProperty("qteSuccessSfx").objectReferenceValue = beepClip;
        uiSO.FindProperty("qteFailSfx").objectReferenceValue = accessDeniedClip;
        uiSO.FindProperty("gateCloseSfx").objectReferenceValue = gateClip;
        uiSO.ApplyModifiedProperties();

        // =========================================================================
        // B. SETUP CCTV ANOMALY MANAGER & 6 CUSTOM SPAWN POINTS IN SCENE
        // =========================================================================
        CCTVAnomalyManager anomalyMgr = Object.FindObjectOfType<CCTVAnomalyManager>(true);
        if (anomalyMgr == null)
        {
            GameObject mgrObj = new GameObject("CCTVAnomalyManager");
            anomalyMgr = mgrObj.AddComponent<CCTVAnomalyManager>();
        }

        // Camera Mapping presisi:
        // CAM 01 (Index 0) = cctv1 (Lobby Counter)
        // CAM 02 (Index 1) = cctv3 (Stairs & Platform)
        // CAM 03 (Index 2) = cctv2 (Booth Perimeter)
        Camera[] cams = new Camera[3];
        foreach (Camera c in Object.FindObjectsOfType<Camera>(true))
        {
            if (c.gameObject.name == "cctv1") cams[0] = c;
            else if (c.gameObject.name == "cctv3") cams[1] = c;
            else if (c.gameObject.name == "cctv2") cams[2] = c;
        }

        // Setup Custom Spawn Points Root Parent
        GameObject spawnParent = GameObject.Find("CCTV_Anomaly_SpawnPoints");
        if (spawnParent == null)
        {
            spawnParent = new GameObject("CCTV_Anomaly_SpawnPoints");
        }

        string[] camLabels = new string[] { "CAM 01 (Lobby)", "CAM 02 (Stairs)", "CAM 03 (Booth)" };
        Transform[] m1Spawns = new Transform[3];
        Transform[] m2Spawns = new Transform[3];

        for (int i = 0; i < 3; i++)
        {
            // 1. Monster 1 Spawn Point (Merangkak di lantai lorong)
            string m1Name = $"SpawnPoint_Monster1_CAM{i + 1}";
            Transform sp1 = spawnParent.transform.Find(m1Name);
            if (sp1 == null)
            {
                GameObject sp1Obj = new GameObject(m1Name);
                sp1Obj.transform.SetParent(spawnParent.transform);
                sp1 = sp1Obj.transform;

                if (cams[i] != null)
                {
                    sp1.position = cams[i].transform.position + cams[i].transform.forward * 3.5f + Vector3.down * 1.0f;
                    sp1.LookAt(cams[i].transform.position);
                }
            }
            CCTVSpawnPointGizmo giz1 = sp1.GetComponent<CCTVSpawnPointGizmo>();
            if (giz1 == null) giz1 = sp1.gameObject.AddComponent<CCTVSpawnPointGizmo>();
            giz1.gizmoColor = Color.cyan;
            giz1.pointLabel = $"[M1 Crawl] {camLabels[i]}";
            m1Spawns[i] = sp1;

            // 2. Monster 2 Spawn Point (Dekat kamera jumpscare)
            string m2Name = $"SpawnPoint_Monster2_CAM{i + 1}";
            Transform sp2 = spawnParent.transform.Find(m2Name);
            if (sp2 == null)
            {
                GameObject sp2Obj = new GameObject(m2Name);
                sp2Obj.transform.SetParent(spawnParent.transform);
                sp2 = sp2Obj.transform;

                if (cams[i] != null)
                {
                    sp2.position = cams[i].transform.position + cams[i].transform.forward * 1.15f + Vector3.down * 1.35f;
                    sp2.LookAt(cams[i].transform.position);
                }
            }
            else
            {
                // Update posisi default jika belum digeser
                if (cams[i] != null && sp2.position.y > cams[i].transform.position.y - 0.5f)
                {
                    sp2.position = cams[i].transform.position + cams[i].transform.forward * 1.15f + Vector3.down * 1.35f;
                    sp2.LookAt(cams[i].transform.position);
                }
            }
            CCTVSpawnPointGizmo giz2 = sp2.GetComponent<CCTVSpawnPointGizmo>();
            if (giz2 == null) giz2 = sp2.gameObject.AddComponent<CCTVSpawnPointGizmo>();
            giz2.gizmoColor = Color.red;
            giz2.pointLabel = $"[M2 Jumpscare] {camLabels[i]}";
            m2Spawns[i] = sp2;
        }

        AudioSource aSource = anomalyMgr.GetComponent<AudioSource>();
        if (aSource == null) aSource = anomalyMgr.gameObject.AddComponent<AudioSource>();
        aSource.loop = true;
        aSource.playOnAwake = false;

        SerializedObject mgrSO = new SerializedObject(anomalyMgr);
        mgrSO.FindProperty("monster1Prefab").objectReferenceValue = m1Prefab;
        mgrSO.FindProperty("monster2Prefab").objectReferenceValue = m2Prefab;
        mgrSO.FindProperty("alarmAudioClip").objectReferenceValue = alarmClip;
        mgrSO.FindProperty("jumpscareAudioClip").objectReferenceValue = jumpscareClip;
        mgrSO.FindProperty("alarmAudioSource").objectReferenceValue = aSource;

        SerializedProperty camsProp = mgrSO.FindProperty("cctvCameras");
        camsProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            camsProp.GetArrayElementAtIndex(i).objectReferenceValue = cams[i];
        }

        SerializedProperty m1Prop = mgrSO.FindProperty("monster1SpawnPoints");
        m1Prop.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            m1Prop.GetArrayElementAtIndex(i).objectReferenceValue = m1Spawns[i];
        }

        SerializedProperty m2Prop = mgrSO.FindProperty("monster2SpawnPoints");
        m2Prop.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            m2Prop.GetArrayElementAtIndex(i).objectReferenceValue = m2Spawns[i];
        }

        mgrSO.ApplyModifiedProperties();

        // Simpan Scene
        EditorUtility.SetDirty(anomalyMgr);
        EditorSceneManager.MarkSceneDirty(currentScene);
        EditorSceneManager.SaveScene(currentScene);

        EditorUtility.DisplayDialog("Sukses!", "Sistem Anomali CCTV Berhasil Dipasang 100% Lengkap:\n\n1. Pemetaan Kamera Diperbaiki:\n   - CAM 01 = cctv1 (Lobby)\n   - CAM 02 = cctv3 (Stairs & Platform)\n   - CAM 03 = cctv2 (Booth Perimeter)\n\n2. Animasi Crawl & Scream sudah Humanoid + LOOP Time aktif.\n3. Monster 1 sekarang Roaming/merangkak maju.\n4. 6 Titik Spawn Visual (CCTV_Anomaly_SpawnPoints) siap digeser di Scene.", "Luar Biasa!");
        Debug.Log("<color=green>[CCTVAnomalyBuilder]</color> Berhasil memasang CCTV Anomaly System & Spawn Points.");
    }

    private static void EnsureHumanoidRigAndLoop(string fbxPath, bool enableLoop)
    {
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer != null)
        {
            bool needReimport = false;

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                needReimport = true;
            }

            var defaultClips = importer.defaultClipAnimations;
            if (defaultClips != null && defaultClips.Length > 0)
            {
                var clips = new ModelImporterClipAnimation[defaultClips.Length];
                for (int i = 0; i < defaultClips.Length; i++)
                {
                    clips[i] = new ModelImporterClipAnimation();
                    clips[i].name = defaultClips[i].name;
                    clips[i].takeName = defaultClips[i].takeName;
                    clips[i].firstFrame = defaultClips[i].firstFrame;
                    clips[i].lastFrame = defaultClips[i].lastFrame;
                    clips[i].loopTime = enableLoop;
                    clips[i].wrapMode = enableLoop ? WrapMode.Loop : WrapMode.Once;
                }
                importer.clipAnimations = clips;
                needReimport = true;
            }

            if (needReimport)
            {
                importer.SaveAndReimport();
                Debug.Log($"<color=cyan>[CCTVAnomalyBuilder]</color> {fbxPath} -> Humanoid & Loop: {enableLoop}");
            }
        }
    }

    private static AnimatorController GetOrCreateMonster1Controller()
    {
        string path = "Assets/Art/npc/withoutskin/monster/Monster1_AnimatorController.controller";
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (ctrl == null)
        {
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

            ctrl.AddParameter("StartCrawl", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Die", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("CrawlType", AnimatorControllerParameterType.Int);

            AnimationClip crawlClip = LoadAnimationClip("Assets/Art/npc/withoutskin/monster/Running Crawl.fbx");
            AnimationClip zombieClip = LoadAnimationClip("Assets/Art/npc/withoutskin/monster/Zombie Crawl.fbx");
            AnimationClip dyingClip = LoadAnimationClip("Assets/Art/npc/withoutskin/monster/Dying.fbx");

            var stateMachine = ctrl.layers[0].stateMachine;

            var runningState = stateMachine.AddState("RunningCrawl");
            runningState.motion = crawlClip;

            var zombieState = stateMachine.AddState("ZombieCrawl");
            zombieState.motion = zombieClip;

            var dyingState = stateMachine.AddState("Dying");
            dyingState.motion = dyingClip;

            stateMachine.defaultState = runningState;

            var toZombie = runningState.AddTransition(zombieState);
            toZombie.AddCondition(AnimatorConditionMode.Equals, 1, "CrawlType");

            var toDying1 = runningState.AddTransition(dyingState);
            toDying1.AddCondition(AnimatorConditionMode.If, 0, "Die");

            var toDying2 = zombieState.AddTransition(dyingState);
            toDying2.AddCondition(AnimatorConditionMode.If, 0, "Die");

            AssetDatabase.SaveAssets();
        }
        return ctrl;
    }

    private static AnimatorController GetOrCreateMonster2Controller()
    {
        string path = "Assets/Art/npc/withoutskin/monster/Monster2_AnimatorController.controller";
        AnimatorController ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (ctrl == null)
        {
            ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

            ctrl.AddParameter("IdleStare", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Scream", AnimatorControllerParameterType.Trigger);

            AnimationClip screamClip = LoadAnimationClip("Assets/Art/npc/withoutskin/monster/Zombie Scream.fbx");

            var stateMachine = ctrl.layers[0].stateMachine;

            var idleState = stateMachine.AddState("IdleStare");
            idleState.motion = screamClip;

            var screamState = stateMachine.AddState("ZombieScream");
            screamState.motion = screamClip;

            stateMachine.defaultState = idleState;

            var toScream = idleState.AddTransition(screamState);
            toScream.AddCondition(AnimatorConditionMode.If, 0, "Scream");

            AssetDatabase.SaveAssets();
        }
        return ctrl;
    }

    private static AnimationClip LoadAnimationClip(string fbxPath)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (Object obj in assets)
        {
            if (obj is AnimationClip clip && !clip.name.Contains("__preview__"))
            {
                return clip;
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
}
#endif
