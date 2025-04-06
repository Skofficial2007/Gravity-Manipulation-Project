using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float totalTime = 120f;                 // Total countdown duration in seconds
    [SerializeField] private Image timerFillImage;                   // UI image with fillAmount (e.g., radial or bar)
    [SerializeField] private TextMeshProUGUI timerText;              // Optional: displays time as MM:SS

    private float currentTime;                                       // Remaining time in seconds
    private bool isRunning = false;                                  // Whether the timer is currently counting down

    private void OnEnable()
    {
        // Subscribe to game state changes to control timer behavior
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Start()
    {
        // Initialize the timer with the full value at game start
        ResetTimer();
    }

    private void Update()
    {
        // Only update timer if it’s actively running
        if (!isRunning) return;

        // Decrease time based on deltaTime and clamp at zero
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);

        // Update the UI fill image based on remaining time
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = currentTime / totalTime;
        }

        // Update the time text in MM:SS format
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // If timer reaches zero during active gameplay, trigger Game Over
        if (currentTime <= 0f && GameManager.CurrentState == GameState.Playing)
        {
            isRunning = false;
            GameManager.SetGameState(GameState.GameOver);
        }
    }

    // Respond to changes in game state to pause/resume timer logic
    private void HandleGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                isRunning = true;
                break;

            case GameState.Paused:
            case GameState.GameOver:
            case GameState.GameWon:
                isRunning = false;
                break;
        }
    }

    // Resets timer back to full and updates the UI accordingly
    public void ResetTimer()
    {
        currentTime = totalTime;

        if (timerFillImage != null)
            timerFillImage.fillAmount = 1f;

        if (timerText != null)
            timerText.text = "02:00"; // Defaults to full time display

        // Only start running if game is currently active
        isRunning = GameManager.CurrentState == GameState.Playing;
    }
}
