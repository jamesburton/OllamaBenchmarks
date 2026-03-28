using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; }

    public WeatherForecast(DateOnly date, int temperatureC, string summary)
    {
        Date = date;
        TemperatureC = temperatureC;
        Summary = summary;
    }
}

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService _weatherService { get; set; }

    private List<WeatherForecast> _forecasts;

    protected override async Task OnInitializedAsync()
    {
        var forecasts = await _weatherService.GetForecastsAsync();

        if (forecasts != null)
        {
            _forecasts = forecasts;
        }
    }

    public IEnumerable<WeatherForecast> Forecasts
    {
        get => _forecasts ?? new List<WeatherForecast>();
    }
}