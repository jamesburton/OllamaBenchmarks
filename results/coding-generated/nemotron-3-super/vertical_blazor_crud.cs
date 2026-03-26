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
    private int _nextId = 1;
    private readonly List<TodoItem> _items = new();

    public Task<List<TodoItem>> GetAllAsync()
    => Task.FromResult(_items.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title.Trim(), false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null) _items.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item = item with { IsCompleted = !item.IsCompleted };
            var index = _items.FindIndex(i => i.Id == id);
            _items[index] = item;
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Buy milk");
        added.Title.Should().Be("Buy milk");
        added.IsCompleted.Should().BeFalse();
        added.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("First");
        await service.AddAsync("Second");
        await service.DeleteAsync(1);
        var all = await service.GetAllAsync();
        all.Should().HaveCount(1);
        all[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Task");
        await service.ToggleAsync(added.Id);
        var toggled = await service.GetAllAsync();
        toggled.Should().ContainSingle();
        toggled[0].IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Render_CallsGetAllAsyncOnInit()
    {
        var todoService = Substitute.For<ITodoService>();
        todoService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(todoService);

        var cut = ctx.RenderComponent<TodoListBase>();

        todoService.Received(1).GetAllAsync();
    }
}