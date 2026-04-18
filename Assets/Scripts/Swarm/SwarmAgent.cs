using UnityEngine;

/// <summary>
/// Attach this to each swarm unit prefab.
/// The agent steers toward all Attractors and away from all Repulsors in the scene,
/// while also doing basic flocking (separation from siblings).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SwarmAgent : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Maximum travel speed.")]
    public float maxSpeed = 5f;

    [Tooltip("How quickly the agent changes direction.")]
    public float steerForce = 3f;

    [Header("Detection")]
    [Tooltip("Agent only chases Attractors within this radius. Set to 0 to always chase.")]
    public float detectionRadius = 10f;

    [Header("Attraction")]
    [Tooltip("How strongly the agent is pulled toward Attractor prefabs.")]
    public float attractionWeight = 1f;

    [Header("Repulsion")]
    [Tooltip("How strongly the agent is pushed away from Repulsor prefabs.")]
    public float repulsionWeight = 2f;

    [Header("Separation (flocking)")]
    [Tooltip("Minimum desired distance from sibling agents.")]
    public float separationRadius = 1.5f;

    [Tooltip("How strongly the agent avoids its siblings.")]
    public float separationWeight = 1.5f;

    // ── internals ──────────────────────────────────────────────────────────
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void FixedUpdate()
    {
        Vector3 desired = Vector3.zero;

        // ── 1. Attraction (only within detection radius) ───────────────────
        SwarmAttractor[] attractors = FindObjectsByType<SwarmAttractor>(FindObjectsSortMode.None);
        foreach (SwarmAttractor a in attractors)
        {
            Vector3 toTarget = a.transform.position - transform.position;
            float distToTarget = toTarget.magnitude;

            bool inRange = detectionRadius <= 0f || distToTarget <= detectionRadius;
            if (!inRange) continue;

            desired += toTarget.normalized * attractionWeight;
        }

        // ── 2. Repulsion ───────────────────────────────────────────────────
        SwarmRepulsor[] repulsors = FindObjectsByType<SwarmRepulsor>(FindObjectsSortMode.None);
        foreach (SwarmRepulsor r in repulsors)
        {
            Vector3 away = transform.position - r.transform.position;
            float dist = away.magnitude;

            if (dist < r.repulsionRadius && dist > 0.001f)
            {
                // Stronger push the closer the repulsor
                float strength = (r.repulsionRadius - dist) / r.repulsionRadius;
                desired += away.normalized * strength * repulsionWeight;
            }
        }

        // ── 3. Separation (avoid crowding siblings) ────────────────────────
        SwarmAgent[] siblings = FindObjectsByType<SwarmAgent>(FindObjectsSortMode.None);
        foreach (SwarmAgent s in siblings)
        {
            if (s == this) continue;

            Vector3 away = transform.position - s.transform.position;
            float dist = away.magnitude;

            if (dist < separationRadius && dist > 0.001f)
            {
                float strength = (separationRadius - dist) / separationRadius;
                desired += away.normalized * strength * separationWeight;
            }
        }

        // ── 4. Steer ───────────────────────────────────────────────────────
        if (desired != Vector3.zero)
        {
            Vector3 targetVelocity = desired.normalized * maxSpeed;
            Vector3 steering = (targetVelocity - rb.linearVelocity) * steerForce;
            rb.AddForce(steering, ForceMode.Acceleration);

            // Clamp speed
            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // ── 5. Face movement direction ─────────────────────────────────────
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);

        // Agents should always be upright
        transform.up = Vector3.up;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (detectionRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        Gizmos.color = new Color(0f, 0.8f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
#endif
}