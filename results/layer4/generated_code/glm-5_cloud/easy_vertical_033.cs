using System;
using System.Collections.Generic;
using System.Linq;

public record SupportTicket(string Id, string Subject, string Priority, string Status);

public class SupportQueue
{
    private readonly List<SupportTicket> _tickets = new();

    public void Submit(SupportTicket ticket)
    {
        _tickets.Add(ticket);
    }

    public List<SupportTicket> GetByPriority(string priority)
    {
        return _tickets.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public SupportTicket? GetNext()
    {
        // Returns first open high-priority, then medium, then low (by submission order)
        return _tickets.FirstOrDefault(t => 
            t.Status.Equals("open", StringComparison.OrdinalIgnoreCase) && 
            t.Priority.Equals("high", StringComparison.OrdinalIgnoreCase)) 
            ?? _tickets.FirstOrDefault(t => 
                t.Status.Equals("open", StringComparison.OrdinalIgnoreCase) && 
                t.Priority.Equals("medium", StringComparison.OrdinalIgnoreCase)) 
            ?? _tickets.FirstOrDefault(t => 
                t.Status.Equals("open", StringComparison.OrdinalIgnoreCase) && 
                t.Priority.Equals("low", StringComparison.OrdinalIgnoreCase));
    }

    public void Resolve(string ticketId)
    {
        var index = _tickets.FindIndex(t => t.Id == ticketId);
        if (index != -1)
        {
            // Use 'with' expression to create a new record with updated Status
            _tickets[index] = _tickets[index] with { Status = "resolved" };
        }
    }
}