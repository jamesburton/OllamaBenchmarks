using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using OneOf;

    public class User { ... }
    public record NotFound;
    public record ValidationError(string Message);
    public interface IUserService { ... }
    public class UsersController : ControllerBase { ... }