using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? WeatherService { get; set; }

    private readonly List<WeatherForecast> _forecasts = new();

    public WeatherForecast[]? Forecasts => _forecasts.ToArray();

    protected override async Task OnInitializedAsync()
    {
        if (WeatherService != null)
        {
            var forecasts = await WeatherService.GetForecastsAsync();
            _forecasts.AddRange(forecasts);
        }
    }
}