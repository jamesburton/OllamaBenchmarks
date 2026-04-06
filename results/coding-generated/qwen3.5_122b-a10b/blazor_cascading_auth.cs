using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Authorization;
        using System.Threading.Tasks; // Needed for Task

public interface IUserService
        {
            Task<string> GetDisplayNameAsync(string userId);
        }

public class UserDashboard : ComponentBase
        {
            [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = default!;
            [Inject] public IUserService UserService { get; set; } = default!;

            public string? DisplayName { get; set; }
            public bool IsAuthenticated { get; set; }

            protected override async Task OnInitializedAsync()
            {
                var authState = await AuthState;
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    IsAuthenticated = true;
                    var name = authState.User.Identity.Name;
                    if (name != null)
                    {
                        DisplayName = await UserService.GetDisplayNameAsync(name);
                    }
                    else
                    {
                        DisplayName = "Guest"; // Fallback if name is null but authenticated
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
    using System.Threading.Tasks;

    public interface IUserService
    {
        Task<string> GetDisplayNameAsync(string userId);
    }

    public class UserDashboard : ComponentBase
    {
        [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = default!;
        [Inject] public IUserService UserService { get; set; } = default!;

        public string? DisplayName { get; set; }
        public bool IsAuthenticated { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthState;

            if (authState.User.Identity?.IsAuthenticated == true)
            {
                IsAuthenticated = true;
                var name = authState.User.Identity.Name;
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
    }

... 
, is that valid C# code? No.
    However, as an AI text model, I cannot output raw text without markdown formatting in this interface usually.
    Standard interpretation: Provide the code inside a markdown block.
    Wait, strict instruction: "Return only valid C# code in a single file."
    If I output 
...

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading.Tasks;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject] public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;

        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name;
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
}

...