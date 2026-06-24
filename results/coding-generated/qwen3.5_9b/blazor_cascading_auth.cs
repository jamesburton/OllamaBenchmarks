public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState>? AuthState { get; set; }

    [Inject] public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthState == null) return;

        var authResult = await AuthState;

        if (authResult.User.Identity?.IsAuthenticated ?? false)
        {
            IsAuthenticated = true;

            // Note: In a real scenario, you would typically get the name from 
            // authResult.User.Identity.Name or claims. The prompt asks to use Identity.Name.
            var userNameClaim = authResult.User.Identity.Name;

            if (!string.IsNullOrEmpty(userNameClaim))
            {
                DisplayName = await UserService.GetDisplayNameAsync(userNameClaim);
            }
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}