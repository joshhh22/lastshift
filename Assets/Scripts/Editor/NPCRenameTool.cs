#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class NPCRenameTool : EditorWindow
{
    private NPCGender targetGender = NPCGender.Male;

    private readonly string[] maleNames =
    {
        "Arthur", "Thomas", "Jack", "William", "Oliver", 
        "Lucas", "Henry", "Ethan", "James", "Benjamin",
        "Alexander", "Daniel", "Matthew", "Samuel", "David",
        "Joshua", "Christopher", "Andrew", "Michael", "John",
        "Nathan", "Victor", "Vance", "Leon", "Cassian"
    };

    private readonly string[] femaleNames =
    {
        "Emma", "Olivia", "Ava", "Sophia", "Isabella", 
        "Mia", "Amelia", "Harper", "Emily", "Abigail",
        "Elizabeth", "Sofia", "Avery", "Ella", "Scarlett",
        "Grace", "Chloe", "Victoria", "Riley", "Aria",
        "Lily", "Nora", "Zoey", "Hannah", "Lillian"
    };

    [MenuItem("Tools/Last Shift/Random Rename NPC Tool")]
    public static void ShowWindow()
    {
        GetWindow<NPCRenameTool>("Rename NPC Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Alat Random Ganti Nama NPC (English)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Pilih file Prefab NPC di folder Project, lalu pilih jenis kelamin di bawah, dan klik Rename! Nama file akan diganti secara acak menggunakan nama bahasa Inggris.", MessageType.Info);
        
        GUILayout.Space(10);

        targetGender = (NPCGender)EditorGUILayout.EnumPopup("Pilih Gender Target:", targetGender);

        GUILayout.Space(10);

        if (GUILayout.Button("Berikan Nama Acak ke Prefab yang Dipilih", GUILayout.Height(40)))
        {
            RenameSelected();
        }
    }

    private void RenameSelected()
    {
        int count = 0;
        
        // Agar nama tidak dobel di folder yang sama (Unity tidak suka nama aset kembar)
        // Kita kocok array-nya atau pilih acak dan tambahin angka kalau kepepet
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) continue;

            string randomName = "";
            if (targetGender == NPCGender.Male)
            {
                randomName = maleNames[Random.Range(0, maleNames.Length)];
            }
            else
            {
                randomName = femaleNames[Random.Range(0, femaleNames.Length)];
            }

            // Tambahkan huruf/angka acak kecil di belakang supaya mencegah error nama file duplikat (kembar) di Unity
            string uniqueSuffix = "_" + Random.Range(100, 999).ToString();
            string finalName = randomName + uniqueSuffix;

            // Proses ganti nama file
            AssetDatabase.RenameAsset(assetPath, finalName);
            count++;
        }

        if (count > 0)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Berhasil!", $"Telah mengganti nama {count} prefab dengan nama-nama English secara acak!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Gagal", "Tidak ada Prefab yang dipilih di jendela Project.", "OK");
        }
    }
}
#endif
