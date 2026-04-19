using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public string checkpointName;

    public GameObject playerRespawn;
    public GameObject buddyRespawn;

    public static List<string> checkpointNames = new();
    public static bool hasValidated = false;

    private void Awake() {
        hasValidated = false;
        checkpointNames.Clear();
    }

    private void Start() {
        checkpointNames.Add(checkpointName);
        Invoke(nameof(ValidateCheckpointNames), 0);
    }

    private void ValidateCheckpointNames() {
        if (hasValidated) {
            return;
        }
        hasValidated = true;

        var distinctNames = checkpointNames.Select(x => x).Distinct().ToList();

        if (checkpointNames.Count != distinctNames.Count) {
            Debug.LogError("Checkpoint names must all be unique");
        }

        foreach (var name in checkpointNames) {
            if (string.IsNullOrWhiteSpace(name)) {
                Debug.LogError("Checkpoint names must not be empty");
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) {
            return;
        }

        if (GameManager.checkpointsTouched.Contains(checkpointName)) {
            return;
        }

        Debug.Log($"touched checkpoint: {checkpointName}");
        GameManager.checkpointsTouched.Add(checkpointName);
        GameManager.respawnLocation = playerRespawn.transform.position;
        GameManager.buddyRespawnLocation = buddyRespawn.transform.position;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        var radius = 0.5f;

        if (playerRespawn != null) {
            Gizmos.DrawWireSphere(playerRespawn.transform.position, radius);
        }

        if (buddyRespawn != null) {
            Gizmos.DrawWireSphere(buddyRespawn.transform.position, radius);
        }
    }

}
