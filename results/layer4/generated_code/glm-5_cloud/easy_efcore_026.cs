using Microsoft.EntityFrameworkCore;

public class Poll
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public int Votes { get; set; }
}

public class PollContext : DbContext
{
    public DbSet<Poll> Polls { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("PollDatabase");
    }
}

public static class PollRepository
{
    public static Poll? Vote(PollContext context, int pollId)
    {
        var updatedRows = context.Polls
            .Where(p => p.Id == pollId)
            .ExecuteUpdate(setters => setters.SetProperty(p => p.Votes, p => p.Votes + 1));

        if (updatedRows == 0)
        {
            return null;
        }

        return context.Polls.Find(pollId);
    }
}