public interface IDataLoader
        {
            Task<string> LoadAsync(string source, CancellationToken ct);
        }

public class ParallelDataService
        {
            private readonly IDataLoader _loaderA;
            private readonly IDataLoader _loaderB;
            private readonly IDataLoader _loaderC;

            public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
            {
                _loaderA = loaderA;
                _loaderB = loaderB;
                _loaderC = loaderC;
            }

            public System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
            {
                var taskA = _loaderA.LoadAsync("sourceA", ct);
                var taskB = _loaderB.LoadAsync("sourceB", ct);
                var taskC = _loaderC.LoadAsync("sourceC", ct);

                return System.Threading.Tasks.Task.WhenAll(taskA, taskB, taskC)
                    .ContinueWith(t => new System.Collections.Generic.List<string> { t.Result[0], t.Result[1], t.Result[2] });
            }
        }

public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
        {
            var results = await System.Threading.Tasks.Task.WhenAll(
                _loaderA.LoadAsync("sourceA", ct),
                _loaderB.LoadAsync("sourceB", ct),
                _loaderC.LoadAsync("sourceC", ct)
            );
            return new System.Collections.Generic.List<string> { results[0], results[1], results[2] };
        }

public System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
        {
            var tasks = new System.Threading.Tasks.Task<string>[3];
            tasks[0] = _loaderA.LoadAsync("sourceA", ct);
            tasks[1] = _loaderB.LoadAsync("sourceB", ct);
            tasks[2] = _loaderC.LoadAsync("sourceC", ct);

            return System.Threading.Tasks.Task.WhenAll(tasks).ContinueWith(t =>
            {
                return new System.Collections.Generic.List<string> { t.Result[0], t.Result[1], t.Result[2] };
            });
        }

public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
        {
            var results = await System.Threading.Tasks.Task.WhenAll(
                _loaderA.LoadAsync("sourceA", ct),
                _loaderB.LoadAsync("sourceB", ct),
                _loaderC.LoadAsync("sourceC", ct)
            );
            return new System.Collections.Generic.List<string> { results[0], results[1], results[2] };
        }

public interface IDataLoader
        {
            System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
        }

public class ParallelDataService
        {
            private readonly IDataLoader _loaderA;
            private readonly IDataLoader _loaderB;
            private readonly IDataLoader _loaderC;

            public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
            {
                _loaderA = loaderA;
                _loaderB = loaderB;
                _loaderC = loaderC;
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
            {
                var results = await System.Threading.Tasks.Task.WhenAll(
                    _loaderA.LoadAsync("sourceA", ct),
                    _loaderB.LoadAsync("sourceB", ct),
                    _loaderC.LoadAsync("sourceC", ct)
                );
                return new System.Collections.Generic.List<string> { results[0], results[1], results[2] };
            }
        }

public interface IDataLoader
        {
            System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
        }

        public class ParallelDataService
        {
            private readonly IDataLoader _loaderA;
            private readonly IDataLoader _loaderB;
            private readonly IDataLoader _loaderC;

            public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
            {
                _loaderA = loaderA;
                _loaderB = loaderB;
                _loaderC = loaderC;
            }

            public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
            {
                var results = await System.Threading.Tasks.Task.WhenAll(
                    _loaderA.LoadAsync("sourceA", ct),
                    _loaderB.LoadAsync("sourceB", ct),
                    _loaderC.LoadAsync("sourceC", ct)
                );
                return new System.Collections.Generic.List<string> { results[0], results[1], results[2] };
            }
        }

...