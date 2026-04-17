namespace Contracts
{
    public record FraudDetected(Guid TransactionId, string AccountId, decimal Amount, string RiskLevel);
}

public class FraudDetectedConsumer : IConsumer<Contracts.FraudDetected>
{
    public List<Contracts.FraudDetected> HighRiskEvents = new();

    public void Consume(Contracts.FraudDetected message)
    {
        if (message == null) return;

        var riskLevel = message.RiskLevel?.ToLowerInvariant();

        if (riskLevel == "high" || riskLevel == "critical")
        {
            HighRiskEvents.Add(message);
        }
    }
}

public interface IConsumer<in T>
{
    void Consume(T message);
}