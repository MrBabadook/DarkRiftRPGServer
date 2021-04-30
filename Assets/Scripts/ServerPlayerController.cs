using UnityEngine;
using UnityEngine.AI;
public class ServerPlayerController : MonoBehaviour
{
    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void UpdateNavTarget(Vector3 target)
    {
        agent.SetDestination(target);
    }
}
