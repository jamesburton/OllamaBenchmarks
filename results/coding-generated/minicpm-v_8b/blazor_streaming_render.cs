using System;
using System.Collections.Generic;

public class WeatherForecast {
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; }
}

public interface IWeatherService {
    Task<WeatherForecast[]> GetForecastsAsync();
}

public abstract class WeatherPageBase : ComponentBase, IDisposable
{
    [Parameter] private string _city;
    [Inject] private readonly IWeatherService _weatherService;

    protected List<WeatherForecast> Forecasts { get; set; } = new List<WeatherForecast>();

    public async Task OnInitializedAsync()
    {
        Forecasts = await _weatherService.GetForecastsAsync();
    }

    public void Dispose()
    {
        // Implement IDisposable pattern
    }
}