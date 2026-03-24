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
    public IUserService? UserService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (string.IsNullOrEmpty(UserName))
        {
            UserName = await UserService?.GetCurrentUserNameAsync() ?? string.Empty;
        }
    }
}