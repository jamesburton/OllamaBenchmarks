using System;
using System.Collections.Generic;
using System.Linq;

public record JobPosting(string Id, string Title, string Company, string Location, bool IsRemote);

public class JobBoard
{
    private readonly List<JobPosting> _postings = new();

    public void Post(JobPosting job)
    {
        _postings.Add(job);
    }

    public List<JobPosting> SearchByTitle(string keyword)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return new List<JobPosting>();
        }

        return _postings
            .Where(p => p.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<JobPosting> GetRemote()
    {
        return _postings.Where(p => p.IsRemote).ToList();
    }

    public int TotalPostings => _postings.Count;
}