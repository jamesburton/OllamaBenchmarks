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
        await Task.Delay(100); // Simulate async work if needed, though AuthState is usually ready

        if (AuthState != null)
        {
            var authState = await AuthState;

            if (authState.User?.Identity?.IsAuthenticated == true)
            {
                IsAuthenticated = true;
                var userName = authState.User.Identity.Name;

                if (!string.IsNullOrEmpty(userName))
                {
                    // Call the injected service
                    DisplayName = await UserService.GetDisplayNameAsync(userName);
                }
                else
                {
                    // Handle case where authenticated but no name is present
                    DisplayName = "Authenticated User";
                }
            }
            else
            {
                IsAuthenticated = false;
                DisplayName = "Guest";
            }
        }
        else
        {
            // Handle case where AuthState is null (should not happen in a standard Blazor context)
            IsAuthenticated = false;
            DisplayName = "Error Loading State";
        }
    }
}