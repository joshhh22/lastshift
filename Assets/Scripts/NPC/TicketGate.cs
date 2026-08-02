using System.Collections;
using UnityEngine;

public class TicketGate : MonoBehaviour
{
    [SerializeField] private Transform arm;

    [SerializeField] private float openAngle = 60f;
    [SerializeField] private float speed = 360f;

    private Quaternion closedRot;
    private Quaternion openedRot;

    private bool moving;

    private void Awake()
    {
        closedRot = arm.localRotation;
        openedRot = closedRot * Quaternion.Euler(0f, 0f, openAngle);
    }

    public void Open()
    {
        if (!moving)
            StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        moving = true;

        while (Quaternion.Angle(arm.localRotation, openedRot) > 1f)
        {
            arm.localRotation = Quaternion.RotateTowards(
                arm.localRotation,
                openedRot,
                speed * Time.deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        while (Quaternion.Angle(arm.localRotation, closedRot) > 1f)
        {
            arm.localRotation = Quaternion.RotateTowards(
                arm.localRotation,
                closedRot,
                speed * Time.deltaTime);

            yield return null;
        }

        moving = false;
    }
}