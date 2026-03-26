public class User { public int Id { get; set; } public string Name { get; set; } public string Email { get; set; } }
public class NotFound : record { }
public class ValidationError : record { }

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user is User user)
        {
            return Ok(user);
        }
        else if (user is NotFound)
        {
            return NotFound();
        }
        else if (user is ValidationError)
        {
            return ValidationError.Result;
        }
        else
        {
            return BadRequest("Invalid request");
        }
    }
}