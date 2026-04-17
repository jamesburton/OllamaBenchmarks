using Microsoft.AspNetCore.Components;

public class AlertComponent : ComponentBase
{
    [Parameter]
    public string Message { get; set; } = "";

    [Parameter]
    public string Type { get; set; } = "info";

    protected bool IsDismissed { get; set; } = false;

    protected void Dismiss()
    {
        IsDismissed = true;
    }

    protected string CssClass => Type switch
    {
        "error" => "alert-danger",
        "success" => "alert-success",
        "warning" => "alert-warning",
        _ => "alert-info"
    };
}