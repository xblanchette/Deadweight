using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToNextSceneTrigger : MonoBehaviour {

    public string sceneToLoad;

    private void OnTriggerEnter(Collider other) {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) {
            return;
        }

        if (player.isCarryingSomething) {
            SceneManager.LoadScene(sceneToLoad);
            return;
        }

        GameManager.instance.PlayerForgotBuddy();
    }

}
