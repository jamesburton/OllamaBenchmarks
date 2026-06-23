[Inject]
public IWeatherService WeatherService { get; set; }

[Inject]
public IMyService MyService { get; set; }

razor
@rendermode InteractiveAuto

[SupplyParameterFromPersistentComponentState]
public MyState? State { get; set; }

[StreamRendering]
[Inject]
public IWeatherService WeatherService { get; set; }
public WeatherForecast[]? Forecasts { get; set; }

override async Task OnInitializedAsync()
{
    var forecasts = await WeatherService.GetForecastsAsync();
    Forecasts = forecasts;
}

public WeatherForecast[]? Forecasts { get; set; }

[Inject]
public IWeatherService WeatherService { get; set; }

using Microsoft.AspNetCore.components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

using Microsoft.AspNetCore Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

[StreamRendering]
public class WeatherPageBase : ComponentBase { ... }

razor
@rendermode InteractiveServer
[StreamRendering]
<MyComponent />

[StreamRendering]
public class WeatherPageBase : ComponentBase { ... }

[StreamRendering]
public class MyComponent : ComponentBase { ... }

using Microsoft.AspNetCore Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

using Microsoft.AspNetCore Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

@inject IOrderService Orders

[Inject]
public IOrderService Orders { get; set; }

public [Inject] IWeatherService WeatherService { get; set; }

[Inject]
public IMyService MyService { get; set; }

public IMyService MyService { get; set; } [Inject];

[Inject]
public IMyService MyService { get; set; }

using Microsoft.AspNetCore Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

using Microsoft.AspNetCore Components;

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    ...
}

using Microsoft.AspNetCore Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

using Microsoft.AspNetCore Components;

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    ...
}

using Microsoft.AspNetCore Components;

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}

using Microsoft.AspNetCore Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }
}