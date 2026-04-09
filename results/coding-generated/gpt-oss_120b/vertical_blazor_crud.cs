using Microsoft.AspNetCore.Components;

record TodoItem(int Id, string Title, bool IsCompleted);

interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
        => Task.FromResult(new List<TodoItem>(_items));

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
            var item = _items[index];
            _items[index] = item with { IsCompleted = !item.IsCompleted };
        }
        return Task.CompletedTask;
    }
}

class TodoListBase : ComponentBase
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

/* Minimal component to render the base class in bUnit tests */
class TodoListComponent : TodoListBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) { }
}

/* ---------- xUnit v3 tests for TodoService ---------- */
class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test task");
        item.Title.Should().Be("Test task");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var a = await service.AddAsync("A");
        var b = await service.AddAsync("B");
        await service.DeleteAsync(a.Id);
        var all = await service.GetAllAsync();
        all.Count.Should().Be(1);
        all[0].Id.Should().Be(b.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Toggle me");
        item.IsCompleted.Should().BeFalse();

        await service.ToggleAsync(item.Id);
        var afterToggle = (await service.GetAllAsync())[0];
        afterToggle.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);
        var afterSecond = (await service.GetAllAsync())[0];
        afterSecond.IsCompleted.Should().BeFalse();
    }
}

/* ---------- bUnit test for TodoListBase ---------- */
class TodoListBaseTests
{
    [Fact]
    public void OnInitializedAsync_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();

        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        ctx.Services.AddSingleton<ITodoService>(mockService);

        var cut = ctx.RenderComponent<TodoListComponent>();

        // Verify that GetAllAsync was called exactly once during initialization
        mockService.Received(1).GetAllAsync();
    }
}