using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace BlazorApp
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

        public WeatherForecast[]? Forecasts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }
}