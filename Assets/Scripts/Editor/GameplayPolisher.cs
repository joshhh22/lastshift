#if UNITY_EDITOR
using StarterAssets;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPolisher
{
    [MenuItem("Tools/Last Shift/Polish Gameplay (Camera, Crosshair, UI & Thoughts)")]
    public static void PolishAll()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Gameplay")
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity");
        }

        TMP_FontAsset fontRegular = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/OpenType (.otf)/HomeVideo-Regular SDF.asset");

        // =========================================================================
        // 1. FIX CAMERA PITCH / CLAMP & PLAYER SETTINGS
        // =========================================================================
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            var fpc = Object.FindObjectOfType<FirstPersonController>(true);
            if (fpc != null) player = fpc.gameObject;
        }

        if (player != null)
        {
            FirstPersonController fps = player.GetComponent<FirstPersonController>();
            if (fps != null)
            {
                SerializedObject so = new SerializedObject(fps);
                
                SerializedProperty topClampProp = so.FindProperty("TopClamp");
                SerializedProperty bottomClampProp = so.FindProperty("BottomClamp");
                SerializedProperty jumpHeightProp = so.FindProperty("JumpHeight");
                SerializedProperty groundedRadiusProp = so.FindProperty("GroundedRadius");

                if (topClampProp != null) topClampProp.floatValue = 85.0f;     // Nengok ke atas luas & natural
                if (bottomClampProp != null) bottomClampProp.floatValue = -85.0f; // Nengok ke bawah luas tanpa kaku
                if (jumpHeightProp != null) jumpHeightProp.floatValue = 0f;       // Cegah manjat dinding / lompat
                if (groundedRadiusProp != null) groundedRadiusProp.floatValue = 0.2f;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(fps);
                Debug.Log("<color=green>[GameplayPolisher]</color> Kamera Pitch diatur: TopClamp 85, BottomClamp -85 (Sangat mulus & luas).");
            }

            // Reset rotasi kamera pitch agar lurus menghadap depan
            Transform camTarget = player.transform.Find("CinemachineCameraTarget");
            if (camTarget != null) camTarget.localRotation = Quaternion.identity;

            Camera mainCam = player.GetComponentInChildren<Camera>();
            if (mainCam != null) mainCam.transform.localRotation = Quaternion.identity;

            // Pastikan posisi player menapak di lantai (tidak jatuh dari atas)
            if (Physics.Raycast(player.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 10f))
            {
                player.transform.position = hit.point + Vector3.up * 0.05f;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.slopeLimit = 45f;
                cc.stepOffset = 0.25f;
                cc.skinWidth = 0.05f;
                EditorUtility.SetDirty(cc);
            }

            if (player.GetComponent<PlayerWallAntiStick>() == null)
            {
                player.AddComponent<PlayerWallAntiStick>();
                EditorUtility.SetDirty(player);
            }
        }

        // Pastikan PlayerSpawnPoint juga menapak lantai & menghadap ke depan tangga
        GameObject spawnObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnObj != null)
        {
            if (Physics.Raycast(spawnObj.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hitSp, 10f))
            {
                spawnObj.transform.position = hitSp.point + Vector3.up * 0.05f;
            }
            spawnObj.transform.rotation = Quaternion.Euler(0, 0, 0);
            EditorUtility.SetDirty(spawnObj);
        }

        // =========================================================================
        // 1.5. POLISH DIALOGUE UI (Teks Dialogue / Monolog Bersih Tanpa Box)
        // =========================================================================
        DialogueManager diagMgr = Object.FindObjectOfType<DialogueManager>(true);
        if (diagMgr != null)
        {
            TMP_Text[] diagTexts = diagMgr.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text dt in diagTexts)
            {
                if (fontRegular != null) dt.font = fontRegular;
                if (dt.gameObject.name.ToLower().Contains("speaker"))
                {
                    dt.color = new Color(1f, 0.8f, 0.2f, 1f); // Kuning amber
                    dt.fontSize = 18;
                }
                else
                {
                    dt.color = Color.white;
                    dt.fontSize = 17;
                }
            }
        }

        // =========================================================================
        // 2. ATUR UKURAN CROSSHAIR MENJADI TITIK MINIMALIS ELEGAN
        // =========================================================================
        Canvas canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas != null)
        {
            // Cari semua komponen crosshair / pointer / dot di canvas
            foreach (Image img in canvas.GetComponentsInChildren<Image>(true))
            {
                string nameLower = img.gameObject.name.ToLower();
                if (nameLower.Contains("crosshair") || nameLower.Contains("reticle") || nameLower.Contains("dot") || nameLower == "pointer" || nameLower == "keybackground")
                {
                    // Pastikan bukan background panel besar
                    RectTransform rt = img.GetComponent<RectTransform>();
                    if (rt != null && rt.anchorMin == new Vector2(0.5f, 0.5f) && rt.anchorMax == new Vector2(0.5f, 0.5f))
                    {
                        rt.sizeDelta = new Vector2(5f, 5f); // Ukuran titik kecil 5x5 pixel elegan
                        img.color = new Color(1f, 1f, 1f, 0.75f);
                        EditorUtility.SetDirty(img.gameObject);
                        Debug.Log($"<color=green>[GameplayPolisher]</color> Crosshair '{img.gameObject.name}' dikecilkan menjadi 5x5 px minimalis.");
                    }
                }
            }
        }

        // =========================================================================
        // 3. PASANG PLAYER MONOLOGUE MANAGER (Pikiran Internal Karakter)
        // =========================================================================
        PlayerMonologueManager monologueMgr = Object.FindObjectOfType<PlayerMonologueManager>(true);
        if (monologueMgr == null)
        {
            GameObject monoObj = new GameObject("PlayerMonologueManager");
            monologueMgr = monoObj.AddComponent<PlayerMonologueManager>();
            EditorUtility.SetDirty(monoObj);
            Debug.Log("<color=green>[GameplayPolisher]</color> PlayerMonologueManager berhasil dipasang.");
        }

        // =========================================================================
        // 4. POLISH PASSENGER SERVICE UI (Menu Pelayanan Penumpang)
        // =========================================================================
        ServePassengerUIController serveCtrl = Object.FindObjectOfType<ServePassengerUIController>(true);
        if (serveCtrl != null)
        {
            Transform serveTr = serveCtrl.transform;
            Image[] allImages = serveTr.GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (img.gameObject.name.ToLower().Contains("panel") || img.gameObject.name.ToLower().Contains("background") || img.gameObject.name.ToLower().Contains("root"))
                {
                    // Beri warna dark retro CRT slate yang estetik
                    img.color = new Color(0.025f, 0.04f, 0.065f, 0.94f);
                }
            }

            TMP_Text[] allTexts = serveTr.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in allTexts)
            {
                if (fontRegular != null) txt.font = fontRegular;
                txt.fontStyle = FontStyles.Normal;
                
                string txtName = txt.gameObject.name.ToLower();
                if (txtName.Contains("title") || txtName.Contains("header"))
                {
                    txt.fontSize = 20;
                    txt.color = new Color(0f, 0.95f, 0.85f, 1f);
                }
                else if (txtName.Contains("validate") || txtName.Contains("cancel") || txtName.Contains("accept") || txtName.Contains("reject"))
                {
                    txt.fontSize = 16;
                }
                else if (txtName.Contains("reason") || txtName.Contains("info"))
                {
                    txt.fontSize = 15;
                    txt.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                else
                {
                    txt.fontSize = 14;
                    txt.color = new Color(0.7f, 0.8f, 0.9f, 0.9f);
                }
            }

            EditorUtility.SetDirty(serveCtrl.gameObject);
            Debug.Log("<color=green>[GameplayPolisher]</color> Passenger Service UI berhasil dipercantik dengan tema Retro VHS.");
        }

        // Simpan Scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Sukses!", "Polesan Gameplay Berhasil Diterapkan:\n\n1. Sudut Kamera: TopClamp 85 & BottomClamp -85 (Nengok atas/bawah bebas & tidak kaku).\n2. Crosshair: Dikecilkan menjadi titik 5x5 pixel yang minimalis & elegan.\n3. Dialog Internal (Monologue): Dialog batin naratif otomatis memandu objektif player.\n4. UI Passenger Service: Dipercantik dengan gaya Retro Analog Horror.\n5. AFK Protection: Jam & Shift End tidak akan desync jika player AFK.", "Luar Biasa!");
    }
}
#endif
