using System.Net;
using System.Threading;

namespace PlainlyIpcTests.Helper;

internal sealed class ConnectionAddressFactory
{
    private static volatile int portCounter;

    public static IPEndPoint GetIpEndPoint()
    {
        Interlocked.Increment(ref portCounter);
        return new(IPAddress.Loopback, 60500 + portCounter++);
    }

    public static string GetNamedPipeName()
    {
        return $"NP-{Guid.NewGuid()}";
    }

}
