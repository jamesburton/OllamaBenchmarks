using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using AwesomeAssertions;
using NSubstitute;
using Xunit;
using Microsoft.AspNetCore.Components;

namespace MyTodoApp
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
        private readonly List<TodoItem> _todos = new();
        private int _nextId;

        public TodoService()
        {
            _nextId = 1;
        }

        public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

        public async Task<TodoItem> AddAsync(string title)
        {
            var item = new TodoItem(_nextId++, title, false);
            _todos.Add(item);
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            var index = _todos.FindIndex(t => t.Id == id);
            if (index >= 0)
            {
                _todos.RemoveAt(index);
            }
        }

        public async Task ToggleAsync(int id)
        {
            var item = _todos.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
            }
        }
    }

    // 4. TodoListBase : ComponentBase code-behind class
    public class TodoListBase : ComponentBase
    {
        [Inject]
        public ITodoService TodoService { get; set; } = null!;

        public List<TodoItem> Todos { get; set; } = new();
        public string NewTitle { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            await TodoService.GetAllAsync();
        }

        public async Task AddTodo()
        {
            if (!string.IsNullOrWhiteSpace(NewTitle))
            {
                var item = await TodoService.AddAsync(NewTitle);
                Todos = await TodoService.GetAllAsync();
                NewTitle = "";
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

    // 5. xUnit v3 tests for TodoService
    public class TodoServiceTests
    {
        private readonly ITodoService _service;

        [Fact]
        public void AddAsyncCreatesItemWithCorrectTitle()
        {
            _service = new TodoService();
            var result = _service.AddAsync("Test Title").Result;

            Assert.Equal(1, result.Id);
            Assert.Equal("Test Title", result.Title);
            Assert.False(result.IsCompleted);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void DeleteAsyncRemovesItem(int id)
        {
            _service = new TodoService();
            var item = _service.AddAsync("Item").Result;
            _service.DeleteAsync(item.Id);

            Assert.Empty(_service.GetAllAsync().Result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToggleAsyncFlipsIsCompleted(bool initial)
        {
            _service = new TodoService();
            var item = _service.AddAsync("Item").Result;
            item.IsCompleted = initial;

            _service.ToggleAsync(item.Id).Wait();

            Assert.Equal(!initial, item.IsCompleted);
        }
    }

    // 6. bUnit test for TodoListBase
    public class TodoListBaseTests
    {
        private readonly TestContext _ctx;
        private ITodoService _mockService;

        public TodoListBaseTests()
        {
            _ctx = new TestContext();
            _mockService = Substitute.For<ITodoService>();
        }

        [Fact]
        public void RenderComponentCallsGetAllAsyncOnInit()
        {
            // Setup mock to return empty list
            _mockService.GetAllAsync().Returns(Task.FromResult(new List<TodoItem>()));

            var cut = _ctx.RenderComponent<TodoListBase>(p => p.Add(p => p.TodoService, _mockService));

            // Verify that GetAllAsync was called during initialization
            _mockService.Received(1).GetAllAsync();
        }
    }
}