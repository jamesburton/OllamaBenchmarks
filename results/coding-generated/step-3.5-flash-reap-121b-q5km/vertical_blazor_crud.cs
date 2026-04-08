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
    private int _nextId = 1;
    private readonly List<TodoItem> _items = new();

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_items.ToList());
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
        if (item != null) _items.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
            _items.Add(new TodoItem(item.Id, item.Title, !item.IsCompleted));
        }
        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await LoadTodos();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test Title");
        result.Title.Should().Be("Test Title");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var added = await service.AddAsync("ToDelete");
        await service.DeleteAsync(added.Id);
        var all = await service.GetAllAsync();
        all.Should().NotContain(i => i.Id == added.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Toggle");
        var initial = await service.GetAllAsync();
        var item = initial.First(i => i.Id == added.Id);
        item.IsCompleted.Should().BeFalse();
        await service.ToggleAsync(added.Id);
        var after = await service.GetAllAsync();
        var toggled = after.First(i => i.Id == added.Id);
        toggled.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void RendersAndLoadsTodosOnInit()
    {
        var substitute = Substitute.For<ITodoService>();
        substitute.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        var testContext = new Bunit.TestContext();
        testContext.Services.AddSingleton(substitute);
        var cut = testContext.RenderComponent<TodoListBase>();
        substitute.Received().GetAllAsync();
    }
}