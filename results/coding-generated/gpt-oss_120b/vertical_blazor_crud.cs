using Microsoft.AspNetCore.Components;

public record TodoItem(int Id, string Title, bool IsCompleted = false);

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
    private int _nextId = 0;

    public Task<List<TodoItem>> GetAllAsync()
        => Task.FromResult(new List<TodoItem>(_todos));

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(++_nextId, title);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index >= 0) _todos.RemoveAt(index);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _todos.FindIndex(t => t.Id == id);
        if (index >= 0)
        {
            var t = _todos[index];
            _todos[index] = t with { IsCompleted = !t.IsCompleted };
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
        => await LoadAsync();

    private async Task LoadAsync()
        => Todos = await TodoService.GetAllAsync();

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadAsync();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadAsync();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadAsync();
    }
}

/* ---------- xUnit v3 tests for TodoService ---------- */

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Test task");

        item.Title.Should().Be("Test task");
        item.Id.Should().Be(1);
        var all = await service.GetAllAsync();
        all.Should().Contain(item);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();

        var item1 = await service.AddAsync("First");
        var item2 = await service.AddAsync("Second");

        await service.DeleteAsync(item1.Id);

        var all = await service.GetAllAsync();
        all.Should().NotContain(item1);
        all.Should().Contain(item2);
        all.Count.Should().Be(1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Toggle me");
        item.IsCompleted.Should().BeFalse();

        await service.ToggleAsync(item.Id);
        var afterToggle = (await service.GetAllAsync()).Find(t => t.Id == item.Id);
        afterToggle!.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);
        var afterSecondToggle = (await service.GetAllAsync()).Find(t => t.Id == item.Id);
        afterSecondToggle!.IsCompleted.Should().BeFalse();
    }
}

/* ---------- bUnit test for TodoListBase ---------- */

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitialized_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();

        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        // Register the substitute in the test DI container without extra using
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
            .AddSingleton<ITodoService>(ctx.Services, service);

        var cut = ctx.RenderComponent<TodoListBase>();

        // Ensure async lifecycle has completed
        await cut.Instance.InvokeAsync(() => Task.CompletedTask);

        service.Received(1).GetAllAsync();
    }
}