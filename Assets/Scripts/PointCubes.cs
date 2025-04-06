using UnityEngine;
using System;

public class PointCubes : MonoBehaviour
{
    public static event Action<int> OnPlayerScored;

    [SerializeField] private int points = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Trigger the event and pass points
            OnPlayerScored?.Invoke(points);

            gameObject.SetActive(false);
        }
    }
}
