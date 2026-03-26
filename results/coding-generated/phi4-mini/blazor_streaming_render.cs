using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject] public IWeatherService WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);