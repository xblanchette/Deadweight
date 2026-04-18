using UnityEngine;

/// <summary>
/// Attach to an empty child GameObject on the player (e.g. "GripProxy").
/// This is a kinematic Rigidbody that follows the hand transform every FixedUpdate.
/// The ConfigurableJoint connects to this, never to the hand transform directly.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class GripProxy : MonoBehaviour
{
    [Tooltip("The hand transform the proxy should follow (e.g. the hand bone or a hand socket).")]
    public Transform handTransform;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (handTransform == null) return;

        rb.MovePosition(handTransform.position);
        rb.MoveRotation(handTransform.rotation);
    }
}
