using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class TaskRacer
{
    public static async Task<T> FirstCompleted<T>(IEnumerable<Task<T>> tasks)
    {
        // Task.WhenAny returns a Task<Task<T>> that completes when any of the provided tasks completes.
        // We await this to get the completed task.
        Task<T> completedTask = await Task.WhenAny(tasks);

        // Return the result of the task that finished first.
        // Note: If the first completed task faulted or was canceled, this will throw the exception.
        return await completedTask;
    }
}