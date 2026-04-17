using System.Threading.Tasks;

public interface IUserRepository
{
    Task<bool> ExistsAsync(string username);
    Task AddAsync(string username);
}

public class RegistrationService
{
    private readonly IUserRepository _userRepository;

    public RegistrationService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> RegisterAsync(string username)
    {
        bool exists = await _userRepository.ExistsAsync(username);

        if (exists)
        {
            return false;
        }

        await _userRepository.AddAsync(username);
        return true;
    }
}