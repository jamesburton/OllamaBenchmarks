using Microsoft.AspNetCore.Components;

public class WeatherPageBase : ComponentBase
{
    [StreamRendering]
    public WeatherForecast? Forecasts { get; set; }

    [Inject]
    public WeatherService WeatherService { get; set; }

    public WeatherForecast DateOnly { get; set; }
    public int TemperatureC { get; set; }
    public string Summary { get; set; }

    public WeatherForecast? LoadForecastsAsync()
    {
        return WeatherService.GetForecastsAsync();
    }

    public void InitializeAsync()
    {
        WeatherForecast[] forecasts = LoadForecastsAsync();
        Forecasts = forecasts;
    }
}