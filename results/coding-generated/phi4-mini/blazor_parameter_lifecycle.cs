using Microsoft.AspNetCore.Components;

[Parameter] public int UserId { get; set; }
[Inject] public IUserService UserService { get; set; } = null!;
public UserProfile? Profile { get; set; }
public bool IsLoading { get; set; }

protected override async Task OnParametersSetAsync()
{
    IsLoading = true;
    Profile = await UserService.GetProfileAsync(UserId);
    IsLoading = false;
}

[Parameter] public string Title { get; set; } = "";
[EventCallback] public EventCallback<string> OnChange { get; set; }