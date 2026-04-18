using UnityEngine;

/// <summary>
/// Place this component on any prefab you want the swarm to be REPULSED by.
/// repulsionRadius — outer push zone.
/// dispelRadius — inner zone where agents get dispelled if forced inside.
/// </summary>
public class SwarmRepulsor : MonoBehaviour
{
    [Tooltip("Agents start fleeing once they are within this radius.")]
    public float repulsionRadius = 5f;

    [Tooltip("If an agent is forced inside this radius, it gets dispelled. Must be smaller than repulsionRadius.")]
    public float dispelRadius = 1.5f;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.4f);

        Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);

        Gizmos.color = new Color(1f, 0f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, dispelRadius);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, "Repulsor");
    }
#endif
}