using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = null!;

    [Inject]
    public IUserService UserService { get; set; } = null!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var userId = user.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(userId!);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}