#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using StarterAssets;

public class InspectPlayerComponents : MonoBehaviour
{
    [MenuItem("Tools/Last Shift/Inspect and Fix Player Wall Stick")]
    public static void InspectAndFix()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            var fpc = Object.FindObjectOfType<FirstPersonController>(true);
            if (fpc != null) player = fpc.gameObject;
        }

        if (player == null)
        {
            Debug.LogError("[InspectPlayer] Player tidak ditemukan di scene!");
            return;
        }

        Debug.Log($"[InspectPlayer] Ditemukan Player: {player.name}");

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            Debug.Log($"[InspectPlayer] CharacterController - SlopeLimit: {cc.slopeLimit}, StepOffset: {cc.stepOffset}, SkinWidth: {cc.skinWidth}, Radius: {cc.radius}, Height: {cc.height}");
            // Fix wall climbing:
            // 1. Slope limit set to 45 degrees
            // 2. Step offset set to 0.25 (cukup untuk tangga pendek, tidak bisa manjat tembok)
            // 3. MinMoveDistance set to 0
            cc.slopeLimit = 45f;
            cc.stepOffset = 0.25f;
            cc.skinWidth = 0.05f;
            EditorUtility.SetDirty(cc);
        }

        FirstPersonController fpsCtrl = player.GetComponent<FirstPersonController>();
        if (fpsCtrl != null)
        {
            SerializedObject so = new SerializedObject(fpsCtrl);
            
            SerializedProperty jumpHeightProp = so.FindProperty("JumpHeight");
            SerializedProperty jumpTimeoutProp = so.FindProperty("JumpTimeout");
            SerializedProperty groundLayersProp = so.FindProperty("GroundLayers");
            SerializedProperty groundedRadiusProp = so.FindProperty("GroundedRadius");
            SerializedProperty groundedOffsetProp = so.FindProperty("GroundedOffset");

            float jumpHeight = jumpHeightProp != null ? jumpHeightProp.floatValue : -1f;
            float groundedRadius = groundedRadiusProp != null ? groundedRadiusProp.floatValue : -1f;
            float groundedOffset = groundedOffsetProp != null ? groundedOffsetProp.floatValue : -1f;

            Debug.Log($"[InspectPlayer] FirstPersonController - JumpHeight: {jumpHeight}, GroundedRadius: {groundedRadius}, GroundedOffset: {groundedOffset}");

            // Untuk game horor simulasi shift realistis:
            // Matikan JumpHeight (JumpHeight = 0) agar player tidak bisa melompati counter / memanjat dinding
            if (jumpHeightProp != null)
            {
                jumpHeightProp.floatValue = 0f;
            }
            if (groundedRadiusProp != null)
            {
                groundedRadiusProp.floatValue = 0.2f; // Perkecil radius agar tidak mendeteksi tembok samping sebagai tanah
            }
            if (groundedOffsetProp != null)
            {
                groundedOffsetProp.floatValue = -0.15f;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(fpsCtrl);
            Debug.Log("<color=green>[InspectPlayer] JumpHeight di-set ke 0 & GroundedRadius diperbaiki agar tidak nempel/manjat tembok!</color>");
        }

        // Tambahkan juga komponen PlayerWallAntiStick jika ingin pengamanan ganda
        PlayerWallAntiStick antiStick = player.GetComponent<PlayerWallAntiStick>();
        if (antiStick == null)
        {
            antiStick = player.AddComponent<PlayerWallAntiStick>();
            EditorUtility.SetDirty(player);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(player.scene);

        EditorUtility.DisplayDialog("Sukses!", "Bug Spiderman / Nempel & Manjat Tembok Berhasil Diperbaiki!\n\n1. JumpHeight di-set ke 0 (Player tidak akan lompat/manjat dinding).\n2. StepOffset & SlopeLimit CharacterController dinormalkan.\n3. PlayerWallAntiStick dipasang untuk memastikan gravitasi dan traksi lantai selalu normal.", "Mantap!");
    }
}
#endif
