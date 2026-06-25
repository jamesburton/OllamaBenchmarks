using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;

namespace TodoListExample
{
    public partial class TodoListBase : ComponentBase, ITodoService
    {
        [Parameter]
        public string NewTitle { get; set; } = "";

        private List<TodoItem> _todos;

        protected override async Task OnInitializedAsync()
        {
            if (_todos == null)
                _todos = new List<TodoItem>();

            await LoadTodos();
        }

        public async Task AddTodo()
        {
            NewTitle = "";
            var todo = new TodoItem { Title = NewTitle, IsCompleted = false };
            await AddAsync(todo);
        }

        public async Task DeleteTodo(int id)
        {
            _todos = _todos.Where(x => x.Id != id).ToList();
            await ReloadTodos();
        }

        public async Task ToggleTodo(int id)
        {
            var todo = _todos.FirstOrDefault(x => x.Id == id);
            if (todo != null && !todo.IsCompleted)
                todo.IsCompleted = true;
            else
                todo.IsCompleted = false;

            await ReloadTodos();
        }

        private async Task LoadTodos()
        {
            _todos = await GetTodosAsync();
        }

        public async Task<List<TodoItem>> GetAllAsync()
        {
            return await GetTodosAsync();
        }

        public async Task AddAsync(TodoItem todo)
        {
            if (todo.Id == 0) // auto-increment Id
                todo.Id = _todos.Count + 1;

            _todos.Add(todo);
            await ReloadTodos();
        }

        public async Task DeleteAsync(int id)
        {
            var todo = _todos.FirstOrDefault(x => x.Id == id);
            if (todo != null)
                _todos.Remove(todo);

            await ReloadTodos();
        }

        private async Task ReloadTodos()
        {
            await LoadTodos();
        }

        [Parameter]
        public event EventHandler<TodoItem> TodoChanged;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (NewTitle != "")
                AddTodo();
        }
    }

    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
    }
}