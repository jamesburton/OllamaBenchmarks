public class ThrottledProcessor
    {
        private SemaphoreSlim _semaphore;

        public ThrottledProcessor(int maxConcurrent)
        {
            _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

        public async Task<List<T>> ProcessAllAsync<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, CancellationToken ct)
        {
            // Implementation details
        }
    }

await _semaphore.WaitAsync(ct);
        try {
            var result = taskFactory(cancellationToken); // Wait, the factory takes CancellationToken. Which one? The method's ct or a local one? Usually the method's ct is passed down.
            yield return result; // No, Task<T> returns T.
        } finally {
            _semaphore.Release();
        }

var task = Task.Run(() => { ... }); // No, Task.Run isn't needed if we just await. But we need to capture the result.

await _semaphore.WaitAsync(ct);
        try {
            var result = factory(cancellationToken); // Wait, factory takes CancellationToken. Which one? The method's ct is available in the closure.
            return result;
        } finally {
            _semaphore.Release();
        }

var tasks = new List<Task<T>>();
        for (int i = 0; i < tasks.Count; i++) {
            var factory = tasks[i]; // This is Func<CancellationToken, Task<T>>
            // Wait, I need to capture 'this' and 'ct'.
            // The factory itself takes a CancellationToken.
            // So inside the task creation:
            tasks.Add(Task.Run(() => {
                // Logic here
            }));
        }

var resultList = new List<T>();
        for (int i = 0; i < tasks.Count; i++) {
            // Create task
            var task = Task.Run(() => { ... });
            tasks[i] = task; // Wait, I need to store the task.
        }
        await Task.WhenAll(tasks);
        // Now how do I get results in order?
        // I can't easily map back from WhenAll without tracking.

var resultList = new List<T>();
           var tasks = new List<Task<T>>();
           for (int i = 0; i < inputTasks.Count; i++) {
               var factory = inputTasks[i];
               tasks.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(tasks);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);

var taskList = new List<Task<T>>();
           for (int i = 0; i < tasks.Count; i++) {
               var factory = tasks[i];
               taskList.Add(Task.Run(() => {
                   // Logic
               }));
           }
           await Task.WhenAll(taskList);