
using UnityEngine.AI;

namespace Utils
{
    public static class NavMeshUtilities
    {
        public static bool IsAtTargetLocation(NavMeshAgent agent)
        {
            return (!agent.hasPath && !agent.pathPending) || (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + agent.radius);
        }
    }

}