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
            // ... implementation
        }

public class TodoListBase : ComponentBase {
            [Inject] public ITodoService TodoService { get; set; }
            public List<TodoItem> Todos { get; set; } = new();
            public string NewTitle { get; set; } = "";
            protected override async Task OnInitializedAsync() {
                Todos = await TodoService.GetAllAsync();
            }
            public async Task AddTodo() {
                if (string.IsNullOrWhiteSpace(NewTitle)) return;
                var item = await TodoService.AddAsync(NewTitle);
                Todos = await TodoService.GetAllAsync();
                NewTitle = "";
            }
            public async Task DeleteTodo(int id) {
                await TodoService.DeleteAsync(id);
                Todos = await TodoService.GetAllAsync();
            }
            public async Task ToggleTodo(int id) {
                await TodoService.ToggleAsync(id);
                Todos = await TodoService.GetAllAsync();
            }
        }

[Fact]
            public async Task AddAsync_CreatesItem() {
                var service = new TodoService();
                var item = await service.AddAsync("Test");
                item.Should().NotBeNull();
                item.Id.Should().Be(1);
                item.Title.Should().Be("Test");
            }

[Fact]
            public async Task DeleteAsync_RemovesItem() {
                var service = new TodoService();
                await service.AddAsync("A");
                await service.AddAsync("B");
                await service.DeleteAsync(1);
                var list = await service.GetAllAsync();
                list.Should().ContainSingle();
                list[0].Id.Should().Be(2);
            }

[Fact]
            public async Task ToggleAsync_FlipsCompleted() {
                var service = new TodoService();
                var item = await service.AddAsync("A");
                item.IsCompleted.Should().BeFalse();
                await service.ToggleAsync(1);
                var list = await service.GetAllAsync();
                list[0].IsCompleted.Should().BeTrue();
            }

[Fact]
            public async Task OnInitializedAsync_LoadsTodos() {
                var substitute = Substitute.For<ITodoService>();
                substitute.GetAllAsync().ReturnsAsync(new List<TodoItem>());
                var ctx = new Bunit.TestContext();
                var cut = ctx.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, substitute));
                // Verify call
                substitute.Received().GetAllAsync();
            }

[Fact]
                public async Task OnInitializedAsync_CallsGetAllAsync() {
                    var substitute = Substitute.For<ITodoService>();
                    substitute.GetAllAsync().ReturnsAsync(new List<TodoItem>());
                    var ctx = new Bunit.TestContext();
                    var cut = ctx.RenderComponent<TodoListBase>(p => p.Add(x => x.TodoService, substitute));
                    substitute.Received().GetAllAsync();
                }