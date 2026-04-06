public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
        {
            Task<WeatherForecast[]> GetForecastsAsync();
        }

public class WeatherPageBase : ComponentBase
        {
            [Inject]
            public IWeatherService WeatherService { get; set; }

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

[StreamRendering]
        public class WeatherPageBase : ComponentBase
        {
            [Inject]
            public IWeatherService WeatherService { get; set; }

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

    [StreamRendering]
    public class WeatherPageBase : ComponentBase
    {
        [Inject]
        public IWeatherService WeatherService { get; set; }

        public WeatherForecast[]? Forecasts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }

using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Components;

    public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

    public interface IWeatherService
    {
        Task<WeatherForecast[]> GetForecastsAsync();
    }

    [StreamRendering]
    public class WeatherPageBase : ComponentBase
    {
        [Inject]
        public IWeatherService WeatherService { get; set; }

        public WeatherForecast[]? Forecasts { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Forecasts = await WeatherService.GetForecastsAsync();
        }
    }

[Inject]
    public IWeatherService WeatherService { get; set; }

protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }

razor
    @attribute [StreamRendering]