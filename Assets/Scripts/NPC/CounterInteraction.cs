using UnityEngine;

public class CounterInteraction : MonoBehaviour
{
    public NPCController currentNPC;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Tekan E");

            if (currentNPC == null)
            {
                Debug.Log("Tidak ada NPC di counter");
                return;
            }

            Debug.Log("Serve : " + currentNPC.name);

            currentNPC.Serve();
        }
    }
}