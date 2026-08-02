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

    public void RemoveFrontNPC()
    {
        if (queue.Count == 0)
            return;

        queue.RemoveAt(0);

        UpdateQueue();
    }

    void UpdateQueue()
    {
        for (int i = 0; i < queue.Count; i++)
        {
            queue[i].MoveToQueue(queuePoints[i]);
        }
    }

    public bool IsCounterAvailable()
    {
        return queue.Count == 0;
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