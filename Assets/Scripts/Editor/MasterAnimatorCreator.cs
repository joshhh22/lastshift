#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class MasterAnimatorCreator
{
    [MenuItem("Tools/Last Shift/Buat Bersih NPC_Master & Semua Variasi Animasi")]
    public static void CreateMaster()
    {
        string folder = "Assets/Art/npc/withoutskin/";
        string masterPath = folder + "NPC_Master.controller";
        string cleaningPath = folder + "CleaningStaff.controller";

        // 1. Ambil SEMUA AnimationClip dari FBX
        AnimationClip idleClip = GetClip(folder + "Idle.fbx");
        AnimationClip walkClip = GetClip(folder + "Walking.fbx");
        AnimationClip nervousClip = GetClip(folder + "Nervously Look Around (1).fbx");
        AnimationClip angryClip = GetClip(folder + "Angry Gesture.fbx");
        AnimationClip phoneClip = GetClip(folder + "Talking On A Cell Phone.fbx");
        AnimationClip textClip = GetClip(folder + "Walking While Texting.fbx");
        AnimationClip runClip = GetClip(folder + "Running.fbx");
        AnimationClip lookClip = GetClip(folder + "Look Around (1).fbx");
        AnimationClip oldManClip = GetClip(folder + "Old Man Idle.fbx");
        AnimationClip stretchClip = GetClip(folder + "Arm Stretching.fbx");

        if (idleClip == null || walkClip == null || nervousClip == null || angryClip == null)
        {
            Debug.LogError($"[MasterAnimatorCreator] Gagal memuat klip dasar!");
            EditorUtility.DisplayDialog("Error", "Ada klip animasi yang tidak ditemukan di folder withoutskin!", "OK");
            return;
        }

        // ==========================================
        // 2. BANGUN NPC_MASTER.CONTROLLER
        // ==========================================
        AssetDatabase.DeleteAsset(masterPath);
        AnimatorController masterCtrl = AnimatorController.CreateAnimatorControllerAtPath(masterPath);

        masterCtrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        masterCtrl.AddParameter("IsSuspicious", AnimatorControllerParameterType.Bool);
        masterCtrl.AddParameter("Angry", AnimatorControllerParameterType.Trigger);

        AnimatorStateMachine sm = masterCtrl.layers[0].stateMachine;
        sm.entryPosition = new Vector3(50, 150, 0);
        sm.anyStatePosition = new Vector3(50, 30, 0);
        sm.exitPosition = new Vector3(750, 30, 0);

        AnimatorState idleState = sm.AddState("idle", new Vector3(200, 150, 0));
        idleState.motion = idleClip;

        AnimatorState moveState = sm.AddState("move", new Vector3(450, 150, 0));
        moveState.motion = walkClip;

        AnimatorState nervousState = sm.AddState("nervous", new Vector3(200, 300, 0));
        nervousState.motion = nervousClip;

        AnimatorState angryState = sm.AddState("angry", new Vector3(350, 30, 0));
        angryState.motion = angryClip;

        // Transisi: idle -> move
        var t1 = idleState.AddTransition(moveState);
        t1.hasExitTime = false;
        t1.duration = 0.2f;
        t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Transisi: move -> idle (saat berhenti dan bukan pembohong)
        var t2 = moveState.AddTransition(idleState);
        t2.hasExitTime = false;
        t2.duration = 0.2f;
        t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, "IsSuspicious");

        // Transisi: move -> nervous (saat berhenti dan pembohong/monster)
        var t3 = moveState.AddTransition(nervousState);
        t3.hasExitTime = false;
        t3.duration = 0.2f;
        t3.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        t3.AddCondition(AnimatorConditionMode.If, 0, "IsSuspicious");

        // Transisi: nervous -> move (saat pembohong lanjut jalan)
        var t4 = nervousState.AddTransition(moveState);
        t4.hasExitTime = false;
        t4.duration = 0.2f;
        t4.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // Transisi: Any State -> angry (saat tiket ditolak)
        var t5 = sm.AddAnyStateTransition(angryState);
        t5.hasExitTime = false;
        t5.duration = 0.15f;
        t5.AddCondition(AnimatorConditionMode.If, 0, "Angry");

        // Transisi: angry -> move (setelah marah selesai -> lari keluar)
        var t6 = angryState.AddTransition(moveState);
        t6.hasExitTime = true;
        t6.exitTime = 1.0f;
        t6.duration = 0.25f;

        sm.defaultState = idleState;
        EditorUtility.SetDirty(masterCtrl);

        // ==========================================
        // 3. BANGUN 4 OVERRIDE CONTROLLER
        // ==========================================
        List<RuntimeAnimatorController> allVariants = new List<RuntimeAnimatorController>();
        allVariants.Add(masterCtrl);

        if (phoneClip != null)
        {
            var aoc = CreateOverride(folder + "NPC_Phone.overrideController", masterCtrl, idleClip, phoneClip);
            if (aoc != null) allVariants.Add(aoc);
        }
        if (textClip != null)
        {
            var aoc = CreateOverride(folder + "NPC_Texting.overrideController", masterCtrl, walkClip, textClip);
            if (aoc != null) allVariants.Add(aoc);
        }
        if (runClip != null)
        {
            var aoc = CreateOverride(folder + "NPC_Runner.overrideController", masterCtrl, walkClip, runClip);
            if (aoc != null) allVariants.Add(aoc);
        }
        if (lookClip != null)
        {
            var aoc = CreateOverride(folder + "NPC_LookAround.overrideController", masterCtrl, idleClip, lookClip);
            if (aoc != null) allVariants.Add(aoc);
        }

        // ==========================================
        // 4. BANGUN CLEANINGSTAFF.CONTROLLER RESMI
        // ==========================================
        AssetDatabase.DeleteAsset(cleaningPath);
        AnimatorController cleanCtrl = AnimatorController.CreateAnimatorControllerAtPath(cleaningPath);
        cleanCtrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        cleanCtrl.AddParameter("IdleType", AnimatorControllerParameterType.Int);

        AnimatorStateMachine cSm = cleanCtrl.layers[0].stateMachine;
        cSm.entryPosition = new Vector3(50, 150, 0);
        cSm.exitPosition = new Vector3(750, 150, 0);

        AnimatorState cIdle = cSm.AddState("idle", new Vector3(200, 150, 0));
        cIdle.motion = idleClip;

        AnimatorState cMove = cSm.AddState("move", new Vector3(450, 150, 0));
        cMove.motion = walkClip;

        AnimatorState cOldMan = cSm.AddState("oldmanidle", new Vector3(200, 300, 0));
        cOldMan.motion = oldManClip != null ? oldManClip : idleClip;

        AnimatorState cStretch = cSm.AddState("armstretch", new Vector3(450, 300, 0));
        cStretch.motion = stretchClip != null ? stretchClip : idleClip;

        // Transisi CleaningStaff: idle -> move
        var ct1 = cIdle.AddTransition(cMove);
        ct1.hasExitTime = false;
        ct1.duration = 0.2f;
        ct1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // move -> idle
        var ct2 = cMove.AddTransition(cIdle);
        ct2.hasExitTime = false;
        ct2.duration = 0.2f;
        ct2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        ct2.AddCondition(AnimatorConditionMode.Equals, 0, "IdleType");

        // move -> oldman
        var ct3 = cMove.AddTransition(cOldMan);
        ct3.hasExitTime = false;
        ct3.duration = 0.2f;
        ct3.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        ct3.AddCondition(AnimatorConditionMode.Equals, 1, "IdleType");

        // move -> stretch
        var ct4 = cMove.AddTransition(cStretch);
        ct4.hasExitTime = false;
        ct4.duration = 0.2f;
        ct4.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        ct4.AddCondition(AnimatorConditionMode.Equals, 2, "IdleType");

        // oldman -> move
        var ct5 = cOldMan.AddTransition(cMove);
        ct5.hasExitTime = false;
        ct5.duration = 0.2f;
        ct5.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        // stretch -> move
        var ct6 = cStretch.AddTransition(cMove);
        ct6.hasExitTime = false;
        ct6.duration = 0.2f;
        ct6.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        cSm.defaultState = cIdle;
        EditorUtility.SetDirty(cleanCtrl);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Otomatis Pasang ke Semua Prefab di Assets/Art/Prefabs
        int updatedCount = ApplyToAllPrefabs(allVariants.ToArray());

        EditorUtility.DisplayDialog("Sukses!", $"Seluruh Controller (NPC_Master, 4 Variasi Override, dan CleaningStaff) berhasil dibangun lengkap dengan SEMUA garis panah transisinya!", "Mantap");
        Debug.Log($"<color=green>[MasterAnimatorCreator]</color> Berhasil membangun ulang seluruh controller dengan semua garis transisi.");
    }

    private static AnimatorOverrideController CreateOverride(string path, AnimatorController baseController, AnimationClip originalClip, AnimationClip overrideClip)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AnimatorOverrideController aoc = new AnimatorOverrideController(baseController);
        
        List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        aoc.GetOverrides(overrides);
        
        for (int i = 0; i < overrides.Count; i++)
        {
            if (overrides[i].Key == originalClip)
            {
                overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(originalClip, overrideClip);
            }
        }
        
        aoc.ApplyOverrides(overrides);
        AssetDatabase.CreateAsset(aoc, path);
        EditorUtility.SetDirty(aoc);
        return aoc;
    }

    private static AnimationClip GetClip(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        if (assets == null || assets.Length == 0) return null;

        foreach (var a in assets)
        {
            if (a is AnimationClip c && !c.name.StartsWith("__preview__"))
                return c;
        }
        return null;
    }

    private static int ApplyToAllPrefabs(RuntimeAnimatorController[] variants)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Art/Prefabs" });
        int count = 0;
        foreach (string guid in guids)
        {
            string pPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
            if (prefab != null)
            {
                Animator anim = prefab.GetComponent<Animator>();
                NPCController npc = prefab.GetComponent<NPCController>();
                
                if (anim != null && variants.Length > 0)
                {
                    anim.runtimeAnimatorController = variants[0];
                }

                if (npc != null)
                {
                    SerializedObject so = new SerializedObject(npc);
                    SerializedProperty prop = so.FindProperty("locomotionVariants");
                    if (prop != null)
                    {
                        prop.arraySize = variants.Length;
                        for (int i = 0; i < variants.Length; i++)
                        {
                            prop.GetArrayElementAtIndex(i).objectReferenceValue = variants[i];
                        }
                        so.ApplyModifiedProperties();
                    }
                    EditorUtility.SetDirty(npc);
                }

                EditorUtility.SetDirty(prefab);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        return count;
    }
}
#endif
