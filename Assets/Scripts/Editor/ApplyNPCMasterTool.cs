#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class ApplyNPCMasterTool : EditorWindow
{
    private RuntimeAnimatorController masterController;
    private DefaultAsset targetFolder;

    [MenuItem("Tools/Last Shift/Apply NPC_Master To Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<ApplyNPCMasterTool>("Apply NPC Master Tool");
    }

    private void OnEnable()
    {
        // Auto-find default base controller (NPC_Master)
        string[] guids = AssetDatabase.FindAssets("NPC_Master t:AnimatorController");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            masterController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        }

        // Auto-find target folder Assets/Art/Prefabs
        targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Art/Prefabs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Alat Pasang NPC_Master ke Semua Prefab NPC", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Tool ini akan memindai semua Prefab NPC di folder 'Assets/Art/Prefabs' (termasuk subfolder 'female', 'male', 'monster') dan secara otomatis memasang Animator Controller 'NPC_Master'.", MessageType.Info);
        GUILayout.Space(10);

        masterController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller:", masterController, typeof(RuntimeAnimatorController), false);
        targetFolder = (DefaultAsset)EditorGUILayout.ObjectField("Target Folder:", targetFolder, typeof(DefaultAsset), false);

        GUILayout.Space(15);

        if (GUILayout.Button("Pasang NPC_Master ke Semua Prefab!", GUILayout.Height(50)))
        {
            ApplyMasterToAllPrefabs();
        }
    }

    private void ApplyMasterToAllPrefabs()
    {
        if (masterController == null)
        {
            EditorUtility.DisplayDialog("Error", "Pilih dulu Animator Controller 'NPC_Master' sebelum menjalankan tool!", "OK");
            return;
        }

        string folderPath = targetFolder != null ? AssetDatabase.GetAssetPath(targetFolder) : "Assets/Art/Prefabs";

        // Cari semua file .prefab di target folder dan semua subfoldernya
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        if (prefabGuids == null || prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Info", $"Tidak ditemukan prefab di dalam folder {folderPath}", "OK");
            return;
        }

        int updatedCount = 0;

        try
        {
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                string fileName = Path.GetFileNameWithoutExtension(path).ToLower();

                // Jangan ubah cleaning staff karena memiliki controller tersendiri
                if (fileName.Contains("cleaningstaff"))
                    continue;

                EditorUtility.DisplayProgressBar("Menerapkan NPC_Master", $"Memproses {Path.GetFileName(path)} ({i + 1}/{prefabGuids.Length})", (float)i / prefabGuids.Length);

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
                if (prefabRoot == null) continue;

                bool modified = false;

                // 1. Ambil atau pasang komponen Animator
                Animator anim = prefabRoot.GetComponent<Animator>();
                if (anim == null)
                {
                    anim = prefabRoot.AddComponent<Animator>();
                    modified = true;
                }

                if (anim.runtimeAnimatorController != masterController)
                {
                    anim.runtimeAnimatorController = masterController;
                    modified = true;
                }

                // 2. Pastikan NPCController (jika ada) mereferensikan Animator yang tepat
                NPCController npcCtrl = prefabRoot.GetComponent<NPCController>();
                if (npcCtrl != null)
                {
                    SerializedObject so = new SerializedObject(npcCtrl);
                    SerializedProperty animProp = so.FindProperty("animator");
                    if (animProp != null && animProp.objectReferenceValue != anim)
                    {
                        animProp.objectReferenceValue = anim;
                        so.ApplyModifiedProperties();
                        modified = true;
                    }
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                    updatedCount++;
                }

                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        EditorUtility.DisplayDialog("Sukses!", $"Berhasil menerapkan '{masterController.name}' ke {updatedCount} prefab NPC!", "Mantap");
        Debug.Log($"<color=green>[NPC Tool]</color> Berhasil menerapkan {masterController.name} ke {updatedCount} prefab.");
    }
}
#endif
