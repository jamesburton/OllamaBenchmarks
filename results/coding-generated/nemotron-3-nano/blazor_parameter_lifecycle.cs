using Microsoft.AspNetCore.Components;

public record UserProfile(int Id, string DisplayName, string Email);

public interface IUserService
{
    global::System.Threading.Tasks.Task<UserProfile?> GetProfileAsync(int userId);
}

public class UserProfileBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter]
    public int UserId { get; set; }

    [Inject]
    public IUserService UserService { get; set; } = null!;

    public UserProfile? Profile { get; set; }

    public bool IsLoading { get; set; }

    protected override global::System.Threading.Tasks.Task OnParametersSetAsync()
    {
        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
        return base.OnParametersSetAsync();
    }
}