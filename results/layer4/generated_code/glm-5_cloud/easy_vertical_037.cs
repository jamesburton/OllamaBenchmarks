using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public record Asset(string Id, string Name, string OwnerId, decimal Value);

public interface IAssetRepository
{
    Task AddAsync(Asset asset);
    Task<List<Asset>> GetByOwnerAsync(string ownerId);
    Task<decimal> GetTotalValueAsync(string ownerId);
}

public class InMemoryAssetRepository : IAssetRepository
{
    private readonly List<Asset> _assets = new List<Asset>();
    private readonly object _lock = new object();

    public Task AddAsync(Asset asset)
    {
        lock (_lock)
        {
            _assets.Add(asset);
        }
        return Task.CompletedTask;
    }

    public Task<List<Asset>> GetByOwnerAsync(string ownerId)
    {
        List<Asset> result;
        lock (_lock)
        {
            result = _assets.Where(a => a.OwnerId == ownerId).ToList();
        }
        return Task.FromResult(result);
    }

    public Task<decimal> GetTotalValueAsync(string ownerId)
    {
        decimal total;
        lock (_lock)
        {
            total = _assets.Where(a => a.OwnerId == ownerId).Sum(a => a.Value);
        }
        return Task.FromResult(total);
    }
}