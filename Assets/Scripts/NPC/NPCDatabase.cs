using System.Collections.Generic;
using UnityEngine;

public class NPCDatabase : MonoBehaviour
{
    public static NPCDatabase Instance;

    [Header("Male NPC")]
    [SerializeField] private List<NPCController> malePrefabs = new();

    [Header("Female NPC")]
    [SerializeField] private List<NPCController> femalePrefabs = new();

    [Header("Special/Anomalies")]
    [SerializeField] private List<NPCController> monsterPrefabs = new();

    private HashSet<NPCController> spawnedNPCsOfToday = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ResetDayNPCs()
    {
        spawnedNPCsOfToday.Clear();
        Debug.Log("Daily spawn history cleared.");
    }

    public NPCController GetRandomNPC()
    {
        NPCController selectedPrefab = null;

        // Peluang memunculkan Monster Anomali semakin besar tiap harinya
        if (DayManager.Instance != null && monsterPrefabs != null && monsterPrefabs.Count > 0)
        {
            float monsterChance = 0f;

            switch (DayManager.Instance.CurrentDay)
            {
                case GameDay.Day4: monsterChance = 0.20f; break;
                case GameDay.Day5: monsterChance = 0.30f; break;
                case GameDay.Day6: monsterChance = 0.40f; break;
                case GameDay.Day7: monsterChance = 0.50f; break;
            }

            if (monsterChance > 0f && Random.value < monsterChance)
            {
                List<NPCController> availableMonsters = new();
                foreach (var p in monsterPrefabs)
                {
                    if (p != null && !spawnedNPCsOfToday.Contains(p))
                    {
                        availableMonsters.Add(p);
                    }
                }

                if (availableMonsters.Count == 0)
                {
                    // Reset only monsters if all were spawned
                    foreach (var p in monsterPrefabs)
                    {
                        if (p != null) spawnedNPCsOfToday.Remove(p);
                    }
                    availableMonsters.AddRange(monsterPrefabs);
                }

                if (availableMonsters.Count > 0)
                {
                    selectedPrefab = availableMonsters[Random.Range(0, availableMonsters.Count)];
                    spawnedNPCsOfToday.Add(selectedPrefab);
                    return selectedPrefab;
                }
            }
        }

        NPCGender gender = Random.value < 0.5f ? NPCGender.Male : NPCGender.Female;

        // Try getting unique NPC from chosen gender first
        selectedPrefab = TryGetUniqueNPC(gender);
        if (selectedPrefab == null)
        {
            // Try opposite gender
            NPCGender oppositeGender = (gender == NPCGender.Male) ? NPCGender.Female : NPCGender.Male;
            selectedPrefab = TryGetUniqueNPC(oppositeGender);
        }

        // If both genders are fully spawned today, reset history and pick again
        if (selectedPrefab == null)
        {
            spawnedNPCsOfToday.Clear();
            gender = Random.value < 0.5f ? NPCGender.Male : NPCGender.Female;
            selectedPrefab = TryGetUniqueNPC(gender);
            if (selectedPrefab == null)
            {
                NPCGender oppositeGender = (gender == NPCGender.Male) ? NPCGender.Female : NPCGender.Male;
                selectedPrefab = TryGetUniqueNPC(oppositeGender);
            }
        }

        if (selectedPrefab != null)
        {
            spawnedNPCsOfToday.Add(selectedPrefab);
        }

        return selectedPrefab;
    }

    private NPCController TryGetUniqueNPC(NPCGender gender)
    {
        List<NPCController> prefabs = (gender == NPCGender.Male) ? malePrefabs : femalePrefabs;
        if (prefabs == null || prefabs.Count == 0)
            return null;

        List<NPCController> available = new();
        foreach (var p in prefabs)
        {
            if (p != null && !spawnedNPCsOfToday.Contains(p))
            {
                available.Add(p);
            }
        }

        if (available.Count > 0)
        {
            return available[Random.Range(0, available.Count)];
        }

        return null;
    }
}