using Microsoft.AspNetCore.Components;
using Xunit;
using NSubstitute;
using Bunit;
using AwesomeAssertions;

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

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos.ToList());
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
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
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var item = await TodoService.AddAsync(NewTitle);
            Todos = await TodoService.GetAllAsync();
            NewTitle = "";
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
        var item = await service.AddAsync("Test Todo");

        Assert.Equal(1, item.Id);
        Assert.Equal("Test Todo", item.Title);
        Assert.False(item.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("To Delete");
        var todosBefore = await service.GetAllAsync();
        Assert.Single(todosBefore);

        await service.DeleteAsync(1);
        var todosAfter = await service.GetAllAsync();
        Assert.Empty(todosAfter);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("To Toggle");

        await service.ToggleAsync(1);
        var todos = await service.GetAllAsync();
        Assert.True(todos[0].IsCompleted);

        await service.ToggleAsync(1);
        todos = await service.GetAllAsync();
        Assert.False(todos[0].IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var todoService = Substitute.For<ITodoService>();
        todoService.GetAllAsync().Returns(new List<TodoItem>());

        var cut = new TestContext();
        var component = new TodoListBase();
        component.TodoService = todoService;

        await cut.RenderComponent(component);

        await Task.Delay(10);
        await cut.InvokeAsync(() => component.OnInitializedAsync());

        await todoService.Received().GetAllAsync();
    }
}