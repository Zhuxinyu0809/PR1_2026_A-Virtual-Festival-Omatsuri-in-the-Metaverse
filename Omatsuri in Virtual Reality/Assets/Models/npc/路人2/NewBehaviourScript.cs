using UnityEngine;
using UnityEngine.AI;

public class LoopPatrol : MonoBehaviour
{
    public Transform[] points;
    private NavMeshAgent agent;
    private int currentPointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(points[currentPointIndex].position);
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % points.Length;
            agent.SetDestination(points[currentPointIndex].position);
        }
    }
}