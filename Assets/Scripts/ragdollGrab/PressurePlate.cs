using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Place on a pressure plate trigger collider in the world.
/// Activates when a buddy with sufficient weight lands on it.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PressurePlate : MonoBehaviour
{
    [Tooltip("Minimum buddy weight required to activate this plate.")]
    public float weightThreshold = 1f;

    [Tooltip("Fired when the plate activates.")]
    public UnityEvent onActivated;

    [Tooltip("Fired when the plate deactivates (buddy removed).")]
    public UnityEvent onDeactivated;

    private bool isActivated = false;
    private int buddiesOnPlate = 0;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        BuddyRagdoll buddy = other.GetComponentInParent<BuddyRagdoll>();
        if (buddy == null || buddy.weight < weightThreshold) return;

        buddiesOnPlate++;

        if (!isActivated)
        {
            isActivated = true;
            onActivated?.Invoke();
        }
    }

    void OnTriggerExit(Collider other)
    {
        BuddyRagdoll buddy = other.GetComponentInParent<BuddyRagdoll>();
        if (buddy == null) return;

        buddiesOnPlate = Mathf.Max(0, buddiesOnPlate - 1);

        if (buddiesOnPlate == 0 && isActivated)
        {
            isActivated = false;
            onDeactivated?.Invoke();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? new Color(0f, 1f, 0f, 1f) : new Color(1f, 1f, 0f, 1f);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
#endif
}
