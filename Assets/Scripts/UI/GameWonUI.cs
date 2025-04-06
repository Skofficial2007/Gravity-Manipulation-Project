using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Needed to set selected UI button for joystick navigation

public class GameWonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameWonPanel;               // Root panel for the Game Won UI
    [SerializeField] private TextMeshProUGUI finalScoreText;        // Displays the final score
    [SerializeField] private Button retryButton;                    // Button to restart the game
    [SerializeField] private Button exitButton;                     // Button to exit the game

    private void Awake()
    {
        // Make sure the UI is hidden at the beginning of the scene
        gameWonPanel?.SetActive(false);

        // Hook up UI buttons to their event methods
        retryButton?.onClick.AddListener(OnRetryButtonPressed);
        exitButton?.onClick.AddListener(OnExitButtonPressed);
    }

    // Called when the player wins — shows the UI and updates score display
    public void Show()
    {
        if (gameWonPanel != null)
            gameWonPanel.SetActive(true);

        if (finalScoreText != null)
        {
            // Pull current score from ScoreManager and display it
            finalScoreText.text = "Final Score: " + ScoreManager.Instance.GetScore().ToString();
        }

        // Ensure the first button (retry) is selected for joystick/keyboard navigation
        if (retryButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Clear any previously selected button
            EventSystem.current.SetSelectedGameObject(retryButton.gameObject); // Select default
        }
    }

    // Resets score and reloads the level when retry is pressed
    private void OnRetryButtonPressed()
    {
        ScoreManager.Instance.ResetScore();
        GameManager.Instance.RetryGame();
    }

    // Quits the application (or exits play mode in editor)
    private void OnExitButtonPressed()
    {
        GameManager.Instance.ExitGame();
    }
}
