using System;

public interface IClockService
{
    DateTime Now();
}

public class DailyGreeter
{
    private readonly IClockService _clockService;

    public DailyGreeter(IClockService clockService)
    {
        _clockService = clockService;
    }

    public string Greet()
    {
        int hour = _clockService.Now().Hour;

        if (hour < 12)
        {
            return "Good morning!";
        }
        else if (hour < 18)
        {
            return "Good afternoon!";
        }
        else
        {
            return "Good evening!";
        }
    }
}