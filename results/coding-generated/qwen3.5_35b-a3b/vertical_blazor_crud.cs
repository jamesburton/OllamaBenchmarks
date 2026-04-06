public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService
        {
            System.Threading.Tasks.Task<System.Collections.Generic.List<TodoItem>> GetAllAsync();
            System.Threading.Tasks.Task<TodoItem> AddAsync(string title);
            System.Threading.Tasks.Task DeleteAsync(int id);
            System.Threading.Tasks.Task ToggleAsync(int id);
        }

public class TodoService : ITodoService
        {
            private System.Collections.Generic.List<TodoItem> _todos = new();
            private int _nextId = 1;

            public System.Threading.Tasks.Task<System.Collections.Generic.List<TodoItem>> GetAllAsync()
            {
                return System.Threading.Tasks.Task.FromResult(_todos.ToList());
            }

            public System.Threading.Tasks.Task<TodoItem> AddAsync(string title)
            {
                var item = new TodoItem(_nextId++, title, false);
                _todos.Add(item);
                return System.Threading.Tasks.Task.FromResult(item);
            }

            public System.Threading.Tasks.Task DeleteAsync(int id)
            {
                var item = _todos.FirstOrDefault(t => t.Id == id);
                if (item != null) _todos.Remove(item);
                return System.Threading.Tasks.Task.CompletedTask;
            }

            public System.Threading.Tasks.Task ToggleAsync(int id)
            {
                var item = _todos.FirstOrDefault(t => t.Id == id);
                if (item != null) item = item with { IsCompleted = !item.IsCompleted };
                // Wait, records are immutable. I need to update the list.
                // TodoItem is a record. `item with` creates a new instance.
                // I need to replace it in the list.
                if (item != null)
                {
                    var index = _todos.IndexOf(item);
                    if (index >= 0)
                    {
                        _todos[index] = item with { IsCompleted = !item.IsCompleted };
                    }
                }
                return System.Threading.Tasks.Task.CompletedTask;
            }
        }

var item = _todos.FirstOrDefault(t => t.Id == id);
        if (item != null)
        {
            var index = _todos.IndexOf(item);
            if (index >= 0)
            {
                _todos[index] = item with { IsCompleted = !item.IsCompleted };
            }
        }

public class TodoListBase : ComponentBase
        {
            [Inject] public ITodoService TodoService { get; set; } = default!;
            public System.Collections.Generic.List<TodoItem> Todos { get; set; } = new();
            public string NewTitle { get; set; } = "";

            protected override async System.Threading.Tasks.Task OnInitializedAsync()
            {
                Todos = await TodoService.GetAllAsync();
            }

            public async System.Threading.Tasks.Task AddTodo()
            {
                if (string.IsNullOrWhiteSpace(NewTitle)) return;
                await TodoService.AddAsync(NewTitle);
                NewTitle = "";
                Todos = await TodoService.GetAllAsync();
            }

            public async System.Threading.Tasks.Task DeleteTodo(int id)
            {
                await TodoService.DeleteAsync(id);
                Todos = await TodoService.GetAllAsync();
            }

            public async System.Threading.Tasks.Task ToggleTodo(int id)
            {
                await TodoService.ToggleAsync(id);
                Todos = await TodoService.GetAllAsync();
            }
        }