using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Bot : MonoBehaviour
{
    public enum Mode { Seek, Flee }

    public GameObject target;
    public Mode mode = Mode.Seek;

    [Header("Distancias")]
    public float detectionRadius = 20f; // radio de detección del objetivo
    public float safeDistance = 8f;   // solo afecta al modo Auto
    public float stopDistance = 1.5f; // distancia a la que se detiene al acercarse al objetivo
    public float fleeDistance = 10f;  // cuánto huye en cada destino

    [Header("Ruta")]
    public Transform[] waypoints;       // puntos de la ruta, en orden
    public float waypointTolerance = 0.5f;

    NavMeshAgent agent;
    int waypointIndex = 0;
    bool patrolling = false;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (target == null)
        {
            Patrol();
            return;
        }

        Vector3 targetPos = target.transform.position;
        float distance = Vector3.Distance(transform.position, targetPos);

        // Objetivo fuera de alcance: sigue la ruta
        if (distance > detectionRadius)
        {
            Patrol();
            return;
        }

        patrolling = false;

        switch (mode)
        {
            case Mode.Seek:
                Seek(targetPos, distance);
                break;
            case Mode.Flee:
                Flee(targetPos);
                break;
        }
    }
    void Seek(Vector3 location, float distance)
    {
        if (distance <= stopDistance)
        {
            agent.ResetPath();
            return;
        }

        // El destino es un punto justo antes del objetivo, no el objetivo en sí
        Vector3 dirToTarget = (location - transform.position).normalized;
        Vector3 destination = location - dirToTarget * stopDistance;
        agent.SetDestination(location);
    }

    void Flee(Vector3 location)
    {
        Vector3 fleeDirection = (transform.position - location).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        // Ajusta el destino al NavMesh para no huir hacia una pared o fuera del mapa
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            agent.ResetPath();
            return;
        }

        // Al volver a la ruta, vuelve al el waypoint en el que iba
        if (!patrolling)
        {
            patrolling = true;
            agent.SetDestination(waypoints[waypointIndex].position);
            return;
        }

        // Al llegar al waypoint, pasa al siguiente
        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[waypointIndex].position);
        }
    }

    // Dibuja los radios en la Scene para ajustarlos visualmente
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            Transform next = waypoints[(i + 1) % waypoints.Length];
            if (next != null) Gizmos.DrawLine(waypoints[i].position, next.position);
        }
    }
}



