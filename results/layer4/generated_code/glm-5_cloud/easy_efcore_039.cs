using Microsoft.EntityFrameworkCore;
using System;

public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Bio { get; set; }
    public string AvatarUrl { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProfileContext : DbContext
{
    public DbSet<UserProfile> Profiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ProfileDatabase");
    }
}

public static class ProfileRepository
{
    public static UserProfile CreateOrUpdate(ProfileContext context, string userId, string bio, string avatarUrl)
    {
        var profile = context.Profiles.FirstOrDefault(p => p.UserId == userId);

        if (profile == null)
        {
            profile = new UserProfile
            {
                UserId = userId,
                Bio = bio,
                AvatarUrl = avatarUrl,
                UpdatedAt = DateTime.UtcNow
            };
            context.Profiles.Add(profile);
        }
        else
        {
            profile.Bio = bio;
            profile.AvatarUrl = avatarUrl;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        context.SaveChanges();
        return profile;
    }
}