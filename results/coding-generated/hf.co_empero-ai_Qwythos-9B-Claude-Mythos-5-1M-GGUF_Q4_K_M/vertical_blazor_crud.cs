using Microsoft.AspNetCore.Components;

public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService {
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService {
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());
    public Task<TodoItem> AddAsync(string title) {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return Task.FromResult(item);
    }
    public Task DeleteAsync(int id) {
        _todos.RemoveAll(i => i.Id == id);
        return Task.CompletedTask;
    }
    public Task ToggleAsync(int id) {
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        return Task.CompletedTask;
    }
}

public partial class TodoListBase : ComponentBase {
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync() {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo() {
        var item = await TodoService.AddAsync(NewTitle);
        Todos = await TodoService.GetAllAsync();
        NewTitle = "";
    }

    public async Task DeleteTodo(int id) {
        await TodoService.DeleteAsync(id);
        Todos = await TodoService.GetAllAsync();
    }

    public async Task ToggleTodo(int id) {
        await TodoService.ToggleAsync(id);
        Todos = await TodoService.GetAllAsync();
    }
}

[Fact]
public void AddAsync_CreatesItemWithCorrectTitle() {
    var service = new TodoService();
    var item = await service.AddAsync("Buy groceries");
    item.Should().NotBeNull();
    item.Id.Should().Be(1);
    item.Title.Should().Be("Buy groceries");
    item.IsCompleted.Should().BeFalse();
}

[Fact]
public async Task DeleteAsync_RemovesItem() {
    var service = new TodoService();
    await service.AddAsync("Delete me");
    await service.DeleteAsync(1);
    (await service.GetAllAsync()).Should().BeEmpty();
}

[Fact]
public async Task ToggleAsync_FlipsIsCompleted() {
    var service = new TodoService();
    var item = await service.AddAsync("Toggle me");
    await service.ToggleAsync(1);
    (await service.GetAllAsync()).Single().IsCompleted.Should().BeTrue();
}

[Fact]
public void OnInitializedAsync_CallsGetAllAsync() {
    var service = Substitute.For<ITodoService>();
    var ctx = new Bunit.TestContext();
    ctx.Services.AddTransient<ITodoService>(service);
    var cut = ctx.RenderComponent<TodoListBase>();
    cut.WaitForState(); // ensure OnInitializedAsync runs
    service.Received().GetAllAsync();
}