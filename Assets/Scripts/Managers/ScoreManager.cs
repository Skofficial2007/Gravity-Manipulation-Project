using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    // Singleton instance for global access
    public static ScoreManager Instance { get; private set; }

    // Event triggered whenever the score is updated
    public static event Action<int> OnScoreChanged;

    private int score = 0; // Current total player score

    private void Awake()
    {
        // Enforce Singleton pattern (only one ScoreManager allowed)
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
        // Subscribe to the event that adds points
        PointCubes.OnPlayerScored += AddScore;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks or null reference calls
        PointCubes.OnPlayerScored -= AddScore;
    }

    // Adds points to the score and notifies subscribers
    public void AddScore(int amount)
    {
        score += amount;

        // Broadcast the updated score to listeners (e.g., Score UI)
        OnScoreChanged?.Invoke(score);
    }

    // Public getter for external systems that need to read the current score
    public int GetScore() => score;

    // Resets score to 0 and notifies listeners (e.g., on retry or game start)
    public void ResetScore()
    {
        score = 0;
        OnScoreChanged?.Invoke(score);
    }
}
