using UnityEngine;

public class PointCubeTracker : MonoBehaviour
{
    private int totalCubes;
    private int collectedCubes;

    private void OnEnable()
    {
        PointCubes.OnPlayerScored += HandleCubeCollected;
    }

    private void OnDisable()
    {
        PointCubes.OnPlayerScored -= HandleCubeCollected;
    }

    private void Start()
    {
        // Count all active Point_Cubes in the scene at start
        GameObject[] pointCubes = GameObject.FindGameObjectsWithTag("Point_Cubes");
        totalCubes = pointCubes.Length;
        collectedCubes = 0;
    }

    private void HandleCubeCollected(int _)
    {
        collectedCubes++;

        if (collectedCubes >= totalCubes)
        {
            // Delay game win by 1 frame to allow score to update
            StartCoroutine(TriggerWinNextFrame());
        }
    }

    private System.Collections.IEnumerator TriggerWinNextFrame()
    {
        yield return null;
        GameManager.SetGameState(GameState.GameWon);
    }
}
