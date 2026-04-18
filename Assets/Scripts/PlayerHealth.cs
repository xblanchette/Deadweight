using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to the Player GameObject.
/// Handles death on ghost contact, disables input, and triggers the death screen.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The DeathScreen component in the scene.")]
    public DeathScreen deathScreen;

    // ── state ──────────────────────────────────────────────────────────────
    private bool isDead = false;

    // ── input ──────────────────────────────────────────────────────────────
    private InputSystem_Actions playerControls;
    private PlayerGrabber playerGrabber;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerGrabber = GetComponent<PlayerGrabber>();
        playerControls = playerController.playerControls;

        // Safety — make sure timescale is always correct on scene load
        Time.timeScale = 1f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // Only ghosts (SwarmAgent) kill the player
        if (collision.gameObject.GetComponent<SwarmAgent>() != null)
            Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Force drop buddy if carrying
        playerGrabber.ForceDrop();

        // Disable all player input
        playerControls.Player.Disable();

        // Stop the swarm
        StopSwarm();

        // Show death screen
        deathScreen.Show();
    }

    void StopSwarm()
    {
        SwarmAgent[] agents = FindObjectsByType<SwarmAgent>(FindObjectsSortMode.None);
        foreach (SwarmAgent agent in agents)
            agent.ForceIdle();
    }

    /// <summary>Called by the restart button on the death screen.</summary>
    public void Restart()
    {
        StartCoroutine(RestartCoroutine());
    }

    private IEnumerator RestartCoroutine()
    {
        // Wait one frame to let physics settle before reloading
        yield return null;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
