public record TodoItem(int Id, string Title, bool IsCompleted);

// ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// TodoService implementation
public class TodoService : ITodoService
{
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return Task.FromResult(newItem);
    }

    public Task DeleteAsync(int id)
    {
        _todos.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }

    private readonly List<TodoItem> _todos = new();
}

// TodoListBase component
public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
        StateHasChanged();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var newItem = await TodoService.AddAsync(NewTitle);
            Todos.Add(newItem);
            NewTitle = "";
            StateHasChanged();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos.RemoveAll(t => t.Id == id);
        StateHasChanged();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = Todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        StateHasChanged();
    }
}

// xUnit tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var newItem = await service.AddAsync("Test Todo");
        newItem.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item1 = await service.AddAsync("Todo 1");
        var item2 = await service.AddAsync("Todo 2");

        await service.DeleteAsync(item1.Id);

        var remainingTodos = await service.GetAllAsync();
        remainingTodos.Should().NotContain(item1);
        remainingTodos.Should().Contain(item2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Todo");

        await service.ToggleAsync(item.Id);
        item.IsCompleted.Should().Be(true);

        await service.ToggleAsync(item.Id);
        item.IsCompleted.Should().Be(false);
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var mockService = NSubstitute.Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>();

        await cut.InvokeAsync(async () =>
        {
            await cut.OnInitializedAsync();
        });

        mockService.Received().GetAllAsync();
    }
}