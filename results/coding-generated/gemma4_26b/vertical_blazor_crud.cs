public interface ITodoService {
            Task<List<TodoItem>> GetAllAsync();
            Task<TodoItem> AddAsync(string title);
            Task DeleteAsync(int id);
            Task ToggleAsync(int id);
        }

public class TodoService : ITodoService {
            private readonly List<TodoItem> _items = new();
            private int _nextId = 1;
            public async Task<List<TodoItem>> GetAllAsync() => _items.ToList();
            public async Task<TodoItem> AddAsync(string title) {
                var item = new TodoItem(_nextId++, title, false);
                _items.Add(item);
                return item;
            }
            public async Task DeleteAsync(int id) => _items.RemoveAll(x => x.Id == id);
            public async Task ToggleAsync(int id) {
                var item = _items.FirstOrDefault(x => x.Id == id);
                if (item != null) {
                    _items.Remove(item);
                    _items.Add(item with { IsCompleted = !item.Is/IsCompleted }); // Wait, records are immutable.
                    // Better:
                    // var index = _items.FindIndex(x => x.Id == id);
                    // if (index != -1) _items[index] = _items[index] with { IsCompleted = !_items[index].IsCompleted };
                }
            }
        }

public async Task ToggleAsync(int id) {
            var index = _items.FindIndex(x => x.Id == id);
            if (index != -1) {
                _im.Items[index] = _items[index] with { IsCompleted = !_items[index].IsCompleted };
            }
        }

using Microsoft.AspNetCore.Components;
        public class TodoListBase : ComponentBase {
            [Inject] public ITodoService TodoService { get; set; } = default!;
            public List<TodoItem> Todos { get; set; } = new();
            public string NewTitle { get; set; } = "";
            protected override async Task OnInitializedAsync() => Todos = await TodoService.GetAllAsync();
            public async Task AddTodo() {
                await TodoService.AddAsync(NewTitle);
                NewTitle = "";
                Todos = await TodoService.GetAllAsync();
            }
            public async Task DeleteTodo(int id) {
                await TodoService.DeleteAsync(id);
                Todos = await TodoService.GetAllAsync();
            }
            public async Task ToggleTodo(int id) {
                await TodoService.ToggleAsync(int id);
                Todos = await TodoService.GetAllAsync();
            }
        }

public class TodoServiceTests {
            [Fact]
            public async Task AddAsync_CreatesItemWithCorrectTitle() {
                var service = new TodoService();
                var item = await service.AddAsync("Test");
                item.Title.Should().Be("Test");
            }
            [Fact]
            public async Task DeleteAsync_RemovesItem() {
                var service = new TodoService();
                await service.AddAsync("Test");
                await service.DeleteAsync(1);
                (await service.GetAllAsync()).Should().BeEmpty();
            }
            [Fact]
            public async Task ToggleAsync_FlipsIsCompleted() {
                var service = new TodoService();
                await service.AddAsync("Test");
                await service.ToggleAsync(1);
                (await service.GetAllAsync())[0].IsCompleted.Should().BeTrue();
                await service.ToggleAsync(1);
                (await service.GetAllAsync())[0].IsCompleted.Should().BeFalse();
            }
        }

public class TodoListTests {
            [Fact]
            public void TodoListBase_Initializes_CallsGetAllAsync() {
                var ctx = new Bunit.TestContext();
                var service = Substitute.For<ITodoService>();
                service.GetAllAsync().Returns(new List<TodoItem>());
                ctx.Services.AddSingleton(service);

                ctx.RenderComponent<TodoListBase>();

                service.Received(1).GetAllAsync();
            }
        }

public async Task ToggleAsync(int id)
    {
        var index = _items.FindIndex(x => x.Id == id);
        if (index != -1)
        {
            _items[index] = _items[index] with { IsCompleted = !_items[index].IsCompleted };
        }
    }

public async Task<TodoItem> AddAsync(string title)
    {
        var item = new TodoItem(_nextId++, title, false);
        _items.Add(item);
        return item;
    }

public async Task DeleteAsync(int id)
    {
        _items.RemoveAll(x => x.Id == id);
    }

public async Task<List<TodoItem>> GetAllAsync() => _items.ToList();