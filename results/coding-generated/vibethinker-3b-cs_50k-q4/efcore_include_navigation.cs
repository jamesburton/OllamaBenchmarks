using Microsoft.EntityFrameworkCore;

public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; } = new();
    public int PostId { get; set; }

    public Blog? Post { get; set; }
}