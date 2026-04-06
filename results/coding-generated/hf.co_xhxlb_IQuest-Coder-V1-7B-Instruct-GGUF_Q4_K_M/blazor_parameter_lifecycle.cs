using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

// 2. An IUserService interface (NOT in a namespace) with:
public interface IUserService
{
    Task<UserProfile?> GetProfileAsync(int userId);
}

// 1. A UserProfile record (NOT in a namespace):
public record UserProfile(int Id, string DisplayName, string Email)
{
}

// 3. A UserProfileBase class inheriting from ComponentBase (NOT in a namespace) with:
public class UserProfileBase : ComponentBase
{
    [Parameter] public int UserId { get; set; }
    [Inject] public IUserService UserService { get; set; } = null!;
    public UserProfile? Profile { get; set; }
    public bool IsLoading { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Handle the case where UserId is 0 or negative to avoid unnecessary calls
        if (UserId <= 0)
        {
            Profile = null;
            IsLoading = false;
            return;
        }

        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
    }
}