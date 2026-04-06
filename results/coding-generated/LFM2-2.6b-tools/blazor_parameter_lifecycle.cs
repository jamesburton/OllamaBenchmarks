using Microsoft.AspNetCore.Components;

public record UserProfile(int Id, string DisplayName, string Email);

public interface IUserService
{
    Task<UserProfile?> GetProfileAsync(int userId);
}

public class UserProfileBase : ComponentBase, IDisposable
{
    [Parameter] public int UserId { get; set; }
    [Inject] public IUserService UserService { get; set; } = null!;
    public UserProfile? Profile { get; set; }
    public bool IsLoading { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !IsLoading)
        {
            UserService.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class UserProfile : UserProfileBase
{
    public UserProfile(int userId) : base() { UserId = userId; }
}