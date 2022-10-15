using PlainlyIpc;
using PlainlyIpc.EventArgs;

namespace PlainlyIpcChatDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Starting NampedPipeChat:");

        string myAddress = $"np-{new Random().Next(10, 99)}";

        Console.WriteLine($"You: {myAddress}");
        // Server
        var server = new NamedPipeIpcServer(myAddress);
        _ = server.StartListenAync();
        server.ObjectReceived += Server_ObjectReceived; ;

        Console.WriteLine($"Enter destination:");
        var destAddress = Console.ReadLine() ?? "";

        Console.WriteLine($"Connecting to {destAddress} ...");
        // Client
        var client = new NamedPipeIpcClient(destAddress);
        await client.ConnectAsync();

        Console.WriteLine($"Ready to send messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.ToLower() == "exit") { break; }
            // Send
            await client.SendAsync(line);
        }

    }

    private static void Server_ObjectReceived(object? sender, ObjectReceivedEventArgs e)
    {
        Console.WriteLine(e?.Value);
    }
}
