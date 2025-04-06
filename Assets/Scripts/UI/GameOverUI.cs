using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Needed to set selected UI button for joystick navigation

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;              // Root panel for the Game Over UI
    [SerializeField] private TextMeshProUGUI finalScoreText;        // Text element to display the final score
    [SerializeField] private Button retryButton;                    // Button to retry the game
    [SerializeField] private Button exitButton;                     // Button to quit the game

    private void Awake()
    {
        // Hide the game over UI at start
        gameOverPanel?.SetActive(false);

        // Hook up UI button listeners to their respective methods
        retryButton?.onClick.AddListener(OnRetryButtonPressed);
        exitButton?.onClick.AddListener(OnExitButtonPressed);
    }

    // Displays the Game Over panel and sets the final score text
    public void Show()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
        {
            // Retrieve the score from ScoreManager and display it
            finalScoreText.text = "Final Score: " + ScoreManager.Instance.GetScore().ToString();
        }

        // Ensure retry button is selected for joystick/keyboard navigation
        if (retryButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Clear previous selection
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject); // Set default selected button
        }
    }

    // Called when the player clicks the "Retry" button
    private void OnRetryButtonPressed()
    {
        // Reset the score and reload the level
        ScoreManager.Instance.ResetScore();
        GameManager.Instance.RetryGame();
    }

    // Called when the player clicks the "Exit" button
    private void OnExitButtonPressed()
    {
        // Exit the application (or stop play mode in editor)
        GameManager.Instance.ExitGame();
    }
}
