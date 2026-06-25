using Microsoft.AspNetCore.Components;

public class UserProfileBase : ComponentBase, IViewComponent
{
    [Parameter] public int UserId { get; set; }
    [Inject] public IUserService UserService { get; set; } = null;

    private UserProfile? _profile;
    public UserProfile? Profile => _profile;

    private bool _isLoading;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;
        _profile = await UserService.GetProfileAsync(UserId);
        _isLoading = false;
    }
}