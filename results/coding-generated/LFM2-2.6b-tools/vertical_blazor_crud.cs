using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
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
    private int _idCounter = 1;
    private List<TodoItem> _todos = new();

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(new List<TodoItem>(_todos));
    public Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_idCounter++, title, false);
        _todos.Add(newItem);
        return Task.FromResult(newItem);
    }
    public Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) _todos.Remove(item);
        return Task.CompletedTask;
    }
    public Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null) item.IsCompleted = !item.IsCompleted;
        return Task.CompletedTask;
    }
}

public class TodoListBase : ComponentBase
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var newItem = await TodoService.AddAsync(NewTitle);
        Todos.Add(newItem);
        NewTitle = "";
        await OnParametersChangedAsync();
    }
    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos.RemoveAll(t => t.Id == id);
        await OnParametersChangedAsync();
    }
    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await OnParametersChangedAsync();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Buy groceries";
        var item = await service.AddAsync(title);
        Assert.Equal(1, item.Id);
        Assert.Equal(title, item.Title);
        Assert.False(item.IsCompleted);
    }
    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = new TodoItem(1, "Test", false);
        service._todos.Add(item);
        await service.DeleteAsync(1);
        Assert.Empty(service._todos);
    }
    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = new TodoItem(1, "Test", false);
        service._todos.Add(item);
        await service.ToggleAsync(1);
        Assert.True(item.IsCompleted);
    }
}

public class TodoListBaseTests : Bunit.TestFixture
{
    [Bunit.Test]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var mockService = Substitute.For<ITodoService>();
        var component = new TodoListBase { TodoService = mockService };
        await component.OnInitializedAsync();
        mockService.GetAllAsync().Should().HaveReturned();
    }
}