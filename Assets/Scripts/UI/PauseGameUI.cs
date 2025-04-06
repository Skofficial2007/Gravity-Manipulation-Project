using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Needed for setting selected UI element

public class PauseGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;        // Root panel for the pause menu
    [SerializeField] private Button resumeButton;          // Button to resume the game
    [SerializeField] private Button exitButton;            // Button to quit the game

    private void Awake()
    {
        // Hide pause panel initially
        pausePanel?.SetActive(false);

        // Register button click events
        resumeButton?.onClick.AddListener(OnResumePressed);
        exitButton?.onClick.AddListener(OnExitPressed);
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState state)
    {
        bool shouldShowPause = state == GameState.Paused;

        pausePanel?.SetActive(shouldShowPause);

        if (shouldShowPause)
        {
            // Clear previous selection to avoid leftover focus
            EventSystem.current.SetSelectedGameObject(null);

            // Select the resume button so it's highlighted and navigable by joystick/keyboard
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    private void OnResumePressed()
    {
        GameManager.Instance.ResumeGame();
    }

    private void OnExitPressed()
    {
        GameManager.Instance.ExitGame();
    }

    public void Show()
    {
        pausePanel?.SetActive(true);

        // Ensure resume button is selected when shown manually
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }
}
