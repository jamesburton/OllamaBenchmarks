public interface IDataLoader {
            System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.Tasks.s_1.Cancellation학생Token ct); // Wait,S_1 is not right.
        }