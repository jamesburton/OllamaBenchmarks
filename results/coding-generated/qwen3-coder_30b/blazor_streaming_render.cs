using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [StreamRendering]
    public WeatherForecast[]? Forecasts { get; set; }

    [Inject]
    public IWeatherService? WeatherService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (WeatherService != null)
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }
}