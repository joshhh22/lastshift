#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class AnimationLoopFixer : AssetPostprocessor
{
    [MenuItem("Tools/Last Shift/Fix Animation Loops (Aktifkan Loop Time)")]
    public static void FixAllAnimationLoops()
    {
        string folder = "Assets/Art/npc/withoutskin";
        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { folder });

        int count = 0;

        foreach (string guid in fbxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path).ToLower();

            // Angry gesture tidak perlu di-loop agar main sekali lalu selesai
            if (fileName.Contains("angry"))
                continue;

            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    clips = importer.defaultClipAnimations;
                }

                if (clips == null || clips.Length == 0)
                {
                    clips = new ModelImporterClipAnimation[]
                    {
                        new ModelImporterClipAnimation
                        {
                            name = "mixamo.com",
                            takeName = "mixamo.com"
                        }
                    };
                }

                bool modified = false;

                foreach (var clip in clips)
                {
                    clip.loopTime = true;
                    clip.loopPose = true;
                    modified = true;
                }

                if (modified)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                    count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Sukses!", $"Berhasil mengaktifkan Loop Time untuk {count} animasi! Sekarang NPC tidak akan macet/sliding lagi saat berjalan.", "Mantap");
        Debug.Log($"<color=green>[Animation Loop Fixer]</color> Berhasil memperbaiki Loop Time pada {count} animasi FBX.");
    }
}
#endif
