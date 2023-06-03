using PlainlyIpcChatDemo.RpcDemo.Services;
using System.Diagnostics;

namespace PlainlyIpcChatDemo.RpcDemo;

internal class NamedPipeRpcDemo
{
    /// <summary>
    /// Demo for a simple named pipe based RPC chat.
    /// </summary>
    public static async Task Run(IpcFactory ipcFactory)
    {
        Console.WriteLine($"Starting NamedPipeRpcChat");

        IIpcHandler ipcHandler;
        Console.WriteLine($"Start a new server (y/n)?");
        var newServer = Console.ReadLine() ?? "";
        if ("y".Equals(newServer, StringComparison.OrdinalIgnoreCase))
        {
            var myAddress = $"np-rpc-chat";
            Console.WriteLine($"Server name/address: {myAddress}");
            ipcHandler = await ipcFactory.CreateNamedPipeIpcServer(myAddress);
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
        ipcHandler.ErrorOccurred += (s, e) => Console.WriteLine(e.Message);
        ipcHandler.RegisterService<IChatService>(new ChatService(x =>
        {
            Debug.WriteLine(x);
            Console.WriteLine(x);
        }));
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
