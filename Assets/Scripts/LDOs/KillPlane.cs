using System.Linq;
using UnityEngine;

/// <summary>
/// Place on a large trigger collider below the map.
/// Kills the player instantly on contact.
/// </summary>
[RequireComponent(typeof(Collider))]
public class KillPlane : MonoBehaviour
{

    PlayerHealth playerHealthInScene;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        playerHealthInScene = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).FirstOrDefault();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            health.Die();

        // If the buddy dies, you also die
        var buddy = other.gameObject.GetComponentInParent<BuddyRagdoll>();
        if (buddy != null && playerHealthInScene != null) {
            playerHealthInScene.Die();
        }
    }
}
