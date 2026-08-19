using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CleaningStaffController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform waypointParent;

    [Header("Patrol & Wait Settings")]
    [SerializeField] private float minWaitTime = 3f;
    [SerializeField] private float maxWaitTime = 6f;

    private List<Transform> allPatrolPoints = new List<Transform>();
    private List<Transform> restrictedPatrolPoints = new List<Transform>();

    public bool HasTalked { get; private set; } = false;

    private int currentIndex;
    private bool isPatrolling;
    private bool isWaitingAtWaypoint;
    private Coroutine waitCoroutine;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        InitializeWaypoints();
    }

    private void Start()
    {
        StartCoroutine(InitializePatrolOnStart());
    }

    private System.Collections.IEnumerator InitializePatrolOnStart()
    {
        yield return null; // Tunggu 1 frame agar ObjectiveManager selesai inisialisasi
        CheckAndResumePatrol();
    }

    public void CheckAndResumePatrol()
    {
        if (ObjectiveManager.Instance != null)
        {
            int currentIdx = ObjectiveManager.Instance.GetCurrentIndex();
            var objectives = ObjectiveManager.Instance.GetObjectives();
            int talkIndex = -1;
            if (objectives != null)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    string t = objectives[i].title.ToLower();
                    if (t.Contains("cleaning") || t.Contains("staff"))
                    {
                        talkIndex = i;
                        break;
                    }
                }
            }

            if (talkIndex != -1 && currentIdx > talkIndex)
            {
                UnlockFullPatrol();
            }
        }

        StartPatrol();
    }

    private void InitializeWaypoints()
    {
        allPatrolPoints.Clear();
        restrictedPatrolPoints.Clear();

        if (waypointParent != null)
        {
            for (int i = 0; i < waypointParent.childCount; i++)
            {
                Transform wp = waypointParent.GetChild(i);
                allPatrolPoints.Add(wp);

                string wpName = wp.name.ToLower().Trim();
                // Filter waypoint restricted: Coffee, Counter, Stair, 1, 2
                if (wpName == "coffee" || wpName.Contains("coffee") ||
                    wpName == "counter" || wpName.Contains("counter") ||
                    wpName == "stair" || wpName.Contains("stair") ||
                    wpName == "1" || wpName == "2")
                {
                    restrictedPatrolPoints.Add(wp);
                }
            }
        }

        if (restrictedPatrolPoints.Count == 0 && allPatrolPoints.Count > 0)
        {
            restrictedPatrolPoints.AddRange(allPatrolPoints);
        }

        Debug.Log($"<color=cyan>[CleaningStaff]</color> Waypoints Loaded: Total = {allPatrolPoints.Count}, Restricted (Pre-talk) = {restrictedPatrolPoints.Count}");
    }

    public void UnlockFullPatrol()
    {
        HasTalked = true;
        Debug.Log("<color=green>[CleaningStaff]</color> Full Patrol Unlocked! Cleaning Staff sekarang bebas menjelajah ke seluruh stasiun.");
    }

    public void ResetToInitialSpawn()
    {
        StopPatrol();

        HasTalked = false;
        currentIndex = 0;

        if (agent != null)
        {
            agent.Warp(initialPosition);
        }
        else
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
        }

        transform.rotation = initialRotation;

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetInteger("IdleType", 0);
        }
    }

    private List<Transform> GetCurrentPatrolPool()
    {
        // Sebelum bicara -> HANYA Coffee, Counter, Stair, 1, 2
        // Setelah bicara -> BEBAS ke semua waypoint (Coffee, Counter, Stair, 1-7, dll)
        if (!HasTalked && restrictedPatrolPoints.Count > 0)
        {
            return restrictedPatrolPoints;
        }

        return allPatrolPoints.Count > 0 ? allPatrolPoints : restrictedPatrolPoints;
    }

    public void StartPatrol()
    {
        if (agent == null)
            return;

        var pool = GetCurrentPatrolPool();
        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning("Cleaning Staff : Patrol Points belum diisi.");
            return;
        }

        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        isWaitingAtWaypoint = false;
        isPatrolling = true;
        agent.isStopped = false;

        if (currentIndex >= pool.Count)
            currentIndex = 0;

        agent.SetDestination(pool[currentIndex].position);
    }

    public void StopPatrol()
    {
        if (waitCoroutine != null)
        {
            StopCoroutine(waitCoroutine);
            waitCoroutine = null;
        }

        isWaitingAtWaypoint = false;
        isPatrolling = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetInteger("IdleType", 0);
        }
    }

    public void FacePlayer(Transform player)
    {
        if (player == null) return;

        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        transform.LookAt(targetPosition);
    }

    private void Update()
    {
        if (isPatrolling && !isWaitingAtWaypoint && agent != null && agent.isOnNavMesh)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.35f)
            {
                waitCoroutine = StartCoroutine(WaitAtWaypointRoutine());
            }
        }

        if (animator != null && agent != null && agent.isOnNavMesh)
        {
            float currentSpeed = isWaitingAtWaypoint ? 0f : agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    private System.Collections.IEnumerator WaitAtWaypointRoutine()
    {
        isWaitingAtWaypoint = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);

            // Pilih animasi idle secara acak:
            // 0 = Idle biasa, 1 = Old Man Idle, 2 = Arm Stretching
            int randomIdle = Random.Range(0, 3);
            animator.SetInteger("IdleType", randomIdle);
        }

        // Tunggu di waypoint selama beberapa detik
        float waitDuration = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitDuration);

        var pool = GetCurrentPatrolPool();
        if (pool != null && pool.Count > 0)
        {
            if (HasTalked)
            {
                // Setelah bicara: bisa pilih waypoint secara acak atau lanjut
                currentIndex = Random.Range(0, pool.Count);
            }
            else
            {
                // Sebelum bicara: berputar teratur di antara (Coffee, Counter, Stair, 1, 2)
                currentIndex++;
                if (currentIndex >= pool.Count)
                    currentIndex = 0;
            }

            if (pool[currentIndex] != null && agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(pool[currentIndex].position);
            }
        }

        isWaitingAtWaypoint = false;
        waitCoroutine = null;
    }
}