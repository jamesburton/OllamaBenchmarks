public interface ILogger
{
    void Log(string message);
}

public class AuditService
{
    private readonly ILogger _logger;

    public AuditService(ILogger logger)
    {
        _logger = logger;
    }

    public void RecordAction(string action)
    {
        _logger.Log($"Action: {action}");
    }
}