using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

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

}
