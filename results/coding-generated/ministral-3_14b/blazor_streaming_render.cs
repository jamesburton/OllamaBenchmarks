using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; } = default!;

    public WeatherForecast[]? Forecasts { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}

public record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public string Summary { get; init; } = string.Empty;
}

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}