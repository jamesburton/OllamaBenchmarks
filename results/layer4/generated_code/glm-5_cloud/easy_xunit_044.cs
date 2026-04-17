using System;
using System.Threading.Tasks;

public interface IWeatherService
{
    Task<string> GetConditionAsync(string city);
}

public class WeatherAlert
{
    private readonly IWeatherService _weatherService;

    public WeatherAlert(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    public async Task<string> GetAlertAsync(string city)
    {
        string condition = await _weatherService.GetConditionAsync(city);

        if (condition == "Storm")
        {
            return $"ALERT: {condition} in {city}";
        }
        else
        {
            return $"Clear: {city}";
        }
    }
}