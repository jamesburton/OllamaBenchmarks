using System;
using System.Collections.Generic;
using System.Linq;

public record Pledge(string UserId, string CampaignId, decimal Amount);

public class Crowdfunding
{
    private readonly List<Pledge> _pledges = new List<Pledge>();

    public void AddPledge(Pledge pledge)
    {
        _pledges.Add(pledge);
    }

    public decimal TotalRaised(string campaignId)
    {
        return _pledges
            .Where(p => p.CampaignId == campaignId)
            .Sum(p => p.Amount);
    }

    public int BackerCount(string campaignId)
    {
        return _pledges
            .Where(p => p.CampaignId == campaignId)
            .Select(p => p.UserId)
            .Distinct()
            .Count();
    }

    public List<Pledge> GetPledgesForCampaign(string campaignId)
    {
        return _pledges
            .Where(p => p.CampaignId == campaignId)
            .ToList();
    }
}