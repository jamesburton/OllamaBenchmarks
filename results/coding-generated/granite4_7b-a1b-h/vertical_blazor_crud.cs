using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;
using NSubstitute;
using Bunit;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService
{
    private static int nextId = 1;
    private readonly List<TodoItem> _todos = new();

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.AsReadOnly());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem
        {
            Id = nextId++,
            Title = title,
            IsCompleted = false
        };
        _todos.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = null!;

    public List<TodoItem> Todos { get; set; } = new();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        Todos.Add(await TodoService.AddAsync(NewTitle));
        NewTitle = "";
        await InvokeAsync(StateHasChanged);
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
    public async Task AddAsync_CreatesItemWithTitle()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        service.AddAsync("Test").Returns(Task.FromResult(new TodoItem { Title = "Test" }));

        // Act
        var result = await service.AddAsync("Test");

        // Assert
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var item = new TodoItem { Id = 1, Title = "Test" };
        service.GetAllAsync().Returns(new List<TodoItem> { item });
        service.DeleteAsync(1).Returns(Task.CompletedTask);

        // Act
        await service.DeleteAsync(1);
        var todos = await service.GetAllAsync();

        // Assert
        Assert.Empty(todos);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var item = new TodoItem { Id = 1, Title = "Test", IsCompleted = true };
        service.GetAllAsync().Returns(new List<TodoItem> { item });
        service.ToggleAsync(1).Returns(Task.CompletedTask);

        // Act
        await service.ToggleAsync(1);
        var todos = await service.GetAllAsync();

        // Assert
        Assert.False(todos.First().IsCompleted);
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_LoadsTodos()
    {
        // Arrange
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, Substitute.For<ITodoService>()
                .Add(x => x.GetAllAsync(), () => Task.FromResult(new List<TodoItem>()))));

        // Act
        var h1 = component.Find("h1");
        var todos = component.Instance.Todos;

        // Assert
        Assert.Equal("Todo List", h1.TextContent);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task AddTodo_AddsNewItem()
    {
        // Arrange
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, Substitute.For<ITodoService>()
                .Add(x => x.AddAsync("Test"), () => Task.FromResult(new TodoItem { Title = "Test" }))
                .Add(x => x.GetAllAsync(), () => Task.FromResult(new List<TodoItem>()))));

        // Act
        component.Find("input").SetValue("New Item");
        component.Find("button").Click();
        var todos = component.Instance.Todos;

        // Assert
        Assert.Single(todos);
        Assert.Equal("New Item", todos.First().Title);
    }

    [Fact]
    public async Task DeleteTodo_RemovesItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem> { new TodoItem { Id = 1, Title = "Test" } });
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, service));

        // Act
        component.Find($"[data-id='{1}']").Click();
        await component.InvokeAsync(() => component.Instance.DeleteTodo(1));
        var todos = component.Instance.Todos;

        // Assert
        Assert.Empty(component.Instance.Todos);
    }

    [Fact]
    public async Task ToggleTodo_FlipsIsCompleted()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var item = new TodoItem { Id = 1, Title = "Test", IsCompleted = true };
        service.GetAllAsync().Returns(new List<TodoItem> { item });
        service.ToggleAsync(1).Returns(Task.CompletedTask);
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, service));

        // Act
        component.Find($"[data-id='{1}']").Click();
        var todos = component.Instance.Todos;

        // Assert
        Assert.False(todos.First().IsCompleted);
    }
}

public class TodoListBaseIntegrationTests
{
    [Fact]
    public async Task RendersCorrectly()
    {
        // Arrange
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, Substitute.For<ITodoService>()
                .Add(x => x.GetAllAsync(), () => Task.FromResult(new List<TodoItem>()))));

        // Act
        var h1 = component.Find("h1");
        var todos = component.Instance.Todos;

        // Assert
        Assert.Equal("Todo List", h1.TextContent);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task AddsNewItem()
    {
        // Arrange
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, Substitute.For<ITodoService>()
                .Add(x => x.AddAsync("Test"), () => Task.FromResult(new TodoItem { Title = "Test" }))
                .Add(x => x.GetAllAsync(), () => Task.FromResult(new List<TodoItem>()))));

        // Act
        component.Find("input").SetValue("New Item");
        component.Find("button").Click();
        var todos = component.Instance.Todos;

        // Assert
        Assert.Single(todos);
        Assert.Equal("New Item", todos.First().Title);
    }

    [Fact]
    public async Task DeletesItem()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem> { new TodoItem { Id = 1, Title = "Test" } });
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, service));

        // Act
        component.Find($"[data-id='1']").Click();
        await component.InvokeAsync(() => component.Instance.DeleteTodo(1));
        var todos = component.Instance.Todos;

        // Assert
        Assert.Empty(component.Instance.Todos);
    }

    [Fact]
    public async Task TogglesCompleted()
    {
        // Arrange
        var service = Substitute.For<ITodoService>();
        var item = new TodoItem { Id = 1, Title = "Test", IsCompleted = true };
        service.GetAllAsync().Returns(new List<TodoItem> { item });
        service.ToggleAsync(1).Returns(Task.CompletedTask);
        var context = new TestContext();
        var component = context.RenderComponent<TodoListBase>(parameters => parameters
            .Add(x => x.TodoService, service));

        // Act
        component.Find($"[data-id='1']").Click();
        var todos = component.Instance.Todos;

        // Assert
        Assert.False(todos.First().IsCompleted);
    }
}