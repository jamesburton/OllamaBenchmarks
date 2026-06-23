public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
            private readonly IWeatherService _service;

            public WeatherPageBase(IWeatherService service)
            {
                _service = service;
            }

            public WeatherForecast[]? Forecasts { get; set; } = null;

            public void OnInitializedAsync()
            {
                _service.GetForecastsAsync().GetAwaiter().GetResult();
            }
        }