using System;
using System.Threading.Tasks;
using BlazorQuickReference;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    private IWeatherService? WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService?.GetForecastsAsync() ?? Array.Empty<WeatherForecast>();
    }
}