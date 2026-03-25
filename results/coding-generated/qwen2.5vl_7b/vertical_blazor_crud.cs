using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;
using xUnit = xunit;

namespace TodoApp
{
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
        private List<TodoItem> _todos = new List<TodoItem>();

        public async Task<List<TodoItem>> GetAllAsync()
        {
            return _todos.Select(t => new TodoItem(t.Id, t.Title, t.IsCompleted)).ToList();
        }

        public async Task<TodoItem> AddAsync(string title)
        {
            var id = _todos.Count + 1;
            var todo = new TodoItem(id, title, false);
            _todos.Add(todo);
            return todo;
        }

        public async Task DeleteAsync(int id)
        {
            _todos.RemoveAll(t => t.Id == id);
        }

        public async Task ToggleAsync(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
            }
        }
    }

    public class TodoListBase : ComponentBase
    {
        [Inject] public ITodoService TodoService { get; set; }

        public List<TodoItem> Todos { get; set; } = new List<TodoItem>();
        public string NewTitle { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            Todos = await TodoService.GetAllAsync();
        }

        public async Task AddTodo()
        {
            if (!string.IsNullOrEmpty(NewTitle))
            {
                var todo = await TodoService.AddAsync(NewTitle);
                Todos.Add(todo);
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

    public class TodoListBaseTest : Bunit.TestContext
    {
        [Fact]
        public void TestTodoListBase()
        {
            // Arrange
            var todoService = Substitute.For<ITodoService>();
            var component = RenderComponent<TodoListBase>();

            // Act
            component.Find("input").Input(NewTitle);
            component.Find("button").Click();

            // Assert
            component.Markup.Should().Contain($"<input value=\"{NewTitle}\">");
            component.Markup.Should().Contain($"<button>Save</button>");
        }
    }

    public class TodoServiceTest : xUnit.TestClass
    {
        [Fact]
        public async Task TestAddAsync()
        {
            // Arrange
            var todoService = new TodoService();
            var component = new TestContext().RenderComponent<ITodoService>(p => p.Add(x => x.GetOrder(1).Returns(new Order { Id = 1 })));

            // Act
            var todo = await todoService.AddAsync("Test Todo");

            // Assert
            todo.Title.Should().Be("Test Todo");
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            // Arrange
            var todoService = new TodoService();
            var component = new TestContext().RenderComponent<ITodoService>(p => p.Add(x => x.GetOrder(1).Returns(new Order { Id = 1 })));

            // Act
            await todoService.DeleteAsync(1);

            // Assert
            todoService.Received().GetAllAsync();
        }

        [Fact]
        public async Task TestToggleAsync()
        {
            // Arrange
            var todoService = new TodoService();
            var component = new TestContext().RenderComponent<ITodoService>(p => p.Add(x => x.GetOrder(1).Returns(new Order { Id = 1 })));

            // Act
            await todoService.ToggleAsync(1);

            // Assert
            todoService.Received().GetAllAsync();
        }
    }
}