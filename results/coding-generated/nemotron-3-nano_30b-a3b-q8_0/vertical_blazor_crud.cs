using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    private List<TodoItem> items = new();
    private int nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return new List<TodoItem>(items);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(nextId++, title, false);
        items.Add(item);
        return item;
    }

    public async Task DeleteAsync(int id)
    {
        items.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleAsync(int id)
    {
        var item = items.Find(i => i.Id == id);
        if (item != null) item = item with { IsCompleted = !item.IsCompleted };
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

public class TodoServiceTests
{
    [Fact]
    public void AddAsync_CreatesItem_WithCorrectTitle()
    {
        // Arrange
        var service = new TodoService();
        // Act
        var result = service.AddAsync("Test Title");
        // Assert
        result.Result.Count.Should().BeGreaterThan(0);
        result.Result[0].Title.Should().Be("Test Title");
    }

    [Fact]
    public void DeleteAsync_RemovesItem()
    {
        // Arrange
        var service = new TodoService();
        service.AddAsync("Item1").Wait();
        service.AddAsync("Item2").Wait();
        // Act
        service.DeleteAsync(1).Wait();
        // Assert
        service.GetAllAsync().Result.Should().HaveCount(1);
        service.GetAllAsync().Result[0].Title.Should().Be("Item2");
    }

    [Fact]
    public void ToggleAsync_FlipsIsCompleted()
    {
        // Arrange
        var service = new TodoService();
        var item = service.AddAsync("Task").Result;
        // Act
        service.ToggleAsync(item.Id).Wait();
        // Assert
        var result = service.GetAllAsync().Result.First(x => x.Id == item.Id);
        result.IsCompleted.Should().BeTrue();
    }
}

public class TodoListBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_CallsService_GetAll()
    {
        // Arrange
        var mockService = Substitute.For<ITodoService>();
        mockService.GetAllAsync().Returns(new List<TodoItem> { new TodoItem(1, "Test", false) });

        // Act
        var context = TestContext.Empty.CreateWithServices();
        context.Services.AddSingleton<ITodoService>(mockService);
        var component = context.RenderComponent<TodoListBase>();
        await component.Instance.OnInitializedAsync();

        // Assert
        mockService.Received(1).GetAllAsync();
    }
}