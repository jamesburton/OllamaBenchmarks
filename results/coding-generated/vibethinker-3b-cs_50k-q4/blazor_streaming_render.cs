public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);
IWeatherService GetForecastsAsync();
public class WeatherPageBase : ComponentBase
{
    [Inject] public IWeatherService Orders;
    private WeatherForecast[] Forecasts;

    public WeatherForecast[]? Forecasts => Forecasts;
    public WeatherForecast? SetWeatherForecast(WeatherForecast forecast) => new WeatherForecast(forecast.Date, forecast.TemperatureC, forecast_summary);

    async Task OnInitializedAsync()
    {
        if (Orders.GetForecastsAsync().Result == null)
            return;

        Forecasts = await Orders.GetForecastsAsync();
    }
}