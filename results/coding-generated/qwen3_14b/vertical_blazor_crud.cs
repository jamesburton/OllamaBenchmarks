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
    private List<TodoItem> _todos = new List<TodoItem>();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId, title, false);
        _todos.Add(item);
        _nextId++;
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new List<TodoItem>();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle))
            return;

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

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = Substitute.For<ITodoService>();
        var expectedTitle = "Test Todo";
        var expectedId = 1;

        service.AddAsync(Arg.Is<string>(t => t == expectedTitle))
            .Returns(new TodoItem(expectedId, expectedTitle, false));

        var result = await service.AddAsync(expectedTitle);

        result.Title.Should().Be(expectedTitle);
        result.Id.Should().Be(expectedId);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var items = new List<TodoItem>
        {
            new TodoItem(1, "Item 1", false),
            new TodoItem(2, "Item 2", false)
        };
        service.GetAllAsync().Returns(items);

        await service.DeleteAsync(1);

        var result = await service.GetAllAsync();
        result.Should().HaveCount(1);
        result.Should().Contain(t => t.Id == 2);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var item = new TodoItem(1, "Test", false);
        service.GetAllAsync().Returns(new List<TodoItem> { item });

        await service.ToggleAsync(1);

        var result = await service.GetAllAsync();
        result.First().IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task TodoListBase_CallsGetAllAsyncOnInit()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem>
        {
            new TodoItem(1, "Test Todo", false)
        };
        service.GetAllAsync().Returns(todos);

        var ctx = new Bunit.TestContext();
        var cut = ctx.RenderComponent<TodoListBase>(p =>
            p.Add(x => x.TodoService, service));

        service.Received().GetAllAsync();
    }
}