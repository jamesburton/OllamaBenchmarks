using System;
using System.Collections.Generic;
using System.Linq;

public record TeamMember(string Id, string Name, string Role);

public class Team
{
    private readonly List<TeamMember> _members = new List<TeamMember>();

    public string Name { get; }

    public Team(string name)
    {
        Name = name;
    }

    public void AddMember(TeamMember member)
    {
        _members.Add(member);
    }

    public void RemoveMember(string memberId)
    {
        _members.RemoveAll(m => m.Id == memberId);
    }

    public List<TeamMember> GetByRole(string role)
    {
        return _members.Where(m => m.Role == role).ToList();
    }

    public int Size => _members.Count;
}