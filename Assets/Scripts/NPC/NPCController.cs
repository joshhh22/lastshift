using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Counter")]
    public bool isBeingServed = false;
    public bool canBeServed = false;

    public NPCState State { get; private set; }

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);

        if (State == NPCState.WalkingToCounter && HasReachedDestination())
        {
            agent.isStopped = true;

            if (CounterManager.Instance.TryOccupy(this))
            {
                SetState(NPCState.AtCounter);

                canBeServed = true;

                animator.SetFloat("Speed", 0);
            }
        }

        if (State == NPCState.WalkingToExit)
        {
            Debug.Log(
                "Remaining = " + agent.remainingDistance +
                " | Stopped = " + agent.isStopped +
                " | Pending = " + agent.pathPending +
                " | HasPath = " + agent.hasPath
            );
        }

        if (State == NPCState.WalkingToExit && HasReachedDestination())
        {
            SetState(NPCState.Exited);

            Debug.Log(name + " keluar.");

            Destroy(gameObject);
        }
    }

    public void SetState(NPCState newState)
    {
        State = newState;

        Debug.Log($"{name} -> {State}");
    }

    // ===========================
    // MENUJU QUEUE
    // ===========================

    public void MoveToQueue(Transform target)
    {
        SetState(NPCState.InQueue);

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    // ===========================
    // MENUJU COUNTER
    // ===========================

    public void MoveToCounter(Transform target)
    {
        Debug.Log(
            $"MoveToCounter -> {name}\n" +
            System.Environment.StackTrace);

        SetState(NPCState.WalkingToCounter);

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    // ===========================
    // MENUJU EXIT
    // ===========================

    public void MoveToExit()
    {
        Debug.Log(name + " mulai jalan ke Exit");
        Debug.Log("=== MoveToExit ===");

        SetState(NPCState.WalkingToExit);

        agent.isStopped = false;

        agent.SetDestination(CounterManager.Instance.ExitPoint.position);

        Debug.Log("Destination = " + CounterManager.Instance.ExitPoint.position);
        Debug.Log("Path Status = " + agent.pathStatus);
        Debug.Log("Has Path = " + agent.hasPath);
    }

    public void StopMoving()
    {
        agent.isStopped = true;

        SetState(NPCState.Idle);
    }

    public void Serve()
    {
        if (!canBeServed)
            return;

        canBeServed = false;
        isBeingServed = true;

        SetState(NPCState.BeingServed);

        Debug.Log(name + " sedang dilayani");

        StartCoroutine(ServeRoutine());
    }

    IEnumerator ServeRoutine()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("ServeRoutine selesai");

        isBeingServed = false;

        // Kosongkan counter
        CounterManager.Instance.ReleaseCounter();

        Debug.Log("Memanggil MoveFrontToCounter");

        // NPC paling depan di queue maju ke counter
        QueueManager.Instance.MoveFrontToCounter();

        // Buka gate
        GateManager.Instance.OpenGate();

        // NPC sekarang menuju exit
        MoveToExit();
    }

    private IEnumerator MoveQueueDelayed()
    {
        yield return new WaitForSeconds(0.3f);

    }

    public bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public void PlayLookAround()
    {
        SetState(NPCState.LookingAround);

        animator.SetTrigger("LookAround");
    }
}