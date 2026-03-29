using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;
using Bunit;
using AwesomeAssertions;

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
        private readonly List<TodoItem> _items = new();
        private int _nextId = 1;

        public async Task<List<TodoItem>> GetAllAsync()
        {
            await Task.Delay(10);
            return new List<TodoItem>(_items);
        }

        public async Task<TodoItem> AddAsync(string title)
        {
            await Task.Delay(10);
            var item = new TodoItem(_nextId++, title, false);
            _items.Add(item);
            return item;
        }

        public async Task DeleteAsync(int id)
        {
            await Task.Delay(10);
            _items.RemoveAll(x => x.Id == id);
        }

        public async Task ToggleAsync(int id)
        {
            await Task.Delay(10);
            var item = _items.Find(x => x.Id == id);
            if (item != null)
            {
                item.IsCompleted = !item.IsCompleted;
            }
        }
    }

    public partial class TodoListBase : ComponentBase
    {
        [Inject] public ITodoService TodoService { get; set; } = null!;
        [Parameter] public List<TodoItem> Todos { get; set; } = new();
        [Parameter] public string NewTitle { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            await Todos;
        }

        public async Task AddTodo()
        {
            if (string.IsNullOrWhiteSpace(NewTitle)) return;
            var item = await TodoService.AddAsync(NewTitle);
            NewTitle = "";
            await RefreshTodos();
        }

        public async Task DeleteTodo(int id)
        {
            await TodoService.DeleteAsync(id);
            await RefreshTodos();
        }

        public async Task ToggleTodo(int id)
        {
            await TodoService.ToggleAsync(id);
            await RefreshTodos();
        }

        private async Task RefreshTodos()
        {
            Todos = await TodoService.GetAllAsync();
        }
    }

    public class TodoServiceTests : TestContext
    {
        private readonly ITodoService _service;

        public TodoServiceTests()
        {
            _service = Substitute.For<ITodoService>();
        }

        [Fact]
        public async Task AddAsync_AddsItemWithCorrectTitle()
        {
            var title = "Test";
            var result = await _service.AddAsync(title);

            _service.GetAllAsync().Returns(new List<TodoItem>
            {
                result
            });

            await _service.GetAllAsync();
            var all = await _service.GetAllAsync();

            all.Count.Should().Be(1);
            all[0].Title.Should().Be(title);
        }

        [Fact]
        public async Task DeleteAsync_RemovesItem()
        {
            var item = new TodoItem(1, "Test", false);
            await _service.AddAsync("Test");

            await _service.DeleteAsync(1);

            var all = await _service.GetAllAsync();
            all.Should().BeEmpty();
        }

        [Fact]
        public async Task ToggleAsync_FlipsIsCompleted()
        {
            var item = new TodoItem(1, "Test", false);
            await _service.AddAsync("Test");

            await _service.ToggleAsync(1);

            var all = await _service.GetAllAsync();
            all.Count.Should().Be(1);
            all[0].IsCompleted.Should().BeTrue();
        }
    }

    public class TodoListBaseTest : TestContext
    {
        private readonly ITodoService _service;

        public TodoListBaseTest()
        {
            _service = Substitute.For<ITodoService>();
        }

        [Fact]
        public void Constructor_SetsService()
        {
            // Just verify it can be instantiated
            var instance = new TodoListBase();
            instance.TodoService.Should().NotBeNull();
        }
    }
}