using System;
using System.Collections.Generic;
using System.Linq;

public record WatchlistItem(string UserId, string MovieId, string Title);

public class Watchlist
{
    private readonly List<WatchlistItem> _items = new();

    public void Add(WatchlistItem item)
    {
        _items.Add(item);
    }

    public void Remove(string userId, string movieId)
    {
        _items.RemoveAll(i => i.UserId == userId && i.MovieId == movieId);
    }

    public bool Contains(string userId, string movieId)
    {
        return _items.Exists(i => i.UserId == userId && i.MovieId == movieId);
    }

    public List<WatchlistItem> GetForUser(string userId)
    {
        return _items.Where(i => i.UserId == userId).ToList();
    }
}