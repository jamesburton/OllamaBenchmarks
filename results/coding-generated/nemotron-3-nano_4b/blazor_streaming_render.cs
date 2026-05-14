using Microsoft.AspNetCore.Components;

public class WeatherPageBase : ComponentBase
{
    [StreamRendering]
    [Inject]
    public IWeatherService? WeatherService { get; set; } = null!;

    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadForecastsAsync();
    }

    private async Task LoadForecastsAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}