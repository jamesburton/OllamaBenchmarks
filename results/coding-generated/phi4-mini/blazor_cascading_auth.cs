using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

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

    private string? displayName;
    public bool IsAuthenticated => authState?.User.Identity != null && !authState.User.Identity.IsAnonymous;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState?.IsAuthenticated == true)
        {
            DisplayName = UserService.GetDisplayNameAsync(authState.User.FindFirstValue("name"));
            IsAuthenticated = true;
        }
        else
        {
            displayName = "Guest";
            IsAuthenticated = false;
        }
    }

    public string? DisplayName { get; set; }
}