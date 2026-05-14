using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    private int currentCount;

    [Parameter] public int InitialCount { get; set; }
    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

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

using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    private int currentCount;

    [Parameter] public int InitialCount { get; set; }
    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

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
@render.razor
@rendermode InteractiveAuto

razor
@attribute [StreamRendering]

[SupplyParameterFromPersistentComponentState]
public MyState? State { get; code }

razor
[Parameter] public string Title { get; set; }

using Bunit;
var ctx = new TestContext();
var cut = ctx.RenderComponent<Counter>(p => p.Add(x => x.InitialCount, 5));

razor
@inject IOrderService Orders