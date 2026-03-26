using Microsoft.AspNetCore.Components;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using Bunit;

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
    private readonly List<TodoItem> items = new();

    public async Task<List<TodoItem>> GetAllAsync()
        => await Task.FromResult(new List<TodoItem>(items));

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(nextId++, title, false);
        items.Add(item);
        return await Task.FromResult(item);
    }

    public async Task DeleteAsync(int id)
    {
        var item = items.FirstOrDefault(x => x.Id == id);
        if (item != null) items.Remove(item);
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var item = items.FirstOrDefault(x => x.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        await Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
        => Todos = await TodoService.GetAllAsync();

    public async Task AddTodo()
    {
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
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();

        var todo = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));

        todo.Should().Be(new TodoItem(1, "Buy milk", false));
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        _ = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));
        _ = AsyncTestHelper.RunSync(service.AddAsync("Read book"));

        AsyncTestHelper.RunSync(service.DeleteAsync(1));

        service.items.Should().BeEquivalentTo(new[] { new TodoItem(2, "Read book", false) });
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        _ = AsyncTestHelper.RunSync(service.AddAsync("Buy milk"));

        AsyncTestHelper.RunSync(service.ToggleAsync(1));

        service.items.Should().BeEquivalentTo(new[] { new TodoItem(1, "Buy milk", true) });
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void RendersAndCallsGetAllAsyncOnInit()
    {
        using var ctx = new Bunit.TestContext();
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

        var cut = ctx.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, mockService));

        // Verify that GetAllAsync was called once
        mockService.Received(1).GetAllAsync();

        cut.Find("h1").TextContent.Should().Be("Todo List");
    }
}