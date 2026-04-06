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
            private int _nextId = 1;

            public Task<List<TodoItem>> GetAllAsync()
            {
                return Task.FromResult(_todos.ToList());
            }

            public Task<TodoItem> AddAsync(string title)
            {
                var item = new TodoItem(_nextId++, title, false);
                _todos.Add(item);
                return Task.FromResult(item);
            }

            public Task DeleteAsync(int id)
            {
                var item = _todos.FirstOrDefault(t => t.Id == id);
                if (item != null)
                {
                    _todos.Remove(item);
                }
                return Task.CompletedTask;
            }

            public Task ToggleAsync(int id)
            {
                var item = _todos.FirstOrDefault(t => t.Id == id);
                if (item != null)
                {
                    item.IsCompleted = !item.IsCompleted;
                }
                return Task.CompletedTask;
            }
        }

public class TodoListBase : ComponentBase
        {
            [Inject] public ITodoService TodoService { get; set; }
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