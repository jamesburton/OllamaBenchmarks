using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = Task.FromResult(AuthenticationState.Anonymous);
    [Inject]
    public IUserService UserService { get; set; } = null!;
    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthState != null)
        {
            var authState = await AuthState;
            if (authState.User.Identity != null && authState.User.Identity.IsAuthenticated)
            {
                IsAuthenticated = true;
                string? name = authState.User.Identity.Name;
                if (name != null)
                {
                    DisplayName = await UserService.GetDisplayNameAsync(name);
                }
                else
                {
                    DisplayName = "Guest";
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
            DisplayName = "Guest";
        }
    }
}