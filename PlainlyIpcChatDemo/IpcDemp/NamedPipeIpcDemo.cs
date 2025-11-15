namespace PlainlyIpcChatDemo.IpcDemo;

internal static class NamedPipeIpcDemo
{
    /// <summary>
    /// Demo for a simple named pipe based IPC chat.
    /// </summary>
    public static async Task Run(IpcFactory ipcFactory)
    {
        Console.WriteLine($"Starting NampedPipeIpcChat");

        IIpcHandler ipcHandler;
        Console.WriteLine($"Start a new server (y/n)?");
        var newServer = Console.ReadLine() ?? "";
        if ("y".Equals(newServer, StringComparison.OrdinalIgnoreCase))
        {
            var myAddress = $"np-ipc-chat";
            Console.WriteLine($"Server name/address: {myAddress}");
#if NET8_0_OR_GREATER
            ipcHandler = await ipcFactory.CreateNamedPipeIpcServer(myAddress, null);
#else
            ipcHandler = await ipcFactory.CreateNamedPipeIpcServer(myAddress);
#endif
            Console.WriteLine("Server is running ...");
        }
        else
        {
            Console.WriteLine($"Enter server name/address:");
            var destAddress = Console.ReadLine() ?? "";
            Console.WriteLine($"Connecting to {destAddress} ...");
            ipcHandler = await ipcFactory.CreateNamedPipeIpcClient(destAddress);
            Console.WriteLine("Client is connected ...");
        }
        ipcHandler.MessageReceived += (s, e) => Console.WriteLine(e.Value);
        ipcHandler.ErrorOccurred += (s, e) => Console.WriteLine(e.Message);

        Console.WriteLine($"Ready to send and receive messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if ("exit".Equals(line, StringComparison.OrdinalIgnoreCase)) { break; }
            await ipcHandler.SendStringAsync(line);
        }
        ipcHandler.Dispose();
    }

}
