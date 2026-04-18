using UnityEngine;

/// <summary>
/// Place this component on any prefab you want the swarm to be ATTRACTED to.
/// No logic needed here — SwarmAgent scans for this tag automatically.
/// </summary>
public class SwarmAttractor : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, "Attractor");
    }
#endif
}
