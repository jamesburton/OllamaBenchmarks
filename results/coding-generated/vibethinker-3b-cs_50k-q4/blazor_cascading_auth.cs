public class UserDashboard
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
    [Inject] public-user-service UserService UserService { get; set; }

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    async Task OnInitializedAsync()
    {
        await AuthState;
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;

            var nameClaim = authState.User.Identity.Name;
            var name = await UserService.GetDisplayNameAsync(nameClaim);
            DisplayName = name;
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }

    }
}