using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject]
    public IWeatherService? WeatherService { get; set; }

    [SupplyParameterFromPersistentComponentState]
    public WeatherForecast[]? Forecasts { get; set; }

    [AttributeUsage(AttributeTargets.Class)]
    public class StreamRenderingAttribute : Attribute { }

    [StreamRendering]
    public override async Task OnInitializedAsync()
    {
        if (WeatherService != null)
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }
}