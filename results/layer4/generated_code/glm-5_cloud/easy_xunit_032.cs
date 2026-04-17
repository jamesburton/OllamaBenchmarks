using System;
using System.Threading.Tasks;

public interface IPaymentGateway
{
    Task<bool> ChargeAsync(string cardToken, decimal amount);
}

public class CheckoutService
{
    private readonly IPaymentGateway _paymentGateway;

    public CheckoutService(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public async Task<string> CheckoutAsync(string cardToken, decimal amount)
    {
        bool isCharged = await _paymentGateway.ChargeAsync(cardToken, amount);
        return isCharged ? "Success" : "Failed";
    }
}