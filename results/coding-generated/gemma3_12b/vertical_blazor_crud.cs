using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Bunit;
using Xunit;
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

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(x => x.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.Find(x => x.Id == id);
        if (todo != null)
        {
            todo.IsCompleted = !todo.IsCompleted;
        }
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync().ConfigureAwait(false);
    }

    public async Task AddTodo()
    {
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = await TodoService.GetAllAsync().ConfigureAwait(false);
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync().ConfigureAwait(false);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync().ConfigureAwait(false);
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Test Title";
        var newItem = await service.AddAsync(title);

        Assert.Equal(title, newItem.Title);
        Assert.Equal(1, newItem.Id);
        Assert.False(newItem.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item1 = await service.AddAsync("Item 1");
        var item2 = await service.AddAsync("Item 2");

        await service.DeleteAsync(item1.Id);

        Assert.Single(await service.GetAllAsync());
        Assert.Equal("Item 2", (await service.GetAllAsync())[0].Title);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Item");

        Assert.False(item.IsCompleted);

        await service.ToggleAsync(item.Id);

        Assert.True(item.IsCompleted);

        await service.ToggleAsync(item.Id);

        Assert.False(item.IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task Renders_CallsGetAllAsync()
    {
        var mockTodoService = Substitute.For<ITodoService>();
        var component = new TodoListBase();
        component.TodoService = mockTodoService;

        var ctx = new TestContext();
        ctx.ComponentFactories.Add<TodoListBase>(() => component);

        var cut = ctx.RenderComponent<TodoListBase>();

        await component.OnInitializedAsync();

        mockTodoService.Received().GetAllAsync();
    }
}