using Microsoft.AspNetCore.Components;

public record WeatherForecast(System.DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    System.Threading.Tasks.Task<WeatherForecast[]> GetForecastsAsync();
}

[StreamRendering]
public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService WeatherService { get; set; } = default!;

    public WeatherForecast[]? Forecasts { get; set; } = default;

    public override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        Forecasts = (await WeatherService.GetForecastsAsync()).ToArray();
        await base.OnInitializedAsync();
    }
}