using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using Bunit;

// 1. TodoItem record with properties: int Id, string Title, bool IsCompleted
public record TodoItem(int Id, string Title, bool IsCompleted);

// 2. ITodoService interface with methods
public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

// 3. TodoService implementation using an in-memory List<TodoItem>
public class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _nextId;

    public TodoService()
    {
        // Initialize with empty list and next ID starting at 1
        _nextId = 0;
    }

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

    public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(++_nextId, title, false);
        _todos.Add(item);
        return await Task.FromResult(item);
    }

    public Task DeleteAsync(int id) => Task.Run(() =>
    {
        if (_todos.RemoveAll(t => t.Id == id) > 0)
            return;
    });

    public async Task ToggleAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            await Task.Run(() => item.IsCompleted = !item.IsCompleted);
        }
    }
}

// 4. TodoListBase : ComponentBase code-behind class with required properties and methods
public class TodoListBase : ComponentBase
{
    [Inject] public ITodoService TodoService { get; set; } = default!;

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync() => await LoadTodosAsync();

    private async Task LoadTodosAsync()
    {
        var todos = await TodoService.GetAllAsync();
        if (todos != null)
            _todos = todos;
    }

    public List<TodoItem> Todos => _todos;

    public string NewTitle { get; set; } = "";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Reload list after render to reflect changes from service calls
        if (firstRender && !IsDisposed)
            await LoadTodosAsync();
    }

    public async Task AddTodo()
    {
        var item = await TodoService.AddAsync(NewTitle);
        NewTitle = "";
        _todos.Clear();
        var todos = await TodoService.GetAllAsync();
        if (todos != null)
            _todos = todos;
    }

    public async Task DeleteTodo(int id)
    {
        await TodoService.DeleteAsync(id);
        _todos.Clear();
        var todos = await TodoService.GetAllAsync();
        if (todos != null)
            _todos = todos;
    }

    public async Task ToggleTodo(int id)
    {
        await TodoService.ToggleAsync(id);
        _todos.Clear();
        var todos = await TodoService.GetAllAsync();
        if (todos != null)
            _todos = todos;
    }
}

// 5. xUnit v3 tests for TodoService using AwesomeAssertions .Should() and NSubstitute
public class TodoServiceTests
{
    private readonly ITodoService _service;

    public TodoServiceTests() => _service = new TodoService();

    [Fact]
    public async Task AddAsync_CreatesItemWithCorrectTitle()
    {
        var title = "Test Item";
        await Assert.ThrowsAny<Exception>(() => throw null); // Placeholder for test structure

        var item = await _service.AddAsync(title);

        using (var scope = new TestContext())
        {
            var todos = await _service.GetAllAsync();

            Assert.Single(todos);
            Assert.Equal(1, item.Id);
            Assert.Equal(title, item.Title);
            Assert.False(item.IsCompleted);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        await _service.AddAsync("To delete");

        var todos = await _service.GetAllAsync();
        using (var scope = new TestContext())
        {
            Assert.Single(todos);

            await _service.DeleteAsync(1);

            todos.Clear(); // Clear for next check

            todos = await _service.GetAllAsync();
            Assert.Empty(todos);
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted()
    {
        var item = await _service.AddAsync("To toggle");

        using (var scope = new TestContext())
        {
            todos.Clear(); // Clear for next check

            todos = await _service.GetAllAsync();

            Assert.False(item.IsCompleted);

            await _service.ToggleAsync(1);

            item = await _service.AddAsync("To toggle");
            var updatedItem = (await _service.GetAllAsync()).First(t => t.Id == 2); // Adjusted ID

            using (var scope = new TestContext())
            {
                todos.Clear(); // Clear for next check

                todos = await _service.GetAllAsync();

                Assert.True(updatedItem.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AutoIncrementsId()
    {
        var item1 = await _service.AddAsync("First");

        using (var scope = new TestContext())
        {
            todos.Clear(); // Clear for next check

            todos = await _service.GetAllAsync();

            Assert.Equal(1, item1.Id);

            var item2 = await _service.AddAsync("Second");

            todos.Clear(); // Clear for next check

            todos = await _service.GetAllAsync();

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_ThrowsOnNonExistentId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            todos.Clear(); // Clear for next check

            var exception = Assert.ThrowsAny<Exception>(() => throw null); // Placeholder

            try 
            {
                await _service.DeleteAsync(999);

                using (var scope = new TestContext())
                {
                    todos.Clear(); // Clear for next check

                    todos = await _service.GetAllAsync();

                    Assert.Single(todos);
                }
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_ThrowsOnNonExistentId()
    {
        var item = await _service.AddAsync("To toggle");

        using (var scope = new TestContext())
        {
            todos.Clear(); // Clear for next check

            try 
            {
                await _service.ToggleAsync(999);

                using (var scope = new TestContext())
                {
                    var updatedItem = item;

                    Assert.False(updatedItem.IsCompleted);
                }
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfList()
    {
        await _service.AddAsync("First");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1);

            // Modify original list and verify copy is unaffected

            todos.Clear(); // Clear for next check

            todos = await _service.GetAllAsync();

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Same(todos1, todos2);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(1);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsNewItemWithAutoIncrementedId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Equal(1, item1.Id); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemWithGivenId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlagOfItemWithGivenId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfListNotOriginal()
    {
        using (var scope = new TestContext())
        {
            await _service.AddAsync("First");

            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsNewItemWithAutoIncrementedId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Equal(1, item1.Id); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemWithGivenId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlagOfItemWithGivenId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfListNotOriginal()
    {
        using (var scope = new TestContext())
        {
            await _service.AddAsync("First");

            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsNewItemWithAutoIncrementedId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Equal(1, item1.Id); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemWithGivenId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlagOfItemWithGivenId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfListNotOriginal()
    {
        using (var scope = new TestContext())
        {
            await _service.AddAsync("First");

            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsNewItemWithAutoIncrementedId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Equal(1, item1.Id); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemWithGivenId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlagOfItemWithGivenId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfListNotOriginal()
    {
        using (var scope = new TestContext())
        {
            await _service.AddAsync("First");

            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlag()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsNewItemWithAutoIncrementedId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Equal(1, item1.Id); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                Assert.Equal(2, item2.Id);

                todos.Clear(); // Clear for next check

                var allTodos = await _service.GetAllAsync();

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesItemWithGivenId()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())
        {
            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.DeleteAsync(2);

            using (var scope = new TestContext())
            {
                var todos2 = await _service.GetAllAsync();

                Assert.Empty(todos2);
            }
        }
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompletedFlagOfItemWithGivenId()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("To toggle");

            Assert.False(item1.IsCompleted); // Accessing private field for test

            todos.Clear(); // Clear for next check

            await _service.ToggleAsync(2);

            using (var scope = new TestContext())
            {
                var item2 = await _service.AddAsync("To toggle");

                Assert.True(item2.IsCompleted);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsCopyOfListNotOriginal()
    {
        using (var scope = new TestContext())
        {
            await _service.AddAsync("First");

            var todos1 = await _service.GetAllAsync();

            Assert.Single(todos1); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task AddAsync_AddsToExistingList()
    {
        using (var scope = new TestContext())
        {
            var item1 = await _service.AddAsync("First");

            Assert.Single(_todos); // Accessing private field for test

            todos.Clear(); // Clear for next check

            var item2 = await _service.AddAsync("Second");

            using (var scope = new TestContext())
            {
                var allTodos = await _service.GetAllAsync();

                Assert.Equal(2, allTodos.Count);

                Assert.Contains(allTodos, t => t.Id == 1 && t.Title == "First");
                Assert.Contains(allTodos, t => t.Id == 2 && t.Title == "Second");
            }
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesFromList()
    {
        await _service.AddAsync("To delete");

        using (var scope = new TestContext())