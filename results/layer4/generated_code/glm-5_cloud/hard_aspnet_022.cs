using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;

// ============================================================================
// 1. CQRS Abstractions
// ============================================================================

/// <summary>
/// Marker interface for queries that return a specific result type.
/// </summary>
public interface IQuery<TResult> { }

/// <summary>
/// Marker interface for commands that return a specific result type.
/// </summary>
public interface ICommand<TResult> { }

/// <summary>
/// Defines a handler for a specific query type.
/// </summary>
public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct);
}

/// <summary>
/// Defines a handler for a specific command type.
/// </summary>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
}

// ============================================================================
// 2. API Dispatcher
// ============================================================================

/// <summary>
/// Dispatches queries and commands to their respective handlers using the IServiceProvider.
/// </summary>
public class ApiDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public ApiDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken ct)
    {
        // Dynamically resolve the generic handler type: IQueryHandler<TQuery, TResult>
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for query type {query.GetType().Name}");

        // Invoke the HandleAsync method via reflection
        var method = handlerType.GetMethod("HandleAsync");

        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found on handler {handlerType.Name}");

        var result = method.Invoke(handler, new object[] { query, ct });

        // Unwrap the result if it's a Task
        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            return (TResult)task.GetType().GetProperty("Result")!.GetValue(task)!;
        }

        return (TResult)result!;
    }

    public async Task<TResult> CommandAsync<TResult>(ICommand<TResult> command, CancellationToken ct)
    {
        // Dynamically resolve the generic handler type: ICommandHandler<TCommand, TResult>
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for command type {command.GetType().Name}");

        // Invoke the HandleAsync method via reflection
        var method = handlerType.GetMethod("HandleAsync");

        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found on handler {handlerType.Name}");

        var result = method.Invoke(handler, new object[] { command, ct });

        // Unwrap the result if it's a Task
        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            return (TResult)task.GetType().GetProperty("Result")!.GetValue(task)!;
        }

        return (TResult)result!;
    }
}

// ============================================================================
// 3. Domain Models & DTOs
// ============================================================================

public record TaskDto(int Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt);

// ============================================================================
// 4. Domain Commands & Queries
// ============================================================================

public record CreateTaskCommand(string Title, string? Description) : ICommand<TaskDto>;
public record GetTaskQuery(int Id) : IQuery<TaskDto?>;
public record ListTasksQuery(bool? CompletedOnly) : IQuery<IReadOnlyList<TaskDto>>;
public record CompleteTaskCommand(int Id) : ICommand<bool>;

// ============================================================================
// 5. In-Memory Infrastructure & Handlers
// ============================================================================

/// <summary>
/// Simple in-memory database simulation.
/// </summary>
public class InMemoryTaskRepository
{
    private readonly ConcurrentDictionary<int, TaskDto> _store = new();
    private int _idCounter = 0;

    public TaskDto Add(string title, string? description)
    {
        var id = Interlocked.Increment(ref _idCounter);
        var task = new TaskDto(id, title, description, false, DateTime.UtcNow);
        _store.TryAdd(id, task);
        return task;
    }

    public TaskDto? Get(int id) => _store.TryGetValue(id, out var task) ? task : null;

    public IReadOnlyList<TaskDto> List(bool? completedOnly)
    {
        var query = _store.Values.AsEnumerable();

        if (completedOnly.HasValue)
        {
            query = query.Where(t => t.IsCompleted == completedOnly.Value);
        }

        return query.ToList().AsReadOnly();
    }

    public bool Complete(int id)
    {
        if (!_store.TryGetValue(id, out var task))
            return false;

        var updatedTask = task with { IsCompleted = true };
        _store.TryUpdate(id, updatedTask, task);
        return true;
    }
}

// Handlers
public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, TaskDto>
{
    private readonly InMemoryTaskRepository _repository;
    public CreateTaskHandler(InMemoryTaskRepository repository) => _repository = repository;

    public Task<TaskDto> HandleAsync(CreateTaskCommand command, CancellationToken ct)
    {
        var task = _repository.Add(command.Title, command.Description);
        return Task.FromResult(task);
    }
}

public class GetTaskHandler : IQueryHandler<GetTaskQuery, TaskDto?>
{
    private readonly InMemoryTaskRepository _repository;
    public GetTaskHandler(InMemoryTaskRepository repository) => _repository = repository;

    public Task<TaskDto?> HandleAsync(GetTaskQuery query, CancellationToken ct)
    {
        var task = _repository.Get(query.Id);
        return Task.FromResult(task);
    }
}

public class ListTasksHandler : IQueryHandler<ListTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly InMemoryTaskRepository _repository;
    public ListTasksHandler(InMemoryTaskRepository repository) => _repository = repository;

    public Task<IReadOnlyList<TaskDto>> HandleAsync(ListTasksQuery query, CancellationToken ct)
    {
        var tasks = _repository.List(query.CompletedOnly);
        return Task.FromResult(tasks);
    }
}

public class CompleteTaskHandler : ICommandHandler<CompleteTaskCommand, bool>
{
    private readonly InMemoryTaskRepository _repository;
    public CompleteTaskHandler(InMemoryTaskRepository repository) => _repository = repository;

    public Task<bool> HandleAsync(CompleteTaskCommand command, CancellationToken ct)
    {
        var result = _repository.Complete(command.Id);
        return Task.FromResult(result);
    }
}

// ============================================================================
// 6. Minimal API Endpoints
// ============================================================================

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapPost("/", async (ApiDispatcher dispatcher, CreateTaskCommand command, CancellationToken ct) =>
        {
            var result = await dispatcher.CommandAsync(command, ct);
            return Results.Created($"/api/tasks/{result.Id}", result);
        });

        group.MapGet("/{id:int}", async (ApiDispatcher dispatcher, int id, CancellationToken ct) =>
        {
            var result = await dispatcher.QueryAsync(new GetTaskQuery(id), ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapGet("/", async (ApiDispatcher dispatcher, bool? completedOnly, CancellationToken ct) =>
        {
            var result = await dispatcher.QueryAsync(new ListTasksQuery(completedOnly), ct);
            return Results.Ok(result);
        });

        group.MapPut("/{id:int}/complete", async (ApiDispatcher dispatcher, int id, CancellationToken ct) =>
        {
            var success = await dispatcher.CommandAsync(new CompleteTaskCommand(id), ct);
            return success ? Results.NoContent() : Results.NotFound();
        });
    }
}

// ============================================================================
// 7. Program.cs (Entry Point)
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// Register Infrastructure
builder.Services.AddSingleton<InMemoryTaskRepository>();

// Register Handlers
builder.Services.AddTransient<ICommandHandler<CreateTaskCommand, TaskDto>, CreateTaskHandler>();
builder.Services.AddTransient<IQueryHandler<GetTaskQuery, TaskDto?>, GetTaskHandler>();
builder.Services.AddTransient<IQueryHandler<ListTasksQuery, IReadOnlyList<TaskDto>>, ListTasksHandler>();
builder.Services.AddTransient<ICommandHandler<CompleteTaskCommand, bool>, CompleteTaskHandler>();

// Register Dispatcher
builder.Services.AddTransient<ApiDispatcher>();

var app = builder.Build();

// Map Endpoints
app.MapTaskEndpoints();

app.Run();