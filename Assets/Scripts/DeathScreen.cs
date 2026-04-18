using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to the Death Screen Canvas GameObject.
/// Handles fade in, button focus for controller, and scene reload.
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The CanvasGroup on the death screen panel for fading.")]
    public CanvasGroup canvasGroup;

    [Tooltip("The first button to select for controller navigation.")]
    public Button firstSelectedButton;

    [Tooltip("The restart button.")]
    public Button restartButton;

    [Tooltip("The main menu button (placeholder).")]
    public Button mainMenuButton;

    [Header("Fade Settings")]
    [Tooltip("How long the fade in takes in seconds.")]
    public float fadeDuration = 0.5f;

    // ── internals ──────────────────────────────────────────────────────────
    private PlayerHealth playerHealth;

    void Awake()
    {
        // Make sure screen starts hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        restartButton.onClick.AddListener(OnRestart);
        mainMenuButton.onClick.AddListener(OnMainMenu);
    }

    /// <summary>Called by PlayerHealth.Die() to show the death screen.</summary>
    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Select first button for controller navigation
        SelectDefaultButton();
    }

    void Update()
    {
        // Edge case: if controller loses focus (e.g. mouse click), reselect default button
        if (canvasGroup.interactable && EventSystem.current != null &&
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

    void OnRestart()
    {
        if (playerHealth != null)
            playerHealth.Restart();
    }

    void OnMainMenu()
    {
        // Placeholder — wire up main menu scene name here when ready
        Debug.Log("Main Menu placeholder — scene not yet assigned.");
    }
}
