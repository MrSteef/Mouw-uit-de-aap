using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WaypointPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTime = 1f;

    private NavMeshAgent agent;
    private int waypointIndex;
    private float waitTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (waypoints.Length > 0)
            MoveToCurrentWaypoint();
    }

    private void Update()
    {
        if (waypoints.Length == 0 || agent.pathPending)
            return;

        bool reachedWaypoint =
            agent.remainingDistance <= agent.stoppingDistance &&
            (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);

        if (!reachedWaypoint)
            return;

        waitTimer += Time.deltaTime;

        if (waitTimer >= waitTime)
        {
            waitTimer = 0f;
            SelectNextWaypoint();
        }
    }

    private void SelectNextWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >= waypoints.Length)
        {
            if (!loop)
            {
                agent.ResetPath();
                enabled = false;
                return;
            }

            waypointIndex = 0;
        }

        MoveToCurrentWaypoint();
    }

    private void MoveToCurrentWaypoint()
    {
        Transform waypoint = waypoints[waypointIndex];

        if (waypoint != null)
            agent.SetDestination(waypoint.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null)
            return;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.25f);

            int next = i + 1;

            if (next < waypoints.Length && waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            else if (loop && waypoints.Length > 1 && waypoints[0] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
        }
    }
}