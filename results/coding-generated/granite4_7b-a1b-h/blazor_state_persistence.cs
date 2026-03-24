using System.Threading.Tasks;
using Blazor.ComponentFramework;

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    private readonly IUserService _userService;

    public UserProfileBase(IUserService userService)
    {
        _userService = userService;
    }

    protected override async Task OnInitializedAsync()
    {
        if (UserName == null)
        {
            UserName = await _userService.GetCurrentUserNameAsync();
        }
    }
}