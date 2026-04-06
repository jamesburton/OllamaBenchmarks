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

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(new List<TodoItem>(_todos));

    public Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return Task.FromResult(todo);
    }

    public Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) _todos.Remove(todo);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            Todos = (await TodoService.GetAllAsync()).ToList();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Title");

        todo.Should().NotBeNull();
        todo.Title.Should().Be("Test Title");
        todo.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Title");
        await service.DeleteAsync(todo.Id);

        (await service.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = await service.AddAsync("Test Title");
        todo.IsCompleted.Should().BeFalse();

        await service.ToggleAsync(todo.Id);
        (await service.GetAllAsync()).First().IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var ctx = Bunit.TestContext.Create();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem> { new(1, "Test", false) });

        var cut = ctx.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, service));

        await cut.Instance.OnInitializedAsync();
        await service.Received().GetAllAsync();
    }
}