using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

// 1. WeatherForecast record
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

// 2. IWeatherService interface
public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

// 3. WeatherPageBase class
[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    // Inject IWeatherService
    [Inject]
    public IWeatherService WeatherService { get; set; } = default!;

    // Public property for forecasts
    public WeatherForecast[]? Forecasts { get; set; }

    // Lifecycle method to load data
    protected override async Task OnInitializedAsync()
    {
        // Load forecasts from the injected service
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}