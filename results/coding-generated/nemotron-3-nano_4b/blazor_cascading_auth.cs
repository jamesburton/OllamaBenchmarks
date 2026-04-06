using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public partial class UserDashboard : ComponentBase
{
    [Inject]
    public IUserService UserService { get; set; } = default!;

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await AuthState;
        var authState = await AuthState;

        if (authState?.User?.Identity != null && authState.User.Identity.IsAuthenticated)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name ?? string.Empty;
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}