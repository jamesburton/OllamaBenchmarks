using System;
using System.Collections.Generic;
using System.Linq;

public record Tag(int Id, string Name);

public record BlogPost(int Id, string Title, List<int> TagIds);

public class BlogRepository
{
    private readonly List<Tag> _tags = new List<Tag>();
    private readonly List<BlogPost> _posts = new List<BlogPost>();

    public void AddTag(Tag tag)
    {
        _tags.Add(tag);
    }

    public void AddPost(BlogPost post)
    {
        _posts.Add(post);
    }

    public List<BlogPost> GetPostsByTag(int tagId)
    {
        return _posts.Where(p => p.TagIds.Contains(tagId)).ToList();
    }

    public int PostCount => _posts.Count;
}