using System.Collections;
using UnityEngine;

public class CleaningStaffInteraction : MonoBehaviour, IInteractable
{
    [Header("Daily Dialogues")]
    [Tooltip("Index 0 = Day 1, Index 1 = Day 2, etc.")]
    [SerializeField] private DialogueData[] dailyDialogues;

    [Header("Fallback Dialogue")]
    [SerializeField] private DialogueData fallbackDialogue;

    [Header("Requirements")]
    [SerializeField] private int requiredObjectiveIndex;

    [Header("Cinematic Camera Focus Settings")]
    [SerializeField] private float zoomFOV = 56f;
    [SerializeField] private float focusSpeed = 3.5f;

    private CleaningStaffController cleaningStaff;
    private bool hasTalked;

    private Camera playerCam;
    private float defaultFOV = 71.2f;
    private Quaternion originalCamRotation;
    private Coroutine cinematicCoroutine;

    public void ResetForNewDay()
    {
        hasTalked = false;
    }

    private void Awake()
    {
        cleaningStaff = GetComponent<CleaningStaffController>();

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.onDialogueFinished.AddListener(OnDialogueFinished);
    }

    private void Start()
    {
        StartCoroutine(CheckInitialTalkState());
    }

    private IEnumerator CheckInitialTalkState()
    {
        yield return null;
        if (ObjectiveManager.Instance != null)
        {
            int currentIdx = ObjectiveManager.Instance.GetCurrentIndex();
            var objectives = ObjectiveManager.Instance.GetObjectives();
            if (objectives != null)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    string t = objectives[i].title.ToLower();
                    if ((t.Contains("cleaning") || t.Contains("staff")) && currentIdx > i)
                    {
                        hasTalked = true;
                        break;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.onDialogueFinished.RemoveListener(OnDialogueFinished);
    }

    public string GetInteractionText()
    {
        if (hasTalked)
            return "";

        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (string.IsNullOrEmpty(curObj) || !curObj.ToLower().Contains("cleaning"))
                return "";
        }

        return "Talk";
    }

    public void Interact()
    {
        if (hasTalked)
            return;

        if (ObjectiveManager.Instance != null && ObjectiveManager.Instance.GetCurrentIndex() != requiredObjectiveIndex)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (string.IsNullOrEmpty(curObj) || !curObj.ToLower().Contains("cleaning"))
                return;
        }

        hasTalked = true;

        if (cleaningStaff != null)
        {
            cleaningStaff.StopPatrol();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cleaningStaff.FacePlayer(playerObj.transform);
            }
        }

        // Matikan goyangan kamera (bobbing/idle breathing) dan sembunyikan crosshair
        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(true);

        CrosshairManager.ShowCrosshair(false);

        // Mulai fokus & zoom kamera ke Cleaning Staff
        StartCinematicCamera(true);

        DialogueData dialogueToPlay = fallbackDialogue;

        if (DayManager.Instance != null)
        {
            int dayIndex = (int)DayManager.Instance.CurrentDay - 1;
            if (dailyDialogues != null && dayIndex >= 0 && dayIndex < dailyDialogues.Length && dailyDialogues[dayIndex] != null)
            {
                dialogueToPlay = dailyDialogues[dayIndex];
            }
        }

        if (dialogueToPlay != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueToPlay, OnDialogueFinished);
        }
    }

    private void StartCinematicCamera(bool focusIn)
    {
        if (playerCam == null)
        {
            playerCam = Camera.main;
        }

        if (playerCam != null && defaultFOV <= 0f)
        {
            defaultFOV = playerCam.fieldOfView > 0 ? playerCam.fieldOfView : 71.2f;
        }

        if (playerCam == null) return;

        if (cinematicCoroutine != null)
            StopCoroutine(cinematicCoroutine);

        cinematicCoroutine = StartCoroutine(CinematicCameraRoutine(focusIn));
    }

    private IEnumerator CinematicCameraRoutine(bool focusIn)
    {
        if (playerCam == null) yield break;

        float targetFOV = focusIn ? zoomFOV : (defaultFOV > 0 ? defaultFOV : 71.2f);
        float startFOV = playerCam.fieldOfView;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 targetLookPos = transform.position + Vector3.up * 1.45f;
        Vector3 lookDir = (targetLookPos - playerCam.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        Quaternion startCamRotation = playerCam.transform.rotation;
        Quaternion startPlayerRotation = playerObj != null ? playerObj.transform.rotation : Quaternion.identity;

        Vector3 flatLookDir = new Vector3(lookDir.x, 0, lookDir.z).normalized;
        Quaternion targetPlayerRotation = flatLookDir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(flatLookDir) : startPlayerRotation;

        float elapsed = 0f;
        float duration = 0.45f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

            playerCam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            if (focusIn)
            {
                playerCam.transform.rotation = Quaternion.Slerp(startCamRotation, targetRotation, t);
                if (playerObj != null)
                {
                    playerObj.transform.rotation = Quaternion.Slerp(startPlayerRotation, targetPlayerRotation, t);
                }
            }

            yield return null;
        }

        playerCam.fieldOfView = targetFOV;
        cinematicCoroutine = null;
    }

    private void OnDialogueFinished()
    {
        // Kembalikan zoom kamera & aktifkan kembali bobbing dan crosshair
        StartCinematicCamera(false);

        if (CameraHeadBob.Instance != null)
            CameraHeadBob.Instance.SetBobbingDisabled(false);

        CrosshairManager.ShowCrosshair(true);

        if (cleaningStaff != null)
        {
            // Buka patroli bebas ke seluruh stasiun
            cleaningStaff.UnlockFullPatrol();
            cleaningStaff.StartPatrol();
        }

        // Selesaikan objektif 'Talk To Cleaning Staff'
        if (ObjectiveManager.Instance != null)
        {
            string curObj = ObjectiveManager.Instance.GetCurrentObjective();
            if (!string.IsNullOrEmpty(curObj) && (curObj.ToLower().Contains("cleaning") || curObj.ToLower().Contains("staff")))
            {
                ObjectiveManager.Instance.CompleteObjective();
            }
        }
    }
}