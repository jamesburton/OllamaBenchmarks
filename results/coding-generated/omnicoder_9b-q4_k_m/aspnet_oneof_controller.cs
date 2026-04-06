return result.Match<IActionResult>(
            user    => Ok(user),
            _       => NotFound(),
            err     => BadRequest(err.Message)
        );

return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );

return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );