using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!

    [Inject]
    public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            DisplayName = await UserService.GetDisplayNameAsync(user.Identity.Name ?? "Unknown");
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}