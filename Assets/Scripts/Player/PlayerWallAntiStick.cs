using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerWallAntiStick : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private float extraGravity = 5.0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        SnapToFloorAndFaceForward();
    }

    private void Start()
    {
        // Snap langsung ke lantai saat spawn agar tidak jatuh dari udara
        SnapToFloorAndFaceForward();
    }

    public void SnapToFloorAndFaceForward()
    {
        if (controller == null) controller = GetComponent<CharacterController>();

        bool wasEnabled = controller != null && controller.enabled;
        if (wasEnabled) controller.enabled = false;

        // Cari spawn point
        GameObject sp = GameObject.Find("PlayerSpawnPoint");
        if (sp != null)
        {
            Vector3 targetPos = sp.transform.position;
            if (Physics.Raycast(targetPos + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 5f))
            {
                targetPos = hit.point + Vector3.up * 0.05f;
            }
            transform.position = targetPos;
            transform.rotation = sp.transform.rotation;
        }
        else
        {
            if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 5f))
            {
                transform.position = hit.point + Vector3.up * 0.05f;
            }
        }

        // Reset Cinemachine Camera Target and Pitch/Yaw in FirstPersonController agar hadap depan lurus
        var fpc = GetComponent<StarterAssets.FirstPersonController>();
        if (fpc != null)
        {
            var type = fpc.GetType();
            var yawField = type.GetField("_cinemachineTargetYaw", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pitchField = type.GetField("_cinemachineTargetPitch", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (yawField != null) yawField.SetValue(fpc, transform.eulerAngles.y);
            if (pitchField != null) pitchField.SetValue(fpc, 0f);

            var camTargetProp = type.GetField("CinemachineCameraTarget", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (camTargetProp != null)
            {
                GameObject camTarget = camTargetProp.GetValue(fpc) as GameObject;
                if (camTarget != null)
                {
                    camTarget.transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                }
            }
        }

        Physics.SyncTransforms();
        if (wasEnabled) controller.enabled = true;
    }

    private void Update()
    {
        if (controller == null) return;

        // Berikan gravitasi lembut hanya saat di udara dan tidak sedang di lantai
        if (!controller.isGrounded)
        {
            controller.Move(Vector3.down * extraGravity * Time.deltaTime);
        }
    }
}
