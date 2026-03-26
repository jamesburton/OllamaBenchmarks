using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [Inject]
    public IUserService? UserService { get; set; }

    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;

    public string? DisplayName { get; set; }

    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitAsync();

        if (AuthState is not null)
        {
            var authState = await AuthState;
            if (authState?.Identity?.IsAuthenticated == true', "User is not authenticated.", "Error");
                return;
            }

            // If we are here, the user is authenticated.
            // We can now safely access the Name claim.
            var name = authState.Identity.Name;
            if (!string.IsNullOrEmpty(name) && UserService is not null)
            {
                DisplayName = await UserService.GetDisplayNameAsync(name);
            }
owns            else
            {
                // F back to the name if the service is missing or the name is empty
                // depending on how strict you want to be.
                DisplayName = name ?? "Guest";
            }
        }
        else
        {
            // Handle case where the service isn't available (e.g. unit testing)
            // or the user is not authenticated.
            IsAuthenticated = false;
            DisplayName = "Guest";
        }

    // ... (Optional: Add a method to manually trigger a refresh if needed)
    // public void Refresh() => InvokeAsync(StateHasChanged);
    // ...
    }

    // Note: The class UserDashboard is not public in the example provided in the prompt 
    // ("A `UserDashboard` class..."), so I will leave it as public.
    // However, usually Blazor components are public. I will leave the access modifier as is
    // or make it public if the user implies a standard Blazor component, but the prompt 
    // specifically says "A `UserDashboard` class...". I will assume public is intended 
    // based on standard .NET practices, but I will double check the prompt constraints.
    // "A `UserDashboard` class" implies the type itself. I will make it public.

using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Authorization;

    public interface IUserService
    {
        Task<string> GetDisplayNameAsync(string userId);
    }

    public class UserDashboard : ComponentBase
    {
        [Inject]
        public IUserService? UserService { get; set; }

        [CascadingParameter]
        public Task<AuthenticationState> AuthState { get; set; } = default!;

        public string? DisplayName { get; set; }

        public bool IsAuthenticated { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitAsync();

            if (AuthState is not null)
            {
                var authState = await AuthState;
                if (authState?.Identity?.IsAuthenticated == true && UserService is not null)
                {
                    IsAuthenticated = true;
                    var name = authState.Identity.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        DisplayName = await UserService.GetDisplayNameAsync(name);
                    }
                    else
                    {
                        DisplayName = name; // Fallback if name is empty
                    }
                }
                else
                {
                    IsAuthenticated = false;
                    DisplayName = "Guest";
                }
            }
著        }
    }