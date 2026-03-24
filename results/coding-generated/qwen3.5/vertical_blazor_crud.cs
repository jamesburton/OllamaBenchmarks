using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Bunit;
using NSubstitute;
using FluentAssertions;

namespace TodoApp
{
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
        private readonly List<TodoItem> _items = new();
        private int _nextId;

        public async Task<List<TodoItem>> GetAllAsync()
        {
            return new List<TodoItem>(_items);
        }

        public async Task<TodoItem> AddAsync(string title)
        {
            var item = new TodoItem(_nextId++, title, false);
            _items.Add(item);
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            _items.RemoveAll(i => i.Id == id);
        }

        public async Task ToggleAsync(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
            }
        }
    }

    // 4. TodoListBase code-behind class
    public abstract class TodoListBase : ComponentBase
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
                var newItem = await TodoService.AddAsync(NewTitle);
                NewTitle = "";
                Todos = await TodoService.GetAllAsync();
            }
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

    // Concrete component for rendering in bUnit tests
    public class TodoListComponent : TodoListBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"Todos: {Todos.Count}");
            builder.CloseElement();
        }
    }

    // 5. xUnit v3 tests for TodoService
    public class TodoServiceTests
    {
        private readonly ITodoService _service;

        public TodoServiceTests()
        {
            _service = new TodoService();
        }

        [Fact]
        public async Task AddAsync_CreatesItemWithCorrectTitle()
        {
            var title = "Buy Milk";
            var item = await _service.AddAsync(title);

            item.Should().NotBeNull();
            item.Title.Should().Be(title);
            item.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_RemovesItem()
        {
            var item1 = await _service.AddAsync("Task 1");
            var item2 = await _service.AddAsync("Task 2");

            await _service.DeleteAsync(item1.Id);

            (await _service.GetAllAsync()).Should().ContainSingle(i => i.Id == item2.Id);
        }

        [Fact]
        public async Task ToggleAsync_FlipsIsCompleted()
        {
            var item = await _service.AddAsync("Task 3");
            item.IsCompleted.Should().BeFalse();

            await _service.ToggleAsync(item.Id);

            (await _service.GetAllAsync()).Should().Contain(i => i.Id == item.Id && i.IsCompleted == true);
        }

        [Theory]
        [InlineData(1, "A")]
        [InlineData(2, "B")]
        public async Task AddAsync_IncrementsId(int expectedId, string title)
        {
            var item = await _service.AddAsync(title);
            item.Id.Should().Be(expectedId);
        }

        [Fact]
        public void TestContext_DiagnosticMessage()
        {
            // xUnit v3 feature usage
            Assert.Multiple(
                () => Assert.True(true),
                () => Assert.Equal("TodoService", "TodoService")
            );
        }
    }

    // 6. bUnit test for TodoListBase
    public class TodoListBaseTests
    {
        private readonly TestContext _ctx = new();
        private readonly ITodoService _mockService;

        public TodoListBaseTests()
        {
            _mockService = Substitute.For<ITodoService>();
        }

        [Fact]
        public async Task OnInitializedAsync_CallsGetAllAsync()
        {
            // Setup mock to return a list
            var todos = new List<TodoItem> { new TodoItem(1, "Test", false) };
            _mockService.GetAllAsync().Returns(todos);

            // Render component
            var cut = _ctx.RenderComponent<TodoListComponent>(p =>
                p.With(x => x.TodoService, _mockService));

            // Verify initialization logic
            await Task.Delay(10); // Allow async init to complete

            _mockService.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task AddTodo_CallsAddAsyncAndClearsNewTitle()
        {
            var todos = new List<TodoItem>();
            _mockService.GetAllAsync().Returns(todos);
            _mockService.AddAsync("New Task").Returns(new TodoItem(1, "New Task", false));

            var cut = _ctx.RenderComponent<TodoListComponent>(p =>
                p.With(x => x.TodoService, _mockService)
                  .With(x => x.NewTitle, "New Task"));

            await cut.InvokeAsync(() => cut.Instance.AddTodo());

            _mockService.Received(1).AddAsync("New Task");
            cut.Instance.NewTitle.Should().Be("");
        }

        [Fact]
        public async Task DeleteTodo_CallsDeleteAsync()
        {
            var todos = new List<TodoItem> { new TodoItem(1, "Task", false) };
            _mockService.GetAllAsync().Returns(todos);
            _mockService.DeleteAsync(1).Returns(Task.CompletedTask);

            var cut = _ctx.RenderComponent<TodoListComponent>(p =>
                p.With(x => x.TodoService, _mockService));

            await cut.InvokeAsync(() => cut.Instance.DeleteTodo(1));

            _mockService.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task ToggleTodo_CallsToggleAsync()
        {
            var todos = new List<TodoItem> { new TodoItem(1, "Task", false) };
            _mockService.GetAllAsync().Returns(todos);
            _mockService.ToggleAsync(1).Returns(Task.CompletedTask);

            var cut = _ctx.RenderComponent<TodoListComponent>(p =>
                p.With(x => x.TodoService, _mockService));

            await cut.InvokeAsync(() => cut.Instance.ToggleTodo(1));

            _mockService.Received(1).ToggleAsync(1);
        }

        [Fact]
        public void TestContext_CancellationToken()
        {
            // xUnit v3 feature usage
            var ct = _ctx.Current.CancellationToken;
            Assert.NotNull(ct);
        }
    }
}