using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase, IDisposable
{
    [Inject] public IWeatherService WeatherService { get; }
    public WeatherForecast[] Forecasts { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        var forecasts = await WeatherService.GetForecastsAsync();
        Forecasts = forecasts;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup if needed
        }
        base.Dispose(disposing);
    }
}