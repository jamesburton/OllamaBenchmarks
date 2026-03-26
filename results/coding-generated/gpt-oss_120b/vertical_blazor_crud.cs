using Microsoft.AspNetCore.Components;

// -------------------------------------------------
// 1. TodoItem record
// -------------------------------------------------
public record TodoItem(int Id, string Title, bool IsCompleted);

// -------------------------------------------------
// 2. ITodoService interface
// -------------------------------------------------
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// -------------------------------------------------
// 3. TodoService implementation (in‑memory)
// -------------------------------------------------
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
        => Task.FromResult(_items.Select(i => i).ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index >= 0) _items.RemoveAt(index);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            var current = _items[index];
            var toggled = current with { IsCompleted = !current.IsCompleted };
            _items[index] = toggled;
        }
        return Task.CompletedTask;
    }
}

// -------------------------------------------------
// 4. TodoListBase component code‑behind
// -------------------------------------------------
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

// -------------------------------------------------
// 5. xUnit v3 tests for TodoService
// -------------------------------------------------
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Write tests");

        item.Title.Should().Be("Write tests");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Task to delete");
        await service.DeleteAsync(item.Id);

        var all = await service.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();

        var item = await service.AddAsync("Toggle me");
        await service.ToggleAsync(item.Id);

        var afterToggle = (await service.GetAllAsync())[0];
        afterToggle.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);
        var afterSecondToggle = (await service.GetAllAsync())[0];
        afterSecondToggle.IsCompleted.Should().BeFalse();
    }
}

// -------------------------------------------------
// 6. bUnit test for TodoListBase
// -------------------------------------------------
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var testContext = new Bunit.TestContext();

        var mockService = Substitute.For<ITodoService>();
        var emptyList = new List<TodoItem>();
        mockService.GetAllAsync().Returns(Task.FromResult(emptyList));

        // Register the mock without additional using statements
        Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions
            .AddSingleton<ITodoService>(testContext.Services, mockService);

        var cut = testContext.RenderComponent<TodoListBase>();

        // Ensure the async lifecycle has completed
        await cut.InvokeAsync(() => Task.CompletedTask);

        await mockService.Received(1).GetAllAsync();
    }
}