using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using NSubstitute;

namespace TodoListExample
{
    public class TodoItem : IEquatable<TodoItem>
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }

        public static readonly Random _random = new();

        public static TodoItem New()
        {
            return new TodoItem
            {
                Id = _random.Next(),
                Title = $"New todo item {Id}",
                IsCompleted = false,
            };
        }
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    public List<TodoItem> Todos { get; set; } = new();

    private ITodoService _todoService;

    [Parameter] public ITodoService TodoService
    {
        get => _todoService;
        set
        {
            _todoService = value;
            OnAddTodo.InvokeAsync(Todos.Last());
        }
    }

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    public void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public interface ITodoService
{
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem> AddAsync(string title);
    Task DeleteAsync(int id);
    Task ToggleAsync(int id);
}

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoService : ITodoService, IDisposable
{
    private readonly List<TodoItem> _todos = new();

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return _todos.ToList();
    }

    public async Task<TodoItem> AddAsync(string title)
    {
        var todoItem = New();
        todoItem.Title = title;
        _todos.Add(todoItem);
        return todoItem;
    }

    public async Task DeleteAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos.RemoveRange(id);
        }
    }

    public async Task ToggleAsync(int id)
    {
        if (_todos.ContainsRange(id))
        {
            _todos[id].IsCompleted = !_todos[id].IsCompleted;
        }
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

public class TodoListBase : ComponentBase, IDisposable
{
    [Parameter] public ITodoService TodoService { get; set; }

    private List<TodoItem> _todos = new();

    protected override async Task OnInitializedAsync()
    {
        Todos = TodoService.GetAllAsync().Result.ToList();
    }

    [Parameter] public string NewTitle { get; set; } = "";
    [Parameter] public EventCallback<TodoItem> OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoItem> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<int> OnDeleteTodo { get; set; }

    private void AddTodo()
    {
        if (string.IsNullOrEmpty(NewTitle))
        {
            return;
        }

        var todoItem = New();
        todoItem.Title = NewTitle;
        Todos.Add(todoItem);
        NewTitle = "";
        OnAddTodo.InvokeAsync(Todos.Last());
    }

    public async Task ToggleTodo(int id)
    {
        Todos[id].IsCompleted = !Todos[id].IsCompleted;
        await OnToggleTodo.InvokeAsync(Todos[id]);
    }

    public void DeleteTodo(int id)
    {
        Todos.Remove(Todos[id]);
        OnDeleteTodo.InvokeAsync(id);
    }

    protected virtual void Dispose(bool disposing) => throw new NotImplementedException();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}