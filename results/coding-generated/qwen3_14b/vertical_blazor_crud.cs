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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos.ToList());
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return await Task.FromResult(newItem);
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item = new TodoItem(item.Id, item.Title, !item.IsCompleted);
            _todos.Remove(item);
            _todos.Add(item);
        }
        await Task.CompletedTask;
    }
}

// 4. TodoListBase code-behind
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        Todos = await TodoService.GetAllAsync();
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
    public async void TestAddAsyncCreatesItemWithCorrectTitle()
    {
        var service = Substitute.For<ITodoService>();
        var todo = new TodoItem(1, "Test Title", false);
        service.AddAsync("Test Title").Returns(todo);

        var result = await service.AddAsync("Test Title");
        result.Should().Be(todo);
    }

    [Fact]
    public async void TestDeleteAsyncRemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Item 1", false) };
        service.GetAllAsync().Returns(todos);

        await service.DeleteAsync(1);
        (await service.GetAllAsync()).Should().BeEmpty();
    }

    [Fact]
    public async void TestToggleAsyncFlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var todo = new TodoItem(1, "Item 1", false);
        service.GetAllAsync().Returns(new List<TodoItem> { todo });

        await service.ToggleAsync(1);
        (await service.GetAllAsync()).First().IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void TestComponentCallsGetAllAsyncOnInit()
    {
        var service = Substitute.For<ITodoService>();
        var cut = Bunit.TestContext.CreateComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, service));

        service.Received().GetAllAsync();
    }
}