using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour, IInteractable
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Interaction")]
    [SerializeField] private string interactionText = "Open Door";

    [Header("Door Settings")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 180f;

    private bool isOpen = false;
    private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private void Start()
    {
        closedRotation = transform.localRotation;

        Vector3 rotation = Vector3.zero;

        switch (rotationAxis)
        {
            case RotationAxis.X:
                rotation.x = openAngle;
                break;

            case RotationAxis.Y:
                rotation.y = openAngle;
                break;

            case RotationAxis.Z:
                rotation.z = openAngle;
                break;
        }

        openRotation = closedRotation * Quaternion.Euler(rotation);
    }

    public string GetInteractionText()
    {
        return isOpen ? "Close Door" : interactionText;
    }

    public void Interact()
    {
        if (isMoving)
            return;

        StartCoroutine(RotateDoor());
    }

    private IEnumerator RotateDoor()
    {
        isMoving = true;

        Quaternion targetRotation = isOpen ? closedRotation : openRotation;

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime);

            yield return null;
        }

        transform.localRotation = targetRotation;

        isOpen = !isOpen;
        isMoving = false;
    }
}