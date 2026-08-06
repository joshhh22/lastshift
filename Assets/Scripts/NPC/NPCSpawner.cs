using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static NPCSpawner Instance;

    [Header("References")]
    [SerializeField] private CounterManager counterManager;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform runtimeParent;

    [Header("NPC Prefabs")]
    [SerializeField] private List<NPCController> npcPrefabs = new();

    private int lastIndex = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // DEBUG
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnNPC();
        }
    }

    public void SpawnNPC()
    {
        int index;

        do
        {
            index = Random.Range(0, npcPrefabs.Count);
        }
        while (npcPrefabs.Count > 1 && index == lastIndex);

        lastIndex = index;

        NPCController npc = Instantiate(
            npcPrefabs[index],
            spawnPoint.position,
            spawnPoint.rotation,
            runtimeParent);

        NPCController controller = npc.GetComponent<NPCController>();
        controller.InitializePassenger();

        StartCoroutine(BeginRoutine(npc));
    }

    private IEnumerator BeginRoutine(NPCController npc)
    {
        npc.SetState(NPCState.Idle);

        yield return new WaitForSeconds(2f);

        npc.PlayLookAround();

        yield return new WaitForSeconds(3f);

        // Counter kosong → langsung ke counter
        if (!counterManager.IsOccupied() &&
            !counterManager.IsReserved())
        {
            counterManager.ReserveCounter();
            npc.MoveToCounter(counterManager.CounterPoint);
        }
        // Counter penuh → masuk queue
        else
        {
            QueueManager.Instance.AddNPC(npc);
        }
    }
}