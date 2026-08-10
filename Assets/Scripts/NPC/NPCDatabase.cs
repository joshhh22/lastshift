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

    private void Awake()
    {
        Instance = this;
    }

    public NPCController GetRandomNPC()
    {
        // Peluang 20% memunculkan Monster Anomali jika sudah masuk Day 4 ke atas
        if (DayManager.Instance != null && DayManager.Instance.CurrentDay >= GameDay.Day4)
        {
            if (monsterPrefabs != null && monsterPrefabs.Count > 0 && Random.value < 0.2f)
            {
                return monsterPrefabs[Random.Range(0, monsterPrefabs.Count)];
            }
        }

        NPCGender gender =
            Random.value < 0.5f ?
            NPCGender.Male :
            NPCGender.Female;

        if (gender == NPCGender.Male && malePrefabs.Count > 0)
        {
            return malePrefabs[
                Random.Range(0, malePrefabs.Count)];
        }

        if (femalePrefabs.Count > 0)
        {
            return femalePrefabs[
                Random.Range(0, femalePrefabs.Count)];
        }

        return null; // Pengaman kalau list kosong
    }
}