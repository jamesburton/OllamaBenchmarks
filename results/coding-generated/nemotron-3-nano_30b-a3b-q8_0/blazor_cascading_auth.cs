using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [Inject]
    public IUserService UserService { get; set; } = default!;

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthState is not null)
        {
            var authState = await AuthState;
            IsAuthenticated = authState.User.Identity?.IsAuthenticated == true;

            if (IsAuthenticated)
            {
                var userName = authState.User.Identity?.Name;
                if (!string.IsNullOrEmpty(userName))
                {
                    DisplayName = await UserService.GetDisplayNameAsync(userName);
                }
                else
                {
                    DisplayName = "Guest";
                }
            }
            else
            {
                DisplayName = "Guest";
            }
        }
    }
}