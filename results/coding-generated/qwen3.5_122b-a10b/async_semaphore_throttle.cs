public ThrottledProcessor(int maxConcurrent)
        {
            _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

var tasksList = new List<Task<T>>();
            foreach (var factory in tasks)
            {
                var task = RunWithSemaphore(factory, ct);
                tasksList.Add(task);
            }
            return await Task.WhenAll(tasksList);

var runningTasks = new List<Task<T>>();
            foreach (var factory in tasks)
            {
                runningTasks.Add(SemaphoreWrapper(factory, ct));
            }
            return await Task.WhenAll(runningTasks);

await _semaphore.WaitAsync(ct);
            try {
                return await factory(ct);
            } finally {
                _semaphore.Release();
            }

public class ThrottledProcessor
    {
        private readonly System.Threading.SemaphoreSlim _semaphore;

        public ThrottledProcessor(int maxConcurrent)
        {
            _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
            System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
            System.Threading.CancellationToken ct)
        {
            var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();
            foreach (var factory in tasks)
            {
                taskList.Add(RunWithSemaphore(factory, ct));
            }
            return await System.Threading.Tasks.Task.WhenAll(taskList);
        }

        private async System.Threading.Tasks.Task<T> RunWithSemaphore<T>(
            System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> factory,
            System.Threading.CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await factory(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

public class ThrottledProcessor
{
    private readonly System.Threading.SemaphoreSlim _semaphore;

    public ThrottledProcessor(int maxConcurrent)
    {
        _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();
        foreach (var factory in tasks)
        {
            taskList.Add(RunWithSemaphore(factory, ct));
        }
        var results = await System.Threading.Tasks.Task.WhenAll(taskList);
        return new System.Collections.Generic.List<T>(results);
    }

    private async System.Threading.Tasks.Task<T> RunWithSemaphore<T>(
        System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> factory,
        System.Threading.CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            return await factory(ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}