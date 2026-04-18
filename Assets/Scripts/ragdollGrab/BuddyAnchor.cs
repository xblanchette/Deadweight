using UnityEngine;

/// <summary>
/// Place on each grabbable bone of the buddy prefab (hands, feet, etc.).
/// Requires a Rigidbody on the same GameObject — the joint will be created here.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BuddyAnchor : MonoBehaviour
{
    public enum AnchorType { LeftHand, RightHand, LeftFoot, RightFoot, Other }

    [Tooltip("What limb this anchor represents. For informational/future use.")]
    public AnchorType anchorType = AnchorType.Other;

    public Rigidbody Rb { get; private set; }

    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.08f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.12f, anchorType.ToString());
    }
#endif
}
