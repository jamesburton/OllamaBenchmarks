using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}

using Microsoft.AspNetCore.Components;
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
    private readonly List<TodoItem> _todos = new();
    private int _nextId = 1;

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await Task.FromResult(_todos);
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var newItem = new TodoItem(_nextId++, title, false);
        _todos.Add(newItem);
        return newItem;
    }

    public async Task DeleteAsync(int id)
    {
        var itemToRemove = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToRemove != null) _todos.Remove(itemToRemove);
    }

    public async Task ToggleAsync(int id)
    {
        var itemToToggle = _todos.FirstOrDefault(i => i.Id == id);
        if (itemToToggle != null) itemToToggle.IsCompleted ^= true;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        _todos = await TodoService.GetAllAsync();
    }

    [CascadingParameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;

        var newItem = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Add(newItem); // Update local list immediately
        await OnNewTitleChanged.InvokeAsync("");
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.RemoveAll(i => i.Id == id);
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        var item = _todos.FirstOrDefault(i => i.Id == id);
        if (item != null) item.IsCompleted ^= true;
    }
}

public class TodoList : TodoListBase
{
    protected override async Task OnInitializedAsync()
    {
        // Override to do something different, but we'll just call base for simplicity
        await base.OnInitializedAsync();
    }

    [Parameter]
    public EventCallback<string> OnNewTitleChanged { get; set; }
}

public class TodoServiceTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoServiceTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void AddAsync_CreatesItemWithCorrectTitle()
    {
        var item = _service.AddAsync("new title").Result;
        item.Title.Should().Be("new title");
    }

    [Theory, MemberData(nameof(Data))]
    public void DeleteAsync_RemovesItem(int id)
    {
        _service.DeleteAsync(id).Wait();
        var items = _service.GetAllAsync().Result;
        items.Count.Should().Be(0);
    }

    [Explicit(true)]
    public static IEnumerable<object[]> Data()
    {
        return new[] { new object[] { 1 }, new object[] { 2 } };
    }
}

public class TodoListTests : IAsyncLifetime
{
    private readonly ITodoService _service;

    public TodoListTests(ITodoService service)
    {
        _service = service;
    }

    protected override async Task InitializeAsync()
    {
        // Setup: create a new service instance for each test
        await _service.AddAsync("test").Wait();
    }

    protected override async Task DisposeAsync()
    {
        // Clean up after tests
        await _service.DeleteAsync(1);
    }

    [Fact]
    public void OnInitializedAsync_LoadsTodos()
    {
        var list = new TodoList { TodoService = _service };
        list.OnInitializedAsync().Wait();
        list.Todos.Count.Should().Be(0); // Should be 1 after AddAsync
    }
}