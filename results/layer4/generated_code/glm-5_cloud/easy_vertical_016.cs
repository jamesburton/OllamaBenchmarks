using System;
using System.Collections.Generic;
using System.Linq;

public record Vote(string UserId, string CandidateId);

public class Election
{
    private readonly List<Vote> _votes = new();

    public void CastVote(Vote vote)
    {
        if (vote == null)
        {
            throw new ArgumentNullException(nameof(vote));
        }

        // Optional: Check if user has already voted to prevent duplicates
        // if (HasVoted(vote.UserId)) 
        // {
        //     throw new InvalidOperationException($"User {vote.UserId} has already voted.");
        // }

        _votes.Add(vote);
    }

    public Dictionary<string, int> GetResults()
    {
        return _votes
            .GroupBy(v => v.CandidateId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public string? Winner()
    {
        var results = GetResults();

        if (results.Count == 0)
        {
            return null;
        }

        int maxVotes = results.Values.Max();

        // Note: If there is a tie, this returns the first candidate with the max votes.
        return results.First(kvp => kvp.Value == maxVotes).Key;
    }

    public bool HasVoted(string userId)
    {
        return _votes.Any(v => v.UserId == userId);
    }
}