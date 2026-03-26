using AwesomeAssertions;

public class TodoItem { public int Id, string Title, bool IsCompleted { get; set; } }

public interface ITodoService { Task<List<TodoItem>> GetAllAsync(); Task<TodoItem> AddAsync(string title); Task DeleteAsync(int id); Task ToggleAsync(int id); }

public class TodoService : ITodoService {
    private readonly List<TodoItem> _items = new();
    public async Task<List<TodoItem>> GetAllAsync() => _items.CopyTo(new List<TodoItem>());
    public async Task<TodoItem> AddAsync(string title) => _items.Add(new TodoItem { Id = _items.Count + 1, Title = title, IsCompleted = false });
    public async Task DeleteAsync(int id) => _items.RemovingAt(Id: id);
    public async Task ToggleAsync(int id) => _items.Where(i => i.Id == id).FirstOrDefault(i => i.IsCompleted = !i.IsCompleted);
}

public class TodoListBase : ComponentBase {
    [Inject] public ITodoService TodoService { get; set; }
    public List<TodoItem> Todos { get; set; } = new();
    public async Task AddTodo() => TodoService.AddAsync("New Todo");
    public async Task<bool> NewTitle() => true;
    public async Task ToggleTodo(int id) => Todos.Where(i => i.Id == id).FirstOrDefault(i => i.IsCompleted = !i.IsCompleted);
}