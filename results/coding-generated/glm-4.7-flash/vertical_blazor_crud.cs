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

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.ToList());

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
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
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
    [Fact]
    public void Test_AddAsync_creates_item_with_correct_title()
    {
        var service = new TodoService();
        var result = service.AddAsync("Test Title").Result;

        result.Title.Should().Be("Test Title");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Test_DeleteAsync_removes_item()
    {
        var service = new TodoService();
        service.AddAsync("Item 1").Wait();
        service.AddAsync("Item 2").Wait();
        service.DeleteAsync(1).Wait();

        var items = service.GetAllAsync().Result;
        items.Should().HaveCount(1);
        items[0].Title.Should().Be("Item 2");
    }

    [Fact]
    public void Test_ToggleAsync_flips_IsCompleted()
    {
        var service = new TodoService();
        service.AddAsync("Item 1").Wait();
        var id = service.GetAllAsync().Result[0].Id;

        service.ToggleAsync(id).Wait();
        var items = service.GetAllAsync().Result;
        items[0].IsCompleted.Should().BeTrue();

        service.ToggleAsync(id).Wait();
        items = service.GetAllAsync().Result;
        items[0].IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void Test_Render_calls_GetAllAsync_on_init()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>());

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton(mockService);

        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received().GetAllAsync();
    }
}