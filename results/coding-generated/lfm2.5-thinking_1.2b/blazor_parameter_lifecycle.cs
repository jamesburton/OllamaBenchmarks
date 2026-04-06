public class UserProfileBase
{
    [Parameter] public int UserId { get; set; }
    [Inject] public IUserService UserService { get; set; }
    public UserProfile? Profile { get; set; }
    public bool IsLoading { get; set; }
    protected override async Task OnParametersSetAsync() => 
    {
        IsLoading = true;
        Profile = await UserService.GetProfileAsync(UserId);
        IsLoading = false;
    }
}

@code {
    public class MyComponent : ComponentBase
    {
        public void HandleClick()
        {
            // Usage of UserProfileBase here
        }
    }
}