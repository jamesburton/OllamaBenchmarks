using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    [Inject]
    public IUserService UserService { get; set; }

    public string? DisplayName { get; set; }

    public bool IsAuthenticated { get; set; }

    public override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            DisplayName = authState.User.Identity.Name;
            var nameDisplay = await UserService.GetDisplayNameAsync(DisplayName ?? string.Empty);
            this.DisplayName = nameDisplay;
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}