using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Username { get; set; }

    public string Email { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("UserDatabase");
    }
}

public static class UserRepository
{
    public static bool Delete(UserContext context, int id)
    {
        var user = context.Users.Find(id);

        if (user == null)
        {
            return false;
        }

        context.Users.Remove(user);
        context.SaveChanges();

        return true;
    }
}