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

    [Header("Locomotion Variants")]
    [Tooltip("Daftar variasi gaya jalan (misal: NPC_Master, NPC_Texting, NPC_Phone, NPC_Runner)")]
    [SerializeField] private RuntimeAnimatorController[] locomotionVariants;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayAngryReaction()
    {
        if (animator != null)
        {
            animator.SetTrigger("Angry");
        }
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

        // Apply anomaly data synchronously to prevent 1-frame race conditions
        AnomalyPassenger anomaly = GetComponent<AnomalyPassenger>();
        if (anomaly != null)
        {
            anomaly.ApplyAnomalyData();
        }

        // Set variasi controller dan status curiga/gelisah
        bool isSuspicious = passengerData.isMonster || (passengerData.ticket != null && passengerData.ticket.status != TicketStatus.Valid);

        if (animator != null)
        {
            if (isSuspicious)
            {
                // Variasi Pembohong:
                // 50% Pembohong Panik: Lari kencang & di loket tolah-toleh nervous
                // 50% Pembohong Tenang: Menyamar sempurna dengan gaya jalan & idle acak (tanpa nervous tell)
                bool isPanicked = Random.value < 0.5f;

                if (isPanicked && locomotionVariants != null && locomotionVariants.Length >= 4)
                {
                    // Index 3 = NPC_Runner (Jalan = Lari kencang, Loket = Nervous)
                    animator.runtimeAnimatorController = locomotionVariants[3];
                    if (agent != null)
                        agent.speed = 4.2f;

                    animator.SetBool("IsSuspicious", true);
                }
                else
                {
                    // Pembohong Tenang: Pilih gaya acak agar pemain harus teliti cek tiket
                    if (locomotionVariants != null && locomotionVariants.Length > 0)
                    {
                        int randIdx = Random.Range(0, locomotionVariants.Length);
                        animator.runtimeAnimatorController = locomotionVariants[randIdx];
                        if (agent != null)
                        {
                            agent.speed = (randIdx == 3) ? 3.8f : 2.5f;
                        }
                    }

                    animator.SetBool("IsSuspicious", false);
                }
            }
            else
            {
                // Penumpang normal: pilih variasi acak (Normal, Telepon, Texting, Runner, LookAround)
                if (locomotionVariants != null && locomotionVariants.Length > 0)
                {
                    int randIdx = Random.Range(0, locomotionVariants.Length);
                    animator.runtimeAnimatorController = locomotionVariants[randIdx];
                    if (agent != null)
                    {
                        agent.speed = (randIdx == 3) ? 3.8f : 2.5f;
                    }
                }

                animator.SetBool("IsSuspicious", false);
            }
        }

        Debug.Log(
            $"Passenger : {passengerData.passengerName} | " +
            $"{passengerData.ticket.originStation} -> " +
            $"{passengerData.ticket.destinationStation} | Suspicious: {isSuspicious}"
        );
    }

    private void Update()
    {
        // 1. Update Speed parameter untuk Animator
        if (animator != null)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh && !agent.isStopped && State != NPCState.AtCounter && State != NPCState.Idle)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
        }

        // 2. Saat di antrean (InQueue) dan sudah sampai titik antrean -> Stop jalan
        if (State == NPCState.InQueue && HasReachedDestination())
        {
            if (agent != null && agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
                if (animator != null)
                    animator.SetFloat("Speed", 0f);
            }
        }

        // 3. Saat jalan ke loket (WalkingToCounter) dan sudah sampai -> Stop & Mulai Layani
        if (State == NPCState.WalkingToCounter && HasReachedDestination())
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }

            if (!CounterManager.Instance.IsOccupied() || CounterManager.Instance.GetCurrentNPC() == this)
            {
                if (CounterManager.Instance.TryOccupy(this))
                {
                    SetState(NPCState.AtCounter);
                    canBeServed = true;
                }
            }
        }

        // 4. Saat jalan ke pintu keluar (WalkingToExit)
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
        if (target == null)
            yield break;

        float timeout = 1f;
        while ((agent == null || !agent.isOnNavMesh) && timeout > 0)
        {
            if (agent != null && !agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (agent != null && agent.isOnNavMesh)
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
        Debug.Log($"MoveToCounter -> {name}");

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

    public bool HasReachedDestination()
    {
        if (agent == null || !agent.isOnNavMesh || !agent.enabled)
            return false;

        if (agent.pathPending)
            return false;

        if (!agent.hasPath)
            return false;

        return agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.6f);
    }

    public void PlayLookAround()
    {
        SetState(NPCState.LookingAround);

        animator.SetTrigger("LookAround");
    }
}