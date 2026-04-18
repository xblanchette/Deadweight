using UnityEngine;

/// <summary>
/// Place this component on any prefab you want the swarm to be REPULSED by.
/// No logic needed here — SwarmAgent scans for this tag automatically.
/// </summary>
public class SwarmRepulsor : MonoBehaviour
{
    [Tooltip("Agents start fleeing once they are within this radius.")]
    public float repulsionRadius = 5f;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, repulsionRadius);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, "Repulsor");
    }
#endif
}