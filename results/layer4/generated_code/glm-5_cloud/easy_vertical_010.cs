using System;
using System.Threading.Tasks;

public record UserProfile(string UserId, string DisplayName, string? AvatarUrl);

public interface IProfileRepository
{
    Task SaveAsync(UserProfile profile);
    Task<UserProfile?> GetAsync(string userId);
}

public class ProfileService
{
    private readonly IProfileRepository _repository;

    public ProfileService(IProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task UpdateDisplayNameAsync(string userId, string newName)
    {
        var profile = await _repository.GetAsync(userId);

        if (profile != null)
        {
            var updatedProfile = profile with { DisplayName = newName };
            await _repository.SaveAsync(updatedProfile);
        }
    }

    public async Task<string> GetDisplayNameAsync(string userId)
    {
        var profile = await _repository.GetAsync(userId);
        return profile?.DisplayName ?? "Unknown";
    }
}