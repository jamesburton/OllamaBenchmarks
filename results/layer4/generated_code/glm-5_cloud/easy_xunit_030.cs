using System;
using System.Collections.Generic;
using System.Linq;

public class ScoreBoard
{
    public Dictionary<string, int> Scores { get; } = new();

    public void AddScore(string player, int points)
    {
        if (Scores.ContainsKey(player))
        {
            Scores[player] += points;
        }
        else
        {
            Scores[player] = points;
        }
    }

    public int GetScore(string player)
    {
        return Scores.TryGetValue(player, out int score) ? score : 0;
    }

    public string? Leader()
    {
        return Scores.Count == 0 ? null : Scores.MaxBy(p => p.Value).Key;
    }
}