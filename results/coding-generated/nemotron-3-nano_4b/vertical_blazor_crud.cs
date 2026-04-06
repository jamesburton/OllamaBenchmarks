using Microsoft.AspNetCore.Components;
using Xunit;
using AwesomeAssertions;
using NSubstitute;
using Bunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

record TodoItem { int Id; string Title; bool IsCompleted; }

interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

class TodoService : ITodoService
{
    private List<TodoItem> _todos;
    private int _nextId = 1;

    public TodoService()
    {
        _todos = new List<TodoItem>();
    }

    public Task<List<TodoItem>> GetAllAsync() => Task.FromResult(_todos.AsReadOnly().ToList());

    public Task<TodoItem> AddAsync(string title)
    {
        var ti = new TodoItem { Id = _nextId++, Title = title, IsCompleted = false };
        _todos.Add(ti);
        return Task.FromResult(ti);
    }

    public Task DeleteAsync(int id)
    {
        var idx = _todos.FindIndex(t => t.Id == id);
        if (idx >= 0)
            _todos.RemoveAt(idx);
        return Task.CompletedTask;
    }

    public Task ToggleAsync(int id)
    {
        var tk = _todos.FirstOrDefault(t => t.Id == id);
        if (tk is not null)
        {
            tk.IsCompleted = !tk.IsCompleted;
            _todos[index(tk)] = tk;
        }
        return Task.CompletedTask;
    }

    private int index(TodoItem ti) => _todos.IndexOf(ti);
}

class TodoListBase : ComponentBase
{
    [Inject]
    public ITodoService TodoService { get; set; } = null!;

    public List<TodoItem> Todos { get; set; } = new();

    public string NewTitle { get; set; } = "";

    protected override async Task OnInitializedAsync()
    {
        Todos = await TodoService.GetAllAsync();
    }

    public async Task AddTodo()
    {
        if (string.IsNullOrWhiteSpace(NewTitle)) return;
        var todo = await TodoService.AddAsync(NewTitle);
        Todos = await TodoService.GetAllAsync();
        NewTitle = "";
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