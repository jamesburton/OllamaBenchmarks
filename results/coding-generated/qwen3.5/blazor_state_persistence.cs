using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace MyApp.Components
{
    public interface IUserService
    {
        Task<string> GetCurrentUserNameAsync();
    }

    public class UserProfileBase : ComponentBase
    {
        [Inject]
        private IUserService UserService { get; set; } = default!;

        [SupplyParameterFromPersistentComponentState]
        public string? UserName { get; set; }

        protected override