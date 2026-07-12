using Microsoft.AspNetCore.Components;

public class WeatherPageBase : ComponentBase {
    public WeatherPageBase(IWeatherService service) {
        _service = service;
    }

    public WeatherForecast[]? Forecasts { get; set; } = null;

    public override async Task OnInitializedAsync() {
        Forecasts = await _service.GetForecastsAsync();
    }
}

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService {
    Task<WeatherForecast[]> GetForecastsAsync();
}