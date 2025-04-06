using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton instance of the GameManager
    public static GameManager Instance { get; private set; }

    // Tracks the current state of the game (Playing, Paused, GameOver, etc.)
    public static GameState CurrentState { get; private set; } = GameState.Playing;

    // Global event triggered whenever the game state changes
    public static event Action<GameState> OnGameStateChanged;

    [Header("References")]
    [SerializeField] private GravityManager gravityManager;   // Responsible for managing player gravity shifts

    [Header("UI Panels")]
    [SerializeField] private GameOverUI gameOverUI;           // UI displayed when the player loses
    [SerializeField] private GameWonUI gameWonUI;             // UI displayed when the player wins
    [SerializeField] private PauseGameUI pauseUI;             // UI panel shown when the game is paused
    [SerializeField] private TimerUI timerUI;                 // UI displaying game timer (if present)

    private void Awake()
    {
        // Ensures only one instance of GameManager exists (singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // Listen for event when player falls off the world
        PlayerLocomotion.OnPlayerFell += HandlePlayerFell;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        PlayerLocomotion.OnPlayerFell -= HandlePlayerFell;
    }

    // Triggered when the player falls; ends the game if it's currently active
    private void HandlePlayerFell()
    {
        if (CurrentState != GameState.Playing) return;

        Debug.Log("Player fell. Triggering GameOver.");
        SetGameState(GameState.GameOver);
    }

    // Updates the current game state and triggers relevant UI/logic
    public static void SetGameState(GameState newState)
    {
        // Ignore if already in the desired state
        if (CurrentState == newState) return;

        CurrentState = newState;
        Debug.Log("GameState Changed: " + newState);

        // Notify all systems subscribed to this event
        OnGameStateChanged?.Invoke(CurrentState);

        // Ensure that an instance exists before handling UI behavior
        if (Instance != null)
        {
            switch (newState)
            {
                case GameState.GameOver:
                    Instance.gameOverUI?.Show();         // Display Game Over UI
                    break;

                case GameState.GameWon:
                    Instance.gameWonUI?.Show();          // Display Game Won UI
                    break;

                case GameState.Paused:
                    Instance.pauseUI?.Show();            // Display Pause UI
                    Time.timeScale = 0f;                 // Freeze time
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;                 // Resume time
                    break;
            }
        }
    }

    // Switches between Playing and Paused states (e.g., when pressing pause button)
    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
        else if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    // Called from the Pause UI to resume gameplay
    public void ResumeGame()
    {
        if (CurrentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    // Reloads the current scene, resets gameplay state and related systems
    public void RetryGame()
    {
        // Ensure state is reset before reload
        CurrentState = GameState.Playing;

        // Reset gravity system to initial state
        gravityManager.ResetGravityToDefault();

        // Resume time in case it was paused
        Time.timeScale = 1f;

        // Reset timer UI if present
        timerUI?.ResetTimer();

        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // Exits the game application (handles both build and editor environments)
    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in editor
#endif
    }
}
