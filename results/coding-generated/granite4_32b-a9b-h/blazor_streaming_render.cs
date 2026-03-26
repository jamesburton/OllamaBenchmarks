using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

[StreamRendering]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject] public IWeatherService WeatherService { get; set; } = default!;
    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}