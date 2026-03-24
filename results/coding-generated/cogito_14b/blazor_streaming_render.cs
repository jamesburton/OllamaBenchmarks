using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace WeatherApp
{
    public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

    public interface IWeatherService
    {
        Task<WeatherForecast[]> GetForecastsAsync();
    }

    [StreamRendering]
    public class WeatherPageBase : ComponentBase
    {
        [Inject]
        public IWeatherService WeatherService { get; set; } = default!;

        public WeatherForecast[]? Forecasts { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }
}