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
                    loaderA.LoadAsync("sourceA", ct),
                    loaderB.LoadAsync("sourceB", ct),
                    loaderC.LoadAsync("sourceC", ct)
                ).ContinueWith(t =>
                {
                    // Wait for completion to ensure order, though WhenAll returns a Task<List<T>>
                    // Actually, Task.WhenAll returns a Task<List<T>> where the list is ordered by the order of tasks passed in.
                    // So I can just return the result directly.
                    return t.Result;
                });
            }
        }

public Task<List<string>> LoadAllAsync(CancellationToken ct)
        {
            return Task.WhenAll(
                new List<Task<string>>
                {
                    loaderA.LoadAsync("sourceA", ct),
                    loaderB.LoadAsync("sourceB", ct),
                    loaderC.LoadAsync("sourceC", ct)
                }
            );
        }

return Task.WhenAll(
            loaderA.LoadAsync("sourceA", ct),
            loaderB.LoadAsync("sourceB", ct),
            loaderC.LoadAsync("sourceC", ct)
        );

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
                loaderA.LoadAsync("sourceA", ct),
                loaderB.LoadAsync("sourceB", ct),
                loaderC.LoadAsync("sourceC", ct)
            );
        }
    }

public interface IDataLoader
        {
            System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
        }

        public class ParallelDataService
        {
            private readonly System.Threading.Tasks.Task<string> _result; // Wait, fields are IDataLoader
            private readonly IDataLoader loaderA;
            private readonly IDataLoader loaderB;
            private readonly IDataLoader loaderC;

            public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
            {
                this.loaderA = loaderA;
                this.loaderB = loaderB;
                this.loaderC = loaderC;
            }

            public System.Collections.Generic.Task<List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
            {
                return System.Threading.Tasks.Task.WhenAll(
                    loaderA.LoadAsync("sourceA", ct),
                    loaderB.LoadAsync("sourceB", ct),
                    loaderC.LoadAsync("sourceC", ct)
                );
            }
        }

public interface IDataLoader
        {
            System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
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

            public System.Collections.Generic.List<string> LoadAllAsync(System.Threading.CancellationToken ct)
            {
                return System.Threading.Tasks.Task.WhenAll(
                    loaderA.LoadAsync("sourceA", ct),
                    loaderB.LoadAsync("sourceB", ct),
                    loaderC.LoadAsync("sourceC", ct)
                );
            }
        }