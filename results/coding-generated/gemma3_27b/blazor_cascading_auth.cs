using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

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
        var authState = await AuthState;
        if (authState?.User?.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            string? name = authState.User.Identity.Name;
            if (name != null)
            {
                DisplayName = await UserService.GetDisplayNameAsync(name);
            }
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}