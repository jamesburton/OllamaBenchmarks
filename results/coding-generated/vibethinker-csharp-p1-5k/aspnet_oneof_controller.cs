public async Task<IActionResult> Get(int id)
{
            OneOf<User, NotFound, ValidationError> result = _userService.GetUserAsync(id);
            return result.Match(
                user => Ok(user),
                _       => NotFound(),
                err     => BadRequest(err.Message)
            );
        }