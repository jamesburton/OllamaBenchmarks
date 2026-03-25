using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly;
using Microsoft.Extensions.DependencyInjection;

namespace SampleComponents
{
    /// <summary>
    /// Service to retrieve the current user's name.
    /// </summary>
    public interface IUserService
    {
        Task<string> GetCurrentUserNameAsync();
    }

    /// <summary>
    /// Base class for a component that wants to supply a user name from persistent
    /// component state. If the <c>UserName</c> parameter is not provided, the name is
    /// fetched from <see cref="IUserService"/>.
    /// </summary>
    public abstract class UserProfileBase : ComponentBase
    {
        /// <summary>
        /// The user name supplied from persistent component state. If this value is
        /// <c>null</c> or empty when the component initializes, it will be populated
        /// by calling <see cref="IUserService.GetCurrentUserNameAsync"/>.
        /// </summary>
        [SupplyParameterFromPersistentComponentState]
        public string? UserName { get; set; }

        /// <summary>
        /// Service used to retrieve the current user name.
        /// </summary>
        [Inject]
        private IUserService UserService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            // If the persistent parameter was not set, fetch the name from the service.
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = await UserService.GetCurrentUserNameAsync();
            }

            await base.OnInitializedAsync();
        }
    }
}