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

            foreach (var taskFactory in tasks)
            {
                // We must start the task immediately to ensure they are all launched before WhenAll
                // But the logic inside the task handles the semaphore.
                taskList.Add(ExecuteWithThrottleAsync(taskFactory, ct));
            }

            var results = await System.Threading.Tasks.Task.WhenAll(taskList);
            return new System.Collections.Generic.List<T>(results);
        }

        private async System.Threading.Tasks.Task<T> ExecuteWithThrottleAsync<T>(
            System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> taskFactory,
            System.Threading.CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await taskFactory(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

public async System.Threading.Tasks.Task<System.Collections.Generic.List<T>> ProcessAllAsync<T>(
        System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>>> tasks,
        System.Threading.CancellationToken ct)
    {
        // Projecting the factories into running tasks. 
        // The 'async' lambda starts the execution of the task factory logic immediately upon creation in the Select.
        var runningTasks = System.Linq.Enumerable.Select(tasks, async factory =>
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
        });

        // Convert to array/list to ensure we have the tasks ready to await
        var taskArray = System.Linq.Enumerable.ToArray(runningTasks);

        var results = await System.Threading.Tasks.Task.WhenAll(taskArray);
        return new System.Collections.Generic.List<T>(results);
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
            var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();

            foreach (var taskFactory in tasks)
            {
                // We define the async wrapper and add it to the list.
                // The execution of the wrapper starts here.
                runningTasks.Add(ProcessSingleTaskAsync(taskFactory, ct));
            }

            var results = await System.Threading.Tasks.Task.WhenAll(runningTasks.ToArray());
            return new System.Collections.Generic.List<T>(results);
        }

        private async System.Threading.Tasks.Task<T> ProcessSingleTaskAsync<T>(
            System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> taskFactory,
            System.Threading.CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await taskFactory(ct);
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
            var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task<T>>();

            foreach (var taskFactory in tasks)
            {
                runningTasks.Add(ExecuteWithThrottleAsync(taskFactory, ct));
            }

            var results = await System.Threading.Tasks.Task.WhenAll(runningTasks.ToArray());
            return new System.Collections.Generic.List<T>(results);
        }

        private async System.Threading.Tasks.Task<T> ExecuteWithThrottleAsync<T>(
            System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task<T>> taskFactory,
            System.Threading.CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                return await taskFactory(ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }