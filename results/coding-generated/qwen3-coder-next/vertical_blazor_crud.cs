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
        if (item != default)
        {
            item = item with { IsCompleted = !item.IsCompleted };
            _items.Remove(item);
            _items.Add(item);
        }
        return Task.CompletedTask;
    }
}

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

public partial class TodoListBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "h2");
        builder.AddContent(1, "Todo List");
        builder.CloseElement();

        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "class", "input-group");
        builder.OpenElement(4, "input");
        builder.AddAttribute(5, "type", "text");
        builder.AddAttribute(6, "value", Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck(NewTitle));
        builder.AddAttribute(7, "oninput", EventCallback.Factory.CreateBinder<string>(this, __value => NewTitle = __value, NewTitle));
        builder.AddAttribute(8, "placeholder", "New todo...");
        builder.CloseElement();
        builder.OpenElement(9, "button");
        builder.AddAttribute(10, "onclick", EventCallback.Factory.Create(this, AddTodo));
        builder.AddContent(11, "Add");
        builder.CloseElement();
        builder.CloseElement();

        builder.OpenElement(12, "ul");
        foreach (var todo in Todos)
        {
            builder.OpenElement(13, "li");
            builder.AddAttribute(14, "class", todo.IsCompleted ? "completed" : "");
            builder.OpenElement(15, "span");
            builder.AddContent(16, todo.Title);
            builder.CloseElement();
            builder.OpenElement(17, "button");
            builder.AddAttribute(18, "onclick", EventCallback.Factory.Create<int>(this, id => ToggleTodo(todo.Id)));
            builder.AddContent(19, todo.IsCompleted ? "Undo" : "Complete");
            builder.CloseElement();
            builder.OpenElement(20, "button");
            builder.AddAttribute(21, "onclick", EventCallback.Factory.Create<int>(this, id => DeleteTodo(todo.Id)));
            builder.AddContent(22, "Delete");
            builder.CloseElement();
            builder.CloseElement();
        }
        builder.CloseElement();
    }
}

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
        var updatedItem = (await service.GetAllAsync()).First(i => i.Id == item.Id);
        updatedItem.IsCompleted.Should().BeTrue();
    }
}

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

        var component = new TodoListBase();
        component.TodoService = mockService;

        // Act
        await component.OnInitializedAsync();

        // Assert
        mockService.Received().GetAllAsync();
        component.Todos.Count.Should().Be(1);
        component.Todos[0].Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task RenderComponent_CallsGetAllAsyncOnInitialization()
    {
        // Arrange
        var mockService = NSubstitute.Substitute.For<ITodoService>();
        var initialTodos = new List<TodoItem>();
        mockService.GetAllAsync().Returns(Task.FromResult(initialTodos));

        var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<ITodoService>(mockService);

        // Act
        var cut = ctx.RenderComponent<TodoListBase>();

        // Assert
        mockService.Received().GetAllAsync();
    }
}