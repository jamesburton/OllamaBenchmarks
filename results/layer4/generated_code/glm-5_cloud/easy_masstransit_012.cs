namespace Contracts
{
    public record ReportGenerated(Guid ReportId, string ReportType, string GeneratedBy);
}

using MassTransit;
using System.Collections.Generic;

public class ReportGeneratedConsumer : IConsumer<Contracts.ReportGenerated>
{
    public List<Contracts.ReportGenerated> Reports = new();

    public Task Consume(ConsumeContext<Contracts.ReportGenerated> context)
    {
        Reports.Add(context.Message);
        return Task.CompletedTask;
    }
}

public class ReportGeneratedConsumerDefinition : ConsumerDefinition<ReportGeneratedConsumer>
{
    public ReportGeneratedConsumerDefinition()
    {
        ConcurrentMessageLimit = 1;
    }
}