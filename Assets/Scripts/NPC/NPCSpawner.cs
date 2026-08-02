using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CounterManager counterManager;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform runtimeParent;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 4f;
    [SerializeField] private bool autoSpawn = true;

    [Header("NPC Prefabs")]
    [SerializeField] private List<NPCController> npcPrefabs = new();

    private int lastIndex = -1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnNPC();
        }
    }

    private void Start()
    {
        if (autoSpawn)
            StartCoroutine(AutoSpawnRoutine());
    }

    private IEnumerator AutoSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Jangan spawn kalau counter + queue penuh
            if (CounterManager.Instance.IsOccupied() &&
                !QueueManager.Instance.HasSpace())
            {
                continue;
            }

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

        StartCoroutine(BeginRoutine(npc));
    }

    IEnumerator BeginRoutine(NPCController npc)
    {
        npc.SetState(NPCState.Idle);

        yield return new WaitForSeconds(2f);

        npc.PlayLookAround();

        yield return new WaitForSeconds(3f);

        if (!counterManager.IsOccupied())
        {
            npc.MoveToCounter(counterManager.CounterPoint);
        }
        else
        {
            QueueManager.Instance.AddNPC(npc);
        }
    }
}