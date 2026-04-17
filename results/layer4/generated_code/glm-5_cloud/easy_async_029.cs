using System.Collections.Generic;
using System.Threading.Tasks;

public static class AsyncString
{
    public static async Task<string> ConcatAsync(IEnumerable<Task<string>> tasks)
    {
        var taskList = new List<Task<string>>(tasks);
        await Task.WhenAll(taskList);

        var stringBuilder = new System.Text.StringBuilder();
        foreach (var task in taskList)
        {
            stringBuilder.Append(task.Result);
        }

        return stringBuilder.ToString();
    }
}