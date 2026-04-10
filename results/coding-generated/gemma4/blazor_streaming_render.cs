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
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; } = default!;

    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Load forecasts from the service
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}