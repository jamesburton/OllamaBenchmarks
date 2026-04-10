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
                if (WeatherService != null)
                {
                    var forecasts = await WeatherService.GetForecastsAsync();
                    Forecasts = forecasts;
                }
            }
        }

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
        {
            Task<WeatherForecast[]> GetForecastsAsync();
        }

using Microsoft.AspNetCore.Components;
using System;

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
        if (WeatherService != null)
        {
            var forecasts = await WeatherService.GetForecastsAsync();
            Forecasts = forecasts;
        }
    }
}

...