using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

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
        private readonly List<TodoItem> _todos = new();

        public Task<List<TodoItem>> GetAllAsync()
        {
            return Task.FromResult(_todos.ToList());
        }

        public Task<TodoItem> AddAsync(string title)
        {
            var todo = new TodoItem(_todos.Count + 1, title, false);
            _todos.Add(todo);
            return Task.FromResult(todo);
        }

        public Task DeleteAsync(int id)
        {
            _todos.RemoveAll(t => t.Id == id);
            return Task.CompletedTask;
        }

        public Task ToggleAsync(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                todo = new TodoItem(todo.Id, todo.Title, !todo.IsCompleted);
                _todos[_todos.IndexOf(todo)] = todo;
            }
            return Task.CompletedTask;
        }
    }

    public class TodoListBase : ComponentBase
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
            if (!string.IsNullOrEmpty(NewTitle))
            {
                await TodoService.AddAsync(NewTitle);
                NewTitle = "";
                await OnInitializedAsync();
            }
        }

        public async Task DeleteTodo(int id)
        {
            await TodoService.DeleteAsync(id);
            await OnInitializedAsync();
        }

        public async Task ToggleTodo(int id)
        {
            await TodoService.ToggleAsync(id);
            await OnInitializedAsync();
        }
    }

    public class TodoServiceTests
    {
        private readonly ITestOutputHelper _output;

        public TodoServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task AddAsync_CreatesItemWithCorrectTitle()
        {
            var service = new TodoService();
            var todo = await service.AddAsync("Test Todo");

            todo.Title.Should().Be("Test Todo");
        }

        [Fact]
        public async Task DeleteAsync_RemovesItem()
        {
            var service = new TodoService();
            await service.AddAsync("Test Todo");
            var initialCount = service._todos.Count;

            await service.DeleteAsync(1);
            service._todos.Count.Should().Be(initialCount - 1);
        }

        [Fact]
        public async Task ToggleAsync_FlipsIsCompleted()
        {
            var service = new TodoService();
            await service.AddAsync("Test Todo");
            var todo = service._todos[0];

            await service.ToggleAsync(todo.Id);
            todo.IsCompleted.Should().BeTrue();
        }
    }

    public class TodoListBaseTests
    {
        [Fact]
        public async Task OnInitializedAsync_CallsGetAllAsync()
        {
            var service = Substitute.For<ITodoService>();
            var todos = new List<TodoItem>
            {
                new(1, "Test Todo", false)
            };
            service.GetAllAsync().Returns(todos);

            var ctx = new TestContext();
            var cut = ctx.RenderComponent<TodoListBase>(p =>
                p.Add(x => x.TodoService, service));

            await cut.WaitForAssertionAsync(() =>
            {
                service.Received(1).GetAllAsync();
            });
        }
    }
}