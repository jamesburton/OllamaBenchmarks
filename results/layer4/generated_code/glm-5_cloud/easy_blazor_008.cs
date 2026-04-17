using Microsoft.AspNetCore.Components;

public class ThemeComponent : ComponentBase
{
    [Parameter]
    public string Theme { get; set; } = "light";

    protected bool IsDarkMode => Theme == "dark";

    protected string CssClass => IsDarkMode ? "theme-dark" : "theme-light";

    protected void ToggleTheme()
    {
        Theme = IsDarkMode ? "light" : "dark";
    }
}