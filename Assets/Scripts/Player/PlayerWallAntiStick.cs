using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerWallAntiStick : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private float extraGravity = 12.0f;
    [SerializeField] private LayerMask groundLayer = ~0; // All layers by default

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (controller == null) return;

        // Jika player sedang melayang/tidak menyentuh lantai (misal terdorong ke sudut dinding)
        if (!controller.isGrounded)
        {
            // Berikan gaya gravitasi ke bawah agar player langsung jatuh kembali ke lantai dan tidak melayang nempel di dinding
            controller.Move(Vector3.down * extraGravity * Time.deltaTime);
        }
    }
}
