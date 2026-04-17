public interface IHashService
{
    string Hash(string input);
}

public class PasswordManager
{
    private readonly IHashService _hashService;

    public PasswordManager(IHashService hashService)
    {
        _hashService = hashService;
    }

    public string HashPassword(string password)
    {
        return _hashService.Hash(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return _hashService.Hash(password) == hash;
    }
}