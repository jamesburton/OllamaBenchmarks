public async Task<IActionResult> Get(int id)
{
    OneOf<User, NotFound, ValidationError> result = _service.GetUser(id);
    return result.Match(
        user => Ok(user),
        notFound => NotFound(),
        err => BadRequest(err.Message)
    );
}