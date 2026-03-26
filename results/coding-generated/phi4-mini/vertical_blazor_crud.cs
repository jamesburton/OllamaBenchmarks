using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using NSubstitute;
using Xunit;

public record TodoItem(int Id, string Title, bool IsCompleted);

interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

class TodoService : ITodoService
{
    private readonly List<TodoItem> _todos = new();
    private int _idCounter = 1;

    public async Task<List<TodoItem>> GetAllAsync() => await Task.FromResult(_todos.ToList());

    public async Task<TodoItem> AddAsync(string title) =>
        var todo = new(TodoId++ % 100, title, false);
        _todos.Add(todo);
        return todo;

    public async Task DeleteAsync(int id)
    {
        int index = Array.FindIndex(_todos.Select(t => t.Id), id);
        if (index >= 0 && index < _todos.Count) _todos.RemoveAt(index);
    }

    public async Task ToggleAsync(int id)
    {
        var todo = _todos.FirstOrDefault(t => t.Id == id);
        if (todo != null) todo.IsCompleted = !todo.IsCompleted;
    }
}

public class TodoListBase : ComponentBase
{
    [Inject]
    private ITodoService _todoService { get; set; }

    public List<TodoItem> Todos { get; } = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = await _todoService.GetAllAsync();
    }

    public void AddTodo() => AddNewTitleToList();

    private void AddNewTitleToList()
    {
        if (!string.IsNullOrEmpty(_newTitle))
        {
            var todoItem = new TodoItem(0, _newTitle, false);
            Todos.Add(todoItem);

            // Reset the input field
            SetAsync(new string(' ', 100));
        }
    }

    public async Task DeleteTodo(int id) => await RemoveAtIndex(id);

    private int? GetIdFromInput() =>
        new StringBuilder().AppendLine(_input).ToString()
                .Split('\n')
                .LastOrDefault()?.TrimStart();

    private void SetAsync(string value)
    {
        _newTitle = value;
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        Todos = await _todoService.GetAllAsync();
    }
}

public class TodoListTests : IClassFixture<TodoTestData>
{
    public readonly ITodoService MockTodoService;

    public TodoListTests(TodoTestData data)
    {
        var mockService = Substitute.For<ITodoService>();
        this.MockTodoService = new(data.Todos, mockService);
    }

    [Theory]
    [MemberData(nameof(MockTodoData))]
    public void AddAsync_CreatesItemWithCorrectTitle(string title) => Assert.Contains(title);

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var initialCount = MockTodoService.GetAll().Length;
        await this.MockTodoService.DeleteAsync(1);
        var updatedList = await this.MockTodoService.GetAll();
        Assert.Equal(initialCount - 1, updatedList.Count());
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted
    {
        int idToToggle = MockTodoService.Add("Initial").Id;
        bool initialState = false;

        var toggledItem = await this.MockTodoService.ToggleAsync(idToToggle);
        Assert.Equal(initialState, !initialState);

        updatedList = await this.MockTodoService.GetAll();
        Assert.True(updatedList.First(t => t.Id == idToToggle).IsCompleted != initialState);
    }
}

public class TodoTestData
{
    public List<TodoItem> Todos { get; set; }

    public static IEnumerable<object[]> MockTodoData =>
        new[]
        {
            ("Initial", true),
            (null, false)
        };
}

// bUnit test for TodoListBase using NSubstitute and Bunit
public class TodoListBUnitTest : IClassFixture<StubbedComponents>
{
    private readonly StubbedComponents _stubbedComponents;

    public TodoListBUnitTest(StubbedComponents stubbedComponents) => _stubbedComponents = stubbedComponents;

    [Fact]
    public void OnInitializedAsync_CallsGetAllAsync()
    {
        var componentUnderTest = TestInstance.Create<TodoListBase>();
        using (var testContext = new Bunit.TestContext())
        {
            // Arrange
            var mockService = Substitute.For<ITodoService>();
            _stubbedComponents.Add(mockService);

            // Act & Assert
            ComponentReference<TestComponent> reference = componentUnderTest.GetRenderAsync().Result;
            TestInstance.Create<Bunit.Component>(reference).InvokeAsync(() => { });
            testContext.Verify(x => x.Render(), Times.Once());
        }
    }

    public class StubbedComponents : IStub<TodoService>
    {
        private readonly List<TodoItem> _todos = new();
        private int _idCounter = 1;

        public ITodoService Create()
        {
            return new() { Ids => ref _idCounter, Todos => ref _todos };
        }

        public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.ToList());

        public async ValueTask AddAsync(string title)
        {
            var todo = new(TodoId++ % 100, title, false);
            _todos.Add(todo);

            return await Task.CompletedTask;
        }

        public void DeleteAsync(int id) =>
            _todos.RemoveAll(t => t.Id == id);

        public async ValueTask ToggleAsync(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null) todo.IsCompleted = !todo.IsCompleted;
        }
    }

    private class TestComponent : ComponentBase
    {
        [Inject]
        protected ITodoService TodoService { get; set; }

        public List<TodoItem> Todos { get; } = new();

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }
    }
}