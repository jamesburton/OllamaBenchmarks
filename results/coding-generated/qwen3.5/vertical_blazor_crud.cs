public record TodoItem(int Id, string Title, bool IsCompleted);

public interface ITodoService {
            Task<List<TodoItem>> GetAllAsync();
            Task<TodoItem> AddAsync(string title);
            Task DeleteAsync(int id);
            Task ToggleAsync(int id);
        }

public class TodoService : ITodoService {
            private readonly List<TodoItem> _todos = new();
            private int _nextId = 1;
            // Implement methods...
        }

public class TodoListBase : ComponentBase {
            [Inject] public ITodoService TodoService { get; set; }
            public List<TodoItem> Todos { get; set; } = new();
            public string NewTitle { get; set; } = "";
            // Methods...
        }