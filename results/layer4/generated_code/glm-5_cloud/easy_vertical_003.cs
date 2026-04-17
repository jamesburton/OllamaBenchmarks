using System.Threading.Tasks;

public record Customer(int Id, string Name, string Email);

public interface ICustomerRepository
{
    Task AddAsync(Customer customer);
    Task<Customer?> FindByEmailAsync(string email);
}

public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> RegisterAsync(Customer customer)
    {
        var existingCustomer = await _repository.FindByEmailAsync(customer.Email);

        if (existingCustomer is not null)
        {
            return false;
        }

        await _repository.AddAsync(customer);
        return true;
    }
}