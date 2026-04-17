using System;
using System.Collections.Generic;
using System.Linq;

public record Player(string Id, string Username, int Level);

public record Achievement(string PlayerId, string Name, DateTime UnlockedAt);

public class GameProfile
{
    private readonly List<Player> _players = new();
    private readonly List<Achievement> _achievements = new();

    public void AddPlayer(Player player)
    {
        if (player != null && !_players.Any(p => p.Id == player.Id))
        {
            _players.Add(player);
        }
    }

    public void UnlockAchievement(Achievement achievement)
    {
        if (achievement != null)
        {
            _achievements.Add(achievement);
        }
    }

    public List<Achievement> GetAchievements(string playerId)
    {
        return _achievements.Where(a => a.PlayerId == playerId).ToList();
    }

    public int AchievementCount(string playerId)
    {
        return _achievements.Count(a => a.PlayerId == playerId);
    }
}