public class TestTodoListComponent : TodoListBase
{
}

using Microsoft.AspNetCore.Components;

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
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
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
            var index = _todos.IndexOf(item);
            _todos[index] = item with { IsCompleted = !item.IsCompleted };
        }
        return Task.CompletedTask;
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

// xUnit tests
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Title");
        item.Title.Should().Be("Test Title");
        item.Id.Should().Be(1);
        var all = await service.GetAllAsync();
        all.Count.Should().Be(1);
        all[0].Should().Be(item);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item1 = await service.AddAsync("Item 1");
        var item2 = await service.AddAsync("Item 2");
        await service.DeleteAsync(item1.Id);
        var all = await service.GetAllAsync();
        all.Count.Should().Be(1);
        all[0].Should().Be(item2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Toggle Test");
        await service.ToggleAsync(item.Id);
        var toggled = (await service.GetAllAsync()).First(t => t.Id == item.Id);
        toggled.IsCompleted.Should().BeTrue();
        await service.ToggleAsync(item.Id);
        var toggledAgain = (await service.GetAllAsync()).First(t => t.Id == item.Id);
        toggledAgain.IsCompleted.Should().BeFalse();
    }
}

// bUnit test
public class TodoListBaseTests
{
    [Fact]
    public void OnInitialized_CallsGetAllAsync()
    {
        var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        ctx.Services.AddSingleton<ITodoService>(mockService);

        // Define a minimal component that inherits from TodoListBase
        var component = ctx.RenderComponent<TestTodoListComponent>();

        mockService.Received().GetAllAsync();
    }
}

public class TestTodoListComponent : TodoListBase
{
}