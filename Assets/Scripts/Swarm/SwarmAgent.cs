using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// Attach this to each swarm unit prefab.
/// The agent steers toward all Attractors and away from all Repulsors in the scene,
/// while also doing basic flocking (separation from siblings).
/// Supports a linger state: if the player leaves detection radius, the agent keeps
/// chasing for lingerDuration seconds before going idle.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SwarmAgent : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource ;

    [Header("Movement")]
    [Tooltip("Maximum travel speed.")]
    public float maxSpeed = 5f;

    [Tooltip("How quickly the agent changes direction.")]
    public float steerForce = 3f;

    [Header("Detection")]
    [Tooltip("Agent only chases Attractors within this radius. Set to 0 to always chase.")]
    public float detectionRadius = 10f;

    [Header("Linger")]
    [Tooltip("How long the agent keeps chasing the player after leaving the detection radius.")]
    public float lingerDuration = 3f;

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

    private enum AgentState { Idle, Chasing, Lingering }
    private AgentState state = AgentState.Idle;
    private float lingerTimer = 0f;
    private SwarmAttractor playerAttractor;

    // ── dispel ─────────────────────────────────────────────────────────────
    private bool isDispelled = false;
    private Renderer agentRenderer;

    [Header("Dispel")]
    [Tooltip("VFX played once when the agent enters a dispel zone.")]
    public ParticleSystem dispelVFX;

    // ── internals ──────────────────────────────────────────────────────────
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        agentRenderer = GetComponentInChildren<Renderer>();
        // Desync zombies sounds

        Invoke("WhisperSound", Random.Range(0f, 2f));
    }

    void FixedUpdate()
    {
        UpdatePlayerAttractorRef();
        UpdateState();
        CheckDispelZones();

        // Skip all steering while dispelled
        if (isDispelled)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 desired = Vector3.zero;

        // ── 1. Attraction ──────────────────────────────────────────────────
        if (state != AgentState.Idle)
        {
            SwarmAttractor[] attractors = FindObjectsByType<SwarmAttractor>(FindObjectsSortMode.None);
            foreach (SwarmAttractor a in attractors)
            {
                // Non-player attractors always apply regardless of state
                if (a.isPlayer && state == AgentState.Idle) continue;

                Vector3 toTarget = a.transform.position - transform.position;
                float distToTarget = toTarget.magnitude;

                // Detection radius only applies to the player attractor
                if (a.isPlayer)
                {
                    bool inRange = detectionRadius <= 0f || distToTarget <= detectionRadius;
                    if (!inRange && state != AgentState.Lingering) continue;
                }

                desired += toTarget.normalized * attractionWeight;
            }
        }

        // ── 2. Repulsion ───────────────────────────────────────────────────
        SwarmRepulsor[] repulsors = FindObjectsByType<SwarmRepulsor>(FindObjectsSortMode.None);
        foreach (SwarmRepulsor r in repulsors)
        {
            Vector3 away = transform.position - r.transform.position;
            float dist = away.magnitude;

            if (dist < r.repulsionRadius && dist > 0.001f)
            {
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
        if (state == AgentState.Idle)
        {
            // Bleed off velocity smoothly
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * steerForce);
        }
        else if (desired != Vector3.zero)
        {
            Vector3 targetVelocity = desired.normalized * maxSpeed;
            Vector3 steering = (targetVelocity - rb.linearVelocity) * steerForce;
            rb.AddForce(steering, ForceMode.Acceleration);

            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // ── 5. Face movement direction ─────────────────────────────────────
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }

    /// <summary>Forces the agent into Idle state — called on player death.</summary>
    public void ForceIdle()
    {
        state = AgentState.Idle;
        rb.linearVelocity = Vector3.zero;
    }

    // ── state machine ──────────────────────────────────────────────────────

    void UpdatePlayerAttractorRef()
    {
        if (playerAttractor != null) return;

        SwarmAttractor[] attractors = FindObjectsByType<SwarmAttractor>(FindObjectsSortMode.None);
        foreach (SwarmAttractor a in attractors)
        {
            if (a.isPlayer) { playerAttractor = a; break; }
        }
    }

    void UpdateState()
    {
        if (playerAttractor == null)
        {
            state = AgentState.Idle;
            return;
        }

        float distToPlayer = Vector3.Distance(transform.position, playerAttractor.transform.position);
        bool playerInRange = detectionRadius <= 0f || distToPlayer <= detectionRadius;

        switch (state)
        {
            case AgentState.Idle:
                if (playerInRange)
                    state = AgentState.Chasing;
                break;

            case AgentState.Chasing:
                if (!playerInRange)
                {
                    state = AgentState.Lingering;
                    lingerTimer = lingerDuration;
                }
                break;

            case AgentState.Lingering:
                if (playerInRange)
                {
                    // Player came back into range — resume chasing
                    state = AgentState.Chasing;
                }
                else
                {
                    lingerTimer -= Time.fixedDeltaTime;
                    if (lingerTimer <= 0f)
                        state = AgentState.Idle;
                }
                break;
        }
    }
    public void WhisperSound()
    {
        SoundManager.instance.PlaySound(audioSource);
    }

    // ── dispel ─────────────────────────────────────────────────────────────

    void CheckDispelZones()
    {
        SwarmRepulsor[] repulsors = FindObjectsByType<SwarmRepulsor>(FindObjectsSortMode.None);
        bool insideAnyDispelZone = false;

        foreach (SwarmRepulsor r in repulsors)
        {
            float dist = Vector3.Distance(transform.position, r.transform.position);
            if (dist < r.dispelRadius)
            {
                insideAnyDispelZone = true;
                break;
            }
        }

        if (insideAnyDispelZone && !isDispelled)
        {
            // Entering dispel zone
            isDispelled = true;
            if (agentRenderer != null) agentRenderer.enabled = false;
            if (dispelVFX != null) dispelVFX.Play();
        }
        else if (!insideAnyDispelZone && isDispelled)
        {
            // Exiting dispel zone
            isDispelled = false;
            if (agentRenderer != null) agentRenderer.enabled = true;
        }
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