using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    [Inject]
    private IUserService _userService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserName))
        {
            UserName = await _userService.GetCurrentUserNameAsync();
        }
    }
}