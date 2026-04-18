using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Spawns swarm agents at valid NavMesh positions around this object.
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

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = count * 5; // avoid infinite loop

        while (spawned < count && attempts < maxAttempts)
        {
            attempts++;
            Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
            Vector3 candidate = transform.position + randomOffset;

            // Find nearest valid NavMesh position
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, spawnRadius, NavMesh.AllAreas))
            {
                Instantiate(agentPrefab, hit.position, Random.rotation);
                spawned++;
            }
        }

        if (spawned < count)
            Debug.LogWarning($"SwarmSpawner: only spawned {spawned}/{count} agents — not enough NavMesh coverage.");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
#endif
}