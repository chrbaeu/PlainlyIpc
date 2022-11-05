using System.Net;

namespace PlainlyIpcChatDemo.IpcDemp;

internal class TcpIpcDemo
{
    /// <summary>
    /// Demo for a simple TCP based IPC chat.
    /// </summary>
    public static async Task Run(IpcFactory ipcFactory)
    {
        Console.WriteLine($"Starting TcpIpcChat");

        IIpcHandler ipcHandler;
        Console.WriteLine($"Start a new server (y/n)?");
        var newServer = Console.ReadLine() ?? "";
        if (newServer.ToLower() == "y")
        {
            IPEndPoint myAddress = new(IPAddress.Any, 60042);
            Console.WriteLine($"Server name/address:");
            foreach (var item in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                Console.WriteLine($"{item}");
            }
            ipcHandler = await ipcFactory.CreateTcpIpcServer(myAddress);
            Console.WriteLine("Server is running ...");
        }
        else
        {
            Console.WriteLine($"Enter server name/address:");
            var destAddress = Console.ReadLine() ?? "";
            ipcHandler = await ipcFactory.CreateTcpIpcClient(new(IPAddress.Parse(destAddress), 60042));
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
        ipcHandler.Dispose();
    }

}
