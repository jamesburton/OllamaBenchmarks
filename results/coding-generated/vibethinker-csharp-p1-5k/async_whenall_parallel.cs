interface DataLoader
{
            Task<string> LoadAsync(string source, CancellationToken ct);
        }

class ParallelDataService
{
            private readonly DataLoader loaderA;
            private readonly DataLoader loaderB;
            private readonly DataLoader loaderC;

            ParallelDataService(DataLoader loaderA, DataLoader loaderB, DataLoader loaderC)
            {
                this.loaderA = loaderA;
                this.loaderB = loaderB;
                this.loaderC = loaderC;
            }

            Task<List<string>> LoadAllAsync CancellationTokenToken cancellationToken)
            {
                return Task.WhenAll(
                    loaderA.LoadAsync("sourceA", cancellationToken),
                    loaderB.LoadAsync("sourceB", cancellationToken),
                    loaderC.LoadAsync("sourceC", cancellationToken))
                    .AsTask();
            }
        }