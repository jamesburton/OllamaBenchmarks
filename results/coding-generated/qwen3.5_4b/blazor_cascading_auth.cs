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
                // Await AuthState
                var authState = await AuthState;

                // Check authentication
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    IsAuthenticated = true;
                    var name = authState.User.Identity.Name;
                    var displayName = await UserService.GetDisplayNameAsync(name);
                    DisplayName = displayName;
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
        [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
        [Inject] public IUserService UserService { get; set; }

        public string? DisplayName { get; set; }
        public bool IsAuthenticated { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthState;

            if (authState.User.Identity?.IsAuthenticated == true)
            {
                IsAuthenticated = true;
                var name = authState.User.Identity.Name;
                var displayName = await UserService.GetDisplayNameAsync(name);
                DisplayName = displayName;
            }
            else
            {
                IsAuthenticated = false;
                DisplayName = "Guest";
            }
        }
    }

...