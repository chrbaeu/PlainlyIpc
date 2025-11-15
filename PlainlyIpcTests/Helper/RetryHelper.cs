using System.Diagnostics;

namespace PlainlyIpcTests.Helper;

internal class RetryHelper
{
    public static async Task<bool> WaitUntilWithTimeoutAsync(Func<bool> condition, bool expectedValue = true, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        var timestamp = Stopwatch.GetTimestamp();
        while (Stopwatch.GetElapsedTime(timestamp) < timeout)
        {
            if (condition() == expectedValue)
            {
                return expectedValue;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
        return condition();
    }
}
