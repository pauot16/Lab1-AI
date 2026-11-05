using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates and updates a 3D flock of boids.
/// Attach this script to an empty GameObject called "FlockManager".
/// </summary>
public class FlockManager : MonoBehaviour
{
    [Header("Setup")]
    public Boid boidPrefab;
    [Min(1)] public int boidCount = 40;
    public Vector3 areaSize = new Vector3(30f, 15f, 30f);

    [Header("Movement")]
    public float minSpeed = 5f;
    public float maxSpeed = 10f;
    public float neighbourRadius = 5f;
    public float avoidanceRadius = 5f;
    public float maxSteerForce = 3f;

    [Header("Behaviour weights")]
    public float separationWeight = 1.5f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float stayInAreaWeight = 1.5f;

    [Header("Area")]
    public bool wrapAround = true;
    public bool drawAreaGizmo = true;

    private readonly List<Boid> boids = new List<Boid>();

    public IReadOnlyList<Boid> Boids => boids;

    private void Start()
    {
        if (boidPrefab == null)
        {
            Debug.LogError("FlockManager: Assign a Boid prefab in the Inspector.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < boidCount; i++)
        {
            Vector3 position = transform.position + Random.insideUnitSphere * 0.5f;
            position = KeepInsideArea(position);
            Boid boid = Instantiate(boidPrefab, position, Random.rotation, transform);
            boid.Initialize(this, Random.onUnitSphere * Random.Range(minSpeed, maxSpeed));
            boids.Add(boid);
        }
    }

    public Vector3 KeepInsideArea(Vector3 worldPosition)
    {
        Vector3 local = worldPosition - transform.position;
        Vector3 half = areaSize * 0.5f;

        if (wrapAround)
        {
            local.x = Wrap(local.x, -half.x, half.x);
            local.y = Wrap(local.y, -half.y, half.y);
            local.z = Wrap(local.z, -half.z, half.z);
        }
        else
        {
            local.x = Mathf.Clamp(local.x, -half.x, half.x);
            local.y = Mathf.Clamp(local.y, -half.y, half.y);
            local.z = Mathf.Clamp(local.z, -half.z, half.z);
        }

        return transform.position + local;
    }

    private float Wrap(float value, float min, float max)
    {
        float length = max - min;
        if (length <= 0f) return 0f;

        if (value < min) value += length;
        else if (value > max) value -= length;

        return value;
    }

    public Vector3 GetAreaSteering(Vector3 position)
    {
        if (wrapAround) return Vector3.zero;

        Vector3 local = position - transform.position;
        Vector3 half = areaSize * 0.5f;
        Vector3 steer = Vector3.zero;

        if (local.x < -half.x * 0.8f) steer.x += 1f;
        if (local.x >  half.x * 0.8f) steer.x -= 1f;
        if (local.y < -half.y * 0.8f) steer.y += 1f;
        if (local.y >  half.y * 0.8f) steer.y -= 1f;
        if (local.z < -half.z * 0.8f) steer.z += 1f;
        if (local.z >  half.z * 0.8f) steer.z -= 1f;

        return steer.sqrMagnitude > 0f ? steer.normalized : Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawAreaGizmo) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
