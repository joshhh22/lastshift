using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CanvasFixer : EditorWindow
{
    [MenuItem("Tools/Last Shift/Fix UI Canvases (1920x1080)")]
    public static void FixCanvases()
    {
        CanvasScaler[] scalers = Resources.FindObjectsOfTypeAll<CanvasScaler>();
        int count = 0;
        
        foreach (var scaler in scalers)
        {
            // Abaikan canvas prefab yang bukan dari scene aktif
            if (scaler.gameObject.scene.name == null) continue;

            Undo.RecordObject(scaler, "Fix Canvas Scaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            count++;
            
            EditorUtility.SetDirty(scaler);
        }
        
        Debug.Log($"[Canvas Fixer] Berhasil memperbaiki {count} Canvas Scaler ke standar 1920x1080 (16:9).");
    }
}
