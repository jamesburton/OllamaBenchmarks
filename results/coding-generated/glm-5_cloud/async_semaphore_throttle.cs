public class ThrottledProcessor
        {
            private readonly System.Threading.SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(IEnumerable<Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks, System.Threading.CancellationToken ct)
            {
                // Implementation
            }
        }

var wrappedTasks = tasks.Select(async factory => {
            await _semaphore.WaitAsync(ct);
            try {
                return await factory(ct);
            } finally {
                _semaphore.Release();
            }
        }).ToList();

public class ThrottledProcessor
        {
            private readonly System.Threading.SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(
                System.Collections.Generic.IEnumerable<Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks,
                System.Threading.CancellationToken ct)
            {
                // Project the factories into a list of running tasks.
                // We use a List to ensure we iterate and start everything immediately.
                var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

                foreach (var factory in tasks)
                {
                    // Define the local async operation
                    async T RunThrottled()
                    {
                        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                        try
                        {
                            return await factory(ct).ConfigureAwait(false);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }

                    // Start the task and add to list
                    runningTasks.Add(RunThrottled());
                }

                // Await all
                await System.Threading.Tasks.Task.WhenAll(runningTasks).ConfigureAwait(false);

                // Gather results in order
                var results = new System.Collections.Generic.List(runningTasks.Count);
                foreach (var t in runningTasks)
                {
                    results.Add(t.Result);
                }

                return results;
            }
        }

public class ThrottledProcessor
        {
            private readonly System.Threading.SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(
                System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks,
                System.Threading.CancellationToken ct)
            {
                var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

                foreach (var factory in tasks)
                {
                    // Create the wrapper task
                    // Note: We must capture 'factory' correctly.
                    // Using a local function is cleanest for the async state machine.
                    async System.Threading.Tasks.Task ThrottledOperation()
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

                    // Start the task immediately
                    taskList.Add(ThrottledOperation());
                }

                // Wait for all to complete
                await System.Threading.Tasks.Task.WhenAll(taskList);

                // Collect results
                var results = new System.Collections.Generic.List(taskList.Count);
                foreach (var t in taskList)
                {
                    results.Add(t.Result);
                }

                return results;
            }
        }

var taskList = tasks.Select(async factory => {
            await _semaphore.WaitAsync(ct);
            try {
                return await factory(ct);
            } finally {
                _semaphore.Release();
            }
        }).ToList();

public class ThrottledProcessor
        {
            private readonly System.Threading.SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(
                System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks,
                System.Threading.CancellationToken ct)
            {
                var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

                foreach (var factory in tasks)
                {
                    runningTasks.Add(RunThrottledFactory(factory, ct));
                }

                await System.Threading.Tasks.Task.WhenAll(runningTasks);

                var results = new System.Collections.Generic.List(runningTasks.Count);
                foreach (var task in runningTasks)
                {
                    results.Add(task.Result);
                }

                return results;
            }

            private async System.Threading.Tasks.Task RunThrottledFactory(
                System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task> factory,
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

foreach (var factory in tasks)
        {
            async System.Threading.Tasks.Task LocalTask()
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
            runningTasks.Add(LocalTask());
        }

public class ThrottledProcessor
        {
            private readonly System.Threading.SemaphoreSlim _semaphore;

            public ThrottledProcessor(int maxConcurrent)
            {
                _semaphore = new System.Threading.SemaphoreSlim(maxConcurrent, maxConcurrent);
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(
                System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks,
                System.Threading.CancellationToken ct)
            {
                var taskList = new System.Collections.Generic.List<System.Threading.Tasks.Task>>();

                foreach (var factory in tasks)
                {
                    taskList.Add(ProcessSingleAsync(factory, ct));
                }

                await System.Threading.Tasks.Task.WhenAll(taskList);

                var results = new System.Collections.Generic.List(taskList.Count);
                foreach (var task in taskList)
                {
                    results.Add(task.Result);
                }

                return results;
            }

            private async System.Threading.Tasks.Task ProcessSingleAsync(
                System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task> factory,
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

            public async System.Threading.Tasks.Task<System.Collections.Generic.List> ProcessAllAsync(
                System.Collections.Generic.IEnumerable<System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>> tasks,
                System.Threading.CancellationToken ct)
            {
                var runningTasks = new System.Collections.Generic.List<System.Threading.Tasks.Task>();

                foreach (var factory in tasks)
                {
                    async System.Threading.Tasks.Task ThrottledTask()
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

                    runningTasks.Add(ThrottledTask());
                }

                await System.Threading.Tasks.Task.WhenAll(runningTasks);

                var results = new System.Collections.Generic.List(runningTasks.Count);
                foreach (var t in runningTasks)
                {
                    results.Add(t.Result);
                }

                return results;
            }
        }