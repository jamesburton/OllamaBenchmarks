using Microsoft.AspNetCore.Components;

namespace MyApp.Services
{
    public interface IUserService
    {
        Task<string> GetCurrentUserNameAsync();
    }
}

namespace MyApp.Components
{
    public class UserProfileBase : ComponentBase
    {
        [SupplyParameterFromPersistentComponentState]
        public string? UserName { get; set; }

        [Inject]
        private IUserService? UserService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (UserName == null && UserService != null)
            {
                UserName = await UserService.GetCurrentUserNameAsync();
            }
        }
    }
}