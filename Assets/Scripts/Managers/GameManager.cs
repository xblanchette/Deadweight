using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public static Vector3 respawnLocation = Vector3.zero;
    public static Vector3 buddyRespawnLocation = Vector3.zero;
    public static bool useCheckpoint = false;

    public static List<string> checkpointsTouched = new();

    public GameObject globalVolume;
    public GameObject mainCanvas;
    public GameObject objectToActivateWhenBuddyLeftBehind;

    private void Awake() {
        if (instance == null) {
            instance = this;

            if (globalVolume == null) {
                Debug.LogError($"Assign the global volume in the Game Manager");
            }
            else {
                globalVolume.SetActive(true);
            }

            Application.targetFrameRate = 60;

            objectToActivateWhenBuddyLeftBehind.SetActive(false);

            GoToCheckpoint();
            useCheckpoint = true;
        }
    }

    public void SetCanvasVisibility(bool visible) {
        mainCanvas.SetActive(visible);
    }

    public void PlayerForgotBuddy() {
        objectToActivateWhenBuddyLeftBehind.SetActive(true);
    }

    public static int LayermaskToLayer(LayerMask layerMask) {
        int layerNumber = 0;
        int layer = layerMask.value;
        while (layer > 0) {
            layer >>= 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    private void GoToCheckpoint() {
        if (!useCheckpoint) {
            return;
        }

        var player = FindObjectsByType<PlayerController>(FindObjectsSortMode.None).First();
        player.transform.position = respawnLocation;

        var buddy = FindObjectsByType<BuddyRagdoll>(FindObjectsSortMode.None).First();
        buddy.transform.position = buddyRespawnLocation;
    }

}
