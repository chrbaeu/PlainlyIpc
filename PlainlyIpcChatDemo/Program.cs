using PlainlyIpc.Converter;
using PlainlyIpc.EventArgs;
using PlainlyIpc.Interfaces;
using PlainlyIpc.IPC;

namespace PlainlyIpcChatDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Starting NampedPipeChat:");

        string myAddress = $"np-{new Random().Next(10, 99)}";

        IObjectConverter objectConverter = new BinaryObjectConverter();
        IpcFactory ipcFactory = new(objectConverter);

        Console.WriteLine($"You: {myAddress}");
        // Server
        IIpcHandler ipcHandler = await ipcFactory.CreateNampedPipeIpcServer(myAddress);
        ipcHandler.MessageReceived += IpcHandler_ObjectReceived;
        ipcHandler.ErrorOccurred += IpcHandler_ErrorOccurred;

        Console.WriteLine($"Enter destination or empty to be the server:");
        var destAddress = Console.ReadLine() ?? "";

        // Client
        if (!string.IsNullOrWhiteSpace(destAddress))
        {
            Console.WriteLine($"Connecting to {destAddress} ...");
            ipcHandler.Dispose();
            ipcHandler = await ipcFactory.CreateNampedPipeIpcClient(destAddress);
            ipcHandler.MessageReceived += IpcHandler_ObjectReceived;
            ipcHandler.ErrorOccurred += IpcHandler_ErrorOccurred;
        }

        Console.WriteLine($"Ready to send messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.ToLower() == "exit") { break; }
            // Send
            try
            {
                await ipcHandler.SendStringAsync(line);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        Console.WriteLine("Chat closed press key to exit app.");
        Console.ReadKey();
    }

    private static void IpcHandler_ObjectReceived(object? sender, IpcMessageReceivedEventArgs e)
    {
        Console.WriteLine(e.Value);
    }

    private static void IpcHandler_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        Console.WriteLine(e.Message);
    }

}
