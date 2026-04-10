using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService Orders { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadForecasts();
    }

    private async Task LoadForecasts()
    {
        var forecasts = await Orders.GetForecastsAsync();
        Forecasts = forecasts;
    }
}