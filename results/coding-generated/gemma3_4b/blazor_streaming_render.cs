using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    public WeatherForecast[]? Forecasts { get; set; }

    private readonly IWeatherService _weatherService;

    [Inject]
    public IWeatherService WeatherService
    {
        get { return _weatherService; }
        set { _weatherService = value; }
    }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}