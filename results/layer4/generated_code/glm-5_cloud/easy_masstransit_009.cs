namespace Contracts
{
    public record FileUploaded(Guid FileId, string FileName, long SizeBytes);
}

public interface IConsumer<T>
{
    void Consume(T message);
}

public class FileUploadedConsumer : IConsumer<Contracts.FileUploaded>
{
    public long TotalBytesProcessed = 0;

    public void Consume(Contracts.FileUploaded message)
    {
        TotalBytesProcessed += message.SizeBytes;
    }
}