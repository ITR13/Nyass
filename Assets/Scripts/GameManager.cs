using System;
using UnityEngine;

public static class GameManager
{
    public static bool Running { get; private set; }
    public static int Score { get; private set; }

    public static event Action<int> OnScoreChanged;

    public static void AddScore(int points)
    {
        if (!Running) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public static void EndGame()
    {
        Running = false;
        Debug.Log($"Final Score: " + Score);
    }

    public static void StartGame()
    {
        Running = true;
        Score = 0;
    }
}