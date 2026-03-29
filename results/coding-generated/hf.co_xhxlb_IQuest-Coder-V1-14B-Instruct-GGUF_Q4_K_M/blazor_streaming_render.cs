{
  "name": "Generate Blazor Weather Page Base Component",
  "description": "Create a Blazor component base class for weather forecasting with streaming rendering support.",
  "tools": ["Write", "Bash"],
  "arguments": {
  "command": "cat > WeatherPageBase.cs << 'EOF'\nusing Microsoft.AspNetCore.Components;\nusing System.Net.Http.Json;\n\npublic record WeatherForecast(DateOnly Date, int TemperatureC, string Summary);\n\npublic interface IWeatherService\n{\n    Task<WeatherForecast[]> GetForecastsAsync();\n}\n\n[StreamRendering]\npublic class WeatherPageBase : ComponentBase\n{\n    [Inject]\n    public IWeatherService Service { get; set; } = null!;\n\n    public WeatherForecast[]? Forecasts { get; set; }\n\n    protected override async Task OnInitializedAsync()\n    {\n        await base.OnInitializedAsync();\n        Forecasts = await Service.GetForecastsAsync();\n    }\n}\nEOF\n",
  "description": "Create the WeatherPageBase.cs file with all required components."
}