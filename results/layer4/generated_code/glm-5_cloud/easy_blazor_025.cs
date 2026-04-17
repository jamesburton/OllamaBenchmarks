using Microsoft.AspNetCore.Components;

public class ThemeSwitcherComponent : ComponentBase
{
    public string Theme { get; set; } = "light";

    public bool IsDarkMode => Theme == "dark";

    public void SetDark()
    {
        Theme = "dark";
    }

    public void SetLight()
    {
        Theme = "light";
    }

    public void ToggleTheme()
    {
        Theme = Theme == "light" ? "dark" : "light";
    }
}