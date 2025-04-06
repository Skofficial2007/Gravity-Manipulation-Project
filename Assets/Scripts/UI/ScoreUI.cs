using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;   // Reference to the score display text element

    private void OnEnable()
    {
        // Subscribe to score and game state events when this UI becomes active
        ScoreManager.OnScoreChanged += UpdateScoreText;
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent memory leaks or null reference calls
        ScoreManager.OnScoreChanged -= UpdateScoreText;
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        // Initialize score text immediately with current score at startup
        UpdateScoreText(ScoreManager.Instance.GetScore());

        // Also ensure the visibility of the score text matches the current game state
        HandleGameStateChanged(GameManager.CurrentState);
    }

    // Updates the score text whenever the score changes
    private void UpdateScoreText(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore.ToString();
        }
    }

    // Hides or shows the score UI based on whether the game is actively being played
    private void HandleGameStateChanged(GameState state)
    {
        if (scoreText != null)
        {
            bool isPlaying = state == GameState.Playing;
            scoreText.gameObject.SetActive(isPlaying);
        }
    }
}
