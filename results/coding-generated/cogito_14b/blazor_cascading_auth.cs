using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = default!;

    [Inject] public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; private set; }
    public bool IsAuthenticated { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var nameClaim = authState.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(nameClaim);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}