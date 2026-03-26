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
        var item = new TodoItem(_nextId++, title, false);
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

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = null!;

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = await TodoService.GetAllAsync();
        StateHasChanged();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync();
        StateHasChanged();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync();
        StateHasChanged();
    }
}

public class TodoServiceTests
{
    private readonly TodoService _service = new();

    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var result = await _service.AddAsync("Test Title");

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Title");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        await _service.AddAsync("Item 1");
        await _service.AddAsync("Item 2");
        var id = await _service.AddAsync("Item 3");

        await _service.DeleteAsync(id);

        var all = await _service.GetAllAsync();
        all.Should().NotContain(x => x.Id == id);
        all.Should().HaveCount(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var item = await _service.AddAsync("Toggle Me");
        var id = item.Id;

        await _service.ToggleAsync(id);

        var all = await _service.GetAllAsync();
        var updated = all.First(x => x.Id == id);
        updated.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task Rendered_CallsGetAllAsync()
    {
        var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        ctx.Services.AddSingleton<ITodoService>(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        await mockService.Received().GetAllAsync();
    }
}