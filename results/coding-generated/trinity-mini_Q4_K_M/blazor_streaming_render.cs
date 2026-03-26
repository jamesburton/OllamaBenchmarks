using Microsoft.AspNetCore.Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? Orders { get; set; }

    private WeatherForecast[]? _forecasts;

    public WeatherForecast[]? Forecasts
    {
        get => _forecasts;
        set => _forecasts = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Orders != null)
        {
            Forecasts = await Orders.GetForecastsAsync();
        }
    }
}

[Inject]
public IWeatherService? Orders { get; set; }

using Microsoft.AspNetCore.Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? Orders { get; set; }

    private WeatherForecast[]? _forecasts;

    public WeatherForecast[]? Forecasts
    {
        get => _forecasts;
        set => _forecasts = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Orders != null)
        {
            Forecasts = await Orders.GetForecastsAsync();
        }
    }
}

using Microsoft.AspNetCore.Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? Orders { get; set; }

    private WeatherForecast[]? _forecasts;

    public WeatherForecast[]? Forecasts
    {
        get => _forecasts;
        set => _forecasts = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Orders != null)
        {
            Forecasts = await Orders.GetForecastsAsync();
        }
    }
}

using Microsoft.AspNetCore.Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? Orders { get; set; }

    private WeatherForecast[]? _forecasts;

    public WeatherForecast[]? Forecasts
    {
        get => _forecasts;
        set => _forecasts = value;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (Orders != null)
        {
            Forecasts = await Orders.GetForecastsAsync();
        }
    }
}