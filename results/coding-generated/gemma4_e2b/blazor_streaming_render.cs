using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService Orders { get; set; }

    [StreamRendering]
    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Orders == null)
        {
            throw new InvalidOperationException("IWeatherService must be injected.");
        }

        Forecasts = await Orders.GetForecastsAsync();
    }
}