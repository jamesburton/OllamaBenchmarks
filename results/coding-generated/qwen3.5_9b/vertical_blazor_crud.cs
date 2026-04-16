using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using AwesomeAssertions;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

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
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem
        {
            Id = _nextId++,
            Title = title,
            IsCompleted = false
        };
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var newItem = await TodoService.AddAsync(NewTitle);
            Todos = await TodoService.GetAllAsync();
            NewTitle = string.Empty;
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
        var item = await service.AddAsync("Buy Milk");

        item.Should().NotBeNull();
        item.Title.Should().Be("Buy Milk");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Item 1");
        await service.AddAsync("Item 2");

        var all = await service.GetAllAsync();
        all.Should().HaveCount(2);

        await service.DeleteAsync(1);

        all = await service.GetAllAsync();
        all.Should().HaveCount(1);
        all.Should().ContainSingle(t => t.Title == "Item 2");
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Item 1");

        item.IsCompleted.Should().BeFalse();

        await service.ToggleAsync(1);

        item = await service.GetAllAsync().FirstAsync();
        item.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(1);

        item = await service.GetAllAsync().FirstAsync();
        item.IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    private readonly Bunit.TestContext _ctx;
    private readonly NSubstitute.Substitute.For<ITodoService> _mockService;

    public TodoListBaseTests()
    {
        _ctx = new Bunit.TestContext();
        _mockService = Substitute.For<ITodoService>();
        _mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>
        {
            new TodoItem { Id = 1, Title = "Test", IsCompleted = false }
        }));
    }

    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var component = _ctx.RenderComponent<TodoListBase>();

        await component.WaitForAsyncWork();

        _mockService.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task AddTodo_AddsItemAndClearsNewTitle()
    {
        var component = _ctx.RenderComponent<TodoListBase>();
        component.SetParametersAndRender(parameters =>
        {
            parameters.Add(p => p.NewTitle, "New Task");
        });

        await component.WaitForAsyncWork();

        _mockService.Received(1).AddAsync("New Task");
        _mockService.Received(2).GetAllAsync();
        component.Instance.NewTitle.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTodo_RemovesItem()
    {
        var component = _ctx.RenderComponent<TodoListBase>();
        component.SetParametersAndRender(parameters =>
        {
            parameters.Add(p => p.NewTitle, "New Task");
        });

        await component.WaitForAsyncWork();

        _mockService.Received(1).AddAsync("New Task");
        _mockService.Received(2).GetAllAsync();

        await component.Instance.DeleteTodo(1);

        _mockService.Received(3).GetAllAsync();
    }

    [Fact]
    public async Task ToggleTodo_FlipsIsCompleted()
    {
        var component = _ctx.RenderComponent<TodoListBase>();
        component.SetParametersAndRender(parameters =>
        {
            parameters.Add(p => p.NewTitle, "New Task");
        });

        await component.WaitForAsyncWork();

        _mockService.Received(1).AddAsync("New Task");
        _mockService.Received(2).GetAllAsync();

        await component.Instance.ToggleTodo(1);

        _mockService.Received(3).GetAllAsync();
        _mockService.Received(4).ToggleAsync(1);
    }
}