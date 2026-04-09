public record TodoItem(int Id, string Title, bool IsCompleted);

public class TodoService : ITodoService {
        private readonly List<TodoItem> _todos = new();
        private int _nextId = 1;
        // ... methods
    }

... 
Assert.Equal(expected, actual);
    Assert.True(condition);

...