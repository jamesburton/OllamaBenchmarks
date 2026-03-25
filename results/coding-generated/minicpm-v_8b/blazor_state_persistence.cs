using Microsoft.AspNetCore.Components;

public interface IUserService {
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase, IDisposable
{
    [Parameter]
    public string? UserName { get; set; }

    [Inject]
    private IUserService _userService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (UserName == null)
        {
            UserName = await _userService.GetCurrentUserNameAsync();
        }
    }

    public void Dispose()
    {
        // Implement IDisposable pattern
    }
}