using UnityEngine;

/// <summary>
/// Place this component on any prefab you want the swarm to be ATTRACTED to.
/// Tick isPlayer on the attractor that belongs to the player.
/// </summary>
public class SwarmAttractor : MonoBehaviour
{
    [Tooltip("Mark this if this attractor is the player. Used for linger chase logic.")]
    public bool isPlayer = false;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.4f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, isPlayer ? "Attractor (Player)" : "Attractor");
    }
#endif
}