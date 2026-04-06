using Bunit;
using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace TodoList.Tests
{
    public class TodoListBaseTests
    {
        private TestContext ctx;
        private Mock<ITodoService> mockTodoService;
        private TodoListBase component;
        private TodoItem dummyTodo;

        public TodoListBaseTests(TestContext ctx)
        {
            this.ctx = ctx;
        }

        [Fact]
        public void AddTodo_AddsItemToTodos()
        {
            // Arrange
            mockTodoService = Substitute.For<ITodoService>();
            component = new TodoListBase(mockTodoService);
            dummyTodo = new TodoItem { Id = 0, Title = "Test Todo", IsCompleted = false };

            // Act
            component.AddTodo();

            // Assert
            mockTodoService.Received().AddAsync("Test Todo");
            component.Todos.Should().Contain(dummyTodo);
        }

        [Fact]
        public void DeleteTodo_RemovesItemFromTodos()
        {
            // Arrange
            mockTodoService = Substitute.For<ITodoService>();
            component = new TodoListBase(mockTodoService);
            dummyTodo = new TodoItem { Id = 1, Title = "Test Todo", IsCompleted = false };

            // Act
            component.DeleteTodo(1);

            // Assert
            mockTodoService.Received().DeleteAsync(1);
            component.Todos.Should().NotContain(dummyTodo);
        }

        [Fact]
        public void ToggleTodo_TogglesIsCompletedFlag()
        {
            // Arrange
            mockTodoService = Substitute.For<ITodoService>();
            component = new TodoListBase(mockTodoService);
            dummyTodo = new TodoItem { Id = 2, Title = "Test Todo", IsCompleted = false };

            // Act
            component.ToggleTodo(2);

            // Assert
            mockTodoService.Received().ToggleAsync(2);
            dummyTodo.IsCompleted.Should().BeTrue();
        }
    }
}

using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ComponentBase]
public class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; }

    public List<TodoItem> Todos { get; set; } = new();
    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        NewTitle = "";
        Todos = await TodoService.AddAsync(NewTitle);
    }

    public async Task DeleteTodo(int id)
    {
        Todos = await TodoService.DeleteAsync(id);
    }

    public async Task ToggleTodo(int id)
    {
        Todos = await TodoService.ToggleAsync(id);
    }
}

using NSubstitute;
using Xunit;

namespace TodoList.Tests.Service
{
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
            return _todos.ToList();
        }

        public async Task<TodoItem> AddAsync(string title)
        {
            var newItem = new TodoItem { Id = _nextId++, Title = title, IsCompleted = false };
            _todos.Add(newItem);
            return newItem;
        }

        public async Task DeleteAsync(int id)
        {
            var itemToRemove = _todos.FirstOrDefault(t => t.Id == id);
            if (itemToRemove != null)
            {
                _todos.Remove(itemToRemove);
            }
        }

        public async Task ToggleAsync(int id)
        {
            var itemToToggle = _todos.FirstOrDefault(t => t.Id == id);
            if (itemToToggle != null)
            {
                itemToToggle.IsCompleted = !itemToToggle.IsCompleted;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}