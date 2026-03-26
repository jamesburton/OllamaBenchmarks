using Microsoft.AspNetCore.Components;
using System;

[global::Microsoft.AspNetCore.Components.Web.Rendering.StreamRenderingAttribute]
public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [Inject] public IWeatherService WeatherService { get; set; } = default!;

    public WeatherForecast[]? Forecasts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
        await base.OnInitializedAsync();
    }
}