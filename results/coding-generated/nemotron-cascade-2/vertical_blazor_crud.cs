using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_items.ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return Task.FromResult(item);
    }

    public Task DeleteAsync(int id) => Task.FromResult(_items.RemoveAll(i => i.Id == id) > 0);

    public Task ToggleAsync(int id) => Task.FromResult(_items.FirstOrDefault(i => i.Id == id)?.IsCompleted = !_items.FirstOrDefault(i => i.Id == id)?.IsCompleted);

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
        await base.OnInitializedAsync();
    }

    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";
    public async Task AddTodo() => await TodoService.AddAsync(NewTitle), Todos = await TodoService.GetAllAsync(), NewTitle = "";
    public async Task DeleteTodo(int id) => await TodoService.DeleteAsync(id), Todos = await TodoService.GetAllAsync();
    public async Task ToggleTodo(int id) => await TodoService.ToggleAsync(id), Todos = await TodoService.GetAllAsync();
}

public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";
    protected override async Task OnInitializedAsync() => await LoadTodosAsync();
    public async Task AddTodo() => await TodoService.AddAsync(NewTitle), await LoadTodosAsync(), NewTitle = "";
    public async Task DeleteTodo(int id) => await TodoService.DeleteAsync(id), await LoadTodosAsync();
    public async Task ToggleTodo(int id) => await TodoService.ToggleAsync(id), await LoadTodosAsync();
    private async Task LoadTodosAsync() => Todos = await TodoService.GetAllAsync();
}

public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Test";
        var item = await service.AddAsync(title);
        item.Title.Should().Be(title);
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Item");
        await service.DeleteAsync(item.Id);
        var all = await service.GetAllAsync();
        all.Should().NotContain(i => i.Id == item.Id);
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Toggle");
        await service.ToggleAsync(item.Id);
        var refreshed = await service.GetAllAsync();
        refreshed.First(i => i.Id == item.Id).IsCompleted.Should().BeTrue();
        await service.ToggleAsync(item.Id);
        var refreshed2 = await service.GetAllAsync();
        refreshed2.First(i => i.Id == item.Id).IsCompleted.Should().BeFalse();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public void OnInitializedAsync_CallsGetAllAsync()
    {
        var mock = Substitute.For<ITodoService>();
        mock.GetAllAsync().Returns(new List<TodoItem> { new(1, "Test", false) });

        var testContext = new Bunit.TestContext();
        var cut = testContext.RenderComponent<TodoListBase>();

        cut.Instance.Todos.Should().HaveCount(1);
        cut.Instance.Todos[0].Title.Should().Be("Test");
        mock.Received(1).GetAllAsync();
    }
}