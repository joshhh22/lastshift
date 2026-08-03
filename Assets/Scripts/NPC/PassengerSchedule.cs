using System;
using UnityEngine;

[Serializable]
public class PassengerSchedule
{
    [Range(0, 23)]
    public int hour;

    [Range(0, 59)]
    public int minute;

    [Min(1)]
    public int spawnCount = 1;

    [Tooltip("Nama kereta / event (opsional)")]
    public string scheduleName;

    [Tooltip("Jeda antar NPC (detik)")]
    public float spawnInterval = 2f;

    [HideInInspector]
    public bool triggered;
}