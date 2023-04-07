using System;
using UnityEngine;

public static class GameManager
{
    public static bool Running { get; private set; }
    public static int Score { get; private set; }
    public static int Hits { get; private set; }
    public static int Misses { get; private set; }

    public static event Action<int> OnScoreChanged;
    public static event Action OnEndGame;

    public static void AddScore(int points)
    {
        if (!Running) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public static void AddShot(bool hit)
    {
        if (hit) Hits++;
        else Misses++;
    }

    public static void EndGame()
    {
        Running = false;
        Debug.Log($"Final Score: " + Score);
        OnEndGame?.Invoke();
    }

    public static void StartGame()
    {
        Running = true;
        Score = 0;
        Hits = 0;
        Misses = 0;
    }
}