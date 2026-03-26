using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public interface IUserService
{
    Task<string> GetCurrentUserNameAsync();
}

public class UserProfileBase : ComponentBase
{
    [SupplyParameterFromPersistentComponentState]
    public string? UserName { get; set; }

    [Inject]
    public IUserService UserService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (UserName == null && UserService != null)
        {
            UserName = await UserService.GetCurrentUserNameAsync();
        }
    }
}