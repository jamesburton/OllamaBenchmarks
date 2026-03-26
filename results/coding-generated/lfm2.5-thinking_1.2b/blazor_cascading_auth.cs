using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
    [Inject] public IUserService UserService { get; set; }
    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    private async void OnInitializedAsync()
    {
        var authState = await AuthState?.GetAsync();
        if (authState != null && authState.User.Identity?.IsAuthenticated)
        {
            DisplayName = await UserService.GetDisplayNameAsync(UserService.UserId);
            IsAuthenticated = true;
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}