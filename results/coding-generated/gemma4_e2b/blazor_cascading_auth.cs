using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [Inject]
    public IUserService UserService { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthState == null)
        {
            return;
        }

        var authState = await AuthState;

        if (authState.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var userName = authState.User.Identity.Name ?? "Unknown User";

            // Fetch display name from the injected service
            string displayName = await UserService.GetDisplayNameAsync(userName);
            DisplayName = displayName;
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}