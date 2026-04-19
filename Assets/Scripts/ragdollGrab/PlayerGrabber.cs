using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerGrabber : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("How far in front of the player the SphereCast reaches.")]
    public float castDistance = 2.5f;

    [Tooltip("Radius of the SphereCast.")]
    public float castRadius = 0.6f;

    [Tooltip("Layer mask for the SphereCast — set to your buddy/ragdoll layer.")]
    public LayerMask grabLayer;

    public LayerMask notGrabbedLayer;

    public LayerMask grabbedLayer;

    [Header("References")]
    [Tooltip("The GripProxy component (kinematic hand follower).")]
    public GripProxy gripProxy;

    [Header("Joint Settings")]
    [Tooltip("How strongly the joint tries to maintain position. Higher = stiffer carry.")]
    public float jointPositionSpring = 3000f;

    [Tooltip("Damping on the joint drive. Higher = less oscillation.")]
    public float jointPositionDamper = 100f;

    [Tooltip("Max force the joint can exert. Lower = buddy slips if too far.")]
    public float jointMaxForce = 5000f;

    [Tooltip("If the anchor bone drifts beyond this distance from the proxy, auto-drop.")]
    public float maxCarryDistance = 2f;

    // ── state ──────────────────────────────────────────────────────────────
    private BuddyRagdoll heldBuddy;
    private BuddyAnchor heldAnchor;
    private ConfigurableJoint activeJoint;
    private BuddyRagdoll detectedBuddy;

    // ── input ──────────────────────────────────────────────────────────────
    private InputSystem_Actions playerControls;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerGrabber: no PlayerController found on this GameObject.");
            return;
        }

        playerControls = playerController.playerControls;
        playerControls.Player.Interact.performed += OnInteract;
    }

    void OnDestroy()
    {
        if (playerControls != null)
            playerControls.Player.Interact.performed -= OnInteract;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (heldBuddy == null)
            TryGrab();
        else
            Release();

        playerController.isCarryingSomething = heldBuddy != null;
    }

    void Update()
    {
        if (heldBuddy == null)
            DetectBuddy();
        else
            CheckAutoDropDistance();


    }

    // ── detection ─────────────────────────────────────────────────────────

    void DetectBuddy()
    {
        detectedBuddy = null;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, castRadius, castDistance, grabLayer);

        float closestDist = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            // Walk up to find BuddyRagdoll on the root
            BuddyRagdoll buddy = hit.collider.GetComponentInParent<BuddyRagdoll>();
            if (buddy == null || !buddy.IsGrabbable) continue;

            float dist = Vector3.Distance(transform.position, hit.point);
            if (dist < closestDist)
            {
                closestDist = dist;
                detectedBuddy = buddy;
            }
        }
    }

    // ── grab ───────────────────────────────────────────────────────────────

    void TryGrab()
    {
        if (detectedBuddy == null) return;

        // Find the closest anchor to the player's hand
        BuddyAnchor[] anchors = detectedBuddy.GetComponentsInChildren<BuddyAnchor>();
        if (anchors.Length == 0) return;

        BuddyAnchor closest = null;
        float closestDist = float.MaxValue;
        Vector3 handPos = gripProxy.transform.position;

        foreach (BuddyAnchor anchor in anchors)
        {
            float dist = Vector3.Distance(anchor.transform.position, handPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = anchor;
            }
        }

        if (closest == null) return;

        heldBuddy = detectedBuddy;
        heldAnchor = closest;

        // Wake all rigidbodies in case they were sleeping
        foreach (Rigidbody rb in heldBuddy.GetComponentsInChildren<Rigidbody>())
            rb.WakeUp();

        heldBuddy.OnGrabbed();

        playerController.visualAnimator.SetBool("isDragging", true);

        // Wait one frame isn't possible here but joint is created after OnGrabbed
        // which sets bones to non-kinematic, so order is correct
        CreateJoint();

        SetLayerOfAllObjects("BuddyWhenGrabbed", heldBuddy.gameObject);
    }

    private void SetLayerOfAllObjects(string layer, GameObject objectToModify) {
        var children = objectToModify.GetComponentsInChildren<Transform>();
        var layerInt = LayerMask.NameToLayer(layer);

        foreach (var child in children) {
            child.gameObject.layer = layerInt;
        }
    }

    void CreateJoint()
    {
        activeJoint = heldAnchor.gameObject.AddComponent<ConfigurableJoint>();
        activeJoint.connectedBody = gripProxy.GetComponent<Rigidbody>();

        // Lock all linear and angular motion — the proxy drives position
        activeJoint.xMotion = ConfigurableJointMotion.Limited;
        activeJoint.yMotion = ConfigurableJointMotion.Limited;
        activeJoint.zMotion = ConfigurableJointMotion.Limited;
        activeJoint.angularXMotion = ConfigurableJointMotion.Free;
        activeJoint.angularYMotion = ConfigurableJointMotion.Free;
        activeJoint.angularZMotion = ConfigurableJointMotion.Free;

        // Spring drive to pull anchor toward proxy
        JointDrive drive = new JointDrive
        {
            positionSpring = jointPositionSpring,
            positionDamper = jointPositionDamper,
            maximumForce = jointMaxForce
        };

        activeJoint.xDrive = drive;
        activeJoint.yDrive = drive;
        activeJoint.zDrive = drive;

        // Soft limit so the joint doesn't snap violently
        SoftJointLimit limit = new SoftJointLimit { limit = 0.01f };
        activeJoint.linearLimit = limit;

        SoftJointLimitSpring limitSpring = new SoftJointLimitSpring
        {
            spring = jointPositionSpring,
            damper = jointPositionDamper
        };
        activeJoint.linearLimitSpring = limitSpring;
    }

    // ── release ────────────────────────────────────────────────────────────

    void Release()
    {
        if (activeJoint != null)
            Destroy(activeJoint);

        if (heldBuddy != null)
            heldBuddy.OnReleased();

        SetLayerOfAllObjects("BuddyNotGrabbed", heldBuddy.gameObject);

        heldBuddy = null;
        heldAnchor = null;
        activeJoint = null;
        playerController.visualAnimator.SetBool("isDragging", false);
        playerController.isCarryingSomething = false;
    }

    // ── auto drop if buddy drifts too far ─────────────────────────────────

    void CheckAutoDropDistance()
    {
        if (heldAnchor == null) return;

        float dist = Vector3.Distance(heldAnchor.transform.position, gripProxy.transform.position);
        if (dist > maxCarryDistance)
            Release();
    }

    // ── public force drop (for cutscenes, death, teleport, etc.) ──────────

    public void ForceDrop()
    {
        if (heldBuddy != null)
            Release();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position + transform.forward * castDistance, castRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * castDistance);
    }
#endif
}