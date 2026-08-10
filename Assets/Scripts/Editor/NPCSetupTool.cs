#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class NPCSetupTool : EditorWindow
{
    [MenuItem("Tools/Last Shift/Bikin NPC Cepat (Setup Tool)")]
    public static void ShowWindow()
    {
        GetWindow<NPCSetupTool>("NPC Setup Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Alat Pasang Komponen NPC Otomatis", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Pilih/Glow (Highlight) satu atau banyak Prefab 3D model NPC-mu di jendela Project (bawah), lalu klik tombol di bawah ini. Tool ini otomatis menambahkan NavMeshAgent, Animator, NPCIdentity, dan NPCController.", MessageType.Info);
        
        GUILayout.Space(10);

        if (GUILayout.Button("Pasang Komponen ke Semua Prefab yang Dipilih!", GUILayout.Height(50)))
        {
            SetupSelectedNPCs();
        }
    }

    private void SetupSelectedNPCs()
    {
        int count = 0;

        // Ambil semua objek yang sedang di-highlight/diselect
        foreach (GameObject obj in Selection.gameObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            
            // Bypass jika yang dipilih bukan prefab di folder Project
            if (string.IsNullOrEmpty(assetPath)) continue;

            // Buka akses ke isi prefab
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);

            // 1. Tambah NavMeshAgent
            if (prefabRoot.GetComponent<NavMeshAgent>() == null)
            {
                NavMeshAgent agent = prefabRoot.AddComponent<NavMeshAgent>();
                agent.radius = 0.3f;
                agent.speed = 2.5f; // setting default kecepatan jalan NPC
                agent.stoppingDistance = 0.5f;
            }

            // 2. Tambah Animator
            if (prefabRoot.GetComponent<Animator>() == null)
            {
                prefabRoot.AddComponent<Animator>();
            }

            // 3. Tambah atau Update NPCIdentity (Buat nentuin Gender otomatis dari nama Folder)
            NPCIdentity identity = prefabRoot.GetComponent<NPCIdentity>();
            if (identity == null)
            {
                identity = prefabRoot.AddComponent<NPCIdentity>();
            }

            // Setup otomatis Gender berdasarkan nama foldernya
            SerializedObject identitySO = new SerializedObject(identity);
            SerializedProperty genderProp = identitySO.FindProperty("gender");
            if (genderProp != null)
            {
                string pathLower = assetPath.ToLower();
                
                // Kalau Prefab ditaruh di dalam folder bernama "female" atau "wanita"
                if (pathLower.Contains("female") || pathLower.Contains("wanita"))
                {
                    genderProp.enumValueIndex = (int)NPCGender.Female;
                }
                // Kalau di folder "male" atau "pria"
                else if (pathLower.Contains("male") || pathLower.Contains("pria"))
                {
                    genderProp.enumValueIndex = (int)NPCGender.Male;
                }
                // (Selain itu biarkan default atau male)
                
                identitySO.ApplyModifiedProperties();
            }

            // 4. Tambah atau Update NPCController
            NPCController controller = prefabRoot.GetComponent<NPCController>();
            if (controller == null)
            {
                controller = prefabRoot.AddComponent<NPCController>();
            }

            // [FIX] Perbaiki referensi Agent dan Animator yang nyasar ke prefab lain
            SerializedObject controllerSO = new SerializedObject(controller);
            SerializedProperty agentProp = controllerSO.FindProperty("agent");
            SerializedProperty animProp = controllerSO.FindProperty("animator");

            if (agentProp != null)
                agentProp.objectReferenceValue = prefabRoot.GetComponent<NavMeshAgent>();

            if (animProp != null)
                animProp.objectReferenceValue = prefabRoot.GetComponent<Animator>();
            
            controllerSO.ApplyModifiedProperties();

            // Simpan hasil editan secara otomatis
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            count++;
        }

        if (count > 0)
        {
            EditorUtility.DisplayDialog("MANTAP!", $"Berhasil menyulap {count} 3D Model biasa menjadi NPC Last Shift Penuh Komponen!", "Lanjutkan");
        }
        else
        {
            EditorUtility.DisplayDialog("Gagal", "Kamu belum memilih (Klik) Prefab di folder Project. Pilih dulu model 3D-nya ya!", "Paham");
        }
    }
}
#endif
