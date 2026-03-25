using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public abstract class WeatherPageBase : ComponentBase
{
    [Inject]
    private IWeatherService WeatherService { get; set; }

    [SupplyParameterFromPersistentComponentState]
    public WeatherForecast[]? Forecasts { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        Forecasts = await WeatherService.GetForecastsAsync();
    }
}

[StreamRendering]
public partial class WeatherPage : WeatherPageBase
{
    [Parameter] public string Title { get; set; } = "Weather Forecast";
    [Parameter] public EventCallback<string> OnChange { get; set; }
    [CascadingParameter] public ThemeState Theme { get; set; } = default!;

    async Task HandleClick() => await OnChange.InvokeAsync("new value");
}