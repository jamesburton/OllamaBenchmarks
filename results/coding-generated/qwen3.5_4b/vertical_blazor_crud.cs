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
        private readonly List<TodoItem> _todos;
        private int _nextId = 1;

        public TodoService()
        {
            _todos = new List<TodoItem>();
        }

        public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());
        public Task<TodoItem> AddAsync(string title)
        {
            var item = new TodoItem(_nextId++, title, false);
            _todos.Add(item);
            return Task.FromResult(item);
        }
        public Task DeleteAsync(int id) => Task.CompletedTask; // Wait, need to remove.
        // Actually, I should implement the logic properly.
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

public class TodoListBase : ComponentBase
    {
        [Inject] public ITodoService TodoService { get; set; }
        public List<TodoItem> Todos { get; set; } = new();
        public string NewTitle { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            await TodoService.GetAllAsync();
        }
        // Wait, the prompt says "OnInitializedAsync that loads todos".
        // But `TodoService` methods return `Task`. So I need to call them.
        // The prompt says: "protected override async Task OnInitializedAsync() that loads todos"
        // It doesn't specify *which* method. Usually `GetAllAsync`.
        // Wait, the prompt says "OnInitializedAsync that loads todos".
        // But `TodoService` has `GetAllAsync`.
        // I will call `await TodoService.GetAllAsync();` inside `OnInitializedAsync`.
    }

public class TodoServiceTests
    {
        [Fact]
        public void AddAsyncCreatesItemWithCorrectTitle()
        {
            var service = new TodoService();
            var result = service.AddAsync("Test Title").Result; // Wait, xUnit v3 doesn't use .Result usually for async tests unless blocking. Better to use `await`.
            // But xUnit v3 tests are synchronous methods that run async code? No, they can be async.
            // I should use `Task` and `Assert`.
            // Actually, xUnit v3 supports async tests.
        }
    }

[Fact]
    public async Task AddAsyncCreatesItemWithCorrectTitle()
    {
        var service = new TodoService();
        var item = await service.AddAsync("Test Title");
        Assert.Equal("Test Title", item.Title);
        Assert.Equal(1, item.Id);
    }

Assert.Equal("Test Title", item.Title).Should(); // No, Assert.Equal returns void.
    // Correct usage:
    var item = await service.AddAsync("Test Title");
    item.Should().HaveProperty("Title").And.Be("Test Title"); // Wait, record doesn't have properties like that easily without GetField.
    // Better:
    item.Title.Should().Be("Test Title");

public class TodoListBaseTests
    {
        [Fact]
        public async Task RenderComponentCallsGetAllAsyncOnInit()
        {
            var mockService = Substitute.For<ITodoService>();
            mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

            var ctx = new Bunit.TestContext(); // Fully qualified as per instruction? "use `Bunit.TestContext` (fully qualified) to avoid ambiguity".
            // Wait, the prompt says "In bUnit tests, use `Bunit.TestContext` (fully qualified)".
            // So I should write `Bunit.TestContext`.
            var cut = ctx.RenderComponent<TodoListBase>(p => p.Add(p => p.NewTitle, "Test"));
            // Wait, I need to verify it calls GetAllAsync.
            // bUnit doesn't easily allow verifying method calls on mocks unless using `Verify` or checking the mock state.
            // NSubstitute has `Received`.
            // But bUnit renders the component.
            // To verify service call:
            // I can check if `mockService.GetAllAsync()` was called.
            // Wait, `TodoListBase` calls `OnInitializedAsync`.
            // In bUnit, I need to render and then maybe wait or check state.
            // Actually, the prompt says "verify it calls GetAllAsync on init".
            // I can use `mockService.Received(1).GetAllAsync()`.
        }
    }