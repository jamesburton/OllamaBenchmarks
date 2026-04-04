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
        var title = "Test Title";
        var newItem = await service.AddAsync(title);
        Assert.Equal(title, newItem.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var initialCount = service._todos.Count;
        var todoItem = new TodoItem(1, "Test Item", false);
        service._todos.Add(todoItem);
        await service.DeleteAsync(1);
        Assert.Equal(initialCount, service._todos.Count);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todoItem = new TodoItem(1, "Test Item", false);
        service._todos.Add(todoItem);
        await service.ToggleAsync(1);
        Assert.True(service._todos.Find(x => x.Id == 1)!.IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task Renders_CallsGetAllAsyncOnInit()
    {
        var mockTodoService = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = mockTodoService };

        mockTodoService.GetAllAsync().Returns(new List<TodoItem>());

        var ctx = new TestContext();
        ctx.ComponentFactories.Add<TodoListBase>(() => component);

        var cut = ctx.RenderComponent<TodoListBase>();

        await component.OnInitializedAsync();

        mockTodoService.Received().GetAllAsync();
    }
}