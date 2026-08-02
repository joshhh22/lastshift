using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [SerializeField] private NPCSpawner npcSpawner;

    private Coroutine spawnRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnPassenger(5, 5f, 10f);
    }

    public void SpawnPassenger(int amount, float minDelay, float maxDelay)
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(
            SpawnAmountRoutine(amount, minDelay, maxDelay));
    }

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
            StopCoroutine(spawnRoutine);
    }

    IEnumerator SpawnAmountRoutine(int amount, float minDelay, float maxDelay)
    {
        for (int i = 0; i < amount; i++)
        {
            npcSpawner.SpawnNPC();

            float delay = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator SpawnForeverRoutine(float minDelay, float maxDelay)
    {
        while (true)
        {
            npcSpawner.SpawnNPC();

            float delay = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(delay);
        }
    }
}