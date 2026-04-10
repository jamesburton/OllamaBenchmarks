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
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(new List<TodoItem>(_items));
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        _items.RemoveAll(x => x.Id == id);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var index = _items.FindIndex(x => x.Id == id);
        if (index >= 0)
        {
            var old = _items[index];
            _items[index] = old with { IsCompleted = !old.IsCompleted };
        }
        return Task.CompletedTask;
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
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Test todo";

        var result = await service.AddAsync(title);

        result.Title.Should().Be(title);
        result.IsCompleted.Should().BeFalse();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Item to delete");
        var initial = await service.GetAllAsync();
        initial.Should().Contain(item);

        await service.DeleteAsync(item.Id);

        var final = await service.GetAllAsync();
        final.Should().NotContain(item);
    }

    [Fact]
    public async Task ToggleAsync_FlippedIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test");
        var initial = await service.GetAllAsync();
        initial[0].IsCompleted.Should().BeFalse();

        await service.ToggleAsync(item.Id);

        var final = await service.GetAllAsync();
        final[0].IsCompleted.Should().BeTrue();

        await service.ToggleAsync(item.Id);

        var final2 = await service.GetAllAsync();
        final2[0].IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Component_CallsGetAllAsyncOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        var expectedItems = new List<TodoItem>
        {
            new(1, "Test", false)
        };
        mockService.GetAllAsync().Returns(expectedItems);

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received(1).GetAllAsync();
        cut.Instance.Todos.Should().BeEquivalentTo(expectedItems);
    }
}