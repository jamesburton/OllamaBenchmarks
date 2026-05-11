using Microsoft.AspNetCore.Components;

public record UserProfile(int Id, string DisplayName, string Email);

public interface IUserService
{
    Task<UserProfile?> GetProfileAsync(int userId);
}

public class UserProfileBase : ComponentBase
{
    [Parameter]
    public int UserId { get; set; }

    [Inject]
    public IUserService UserService { get; set; } = null!;

    public UserProfile? Profile { get; set; }
    public bool IsLoading { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
    }
}

public class UserProfileComponent : UserProfileBase
{
    // This component uses the base class logic and exposes the data.
    // In a real Blazor scenario, this would be the component definition.
}