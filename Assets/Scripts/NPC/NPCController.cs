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

    [Header("Passenger Data")]
    public PassengerData passengerData = new PassengerData();

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void InitializePassenger()
    {
        passengerData = TicketGenerator.GeneratePassenger();

        NPCIdentity identity = GetComponent<NPCIdentity>();

        if (identity != null)
        {
            string cleanName = identity.PassengerName;
            
            // Hapus akhiran acak "_123" yang dibuat oleh tool rename jika ada
            int underscoreIndex = cleanName.IndexOf('_');
            if (underscoreIndex > 0)
            {
                cleanName = cleanName.Substring(0, underscoreIndex);
            }

            passengerData.passengerName = cleanName;
            passengerData.gender = identity.Gender;
        }

        Debug.Log(
            $"Passenger : {passengerData.passengerName} | " +
            $"{passengerData.ticket.originStation} -> " +
            $"{passengerData.ticket.destinationStation}"
        );
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
        StartCoroutine(MoveWhenReady(target));
    }

    private IEnumerator MoveWhenReady(Transform target)
    {
        // Tunggu maksimal 1 detik atau sampai agent berhasil berpijak di atas NavMesh
        float timeout = 1f;
        while (!agent.isOnNavMesh && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }
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
        StartCoroutine(MoveWhenReady(target));
    }

    // ===========================
    // MENUJU EXIT
    // ===========================

    public void MoveToExit(Transform target)
    {
        Debug.Log(name + " mulai jalan ke Exit");
        Debug.Log("=== MoveToExit ===");

        SetState(NPCState.WalkingToExit);
        StartCoroutine(MoveWhenReady(target));
        
        Debug.Log("Destination (via Coroutine) = " + target.position);
        Debug.Log("Path Status = " + agent.pathStatus);
        Debug.Log("Has Path = " + agent.hasPath);
    }

    public void StopMoving()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

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

        ObjectiveManager.Instance.AddProgress();

        isBeingServed = false;

        // Kosongkan counter
        CounterManager.Instance.ReleaseCounter();

        Debug.Log("Memanggil MoveFrontToCounter");

        // NPC paling depan di queue maju ke counter
        QueueManager.Instance.MoveFrontToCounter();

        // Buka gate
        GateManager.Instance.OpenGate();

        // NPC sekarang menuju exit
        MoveToExit(
            CounterManager.Instance.PlatformExitPoint);

        
    }

    public void Reject()
    {
        if (!canBeServed)
            return;

        canBeServed = false;
        isBeingServed = true;

        SetState(NPCState.BeingServed);

        Debug.Log(name + " ditolak");

        StartCoroutine(RejectRoutine());
    }

    private IEnumerator RejectRoutine()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("RejectRoutine selesai");

        ObjectiveManager.Instance.AddProgress();

        CounterManager.Instance.ReleaseCounter();

        QueueManager.Instance.MoveFrontToCounter();

        MoveToExit(
            CounterManager.Instance.LobbyExitPoint);
    }

    private IEnumerator MoveQueueDelayed()
    {
        yield return new WaitForSeconds(0.3f);

    }

    public bool HasReachedDestination()
    {
        if (!agent.isOnNavMesh)
            return false;

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