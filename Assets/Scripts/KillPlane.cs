using UnityEngine;

/// <summary>
/// Place on a large trigger collider below the map.
/// Kills the player instantly on contact.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillPlane : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.Die();
    }
}
