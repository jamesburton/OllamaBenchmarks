using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Authorization;
        using System.Threading.Tasks; // Needed for Task

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
                // Handle AuthState
                // Note: AuthState is a Task<AuthenticationState>.
                // We need to await it.
                // However, AuthState is usually passed as a Task.
                // Wait, CascadingParameter usually expects the value directly or a Task.
                // The prompt specifically asks for: [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
                // This is slightly unusual for standard Blazor patterns (usually it's AuthenticationState directly), but I must follow the prompt.
                // If AuthState is a Task, I need to await it inside OnInitializedAsync.
            }
        }

protected override async Task OnInitializedAsync()
        {
            if (AuthState != null)
            {
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

using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Authorization;
        using System.Threading.Tasks;

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
                // Logic here
            }
        }

protected override async Task OnInitializedAsync()
        {
            if (AuthState != null)
            {
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

...