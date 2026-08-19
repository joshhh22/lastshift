using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [SerializeField] private NPCSpawner npcSpawner;

    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Spawn sejumlah NPC
    public void SpawnPassenger(int amount, float minDelay, float maxDelay)
    {
        Debug.Log($"SpawnPassenger Dipanggil ({amount})");

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(
            SpawnAmountRoutine(amount, minDelay, maxDelay));
    }

    // Spawn tanpa henti
    public void SpawnForever(float minDelay, float maxDelay)
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(
            SpawnForeverRoutine(minDelay, maxDelay));
    }

    public void StopSpawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnAmountRoutine(int amount, float minDelay, float maxDelay)
    {
        Debug.Log("Coroutine Spawn Jalan");

        // Berikan jeda sejenak agar transisi fade-in selesai sebelum NPC pertama muncul
        yield return new WaitForSeconds(3.5f);

        for (int i = 0; i < amount; i++)
        {
            Debug.Log("Spawn NPC Ke-" + (i + 1));

            while (CounterManager.Instance.IsOccupied() &&
                   !QueueManager.Instance.HasSpace())
            {
                yield return new WaitForSeconds(1f);
            }

            npcSpawner.SpawnNPC();

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }

        spawnRoutine = null;
    }

    private IEnumerator SpawnForeverRoutine(float minDelay, float maxDelay)
    {
        while (true)
        {
            while (CounterManager.Instance.IsOccupied() &&
                   !QueueManager.Instance.HasSpace())
            {
                yield return new WaitForSeconds(1f);
            }

            npcSpawner.SpawnNPC();

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }

    public void SpawnDay1Passengers()
    {
        SpawnPassenger(5, 5f, 10f);
    }

    public void SpawnContinueWorking()
    {
        SpawnForever(12f, 20f);
    }
}