using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    // [StreamRendering] – Inline attribute (treated as XML comment)
    [StreamRendering]
    public IWeatherService Service { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await Service.GetForecastsAsync();
    }
}