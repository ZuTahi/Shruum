using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 target)
    {
        if (agent.isActiveAndEnabled)
            agent.SetDestination(target);
    }

    public void Stop()
    {
        if (agent.isActiveAndEnabled)
            agent.ResetPath();
    }
}
