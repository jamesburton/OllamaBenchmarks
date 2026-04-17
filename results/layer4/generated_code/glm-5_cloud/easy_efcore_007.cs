using Microsoft.EntityFrameworkCore;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class TagContext : DbContext
{
    public DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TagDatabase");
    }
}

public static class TagRepository
{
    public static Tag Upsert(TagContext context, string name, string color)
    {
        var existingTag = context.Tags
            .FirstOrDefault(t => t.Name.ToLower() == name.ToLower());

        if (existingTag != null)
        {
            existingTag.Color = color;
        }
        else
        {
            existingTag = new Tag
            {
                Name = name,
                Color = color
            };
            context.Tags.Add(existingTag);
        }

        context.SaveChanges();
        return existingTag;
    }
}