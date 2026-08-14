using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootstep : MonoBehaviour
{
    [SerializeField] private float walkInterval = 0.55f;
    [SerializeField] private float sprintInterval = 0.35f;

    private CharacterController controller;
    private FirstPersonController player;

    private float timer;

    private StarterAssetsInputs starterInputs;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<FirstPersonController>();
        starterInputs = GetComponent<StarterAssetsInputs>();
    }

    private void OnDisable()
    {
        timer = 0f;
    }

    private void Update()
    {
        // Jangan putar langkah kaki jika player sedang dinonaktifkan (misal: saat buka komputer atau UI)
        if (player == null || !player.enabled || !player.CanControl)
        {
            timer = 0f;
            return;
        }

        if (!player.Grounded)
            return;

        // Cek juga input player, jika tidak ada input gerak maka jangan putar langkah
        if (starterInputs != null && starterInputs.move == Vector2.zero)
        {
            timer = 0f;
            return;
        }

        Vector3 horizontalVelocity =
            new Vector3(
                controller.velocity.x,
                0,
                controller.velocity.z);

        if (horizontalVelocity.magnitude < 0.1f)
        {
            timer = 0f;
            return;
        }

        bool sprinting =
            horizontalVelocity.magnitude >
            (player.MoveSpeed + player.SprintSpeed) * 0.5f;

        timer += Time.deltaTime;

        float interval =
            sprinting ? sprintInterval : walkInterval;

        if (timer >= interval)
        {
            timer = 0f;

            AudioManager.Instance.PlayFootstep();
        }
    }
}