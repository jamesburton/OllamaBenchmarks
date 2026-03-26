using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Encodings.Web;
using System.Net.Http;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using System;

public class UserService
{
    public Task<string> GetDisplayNameAsync(string userId)
    {
        // Dummy implementation for demonstration purposes.
        return Task.FromResult($"User_{userId}");
    }
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; }

    [Inject]
    public IUserService UserService { get; set; } = default!;

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        var user = authState.User;

        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            IsAuthenticated = true;
            DisplayName = user.Identity.Name;
            var nameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (!string.IsNullOrEmpty(nameClaim?.Value))
            {
                DisplayName = await UserService.GetDisplayNameAsync(nameClaim.Value);
            }
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}