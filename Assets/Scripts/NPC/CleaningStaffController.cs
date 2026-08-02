using UnityEngine;
using UnityEngine.AI;

public class CleaningStaffController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [SerializeField] private Transform waypointParent;

    private Transform[] patrolPoints;

    private int currentIndex;
    private bool isPatrolling;

    private void Awake()
    {
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

    public void StartPatrol()
    {
        if (agent == null)
            return;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("Cleaning Staff : Patrol Points belum diisi.");
            return;
        }

        agent.isStopped = false;

        isPatrolling = true;
        currentIndex = 0;

        agent.SetDestination(patrolPoints[currentIndex].position);
    }

    public void StopPatrol()
    {
        if (agent == null)
            return;

        agent.isStopped = true;
        isPatrolling = false;
    }

    private void Update()
    {
        if (isPatrolling)
        {
            if (!agent.pathPending && agent.remainingDistance <= 0.2f)
            {
                currentIndex++;

                if (currentIndex >= patrolPoints.Length)
                    currentIndex = 0;

                agent.SetDestination(patrolPoints[currentIndex].position);
            }
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}