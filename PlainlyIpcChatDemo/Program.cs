using PlainlyIpc.Converter;
using PlainlyIpc.EventArgs;
using PlainlyIpc.Interfaces;
using PlainlyIpc.IPC;
using PlainlyIpc.NamedPipe;

namespace PlainlyIpcChatDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Starting NampedPipeChat:");

        string myAddress = $"np-{new Random().Next(10, 99)}";

        IObjectConverter objectConverter = new BinaryObjectConverter();

        Console.WriteLine($"You: {myAddress}");
        // Server
        var server = new NamedPipeServer(myAddress);
        _ = server.StartListenAync();
        var ipcReceiver = new IpcReceiver(server, objectConverter);
        ipcReceiver.MessageReceived += Server_ObjectReceived;

        Console.WriteLine($"Enter destination:");
        var destAddress = Console.ReadLine() ?? "";

        Console.WriteLine($"Connecting to {destAddress} ...");
        // Client
        var client = new NamedPipeClient(destAddress);
        await client.ConnectAsync();
        var ipcSender = new IpcSender(client, objectConverter);

        Console.WriteLine($"Ready to send messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.ToLower() == "exit") { break; }
            // Send
            await ipcSender.SendStringAsync(line);
        }

    }

    private static void Server_ObjectReceived(object? sender, IpcMessageReceivedEventArgs e)
    {
        Console.WriteLine(e?.Value);
    }
}
