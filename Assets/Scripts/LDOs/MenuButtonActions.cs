using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonActions : MonoBehaviour {

    public void QuitGame() {
        Application.Quit();
    }

    public void PlayGame() {
        SceneManager.LoadScene("Tuto");
    }

    public void GoToMenu() {
        SceneManager.LoadScene("Main Menu");
    }

}
