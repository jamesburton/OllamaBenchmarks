using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public record TodoItem(int Id, string Title, bool IsCompleted);

// Interface
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// Implementation
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync() =>
        new List<TodoItem>(_items);

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id) =>
        _items.RemoveAll(i => i.Id == id);

    public async Task ToggleAsync(int id) =>
        _items.Find(i => i.Id == id)?.IsCompleted = !_items.Find(i => i.Id == id).IsCompleted;
}

// Component code-behind
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
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
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

// xUnit v3 tests
public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = service.AddAsync("Test").Result;

        result.Title.Should().Be("Test");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        service.AddAsync("Item1").Wait();
        service.AddAsync("Item2").Wait();

        service.DeleteAsync(1).Wait();
        var items = service.GetAllAsync().Result;

        items.Should().HaveCount(1);
        items[0].Title.Should().Be("Item2");
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        service.AddAsync("Item").Wait();
        var id = 1;

        service.ToggleAsync(id).Wait();
        var item = service.GetAllAsync().Result.First();

        item.IsCompleted.Should().BeTrue();
    }
}

// bUnit test
public class TodoListBaseTests
{
    [Fact]
    public void OnInitializedAsync_CallsGetAllAsync()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem>
        {
            new TodoItem(1, "Test", false)
        });

        var context = new Bunit.TestContext();
        var component = context.RenderComponent<TodoListBase>(b =>
        {
            b.AddParameter(x => x.TodoService, mockService);
        });

        // Act
        await component.Instance.OnInitializedAsync();

        // Assert
        mockService.Received().GetAllAsync();
    }
}