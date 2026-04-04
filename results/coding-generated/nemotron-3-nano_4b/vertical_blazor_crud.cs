interface ITodoService  
{  
    Task<List<TodoItem>> GetAllAsync();  
    Task<TodoItem> AddAsync(string title);  
    Task DeleteAsync(int id);  
    Task<TodoItem> ToggleAsync(int id);  
}  

class TodoService  
{  
    private readonly List<TodoItem> _items = new();  
    private int _nextId;  

    public async Task<List<TodoItem>> GetAllAsync()  
    {  
        _nextId++;  
        var newTodo = new TodoItem { Id = _nextId, Title = string.Empty, IsCompleted = false };  
        _items.Add(newTodo);  
        return new List<TodoItem>(_items);  
    }  

    public async Task<TodoItem> AddAsync(string title)  
    {  
        var todo = new TodoItem { Id = _nextId, Title = title, IsCompleted = false };  
        _items.Add(todo);  
        _nextId++;  
        return todo;  
    }  

    public async Task DeleteAsync(int id)  
    {  
        if (! _items.Contains(todo => todo.Id == id))  
            return;  
        _items.Remove(todo => todo.Id == id);  
    }  

    public async Task<TodoItem> ToggleAsync(int id)  
    {  
        var idx = _items.FindIndex(t => t.Id == id);  
        if (idx < 0 || idx >= _items.Count)  
            return _items[idx];  
        _items[idx].IsCompleted = !_items[idx].IsCompleted;  
        return _items[idx];  
    }  
}  

class TodoListBase : ComponentBase  
{  
    [Inject] public ITodoService TodoService { get; set; } = new() not null;  
    public List<TodoItem> Todos { get; set; } = new();  
    public string NewTitle { get; set; } = string.Empty;  

    protected override Task OnInitializedAsync()  
    {  
        Todos = await TodoService.GetAllAsync();  
        return default;  
    }  

    public async Task AddTodo()  
    {  
        var todo = await TodoService.AddAsync(NewTitle);  
        Todos = await TodoService.GetAllAsync();  
        NewTitle = string.Empty;  
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

[Fact]  
public void AddAsync_CreatesItemWithCorrectTitle()  
{  
    var service = new TodoService();  
    var todo = await service.AddAsync("Buy milk");  
    todo.Should().Be(new TodoItem { Id = 1, Title = "Buy milk", IsCompleted = false });  
}  

[Fact]  
public void DeleteAsync_RemovesItem()  
{  
    var service = new TodoService();  
    var first = await service.AddAsync("First item");  
    var second = await service.AddAsync("Second item");  
    service.DeleteAsync(first.Id);  
    var secondAfter = await service.GetAllAsync();  
    Assert.True(secondAfter.Any(t => t.Title == "Second item"));  
    Assert.False(secondAfter.Any(t => t.Title == "First item"));  
}  

[Fact]  
public async Task ToggleAsync_FlipsIsCompleted()  
{  
    var service = new TodoService();  
    await service.AddAsync("Toggle me");  
    var todo = await todo => todo.Id == 1;  
    await service.ToggleAsync(todo.Id);  
    todo.Should().Be(new TodoItem { Id = 1, Title = "Toggle me", IsCompleted = true });  
}  

void TestGetAllAsyncCalledOnInit()  
{  
    var ctc = TestContext.Current;  
    var cut = ctc.GetComponent<TodoListBase>();  

    // Verify GetAllAsync is called with no arguments  
    cut.DidReceiveAsyncCall("GetAllAsync").Should().BeTrue();  

    // Verify items are loaded (using in‑memory list mock)  
    var mockService = (MockITodoService)cut.TodoService;  
    mockService.GetAllAsync(Returns(new List<TodoItem>()...));  
}  

// Global usings already present: using Microsoft.AspNetCore.Components; using Xunit; using AwesomeAssertions; using NSubstitute; using Bunit;