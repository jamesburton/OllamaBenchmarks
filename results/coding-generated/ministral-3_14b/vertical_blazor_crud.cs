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

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return await Task.FromResult(todo);
    }

    public async Task DeleteAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) _todos.Remove(todo);
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
        await Task.CompletedTask;
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
        await service.ToggleAsync(todo.Id);

        var toggledTodo = (await service.GetAllAsync()).First();
        toggledTodo.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var service = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = service };
        await component.OnInitializedAsync();

        await service.Received(1).GetAllAsync();
    }

    [Fact]
    public async Task AddTodo_CallsAddAsyncAndReloadsList()
    {
        var service = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = service, NewTitle = "Test" };
        component.Todos = new List<TodoItem>();

        await component.AddTodo();

        await service.Received(1).AddAsync("Test");
        await service.Received(1).GetAllAsync();
        component.NewTitle.Should().BeEmpty();
    }
}