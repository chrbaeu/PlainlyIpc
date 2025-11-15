using System.Net;

namespace PlainlyIpcTests.Helper;

internal static class ConnectionAddressFactory
{
    private static object lockObject = new();
    private static int portCounter;

    public static IPEndPoint GetIpEndPoint()
    {
        lock (lockObject)
        {
            return new(IPAddress.Loopback, 60500 + portCounter++);
        }
    }

    public static string GetNamedPipeName()
    {
        return $"NP-{Guid.NewGuid()}";
    }

}
