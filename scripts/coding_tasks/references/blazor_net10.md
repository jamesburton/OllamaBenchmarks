# Blazor .NET 10 — Quick Reference

## Render Modes

```razor
@* Per-component *@
@rendermode InteractiveServer
@rendermode InteractiveWebAssembly
@rendermode InteractiveAuto

@* From parent *@
<MyComponent @rendermode="InteractiveServer" />
```

## Streaming SSR

```razor
@attribute [StreamRendering]

@* Content renders incrementally as data arrives *@
@if (items == null) { <p>Loading...</p> }
else { @foreach (var i in items) { <p>@i.Name</p> } }
```

## Persistent State Across Prerender

```csharp
[SupplyParameterFromPersistentComponentState]
public MyState? State { get; set; }
```

## Component Parameters

```razor
@code {
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public EventCallback<string> OnChange { get; set; }
    [CascadingParameter] public ThemeState Theme { get; set; } = default!;

    async Task HandleClick() => await OnChange.InvokeAsync("new value");
}
```

## bUnit Testing

```csharp
using Bunit;

var ctx = new TestContext();

// Render with parameters
var cut = ctx.RenderComponent<Counter>(p =>
    p.Add(x => x.InitialCount, 5));

// Interact and assert
cut.Find("button").Click();
cut.Markup.Should().Contain("6");

// Find element
var h1 = cut.Find("h1");
h1.TextContent.Should().Be("Counter");
```

## Inject Services in Components

```razor
@inject IOrderService Orders
@inject NavigationManager Nav
```

**Source:** https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes
