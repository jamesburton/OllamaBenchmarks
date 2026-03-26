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

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var nameClaim = authState.User.FindFirst("name")?.Value;
            DisplayName = await UserService.GetDisplayNameAsync(nameClaim ?? string.Empty);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}