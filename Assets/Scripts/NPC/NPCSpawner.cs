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
        NPCController prefab =
            NPCDatabase.Instance.GetRandomNPC();

        NPCController npc = Instantiate(
            prefab,
            spawnPoint.position,
            spawnPoint.rotation,
            runtimeParent);

        // ===== SNAPPING KE NAVMESH =====
        // Agar NPC baru yang pivot/porosnya agak melayang bisa dipaksa nempel ke tanah
        UnityEngine.AI.NavMeshAgent agent = npc.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPoint.position, out UnityEngine.AI.NavMeshHit hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                npc.transform.position = hit.position;
            }
            agent.enabled = true;
        }
        // ===============================

        npc.InitializePassenger();

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