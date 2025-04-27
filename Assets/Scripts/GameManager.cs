using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    // Game state variables
    private int currentScore = 0;

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points = 1)
    {
        currentScore += points;
        Debug.Log($"Score: {currentScore}");
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}
