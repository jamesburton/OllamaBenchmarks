using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Bunit;
using Xunit;
using AwesomeAssertions;

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
    private readonly List<TodoItem> _todos;
    private int _nextId;

    public TodoService()
    {
        _todos = new List<TodoItem>();
        _nextId = 1;
    }

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.ToList());
    }

    public Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId, title, false);
        _nextId++;
        _todos.Add(newItem);
        return Task.FromResult(newItem);
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

// 4. TodoListBase Component
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = null!;
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

// 5. xUnit v3 tests for TodoService
public class TodoServiceTests
{
    [Fact]
    public void AddAsyncCreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var result = service.AddAsync("Test Task").Result;
        result.Should().BeEquivalentTo(new TodoItem(1, "Test Task", false));
    }

    [Fact]
    public void DeleteAsyncRemovesItem()
    {
        var service = new TodoService();
        service.AddAsync("Item 1").Result.Should().BeEquivalentTo(new TodoItem(1, "Item 1", false));
        service.AddAsync("Item 2").Result.Should().BeEquivalentTo(new TodoItem(2, "Item 2", false));

        service.DeleteAsync(1).Wait();
        service.GetAllAsync().Result.Should().ContainOnly(new TodoItem(2, "Item 2", false));
    }

    [Fact]
    public void ToggleAsyncFlipsIsCompleted()
    {
        var service = new TodoService();
        service.AddAsync("Task").Result.Should().BeEquivalentTo(new TodoItem(1, "Task", false));

        service.ToggleAsync(1).Wait();
        service.GetAllAsync().Result.Should().Contain(t => t.IsCompleted == true);
    }

    [Theory, MemberData(nameof(ToggleTestData))]
    public void ToggleAsyncFlipsSpecificId(int id, bool expectedState)
    {
        var service = new TodoService();
        service.AddAsync("Task").Result.Should().BeEquivalentTo(new TodoItem(1, "Task", false));
        service.AddAsync("Task").Result.Should().BeEquivalentTo(new TodoItem(2, "Task", false));

        service.ToggleAsync(id).Wait();
        var item = service.GetAllAsync().Result.FirstOrDefault(t => t.Id == id);
        item.Should().NotBeNull();
        item!.IsCompleted.Should().Be(expectedState);
    }

    public static MatrixTheoryData<int, bool> ToggleTestData =>
        new([1, 2], [false, true]);

    [Fact]
    public void GetAllAsyncReturnsCopy()
    {
        var service = new TodoService();
        service.AddAsync("A").Result.Should().BeEquivalentTo(new TodoItem(1, "A", false));
        service.AddAsync("B").Result.Should().BeEquivalentTo(new TodoItem(2, "B", false));

        var list1 = service.GetAllAsync().Result;
        list1.Add(new TodoItem(999, "C", false));

        var list2 = service.GetAllAsync().Result;
        list2.Should().NotContain(new TodoItem(999, "C", false));
    }
}

// 6. bUnit test for TodoListBase
public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsyncCallsGetAllAsync()
    {
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem> { new TodoItem(1, "Init", false) }));

        var cut = Bunit.TestContext.RenderComponent<TodoListBase>(p =>
            p.Add(p => p.TodoService, mockService));

        await Task.Delay(10); // Allow async initialization to complete

        mockService.Received(1).GetAllAsync();
    }
}