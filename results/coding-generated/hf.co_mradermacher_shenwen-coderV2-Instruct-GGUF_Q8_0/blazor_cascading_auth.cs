using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    private string? DisplayName => AuthState?.User.Identity?.IsAuthenticated == true ? UserService.GetDisplayNameAsync(AuthState.User.Identity.Name).Result : "Guest";

    protected override void OnInitializedAsync()
    {
        if (AuthState != null)
        {
            IsAuthenticated = AuthState.User.Identity?.IsAuthenticated == true;
        }
    }

    public string? DisplayName { get; set; }
}