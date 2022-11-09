using PlainlyIpcChatDemo.RpcDemo.Services;
using System.Net;

namespace PlainlyIpcChatDemo.RpcDemo;

internal class TcpRpcDemo
{
    /// <summary>
    /// Demo for a simple TCP based RPC chat.
    /// </summary>
    public static async Task Run(IpcFactory ipcFactory)
    {
        Console.WriteLine($"Starting TcpRpcChat");

        IIpcHandler ipcHandler;
        Console.WriteLine($"Start a new server (y/n)?");
        var newServer = Console.ReadLine() ?? "";
        if ("y".Equals(newServer, StringComparison.OrdinalIgnoreCase))
        {
            IPEndPoint myAddress = new(IPAddress.Any, 60042);
            Console.WriteLine($"Server name/address:");
            foreach (var item in (await Dns.GetHostEntryAsync(Dns.GetHostName())).AddressList)
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
        ipcHandler.ErrorOccurred += (s, e) => Console.WriteLine(e.Message);
        ipcHandler.RegisterService<IChatService>(new ChatService(x => Console.WriteLine(x)));
        ChatServiceRemoteProxy proxy = new(ipcHandler);

        Console.WriteLine($"Ready to send and receive messages (Enter 'exit' to exit the app).");
        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrEmpty(line)) { continue; }
            if ("exit".Equals(line, StringComparison.OrdinalIgnoreCase)) { break; }
            await proxy.SendMessage(line);
        }
        ipcHandler.Dispose();
    }

}
