public interface IDataLoader
        {
            Task<string> LoadAsync(string source, CancellationToken ct);
        }

public class ParallelDataService
        {
            private readonly IDataLoader loaderA;
            private readonly IDataLoader loaderB;
            private readonly IDataLoader loaderC;

            public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
            {
                this.loaderA = loaderA;
                this.loaderB = loaderB;
                this.loaderC = loaderC;
            }

            public Task<List<string>> LoadAllAsync(CancellationToken ct)
            {
                return Task.WhenAll(
                    new List<Task<string>>()
                    {
                        loaderA.LoadAsync("sourceA", ct),
                        loaderB.LoadAsync("sourceB", ct),
                        loaderC.LoadAsync("sourceC", ct)
                    }
                ).ContinueWith(t =>
                {
                    // Wait, Task.WhenAll returns a Task<Task<string>>? No.
                    // Task.WhenAll takes IEnumerable<Task<T>> and returns Task<T>.
                    // But here we have different tasks potentially returning strings.
                    // Actually, Task.WhenAll returns Task<T> only if all tasks return T.
                    // Wait, Task.WhenAll(IEnumerable<Task<T>>) returns Task<T>.
                    // So I need to collect the results into a list first?
                    // No, Task.WhenAll returns a single Task that completes when all inner tasks complete.
                    // But it doesn't give me an ordered list easily without mapping.
                    // Actually, Task.WhenAll returns Task<Task<string>> if I pass IEnumerable<Task<string>>.
                    // Wait, the return type of LoadAsync is Task<string>.
                    // So I can do:
                    // var tasks = new List<Task<string>> { ... };
                    // return Task.WhenAll(tasks).ContinueWith(t => t.Result); // No, that's not right.
                });
            }
        }

var tasks = new List<Task<string>> { ... };
        Task.WhenAll(tasks).Wait(); // Blocks until done
        return new List<string> { taskA.Result, taskB.Result, taskC.Result };

var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        Task.WhenAll(taskA, taskB, taskC).Wait(); // Wait for completion

        return new List<string> { taskA.Result, taskB.Result, taskC.Result };

public Task<List<string>> LoadAllAsync(CancellationToken ct)
        {
            var taskA = loaderA.LoadAsync("sourceA", ct);
            var taskB = loaderB.LoadAsync("sourceB", ct);
            var taskC = loaderC.LoadAsync("sourceC", ct);

            return Task.WhenAll(new[] { taskA, taskB, taskC }).ContinueWith(t =>
            {
                // Wait for completion? No.
                // ContinueWith doesn't give me the results easily without accessing .Result on each task.
                // But I need to return a Task<List<string>>.
                // So I can use Task.Run or just await logic inside an async method.
                // Since this is C# 5+ (Task), I can use `await` if it's async, but the signature says `Task<List<string>>`.
                // If I return a Task, I need to yield control until done.
            });
        }

public Task<List<string>> LoadAllAsync(CancellationToken ct)
        {
            var taskA = loaderA.LoadAsync("sourceA", ct);
            var taskB = loaderB.LoadAsync("sourceB", ct);
            var taskC = loaderC.LoadAsync("sourceC", ct);

            return Task.WhenAll(new[] { taskA, taskB, taskC }).ContinueWith(t =>
            {
                // This is tricky. ContinueWith takes a Func<Task> or Action?
                // It returns the result of the continuation.
                // I need to return a Task<List<string>>.
                // So I can't just use ContinueWith on the WhenAll task easily to get the list without awaiting the individual tasks inside the continuation.
            });
        }

var tasks = new List<Task<string>> { taskA, taskB, taskC };
        return Task.WhenAll(tasks).ContinueWith(t => 
        {
            // t.Result is the result of WhenAll? No.
            // WhenAll returns Task.
            // I need to access taskA.Result etc.
            var results = new List<string> { taskA.Result, taskB.Result, taskC.Result };
            return Task.FromResult(results);
        });

public Task<List<string>> LoadAllAsync(CancellationToken ct)
        {
            var taskA = loaderA.LoadAsync("sourceA", ct);
            var taskB = loaderB.LoadAsync("sourceB", ct);
            var taskC = loaderC.LoadAsync("sourceC", ct);

            return Task.WhenAll(new[] { taskA, taskB, taskC }).ContinueWith(t => 
            {
                // Wait for completion? No, WhenAll completes when all are done.
                // But I need to access the results of A, B, C.
                // Since they are stored in variables, I can access them.
                // However, accessing .Result on a Task inside a continuation is fine.
                var list = new List<string> { taskA.Result, taskB.Result, taskC.Result };
                return Task.FromResult(list);
            });
        }

var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        return Task.WhenAll(new[] { taskA, taskB, taskC }).ContinueWith(t => 
        {
            // t.Result is the result of WhenAll (which is void/Task).
            // I need to access taskA.Result.
            var list = new List<string> { taskA.Result, taskB.Result, taskC.Result };
            return Task.FromResult(list);
        });