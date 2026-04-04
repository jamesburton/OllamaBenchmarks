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

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            var index = _todos.IndexOf(item);
            _todos[index] = item with { IsCompleted = !item.IsCompleted };
        }
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
        await LoadTodos();
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await LoadTodos();
        }
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
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_ShouldCreateItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();
        var title = "Test Todo";

        // Act
        var result = await service.AddAsync(title);

        // Assert
        result.Title.Should().Be(title);
        result.IsCompleted.Should().BeFalse();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        // Arrange
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initialCount = (await service.GetAllAsync()).Count;

        // Act
        await service.DeleteAsync(item.Id);
        var items = await service.GetAllAsync();

        // Assert
        items.Count.Should().Be(initialCount - 1);
        items.Should().NotContain(x => x.Id == item.Id);
    }

    [Fact]
    public async Task ToggleAsync_ShouldFlipIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initialStatus = item.IsCompleted;

        // Act
        await service.ToggleAsync(item.Id);
        var updatedItem = (await service.GetAllAsync()).First(x => x.Id == item.Id);

        // Assert
        updatedItem.IsCompleted.Should().Be(!initialStatus);
    }
}

public class TodoListTests
{
    [Fact]
    public void OnInitializedAsync_ShouldLoadTodos()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Test", false) };
        mockService.GetAllAsync().Returns(todos);

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(mockService);

        // Act
        var cut = ctx.RenderComponent<TodoListBase>();

        // Assert
        mockService.Received(1).GetAllAsync();
        cut.Instance.Todos.Should().BeEquivalentTo(todos);
    }
}