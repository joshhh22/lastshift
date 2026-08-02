using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public static CounterManager Instance { get; private set; }

    [SerializeField] private Transform counterPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CounterInteraction counterInteraction;

    private NPCController currentNPC;
    private bool reserved = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Transform CounterPoint => counterPoint;
    public Transform ExitPoint => exitPoint;

    public bool IsOccupied()
    {
        return currentNPC != null;
    }

    public bool IsReserved()
    {
        return reserved;
    }

    public void ReserveCounter()
    {
        reserved = true;
    }

    public bool TryOccupy(NPCController npc)
    {
        Debug.Log("TryOccupy dipanggil oleh : " + npc.name);

        if (currentNPC != null)
        {
            Debug.Log("Counter masih dipakai : " + currentNPC.name);
            return false;
        }

        currentNPC = npc;

        npc.SetState(NPCState.AtCounter);

        counterInteraction.currentNPC = npc;

        Debug.Log("Counter sekarang dipakai : " + currentNPC.name);

        return true;
    }

    public void ReleaseCounter()
    {
        Debug.Log("ReleaseCounter");

        currentNPC = null;
        reserved = false;

        counterInteraction.currentNPC = null;
    }

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }
}