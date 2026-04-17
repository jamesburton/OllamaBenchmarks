using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record Campaign(string Id, string Name, bool IsActive, decimal Budget);

public interface ICampaignRepository
{
    Task SaveAsync(Campaign campaign);
    Task<List<Campaign>> GetActiveAsync();
    Task<Campaign?> GetByIdAsync(string id);
}

public class CampaignManager
{
    private readonly ICampaignRepository _repository;

    public CampaignManager(ICampaignRepository repository)
    {
        _repository = repository;
    }

    public async Task DeactivateAsync(string id)
    {
        Campaign? campaign = await _repository.GetByIdAsync(id);

        if (campaign is null)
        {
            throw new InvalidOperationException($"Campaign with ID '{id}' not found.");
        }

        Campaign deactivatedCampaign = campaign with { IsActive = false };

        await _repository.SaveAsync(deactivatedCampaign);
    }
}