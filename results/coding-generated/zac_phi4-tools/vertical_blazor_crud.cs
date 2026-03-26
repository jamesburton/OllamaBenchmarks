using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Xunit;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos.ToList());
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(++_nextId, title, false);
        _todos.Add(newItem);
        return await Task.FromResult(newItem);
    }

    public async Task DeleteAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            _todos.Remove(item);
        }
        await Task.CompletedTask;
    }

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            item.IsCompleted = !item.IsCompleted;
        }
        await Task.CompletedTask;
    }
}

public partial class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = default!;

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
            await LoadTodos();
        }
    }

    private async Task LoadTodos()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        await LoadTodos();
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        await LoadTodos();
    }
}

public class TodoServiceTests
{
    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var title = "New Task";
        var item = await service.AddAsync(title);

        item.Title.Should().Be(title);
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var service = new TodoService();
        await service.AddAsync("Task 1");
        await service.DeleteAsync(1);

        var items = await service.GetAllAsync();
        items.Count.Should().Be(0);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var service = new TodoService();
        await service.AddAsync("Task 1");

        await service.ToggleAsync(1);
        var item = (await service.GetAllAsync()).First();

        item.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsGetAllAsync()
    {
        var todoService = Substitute.For<ITodoService>();
        var component = new TestContext().RenderComponent<TodoListBase>(parameters => parameters.Add(p => p.TodoService, todoService));

        await component.Instance.InvokeAsync(() => { });

        todoService.Received(1).GetAllAsync();
    }
}