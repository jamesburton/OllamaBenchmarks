using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public class UserDashboard : ComponentBase, IRenderModeProvider
{
    [Inject]
    public IUserService UserService { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    private string? _displayName;
    private bool _isAuthenticated;

    protected override async Task OnInitializedAsync()
    {
        await AuthState;
        if (AuthState.User.Identity?.IsAuthenticated == true)
        {
            var name = AuthState.User.Identity.Name;
            _displayName = await UserService.GetDisplayNameAsync(name);
            _isAuthenticated = true;
        }
        else
        {
            _isAuthenticated = false;
            _displayName = "Guest";
        }
    }

    public string? DisplayName => _displayName;

    public bool IsAuthenticated => _isAuthenticated;
}