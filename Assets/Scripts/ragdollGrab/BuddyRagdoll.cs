using UnityEngine;

/// <summary>
/// Place on the ROOT GameObject of the buddy prefab.
/// Manages ragdoll state and exposes weight for pressure plates.
/// </summary>
public class BuddyRagdoll : MonoBehaviour
{
    [Header("Weight")]
    [Tooltip("Weight value read by pressure plates in the world.")]
    public float weight = 1f;

    [Header("References")]
    [Tooltip("Assign the blood trail ParticleSystem here (placeholder).")]
    public TrailRenderer bloodTrail;

    public GameObject partThatMoves;

    // ── state ──────────────────────────────────────────────────────────────
    public bool IsHeld { get; private set; }

    private Rigidbody[] allBodies;
    private Animator animator;

    // ── cooldown to prevent instant re-grab after release ─────────────────
    private float releaseCooldown = 0f;
    private const float RELEASE_COOLDOWN_DURATION = 0.1f;
    public bool IsGrabbable => !IsHeld && releaseCooldown <= 0f;

    void Awake()
    {
        allBodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (releaseCooldown > 0f)
            releaseCooldown -= Time.deltaTime;
    }

    // ── public API ─────────────────────────────────────────────────────────

    /// <summary>Called by PlayerGrabber when the grab starts.</summary>
    public void OnGrabbed()
    {
        IsHeld = true;

        if (animator != null)
            animator.enabled = false;

        // Blood trail stub — enable particle system if assigned
        if (bloodTrail != null)
            //bloodTrail.Play();
            bloodTrail.emitting = true;
    }

    /// <summary>Called by PlayerGrabber when the player releases.</summary>
    public void OnReleased()
    {
        IsHeld = false;
        releaseCooldown = RELEASE_COOLDOWN_DURATION;

        // Activate full ragdoll
        SetRagdoll(true);

        // Zero out all velocities so the buddy doesn't inherit player momentum
        foreach (Rigidbody rb in allBodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Stop blood trail
        if (bloodTrail != null)
            bloodTrail.emitting = false;
    }

    /// <summary>Force-drop from external systems (death, cutscene, teleport, etc.).</summary>
    public void ForceDrop()
    {
        if (IsHeld)
            OnReleased();
    }

    // ── internals ──────────────────────────────────────────────────────────

    private void SetRagdoll(bool active)
    {
        foreach (Rigidbody rb in allBodies)
        {
            rb.isKinematic = !active;
        }
    }
}
