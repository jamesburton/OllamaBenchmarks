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
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        var copy = _items.Select(i => i with { }).ToList();
        return Task.FromResult(copy);
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
            _items.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            var item = _items[index];
            var toggled = item with { IsCompleted = !item.IsCompleted };
            _items[index] = toggled;
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
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var items = await TodoService.GetAllAsync();
        Todos = items;
    }

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

public class TodoListComponent : TodoListBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddContent(1, "Todo List");
        builder.CloseElement();
    }
}

/* xUnit tests for TodoService */
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        item.Title.Should().Be("Test");
        item.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var a = await service.AddAsync("A");
        var b = await service.AddAsync("B");
        await service.DeleteAsync(a.Id);
        var all = await service.GetAllAsync();
        all.Should().ContainSingle(i => i.Id == b.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("C");
        await service.ToggleAsync(item.Id);
        var all = await service.GetAllAsync();
        var toggled = all.First(i => i.Id == item.Id);
        toggled.IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);
        all = await service.GetAllAsync();
        toggled = all.First(i => i.Id == item.Id);
        toggled.IsCompleted.Should().BeFalse();
    }
}

/* bUnit test for TodoListComponent */
public class TodoListComponentTests
{
    [Fact]
    public async Task OnInitialized_CallsGetAllAsync()
    {
        using var ctx = new Bunit.TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());
        ctx.Services.AddSingleton<ITodoService>(service);

        var cut = ctx.RenderComponent<TodoListComponent>();

        await service.Received(1).GetAllAsync();
    }
}