using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach this to each swarm unit prefab.
/// Uses NavMeshAgent for movement — handles stairs and elevation natively.
/// Steers toward Attractors, away from Repulsors, with linger and dispel support.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SwarmAgent : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Movement")]
    [Tooltip("Maximum travel speed.")]
    public float maxSpeed = 5f;

    [Tooltip("How quickly the agent accelerates.")]
    public float acceleration = 8f;

    [Header("Detection")]
    [Tooltip("Agent only chases the player within this radius. Set to 0 to always chase.")]
    public float detectionRadius = 10f;

    [Header("Linger")]
    [Tooltip("How long the agent keeps chasing after the player leaves detection radius.")]
    public float lingerDuration = 3f;

    [Header("Repulsion")]
    [Tooltip("How strongly the agent is steered away from Repulsor prefabs.")]
    public float repulsionWeight = 2f;

    [Tooltip("Speed override when inside a repulsion radius. Set to 0 to use maxSpeed.")]
    public float repulsionSpeed = 8f;

    [Header("Dispel")]
    [Tooltip("VFX played once when the agent enters a dispel zone.")]
    public ParticleSystem dispelVFX;
    public Animator monsterAnimator;

    // ── state ──────────────────────────────────────────────────────────────
    private enum AgentState { Idle, Chasing, Lingering }
    private AgentState state = AgentState.Idle;
    private float lingerTimer = 0f;
    private SwarmAttractor playerAttractor;

    // ── dispel ─────────────────────────────────────────────────────────────
    private bool isDispelled = false;
    private Renderer agentRenderer;

    // ── internals ──────────────────────────────────────────────────────────
    private NavMeshAgent nav;

    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
        nav.speed = maxSpeed;
        nav.acceleration = acceleration;
        nav.angularSpeed = 360f;
        nav.autoBraking = false;
        agentRenderer = GetComponentInChildren<Renderer>();
        agentCollider = GetComponentInChildren<Collider>();

        monsterAnimator.Play(0, -1, Random.Range(0f, 1f));
        // Desync zombies sounds
        Invoke(nameof(WhisperSound), Random.Range(0f, 2f));
    }

    void Update()
    {
        UpdatePlayerAttractorRef();
        UpdateState();
        CheckDispelZones();

        if (isDispelled)
        {
            nav.ResetPath();
            nav.velocity = Vector3.zero;
            return;
        }

        if (state == AgentState.Idle)
        {
            nav.ResetPath();
            return;
        }

        // ── Compute and set destination ────────────────────────────────────
        nav.SetDestination(ComputeDestination());
    }

    // ── destination ────────────────────────────────────────────────────────

    Vector3 ComputeDestination()
    {
        Vector3 desired = transform.position;

        // ── 1. Attraction ──────────────────────────────────────────────────
        SwarmAttractor[] attractors = FindObjectsByType<SwarmAttractor>(FindObjectsSortMode.None);
        foreach (SwarmAttractor a in attractors)
        {
            Vector3 toTarget = a.transform.position - transform.position;
            float distToTarget = toTarget.magnitude;

            if (a.isPlayer)
            {
                bool inRange = detectionRadius <= 0f || distToTarget <= detectionRadius;
                if (!inRange && state != AgentState.Lingering) continue;
            }

            desired += toTarget.normalized;
        }

        // ── 2. Repulsion — set destination to a flee point away from repulsors ──
        bool insideRepulsion = false;
        Vector3 repulsionOffset = Vector3.zero;
        SwarmRepulsor[] repulsors = FindObjectsByType<SwarmRepulsor>(FindObjectsSortMode.None);
        foreach (SwarmRepulsor r in repulsors)
        {
            Vector3 away = transform.position - r.transform.position;
            float dist = away.magnitude;

            if (dist < r.repulsionRadius && dist > 0.001f)
            {
                float strength = (r.repulsionRadius - dist) / r.repulsionRadius;
                repulsionOffset += away.normalized * strength;
                insideRepulsion = true;
            }
        }

        if (insideRepulsion)
        {
            // Override destination entirely — flee to a point far behind the agent
            Vector3 fleeTarget = transform.position + repulsionOffset.normalized * repulsionWeight * 10f;

            // Clamp to valid NavMesh position so the agent doesn't ignore it
            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, repulsionWeight * 10f, NavMesh.AllAreas))
                desired = hit.position;
            else
                desired = transform.position + repulsionOffset.normalized * 2f; // fallback close point
        }

        // Switch speed based on whether we're being repulsed
        nav.speed = insideRepulsion && repulsionSpeed > 0f ? repulsionSpeed : maxSpeed;

        return desired;
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
                    state = AgentState.Chasing;
                }
                else
                {
                    lingerTimer -= Time.deltaTime;
                    if (lingerTimer <= 0f)
                        state = AgentState.Idle;
                }
                break;
        }
    }

    public void WhisperSound() {
        SoundManager.instance.PlaySound(audioSource);
    }

    // ── dispel ─────────────────────────────────────────────────────────────
    private Collider agentCollider;

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
            isDispelled = true;
            if (agentRenderer != null) agentRenderer.enabled = false;
            if (agentCollider != null) agentCollider.enabled = false;
            if (dispelVFX != null) dispelVFX.Play();
        }
        else if (!insideAnyDispelZone && isDispelled)
        {
            isDispelled = false;
            if (agentRenderer != null) agentRenderer.enabled = true;
            if (agentCollider != null) agentCollider.enabled = true;
        }
    }

    public bool IsDispelled => isDispelled;

    // ── public API ─────────────────────────────────────────────────────────

    /// <summary>Forces the agent into Idle — called on player death.</summary>
    public void ForceIdle()
    {
        monsterAnimator.SetBool("IdleMonster", true);
        state = AgentState.Idle;
        nav.ResetPath();
        nav.velocity = Vector3.zero;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (detectionRadius > 0f)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
#endif
}