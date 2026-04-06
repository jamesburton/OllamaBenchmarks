using Microsoft.AspNetCore.Components;

public class UserProfileBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter] public int UserId { get; set; }
    [Inject] public IUserService UserService { get; set; } = null!;
    public UserProfile? Profile { get; set; }
    public bool IsLoading { get; set; }

    public UserProfileBase() { }

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
    }
}