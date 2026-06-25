using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateTime Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase, IDisposable
{
    [Parameter]
    public DateTime? InitialDate { get; set; }

    [Inject]
    private IWeatherService _weatherService { get; set; } = default;

    [Parameter]
    public string Title { get; set; } = "";

    [Parameter]
    public EventCallback<string> OnChange { get; set; }

    [CascadingParameter]
    public ThemeState Theme { get; set; }

    private WeatherForecast[]? _forecasts;
    public WeatherForecast[] Forecasts
    {
        get => _forecasts ??= new WeatherForecast[0];
        set => Set(ref _forecasts, value);
    }

    protected override async Task OnInitializedAsync()
    {
        if (InitialDate != null)
        {
            var dateOnly = InitialDate.Value;
            await RefreshForecastsAsync(dateOnly);
        }
    }

    private async Task RefreshForecastsAsync(DateTime dateOnly)
    {
        _forecasts = await _weatherService.GetForecastsAsync();
        StateHasChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _weatherService != null)
        {
            _weatherService.Dispose();
        }
        base.Dispose(disposing);
    }
}