using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter]
    public int InitialCount { get; set; }

    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount => currentCount;
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

var ctx = testContext();

// Render with parameters
var cut = ctx.RenderComponent<Counter>(p =>
    p.Add(x => x.InitialCount, 5));

// Interact and assert
cut.Find("button").Click();
cut.Markup.Should().Contain("6");

// Find element
var h1 = cut.Find("h1");
h1.TextContent.Should().Be("Counter");

razor
@inject IOrderService Orders
@inject NavigationManager Nav