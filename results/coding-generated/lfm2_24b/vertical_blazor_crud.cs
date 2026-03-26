using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using AwesomeAssertions;
using Bunit;
using NSubstitute;

// 1. TodoItem record
public record TodoItem(int Id, string Title, bool IsCompleted);

// 2. ITodoService interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// 3. TodoService implementation
public class TodoService : ITodoService
{
    private int _nextId = 1;
    private readonly List<TodoItem> _items = new();

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(new List<TodoItem>(_items));

    public Task<TodoItem> AddAsync(string title) =>
        Task.FromResult(AddItem(title));

    public Task DeleteAsync(int id) =>
        Task.WhenAll(
            Task.WhenAll(_items.Where(i => i.Id == id).Select(i => Task.Run(() => _items.Remove(i)))),
            Task.Completed
        );

    public Task ToggleAsync(int id) =>
        Task.FromResult(ToggleItem(id));

    private TodoItem AddItem(string title) =>
        new(_nextId++, title, false)
    {
        It.Id.Should().Be(_nextId - 1),
        It.Title.Should().Be(title),
        It.IsCompleted.Should().BeFalse()
    };

    private Task ToggleItem(int id)
    {
        var item = _items.Find(i => i.Id == id);
        if (item is null) return Task.Completed;
        item = item with { IsCompleted = !item.IsCompleted };
        _items[_items.FindIndex(i => i.Id == id)] = item;
        return Task.Completed;
    }
}

// 4. TodoListBase component
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadTodos();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var item = await TodoService.AddAsync(NewTitle);
        Todos = (await TodoService.GetAllAsync()).ToList();
        NewTitle = "";
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        Todos = (await TodoService.GetAllAsync()).ToList();
    }
}

// 5. xUnit tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Title");
        item.Title.Should().Be("Test Title");
        item.Id.Should().Be(1);
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Delete Me");
        await service.DeleteAsync(added.Id);
        var list = await service.GetAllAsync();
        list.Should().NotContain(i => i.Id == added.Id);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var added = await service.AddAsync("Toggle Me");
        added.IsCompleted.Should().BeFalse();
        await service.ToggleAsync(added.Id);
        var toggled = await service.GetAllAsync();
        toggled[0].IsCompleted.Should().BeTrue();
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests : TestContext
{
    [Fact]
    public async Task Render_VerifiesGetAllAsyncCalledOnInit()
    {
        var mockService = Substitute.For<ITodoService>();
        var mockServiceMock = MockFor<ITodoService>(mockService);
        mockServiceMock.GetAllAsync().ReturnsAsync(new List<TodoItem>
        {
            new() { Id = 1, Title = "Test", IsCompleted = false }
        });

        var cut = RenderComponent<TodoListBase>(parameters: new { TodoListBase = mockServiceMock });
        cut.Markup.Should().Contain("Test");
        await cut.Instance.LoadTodos();
        await cut.Instance.TodoService.Received(1).GetAllAsync();
    }
}