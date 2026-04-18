using UnityEngine;

/// <summary>
/// Optional helper — drop this in the scene to spawn a bunch of agents at startup.
/// Assign your agent prefab (with SwarmAgent on it) in the Inspector.
/// </summary>
public class SwarmSpawner : MonoBehaviour
{
    [Tooltip("Prefab that has the SwarmAgent component.")]
    public GameObject agentPrefab;

    [Tooltip("How many agents to spawn.")]
    public int count = 30;

    [Tooltip("Random spread radius around this object's position.")]
    public float spawnRadius = 3f;

    void Start()
    {
        if (agentPrefab == null)
        {
            Debug.LogWarning("SwarmSpawner: no agentPrefab assigned.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitSphere * spawnRadius;
            Instantiate(agentPrefab, transform.position + offset, Random.rotation);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}
