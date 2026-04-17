using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISearchService
{
    Task<List<string>> SearchAsync(string query);
}

public class SearchCache
{
    private readonly ISearchService _searchService;
    private readonly ConcurrentDictionary<string, Task<List<string>>> _cache;

    public SearchCache(ISearchService searchService)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _cache = new ConcurrentDictionary<string, Task<List<string>>>();
    }

    public Task<List<string>> GetResultsAsync(string query)
    {
        // GetOrAdd ensures the underlying service is called only once per unique query.
        // It returns the cached Task for subsequent calls.
        return _cache.GetOrAdd(query, _ => _searchService.SearchAsync(query));
    }
}