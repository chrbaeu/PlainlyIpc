using System.Net;
using System.Threading;

namespace PlainlyIpcTests.Shared;

internal class ConnectionAddressFactory
{
    private static volatile int portCounter = 0;

    public static IPEndPoint GetIpEndPoint()
    {
        Interlocked.Increment(ref portCounter);
        return new(IPAddress.Loopback, 60000 + portCounter++);
    }

    public static string GetNamedPipeName()
    {
        return $"NP-{Guid.NewGuid()}";
    }

}
