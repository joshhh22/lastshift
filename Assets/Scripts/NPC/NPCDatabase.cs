using System.Collections.Generic;
using UnityEngine;

public class NPCDatabase : MonoBehaviour
{
    public static NPCDatabase Instance;

    [Header("Male NPC")]
    [SerializeField] private List<NPCController> malePrefabs = new();

    [Header("Female NPC")]
    [SerializeField] private List<NPCController> femalePrefabs = new();

    private void Awake()
    {
        Instance = this;
    }

    public NPCController GetRandomNPC()
    {
        NPCGender gender =
            Random.value < 0.5f ?
            NPCGender.Male :
            NPCGender.Female;

        if (gender == NPCGender.Male)
        {
            return malePrefabs[
                Random.Range(0, malePrefabs.Count)];
        }

        return femalePrefabs[
            Random.Range(0, femalePrefabs.Count)];
    }
}