using UnityEngine;
using StarterAssets;

/// <summary>
/// Pasang script ini pada Camera GameObject (anak dari Player).
/// Menambahkan efek:
///   1. HEAD BOBBING   – kamera bergoyang sesuai langkah
///   2. BREATHING      – naik-turun halus saat diam
///   3. HAND SWAY      – sedikit miring saat berbelok (tilt)
///   4. LANDING PUNCH  – hentakan kamera saat mendarat dari lompat
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraHeadBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FirstPersonController fpsController;

    [Header("Head Bob")]
    [SerializeField] private float walkBobFrequency = 8f;
    [SerializeField] private float sprintBobFrequency = 12f;
    [SerializeField] private float walkBobAmplitude = 0.006f;
    [SerializeField] private float sprintBobAmplitude = 0.012f;

    [Header("Breathing (Idle)")]
    [SerializeField] private float breathFrequency = 1.1f;
    [SerializeField] private float breathAmplitude = 0.003f;

    [Header("Tilt / Sway")]
    [SerializeField] private float tiltAmount = 1.8f;    // derajat kemiringan max
    [SerializeField] private float tiltSmoothSpeed = 6f;

    [Header("Landing Punch")]
    [SerializeField] private float landingPunchAmount = 0.04f;
    [SerializeField] private float landingPunchSpeed = 12f;

    // --------------------------------------------------------
    private Vector3 originalLocalPos;
    private float bobTimer;
    private float currentTilt;
    private float targetTilt;
    private float landingPunchOffset;
    private float landingPunchVelocity;
    private bool wasGrounded;

    private void Awake()
    {
        // Cari References otomatis jika belum diassign di Inspector
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
        if (fpsController == null)
            fpsController = GetComponentInParent<FirstPersonController>();

        originalLocalPos = transform.localPosition;
    }

    private void Update()
    {
        // ── LANDING PUNCH ──────────────────────────────────────────
        bool grounded = fpsController != null && fpsController.Grounded;
        if (!wasGrounded && grounded)
        {
            // Baru saja mendarat
            landingPunchOffset = -landingPunchAmount;
        }
        wasGrounded = grounded;

        // Spring kembali ke nol
        landingPunchOffset = Mathf.SmoothDamp(
            landingPunchOffset, 0f,
            ref landingPunchVelocity, 1f / landingPunchSpeed);

        // ── PERGERAKAN ─────────────────────────────────────────────
        Vector3 flatVel = new(
            characterController.velocity.x, 0,
            characterController.velocity.z);
        float speed = flatVel.magnitude;

        bool isMoving   = speed > 0.1f;
        bool isSprinting = fpsController != null &&
                           speed > (fpsController.MoveSpeed + fpsController.SprintSpeed) * 0.5f;

        float bobFreq = isSprinting ? sprintBobFrequency : walkBobFrequency;
        float bobAmp  = isSprinting ? sprintBobAmplitude  : walkBobAmplitude;

        // ── BOB / BREATHING ────────────────────────────────────────
        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFreq;
        }
        else
        {
            // Idle → breathing slow
            bobTimer += Time.deltaTime * breathFrequency;
            bobAmp = breathAmplitude;
        }

        float bobY = Mathf.Sin(bobTimer) * bobAmp;
        float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmp * 0.5f; // sedikit horizontal sway

        // ── TILT saat berbelok ──────────────────────────────────────
        // Ambil input horizontal dari StarterAssetsInputs
        float horizontalInput = 0f;
        if (fpsController != null)
        {
            StarterAssetsInputs inp = fpsController.GetComponent<StarterAssetsInputs>();
            if (inp != null) horizontalInput = inp.move.x;
        }
        targetTilt = -horizontalInput * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        // ── TERAPKAN ───────────────────────────────────────────────
        transform.localPosition = new Vector3(
            originalLocalPos.x + bobX,
            originalLocalPos.y + bobY + landingPunchOffset,
            originalLocalPos.z);

        Quaternion tiltRot = Quaternion.Euler(0f, 0f, currentTilt);
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, tiltRot, Time.deltaTime * tiltSmoothSpeed);
    }
}
