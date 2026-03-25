using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace MyApp.Components;

/// <summary>
/// Represents a single weather forecast entry.
/// </summary>
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

/// <summary>
/// Service contract for retrieving weather forecast data.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Retrieves an array of weather forecasts.
    /// </summary>
    Task<WeatherForecast[]> GetForecastsAsync();
}

/// <summary>
/// Base class for a weather page that uses streaming server‑side rendering.
/// </summary>
[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    /// <summary>
    /// Injected service used to obtain forecast data.
    /// </summary>
    [Inject]
    private IWeatherService? WeatherService { get; set; }

    /// <summary>
    /// The forecasts loaded from the service. Null until initialization completes.
    /// </summary>
    public WeatherForecast[]? Forecasts { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        if (WeatherService is not null)
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
        else
        {
            // Service not provided – fallback to an empty array.
            Forecasts = Array.Empty<WeatherForecast>();
        }

        await base.OnInitializedAsync();
    }
}