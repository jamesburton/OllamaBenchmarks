using Microsoft.AspNetCore.Components;
using System;

public class ProgressBarComponent : ComponentBase
{
    [Parameter]
    public double Min { get; set; } = 0;

    [Parameter]
    public double Max { get; set; } = 100;

    [Parameter]
    public double Value { get; set; } = 0;

    protected double Percentage => Max == Min ? 0 : Math.Clamp((Value - Min) / (Max - Min) * 100, 0, 100);
}