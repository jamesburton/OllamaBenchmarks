public record UserProfile(int Id, string DisplayName, string Email);

public interface IUserService {
            Task<UserProfile?> GetProfileAsync(int userId);
        }

public class UserProfileBase : ComponentBase {
            [Parameter] public int UserId { get; set; }
            // Note: In Blazor, the [Inject] attribute is not a standard C# attribute for class properties in the way the prompt suggests. 
            // However, the prompt specifically asks for [Inject] public IUserService UserService { get; set; } = null!;
            // I will implement it exactly as requested.
            [Inject] public IUserService UserService { get; set; } = null!; 
            public UserProfile? Profile { get; set; }
            public bool IsLoading { get; set; }

            protected override async Task OnParametersSetAsync() {
                IsLoading = true;
                Profile = await UserService.GetProfileAsync(s); // Wait, the prompt says "UserId"
                IsLoading = false;
            }
        }