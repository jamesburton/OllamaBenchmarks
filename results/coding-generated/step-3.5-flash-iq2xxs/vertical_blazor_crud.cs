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
    private readonly List<TodoItem> _todos = new List<TodoItem>();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newTodo = new TodoItem(_nextId++, title, false);
        _todos.Add(newTodo);
        return newTodo;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(x => x.Id == id);
        if (item != null) _todos.Remove(item);
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            var newItem = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            _todos.Remove(item);
            _todos.Add(newItem);
        }
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await ReloadTodos();
    }

    public async Task ReloadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await ReloadTodos();
        }
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await ReloadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await ReloadTodos();
    }
}

// Tests
public class TodoServiceTests
{
    private readonly TodoService _service = new TodoService();

    [Fact]
    public async void AddAsync_CreatesItemWithCorrectTitle()
    {
        var title = "Test Title";
        var result = await _service.AddAsync(title);
        result.Title.Should().Be(title);
    }

    [Fact]
    public async void DeleteAsync_RemovesItem()
    {
        var item = await _service.AddAsync("Test");
        await _service.DeleteAsync(item.Id);
        var all = await _service.GetAllAsync();
        all.Should().NotContain(x => x.Id == item.Id);
    }

    [Fact]
    public async void ToggleAsync_FlipsIsCompleted()
    {
        var item = await _service.AddAsync("Test");
        var initial = await _service.GetAllAsync();
        var originalItem = initial.First(x => x.Id == item.Id);
        await _service.ToggleAsync(item.Id);
        var after = await _service.GetAllAsync();
        var toggledItem = after.First(x => x.Id == item.Id);
        toggledItem.IsCompleted.Should().Be(!originalItem.IsCompleted);
    }
}

public class TodoListBaseTests
{
    private readonly ITodoService _mockService = Substitute.Substitute.For<ITodoService>(Substitute.AllToInstance().Instance);
    private readonly Bunit.TestContext _testContext = new Bunit.TestContext();

    [Fact]
    public async void Renders_Component_And_Verifies_GetAllAsync_Called_On_Init()
    {
        var mock = Substitute.For<ITodoService>();
        mock.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));
        var cut = new TodoListBase
        {
            TodoService = mock
        };
        await cut.OnInitializedAsync();
        mock.Received().GetAllAsync();
    }
}