using Microsoft.AspNetCore.Components;

public class GreeterComponent : ComponentBase
{
    [Parameter]
    public string Name { get; set; } = "World";

    protected string Greeting => $"Hello, {Name}!";
}