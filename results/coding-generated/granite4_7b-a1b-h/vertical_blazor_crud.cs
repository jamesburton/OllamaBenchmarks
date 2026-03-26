using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private static int _nextId = 1;
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync() => await Task.FromResult(_todos.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _todos.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id) => _todos.RemoveAll(t => t.Id == id);

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null)
            todo.IsCompleted = !todo.IsCompleted;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

    public List<TodoItem> Todos { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo() => 
        Todos.Add(await TodoService.AddAsync(NewTitle));

    public async Task DeleteTodo(int id) => 
        Todos.RemoveAll(t => t.Id == id), await ReloadPage();

    public async Task ToggleTodo(int id) => 
        await TodoService.ToggleAsync(id), await ReloadPage();
}

public class TodoListBaseTests
{
    [Fact]
    public void AddAsync_CreatesNewItemWithCorrectTitle()
    {
        var service = Substitute.For<ITodoService>();
        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        component.InvokeAsync(() => component.AddTodo());

        service.Received().AddAsync(Arg.Any<string>());
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        var todo = new TodoItem(1, "Test", false);
        service.GetAllAsync().Returns(new List<TodoItem> { todo });

        component.InvokeAsync(() => component.DeleteTodo(1));
        service.Received().DeleteAsync(1);

        Assert.Empty(component.FindComponent<TodoListBase>().Todos);
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        var todo = new TodoItem(1, "Test", false);
        service.GetAllAsync().Returns(new List<TodoItem> { todo });

        component.InvokeAsync(() => component.ToggleTodo(1));
        Assert.True(component.FindComponent<TodoListBase>().Todos.First().IsCompleted);
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfTodos()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Test", false) };
        service.GetAllAsync().Returns(todos);

        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        Assert.Equal(todos, await component.InvokeAsync(() => component.Todos));
    }

    [Fact]
    public async Task AddAsync_CreatesNewItemWithCorrectId()
    {
        var service = Substitute.For<ITodoService>();
        service.GetAllAsync().Returns(new List<TodoItem>());
        service.AddAsync("Test").Returns(new TodoItem(1, "Test", false));

        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        await component.InvokeAsync(() => component.AddTodo());

        Assert.Equal(1, component.FindComponent<TodoListBase>().Todos.First().Id);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Test", false) };
        service.GetAllAsync().Returns(todos);
        service.DeleteAsync(1).Returns(Task.CompletedTask);

        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        await component.InvokeAsync(() => component.DeleteTodo(1));
        Assert.Empty(component.FindComponent<TodoListBase>().Todos);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = Substitute.For<ITodoService>();
        var todos = new List<TodoItem> { new(1, "Test", false) };
        service.GetAllAsync().Returns(todos);
        service.ToggleAsync(1).Returns(Task.CompletedTask);

        var component = RenderComponent<TodoListBase>(builder =>
            builder.Add(c => c.TodoService, service));

        await component.InvokeAsync(() => component.ToggleTodo(1));
        Assert.True(component.FindComponent<TodoListBase>().Todos.First().IsCompleted);
    }
}