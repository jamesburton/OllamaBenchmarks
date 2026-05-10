using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(new List<TodoItem>(_todos));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos[_todos.IndexOf(item)] = item with { IsCompleted = !item.IsCompleted };
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadTodos();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Buy milk");

        item.Title.Should().Be("Buy milk");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Buy milk");
        await service.DeleteAsync(1);

        var all = await service.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Buy milk");
        await service.ToggleAsync(1);

        var all = await service.GetAllAsync();
        all.Should().ContainSingle(t => t.IsCompleted == true);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        ctx.Services.AddScoped<ITodoService>(_ => mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        await mockService.Received().GetAllAsync();
    }
}