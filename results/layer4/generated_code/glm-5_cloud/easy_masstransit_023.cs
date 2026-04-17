namespace Contracts
{
    public record BulkImportCompleted(Guid ImportId, int RowsImported, int RowsSkipped, int RowsFailed);
}

public interface IConsumer<T>
{
    Task Consume(T message);
}

public class BulkImportConsumer : IConsumer<Contracts.BulkImportCompleted>
{
    public int TotalImported = 0;
    public int TotalFailed = 0;

    public Task Consume(Contracts.BulkImportCompleted message)
    {
        TotalImported += message.RowsImported;
        TotalFailed += message.RowsFailed;
        return Task.CompletedTask;
    }
}