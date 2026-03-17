using UnityEngine;
using UnityEngine.AI;

public class LoopNavigation : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform targetPointA;
    public Transform targetPointB;
    private Transform currentTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentTarget = targetPointA;
        SetNewTarget(currentTarget);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentTarget = (currentTarget == targetPointA) ? targetPointB : targetPointA;
            SetNewTarget(currentTarget);
        }
    }

    void SetNewTarget(Transform target)
    {
        agent.SetDestination(target.position);
    }
}