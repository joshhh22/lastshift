using UnityEngine;

public class CounterManager : MonoBehaviour
{
    public static CounterManager Instance { get; private set; }

    [SerializeField] private Transform counterPoint;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private CounterInteraction counterInteraction;

    private NPCController currentNPC;

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

    public bool TryOccupy(NPCController npc)
    {
        if (currentNPC != null)
            return false;

        currentNPC = npc;
        npc.SetState(NPCState.AtCounter);
        counterInteraction.currentNPC = npc;
        Debug.Log($"{npc.name} occupied counter.");

        return true;
    }

    public void ReleaseCounter()
    {
        currentNPC = null;
        Debug.Log("Counter released.");
    }

    public NPCController GetCurrentNPC()
    {
        return currentNPC;
    }
}