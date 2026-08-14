#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class NPCModelRigFixer
{
    [MenuItem("Tools/Last Shift/Fix NPC Models & Avatars (Perbaiki Tangan/Tulang Patah)")]
    public static void FixAllModelRigsAndAvatars()
    {
        string modelsFolder = "Assets/Art/npc/Models";
        string prefabsFolder = "Assets/Art/Prefabs";

        // 1. Perbaiki Rig semua file FBX Model menjadi "Create From This Model"
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { modelsFolder });
        int fixedModelsCount = 0;

        try
        {
            for (int i = 0; i < modelGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(modelGuids[i]);
                EditorUtility.DisplayProgressBar("Memperbaiki Rig Model", $"Memproses {Path.GetFileName(path)} ({i + 1}/{modelGuids.Length})", (float)i / modelGuids.Length);

                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null)
                {
                    if (importer.animationType != ModelImporterAnimationType.Human || importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                    {
                        importer.animationType = ModelImporterAnimationType.Human;
                        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                        importer.SaveAndReimport();
                        fixedModelsCount++;
                    }
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 2. Pasang Avatar yang sudah benar ke semua Prefab di Assets/Art/Prefabs
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabsFolder });
        int fixedPrefabsCount = 0;

        try
        {
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                string fileName = Path.GetFileNameWithoutExtension(prefabPath).ToLower();

                if (fileName.Contains("cleaningstaff"))
                    continue;

                EditorUtility.DisplayProgressBar("Memasang Avatar ke Prefab", $"Memproses {Path.GetFileName(prefabPath)} ({i + 1}/{prefabGuids.Length})", (float)i / prefabGuids.Length);

                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                if (prefabRoot == null) continue;

                bool modified = false;
                Animator anim = prefabRoot.GetComponent<Animator>();
                if (anim == null)
                {
                    anim = prefabRoot.AddComponent<Animator>();
                    modified = true;
                }

                // Cari Avatar dari model FBX sumbernya
                Avatar correctAvatar = null;
                
                // Cari avatar dari child objek atau dari asset model
                Avatar[] avatars = Resources.FindObjectsOfTypeAll<Avatar>();
                
                // Coba ambil dari asset path yang sesuai dengan nama prefab/model
                string[] matchingModelGuids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(prefabPath) + " t:Model", new[] { modelsFolder });
                if (matchingModelGuids != null && matchingModelGuids.Length > 0)
                {
                    string matchedModelPath = AssetDatabase.GUIDToAssetPath(matchingModelGuids[0]);
                    correctAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(matchedModelPath);
                }

                // Jika belum ketemu, cari file FBX di dalam modelsFolder yang relevan
                if (correctAvatar == null)
                {
                    // Cek komponen SkinnedMeshRenderer untuk mencari sumber mesh
                    SkinnedMeshRenderer smr = prefabRoot.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (smr != null && smr.sharedMesh != null)
                    {
                        string meshPath = AssetDatabase.GetAssetPath(smr.sharedMesh);
                        if (!string.IsNullOrEmpty(meshPath))
                        {
                            correctAvatar = AssetDatabase.LoadAssetAtPath<Avatar>(meshPath);
                        }
                    }
                }

                if (correctAvatar != null && anim.avatar != correctAvatar)
                {
                    anim.avatar = correctAvatar;
                    modified = true;
                }

                if (modified)
                {
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    fixedPrefabsCount++;
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

        EditorUtility.DisplayDialog("Sukses!", $"Berhasil memperbaiki {fixedModelsCount} Model Rig dan memperbarui Avatar pada {fixedPrefabsCount} Prefab!\n\nSekarang animasi tangan dan tulang NPC akan bergerak mulus dan natural tanpa patah/terputar.", "Mantap");
        Debug.Log($"<color=green>[NPC Rig Fixer]</color> Berhasil memperbaiki {fixedModelsCount} Rig model dan {fixedPrefabsCount} Avatar prefab.");
    }
}
#endif
