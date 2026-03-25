using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Bunit;
using AwesomeAssertions;

#region TodoItem Record

public record TodoItem(int Id, string Title, bool IsCompleted);

#endregion

#region ITodoService Interface

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

#endregion

#region TodoService Implementation

public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public Task<List<TodoItem>> GetAllAsync()
    {
        return Task.FromResult(_todos.ToList());
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todo = new TodoItem(_nextId++, title, false);
        _todos.Add(todo);
        return todo;
    }

    public async Task DeleteAsync(int id)
    {
        _todos.RemoveAll(x => x.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.Find(x => x.Id == id);
        if (todo != null)
        {
            todo.IsCompleted = !todo.IsCompleted;
        }
    }
}

#endregion

#region TodoListBase Component

using Microsoft.AspNetCore.Components;

public partial class TodoListBase : ComponentBase
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

#endregion

#region xUnit Tests for TodoService

[TestFixture]
public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "Test Title";
        var newItem = await service.AddAsync(title);

        newItem.Should().NotBeNull();
        newItem.Title.Should().Be(title);
        newItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        var initialCount = service._todos.Count;
        var todo = new TodoItem(1, "Test", false);
        service._todos.Add(todo);

        await service.DeleteAsync(1);

        service._todos.Count.Should().Be(initialCount);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        var todo = new TodoItem(1, "Test", false);
        service._todos.Add(todo);

        await service.ToggleAsync(1);

        service._todos.Find(x => x.Id == 1).IsCompleted.Should().BeTrue();
    }
}

#endregion

#region bUnit Test for TodoListBase

[TestFixture]
public class TodoListBaseTests
{
    [Test]
    public async Task Renders_CallsGetAllAsyncOnInit()
    {
        var serviceMock = Substitute.For<ITodoService>();
        var component = new TodoListBase();
        component.TodoService = serviceMock;

        await component.OnInitializedAsync();

        await serviceMock.Received().GetAllAsync();
    }
}

#endregion