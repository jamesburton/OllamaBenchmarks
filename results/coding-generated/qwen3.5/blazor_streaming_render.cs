using Microsoft.AspNetCore.Components;
        using System;
        using System.Collections.Generic;
        using System.Threading.Tasks;

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

using Microsoft.AspNetCore.Components;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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

using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Web;
    using System;
    using System.Threading.Tasks;

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