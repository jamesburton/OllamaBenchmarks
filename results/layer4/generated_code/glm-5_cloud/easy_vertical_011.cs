using System;
using System.Collections.Generic;
using System.Linq;

public record Ticket(int Id, string Title, string Status, string Assignee);

public class TicketBoard
{
    private readonly List<Ticket> _tickets = new List<Ticket>();

    public void AddTicket(Ticket ticket)
    {
        _tickets.Add(ticket);
    }

    public List<Ticket> GetByStatus(string status)
    {
        return _tickets.Where(t => t.Status == status).ToList();
    }

    public List<Ticket> GetByAssignee(string assignee)
    {
        return _tickets.Where(t => t.Assignee == assignee).ToList();
    }

    public int TotalCount => _tickets.Count;
}