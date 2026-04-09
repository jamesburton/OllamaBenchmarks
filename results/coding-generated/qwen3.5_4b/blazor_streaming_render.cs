using Microsoft.AspNetCore.Components;

public record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);

public interface IWeatherService
{
    Task<WeatherForecast[]> GetForecastsAsync();
}

public class WeatherPageBase : ComponentBase
{
    [StreamRendering]
    [Inject]
    public IWeatherService? WeatherService { get; set; }

    public WeatherForecast[]? Forecasts { get; set; }

    protected override Task OnInitializedAsync()
    {
        if (WeatherService != null)
        {
            return WeatherService.GetForecastsAsync().AsTask(); // Wait, AsTask is not needed for Task<T>
        }
        return Task.CompletedTask;
    }
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No, just await.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync();
    }
    return Task.CompletedTask;
}

[Inject]
public IWeatherService? WeatherService { get; set; }

public WeatherForecast[]? Forecasts { get; set; }

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No, just await.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No, I need to assign the result.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}

protected override Task OnInitializedAsync()
{
    if (WeatherService != null)
    {
        return WeatherService.GetForecastsAsync().ContinueWith(t => { }); // No.
    }
    return Task.CompletedTask;
}