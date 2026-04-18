using UnityEngine;

/// <summary>
/// Place this component on any prefab you want the swarm to be REPULSED by.
/// No logic needed here — SwarmAgent scans for this tag automatically.
/// </summary>
public class SwarmRepulsor : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        // Draw the repulsion radius so you can see it in the Scene view
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.15f);

        // Find any agent in the scene to read the configured radius
        SwarmAgent sample = FindFirstObjectByType<SwarmAgent>();
        float radius = sample != null ? sample.repulsionRadius : 5f;
        Gizmos.DrawWireSphere(transform.position, radius);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, "Repulsor");
    }
#endif
}
