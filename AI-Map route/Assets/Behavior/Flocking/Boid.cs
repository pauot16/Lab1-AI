using UnityEngine;

/// <summary>
/// One 3D boid. Implements separation, alignment and cohesion.
/// Attach this script to the visible boid prefab.
/// </summary>
public class Boid : MonoBehaviour
{
    private FlockManager flock;
    private Vector3 velocity;

    public Vector3 Velocity => velocity;

    public void Initialize(FlockManager manager, Vector3 initialVelocity)
    {
        flock = manager;
        velocity = initialVelocity;

        if (velocity.sqrMagnitude < 0.001f)
            velocity = transform.forward * flock.minSpeed;

        RotateToVelocity();
    }

    private void Update()
    {
        if (flock == null) return;

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 centreOfNeighbours = Vector3.zero;
        int neighbourCount = 0;
        int separationCount = 0;

        float neighbourRadiusSqr = flock.neighbourRadius * flock.neighbourRadius;
        float avoidanceRadiusSqr = flock.avoidanceRadius * flock.avoidanceRadius;

        // Examine the other boids and gather neighbour information.
        foreach (Boid other in flock.Boids)
        {
            if (other == this) continue;

            Vector3 offset = other.transform.position - transform.position;
            float distanceSqr = offset.sqrMagnitude;

            if (distanceSqr <= 0.0001f || distanceSqr > neighbourRadiusSqr)
                continue;

            neighbourCount++;
            centreOfNeighbours += other.transform.position;
            alignment += other.velocity;

            // Push away from nearby boids. Closer boids have more influence.
            if (distanceSqr < avoidanceRadiusSqr)
            {
                separation -= offset / distanceSqr;
                separationCount++;
            }
        }

        Vector3 steering = Vector3.zero;

        if (separationCount > 0)
            steering += SteerTowards(separation.normalized) * flock.separationWeight;

        if (neighbourCount > 0)
        {
            Vector3 averageVelocity = alignment / neighbourCount;
            if (averageVelocity.sqrMagnitude > 0.001f)
                steering += SteerTowards(averageVelocity.normalized) * flock.alignmentWeight;

            Vector3 centre = centreOfNeighbours / neighbourCount;
            Vector3 directionToCentre = centre - transform.position;
            if (directionToCentre.sqrMagnitude > 0.001f)
                steering += SteerTowards(directionToCentre.normalized) * flock.cohesionWeight;
        }

        steering += SteerTowards(flock.GetAreaSteering(transform.position)) * flock.stayInAreaWeight;

        velocity += steering * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, flock.maxSpeed);

        if (velocity.magnitude < flock.minSpeed)
        {
            Vector3 direction = velocity.sqrMagnitude > 0.001f
                ? velocity.normalized
                : transform.forward;
            velocity = direction * flock.minSpeed;
        }

        transform.position += velocity * Time.deltaTime;
        transform.position = flock.KeepInsideArea(transform.position);
        RotateToVelocity();
    }

    private Vector3 SteerTowards(Vector3 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude < 0.001f) return Vector3.zero;

        Vector3 desiredVelocity = desiredDirection.normalized * flock.maxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity - velocity, flock.maxSteerForce);
    }

    private void RotateToVelocity()
    {
        if (velocity.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
    }
}
