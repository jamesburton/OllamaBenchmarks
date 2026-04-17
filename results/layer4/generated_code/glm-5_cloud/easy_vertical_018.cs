using System.Threading.Tasks;

public record NewsletterSubscription(string Email, bool IsActive);

public interface ISubscriptionRepository
{
    Task AddAsync(NewsletterSubscription subscription);
    Task<NewsletterSubscription?> FindByEmailAsync(string email);
    Task UpdateAsync(NewsletterSubscription subscription);
}

public class SubscriptionService
{
    private readonly ISubscriptionRepository _repository;

    public SubscriptionService(ISubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task SubscribeAsync(string email)
    {
        var existingSubscription = await _repository.FindByEmailAsync(email);

        if (existingSubscription == null)
        {
            var newSubscription = new NewsletterSubscription(email, IsActive: true);
            await _repository.AddAsync(newSubscription);
        }
        else if (!existingSubscription.IsActive)
        {
            var reactivatedSubscription = existingSubscription with { IsActive = true };
            await _repository.UpdateAsync(reactivatedSubscription);
        }
    }

    public async Task UnsubscribeAsync(string email)
    {
        var existingSubscription = await _repository.FindByEmailAsync(email);

        if (existingSubscription != null && existingSubscription.IsActive)
        {
            var deactivatedSubscription = existingSubscription with { IsActive = false };
            await _repository.UpdateAsync(deactivatedSubscription);
        }
    }
}