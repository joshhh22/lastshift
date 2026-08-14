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

    private Transform[] patrolPoints;

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

        if (waypointParent != null)
        {
            patrolPoints = new Transform[waypointParent.childCount];

            for (int i = 0; i < waypointParent.childCount; i++)
            {
                patrolPoints[i] = waypointParent.GetChild(i);
            }
        }
    }

    public void ResetToInitialSpawn()
    {
        StopPatrol();

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

    public void StartPatrol()
    {
        if (agent == null)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
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

        agent.SetDestination(patrolPoints[currentIndex].position);
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
            if (!agent.pathPending && agent.remainingDistance <= 0.3f)
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

        // Lanjut ke waypoint berikutnya
        currentIndex++;
        if (currentIndex >= patrolPoints.Length)
            currentIndex = 0;

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[currentIndex] != null)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(patrolPoints[currentIndex].position);
            }
        }

        isWaitingAtWaypoint = false;
        waitCoroutine = null;
    }
}