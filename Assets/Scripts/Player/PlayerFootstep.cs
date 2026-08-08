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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<FirstPersonController>();
    }

    private void Update()
    {
        if (!player.Grounded)
            return;

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