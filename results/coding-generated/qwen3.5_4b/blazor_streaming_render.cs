using Microsoft.AspNetCore.Components;

    public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

    public interface IWeatherService
    {
        Task<WeatherForecast[]> GetForecastsAsync();
    }

    public class WeatherPageBase : ComponentBase
    {
        [Inject]
        public IWeatherService WeatherService { get; set; } = default!;

        [StreamRendering]
        public WeatherForecast[]? Forecasts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }

using System;
    using Microsoft.AspNetCore.Components;

    public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

    public interface IWeatherService
    {
        Task<WeatherForecast[]> GetForecastsAsync();
    }

    public class WeatherPageBase : ComponentBase
    {
        [Inject]
        public IWeatherService WeatherService { get; set; } = default!;

        [StreamRendering]
        public WeatherForecast[]? Forecasts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }

... 
...