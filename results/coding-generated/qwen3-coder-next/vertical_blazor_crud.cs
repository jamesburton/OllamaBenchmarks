using Microsoft.AspNetCore.Components;

// 1. TodoItem record
public record TodoItem(int Id, string Title, bool IsCompleted);

// 2. ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// 3. TodoService implementation
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

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
        _items.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _items.FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            _items.Remove(item);
            _items.Add(item with { IsCompleted = !item.IsCompleted });
        }
        return Task.CompletedTask;
    }
}

// 4. TodoListBase component code-behind
public partial class TodoListBase : ComponentBase
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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();

        // Act
        var result = await service.AddAsync("Test Todo");

        // Assert
        result.Title.Should().Be("Test Todo");
        result.Id.Should().Be(1);
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        await service.AddAsync("Todo 1");
        await service.AddAsync("Todo 2");
        var itemsBefore = await service.GetAllAsync();
        itemsBefore.Count.Should().Be(2);

        // Act
        await service.DeleteAsync(1);

        // Assert
        var itemsAfter = await service.GetAllAsync();
        itemsAfter.Count.Should().Be(1);
        itemsAfter[0].Id.Should().Be(2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        var item = await service.AddAsync("Todo");
        item.IsCompleted.Should().BeFalse();

        // Act
        await service.ToggleAsync(item.Id);

        // Assert
        var updatedItems = await service.GetAllAsync();
        updatedItems[0].IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        // Arrange
        var mockService = NSubstitute.Substitute.For<ITodoService>();
        var initialTodos = new List<TodoItem>
        {
            new(1, "Test Todo", false)
        };
        mockService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<ITodoService>(mockService);

        // Act
        var cut = ctx.RenderComponent<TodoListBase>();

        // Assert
        await mockService.Received().GetAllAsync();
    }
}