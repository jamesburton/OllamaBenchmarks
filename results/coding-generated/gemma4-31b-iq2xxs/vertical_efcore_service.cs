*   Goal: Create a complete EF Core service vertical slice in a single C# file.
    *   Entities: `TodoItem` (Id, Title, IsComplete, CreatedAt).
    *   DbContext: `TodoDbContext` (inheriting from `DbContext`).
    *   Interface: `ITodoService` (CreateAsync, GetAllAsync, GetByIdAsync, CompleteAsync, DeleteAsync).
    *   Implementation: `TodoService` (implementing `ITodoService`).
    *   Constraints:
        *   No namespaces.
        *   `using Microsoft.EntityFramework