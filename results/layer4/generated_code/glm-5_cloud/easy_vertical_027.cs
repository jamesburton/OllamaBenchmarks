using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record Project(string Id, string Name, string OwnerId, bool IsArchived);

public interface IProjectRepository
{
    Task AddAsync(Project project);
    Task<List<Project>> GetByOwnerAsync(string ownerId);
    Task<Project?> GetByIdAsync(string id);
    Task UpdateAsync(Project project);
}

public class ProjectService
{
    private readonly IProjectRepository _repository;

    public ProjectService(IProjectRepository repository)
    {
        _repository = repository;
    }

    public async Task ArchiveAsync(string projectId)
    {
        var project = await _repository.GetByIdAsync(projectId);

        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} not found.");
        }

        var archivedProject = project with { IsArchived = true };

        await _repository.UpdateAsync(archivedProject);
    }
}