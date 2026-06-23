using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

public record GitHubUser(string Login, string Name, int PublicRepos) {
    [System.Text.Json.SerializeJsonPropertyName("login")]
    private string login = Login;
    [System.Text.Json.SerializeJson propertyName("name")]
    private string name = Name;
    [System.Text.Json.SerializeJsonProperty("public_repos")]
    private int public_repos = PublicRepos;
}