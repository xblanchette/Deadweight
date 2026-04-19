using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to the Main Menu Canvas.
/// Handles controller navigation and button actions.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("The Play button — selected by default for controller navigation.")]
    public Button firstSelectedButton;

    void Start()
    {
        StartCoroutine(SelectNextFrame());
    }

    IEnumerator SelectNextFrame()
    {
        yield return null;
        SelectDefaultButton();
    }

    void Update()
    {
        // Reselect if controller loses focus
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == null)
        {
            SelectDefaultButton();
        }
    }

    void SelectDefaultButton()
    {
        EventSystem.current.SetSelectedGameObject(null);
        firstSelectedButton.Select();
    }

    // ── button actions ─────────────────────────────────────────────────────

    public void PlayGame()
    {
        SceneManager.LoadScene("Tuto");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}