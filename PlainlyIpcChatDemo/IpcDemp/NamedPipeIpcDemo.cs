namespace PlainlyIpcChatDemo.IpcDemp;

internal class NamedPipeIpcDemo
{
    /// <summary>
    /// Demo for a simple named pipe based chat.
    /// </summary>
    public static async Task Run(IpcFactory ipcFactory)
    {
        Console.WriteLine($"Starting NampedPipeIpcChat");

        IIpcHandler ipcHandler;
        Console.WriteLine($"Start a new server (y/n)?");
        var newServer = Console.ReadLine() ?? "";
        if (newServer.ToLower() == "y")
        {
            var myAddress = $"np-{new Random().Next(10, 99)}";
            Console.WriteLine($"Server name/address: {myAddress}");
            ipcHandler = await ipcFactory.CreateNampedPipeIpcServer(myAddress);
            Console.WriteLine("Server is running ...");
        }
        else
        {
            Console.WriteLine($"Enter server name/address:");
            var destAddress = Console.ReadLine() ?? "";
            Console.WriteLine($"Connecting to {destAddress} ...");
            ipcHandler = await ipcFactory.CreateNampedPipeIpcClient(destAddress);
            Console.WriteLine("Client is connected ...");
        }
        ipcHandler.MessageReceived += (s, e) => Console.WriteLine(e.Value);
        ipcHandler.ErrorOccurred += (s, e) => Console.WriteLine(e.Message);

        Console.WriteLine($"Ready to send and receive messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if (line.ToLower() == "exit") { break; }
            await ipcHandler.SendStringAsync(line);
        }
    }

}
