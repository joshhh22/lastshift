using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        ObjectiveManager.Instance.CompleteObjective();
    }
}