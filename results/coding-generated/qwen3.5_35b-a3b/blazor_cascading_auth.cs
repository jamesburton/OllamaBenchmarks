using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

public interface IUserService
{
    Task<string> GetDisplayNameAsync(string userId);
}