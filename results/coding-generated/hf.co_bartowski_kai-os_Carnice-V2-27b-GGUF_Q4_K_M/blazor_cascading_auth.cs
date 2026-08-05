using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}

public class UserDashboard : ComponentBase
{
    [CascadingParameter] public Task<AuthenticationState> AuthState { get; set; }
    [Inject] public IUserService UserService { get; set; }

    public string? DisplayName { get; set; }
    public bool IsAuthenticated { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthState;
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            IsAuthenticated = true;
            var name = authState.User.Identity.Name;
            DisplayName = await UserService.GetDisplayNameAsync(name);
        }
        else
        {
            IsAuthenticated = false;
            DisplayName = "Guest";
        }
    }
}

razor
@* Per-component *@
@rendermode InteractiveServer
@rendermode InteractiveWebAssembly
@rendermode InteractiveAuto

@* From parent *@
<MyComponent @rendermode="InteractiveServer" />

razor
@attribute [StreamRendering]

@* Content renders incrementally as data arrives *@
@if (items == null) { <p>Loading...</p> }
else { @foreach (var i in items) { <p>@i.Name</p> } }

[SupplyParameterFromPersistentComponentState]
public MyState? State { get; set; }

razor
@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public EventCallback<string> OnChange { get; set; }
    [CascadingParameter] public ThemeState Theme { get; set; } = default!;

    async Task HandleClick() => await OnChange.InvokeAsync("new value");
}

using Bunit;

var ctx = new TestContext();

// Render with parameters
var cut = ctx.RenderComponent<Counter>(p =>
    p.Add(x => x.InitialCount, 5));

## Inject Services in Components