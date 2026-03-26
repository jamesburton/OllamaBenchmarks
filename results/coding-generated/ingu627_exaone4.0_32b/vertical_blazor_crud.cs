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
    private int nextId = 1;
    private readonly List<TodoItem> todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(nextId++, title, false);
        todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        todos.RemoveAll(t => t.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var todo = todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
    }
}

public partial class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }

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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = await service.AddAsync("Test");
        result.Should().NotBeNull();
        result.Title.Should().Be("Test");
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        var initialCount = service.GetAllAsync().Result.Count;
        var idToDelete = service.GetAllAsync().Result[0].Id;
        await service.DeleteAsync(idToDelete);
        var finalCount = service.GetAllAsync().Result.Count;
        finalCount.Should().Be(initialCount - 1);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Test");
        var initial = service.GetAllAsync().Result[0];
        initial.IsCompleted.Should().BeFalse();
        await service.ToggleAsync(initial.Id);
        var updated = service.GetAllAsync().Result[0];
        updated.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task Render_CallsGetAllAsyncOnInit()
    {
        var ctx = new Bunit.TestContext();
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());

        var cut = ctx.RenderComponent<TodoListBase>(c => c.TodoService(service));

        service.Received().GetAllAsync();
    }
}