using UnityEngine;

public class CounterInteraction : MonoBehaviour
{
    public NPCController currentNPC;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E DITEKAN");

            if (currentNPC == null)
            {
                Debug.Log("NPC NULL");
                return;
            }

            Debug.Log("SERVE");

            currentNPC.Serve();
        }
    }
}