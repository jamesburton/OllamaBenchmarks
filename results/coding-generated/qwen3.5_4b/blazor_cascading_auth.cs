using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
        {
            Task<string> GetDisplayNameAsync(string userId);
        }

await AuthState.ConfigureAwait(false); // Wait, AuthState is a property here?
                // Actually, if it's a Task<AuthenticationState>, awaiting it gives AuthenticationState.
                var authState = await AuthState;

using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;

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
            // Logic here
        }
    }

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
        if (AuthState == null) return;

        var authState = await AuthState;

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name;
            if (!string.IsNullOrEmpty(name))
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
}

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

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
        if (AuthState == null) return;

        var authState = await AuthState;

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}