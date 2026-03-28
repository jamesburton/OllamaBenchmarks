using Microsoft.AspNetCore.Components;
using NSubstitute;

namespace TodoApp.Pages
{
    public partial class TodoList : ComponentBase
    {
        private readonly IOrderService _orderService;
        private List<TodoItem> _todos = new();
        private string _newTitle = "";

        [Inject] public IOrderService OrderService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadTodosAsync();
        }

        private async Task LoadTodosAsync()
        {
            var todos = await _orderService.GetAllAsync();
            _todos.AddRange(todos);
        }

        private void AddTodo()
        {
            if (_newTitle == "")
                return;

            _todos.Add(new TodoItem
            {
                Id = _todos.Count + 1,
                Title = _newTitle,
                IsCompleted = false
            });

            _newTitle = "";
        }
    }
}