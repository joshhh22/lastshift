using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance;

    [Header("Queue Points")]
    [SerializeField] private Transform[] queuePoints;

    private readonly List<NPCController> queue = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool AddNPC(NPCController npc)
    {
        if (queue.Count >= queuePoints.Length)
            return false;

        queue.Add(npc);

        UpdateQueue();

        return true;
    }

    // ==========================
    // NPC TERDEPAN MAJU KE COUNTER
    // ==========================

    public void MoveFrontToCounter()
    {
        Debug.Log("MoveFrontToCounter dipanggil");

        Debug.Log("Queue Count = " + queue.Count);

        if (queue.Count > 0)
            Debug.Log("NPC yang maju = " + queue[0].name);

        if (queue.Count == 0)
            return;

        NPCController npc = queue[0];

        queue.RemoveAt(0);

        npc.MoveToCounter(CounterManager.Instance.CounterPoint);

        UpdateQueue();
    }

    // ==========================
    // GESER SEMUA ANTRIAN
    // ==========================

    void UpdateQueue()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            queue[i].MoveToQueue(queuePoints[i]);
        }
    }

    public bool HasSpace()
    {
        return queue.Count < queuePoints.Length;
    }

    public int GetQueueCount()
    {
        return queue.Count;
    }

    public NPCController GetFrontNPC()
    {
        if (queue.Count == 0)
            return null;

        return queue[0];
    }
}