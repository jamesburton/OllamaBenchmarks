using Microsoft.AspNetCore.Components;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using Bunit;

record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private List<TodoItem> _items = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync() => await Task.FromResult(_items.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return await Task.FromResult(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
        }
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        await Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrEmpty(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = await TodoService.GetAllAsync();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test Title");
        result.Title.Should().Be("Test Title");
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item 1");
        await service.AddAsync("Item 2");
        await service.DeleteAsync(1);
        service._items.Count.Should().Be(1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        item.IsCompleted.Should().BeFalse();
        await service.ToggleAsync(item.Id);
        item.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task RendersTodosFromService()
    {
        var ctx = new TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().ReturnsForAnyArgs(new List<TodoItem> { new(1, "Test", false) });

        var cut = ctx.RenderComponent<TodoListBase>(p => p.TodoService = service);
        cut.Todos.Should().HaveCount(1);
        cut.Todos[0].Title.Should().Be("Test");
    }
}