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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
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

        bool isLiarOrMonster = npc.passengerData.isMonster || (npc.passengerData.ticket != null && npc.passengerData.ticket.status != TicketStatus.Valid);

        // Jika pembohong/panik: langsung buru-buru turun (jeda singkat 0.5s)
        // Jika penumpang normal: jeda natural 1.5s - 2.5s sebelum mulai jalan turun
        float spawnWait = isLiarOrMonster ? 0.5f : Random.Range(1.5f, 2.5f);
        yield return new WaitForSeconds(spawnWait);

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

    public void ClearRuntimeNPCs()
    {
        if (runtimeParent != null)
        {
            foreach (Transform child in runtimeParent)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}