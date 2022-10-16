namespace PlainlyIpc.Internal;
internal static class TaskExtensions
{
#if NETSTANDARD
    public static async Task WaitAsync(this Task task, TimeSpan timeout)
    {
        var delayTask = Task.Delay(timeout);
        await Task.WhenAny(task, delayTask);
        if (!task.IsCompleted) { throw new TimeoutException(); }
    }
#endif
}
