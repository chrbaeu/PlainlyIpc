namespace PlainlyIpc.Internal;
internal static class TaskExtensions
{
#if NETSTANDARD
    public static async Task WaitAsync(this Task task, TimeSpan timeout)
    {
        var delayTask = Task.Delay(timeout);
        await Task.WhenAny(task, delayTask).ConfigureAwait(false);
        if (!task.IsCompleted) { throw new TimeoutException(); }
    }
    public static async Task<T> WaitAsync<T>(this Task<T> task, TimeSpan timeout)
    {
        var delayTask = Task.Delay(timeout);
        await Task.WhenAny(task, delayTask).ConfigureAwait(false);
        if (!task.IsCompleted) { throw new TimeoutException(); }
        return task.GetAwaiter().GetResult();
    }
#endif
}
