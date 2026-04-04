using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
    [Inject] public IUserService UserService { get; set; }
    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await AuthState.OnCompletedAsync();
        if (AuthState.Value?.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var userName = AuthState.Value.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(userName);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}