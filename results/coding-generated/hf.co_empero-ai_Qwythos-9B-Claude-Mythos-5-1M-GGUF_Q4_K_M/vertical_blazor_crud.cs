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

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(Id: _nextId++, Title, IsCompleted: false);
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) _todos.Remove(item);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        return Task.CompletedTask;
    }
}

public abstract class TodoListBase : ComponentBase
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var newItem = await TodoService.AddAsync(NewTitle);
        Todos = await TodoService.GetAllAsync();
        NewTitle = "";
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

public class TodoList : TodoListBase { }

[Fact]
public async Task AddAsync_CreatesItemWithCorrectTitle()
{
    var service = new TodoService();
    var item = await service.AddAsync("Test");
    item.Should().NotBeNull();
    item.Title.Should().Be("Test");
}

[Fact]
public async Task DeleteAsync_RemovesItem()
{
    var service = new TodoService();
    await service.AddAsync("A");
    await service.DeleteAsync(1);
    (await service.GetAllAsync()).Should().BeEmpty();
}

[Fact]
public async Task ToggleAsync_FlipsIsCompleted()
{
    var service = new TodoService();
    var item = await service.AddAsync("B");
    item.IsCompleted.Should().BeFalse();
    await service.ToggleAsync(1);
    (await service.GetAllAsync()).Single().IsCompleted.Should().BeTrue();
}

[Fact]
public async Task OnInitializedAsync_LoadsTodos()
{
    var sub = Substitute.For<ITodoService>();
    sub.GetAllAsync().Returns(new List<TodoItem>());
    var ctx = new Bunit.TestContext();
    await ctx.RenderComponent<TodoList>(p => p.WithParameter("TodoService", sub));
    sub.Received(1).GetAllAsync();
}