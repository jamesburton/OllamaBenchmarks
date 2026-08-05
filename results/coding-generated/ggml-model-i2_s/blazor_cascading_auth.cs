using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserDashboard : ComponentBase
{
    public CascadingParameter AuthState { get; set; }
    public IUserService UserService { get; set; }

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    public UserDashboard(IUserService userService)
    {
        this.userService = userService;
    }

    public async Task OnInitializedAsync()
    {
        await this.AuthState.GetAuthenticationStateAsync();

        if (this.AuthState.User.Identity?.IsAuthenticated)
        {
            this.IsAuthenticated = true;
            var userClaim = await this.AuthState.User.Identity?.NameAsync();
            var displayName = await this.userService.GetDisplayNameAsync(userClaim);
            this.DisplayName = displayName;
        }
        else
        {
            this.IsAuthenticated = false;
            this.DisplayName = "Guest";
        }
    }
}