using Microsoft.AspNetCore.Components;

record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
{
    System.Threading.Tasks.Task<List<TodoItem>> GetAllAsync();
    System.Threading.Tasks.Task<TodoItem> AddAsync(string title);
    System.Threading.Tasks.Task DeleteAsync(int id);
    System.Threading.Tasks.Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private readonly System.Collections.Generic.List<TodoItem> items = new();
    private int nextId = 1;

    public System.Threading.Tasks.Task<List<TodoItem>> GetAllAsync() =>
        System.Threading.Tasks.Task.FromResult(new System.Collections.Generic.List<TodoItem>(items));

    public System.Threading.Tasks.Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(nextId++, title, false);
        items.Add(item);
        return System.Threading.Tasks.Task.FromResult(item);
    }

    public System.Threading.Tasks.Task DeleteAsync(int id)
    {
        items.RemoveAll(i => i.Id == id);
        return System.Threading.Tasks.Task.CompletedTask;
    }

    public System.Threading.Tasks.Task ToggleAsync(int id)
    {
        var item = items.Find(i => i.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        return System.Threading.Tasks.Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public System.Collections.Generic.List<TodoItem> Todos { get; set; } = new System.Collections.Generic.List<TodoItem>();
    public string NewTitle { get; set; } = "";

    protected override async System.Threading.Tasks.Task OnInitializedAsync()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
        await base.OnInitializedAsync();
    }

    public async System.Threading.Tasks.Task AddTodo()
    {
        await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        await LoadAsync();
    }

    public async System.Threading.Tasks.Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadAsync();
    }

    public async System.Threading.Tasks.Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadAsync();
    }

    private async System.Threading.Tasks.Task LoadAsync()
    {
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

// xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = service.AddAsync("Test").Result;

        item.Should().NotBeNull();
        item.Title.Should().Be("Test");
        item.IsCompleted.Should().BeFalse();
        item.Id.Should().Be(1);
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        service.AddAsync("First").Wait();
        service.AddAsync("Second").Wait();

        service.DeleteAsync(1).Wait();

        var all = service.GetAllAsync().Result;
        all.Should().NotContain(i => i.Id == 1);
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = service.AddAsync("Toggle").Result;
        service.ToggleAsync(item.Id).Wait();

        item.IsCompleted.Should().BeTrue();
    }
}

// bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public void TodoListBase_Renders_And_Calls_GetAllAsync_OnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new System.Collections.Generic.List<TodoItem>
        {
            new TodoItem(1, "Sample", false)
        });

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<ITodoService>(mockService);
        var cut = ctx.RenderComponent<TodoListBase>();

        mockService.Received(1).GetAllAsync();
    }
}