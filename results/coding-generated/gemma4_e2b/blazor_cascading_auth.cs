using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

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
        if (AuthState != null)
        {
            var authState = await AuthState.Result;

            if (authState.User?.Identity?.IsAuthenticated == true)
            {
                IsAuthenticated = true;
                var userName = authState.User.Identity?.Name ?? "Unknown User";
                DisplayName = await UserService.GetDisplayNameAsync(userName);
            }
            else
            {
                IsAuthenticated = false;
                DisplayName = "Guest";
            }
        }
    }
}

// Placeholder for IUserService as it is required for compilation, 
// although the actual implementation is not requested.
public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}