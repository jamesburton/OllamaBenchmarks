using Microsoft.AspNetCore.Components;

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject] public IWeatherService Orders;
    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await Orders!.GetForecastsAsync();
    }
}