using PlainlyIpcChatDemo.IpcDemp;

namespace PlainlyIpcChatDemo;

internal class Program
{
    public static async Task Main(string[] args)
    {
        IpcFactory ipcFactory = new();

        //var outputPath = @"C:\Git\chrbaeu\PlainlyIpc\PlainlyIpcChatDemo\RpcDemo\";
        //RemoteProxyCreator.CreateProxyClass<IChatService>(outputPath, $"{nameof(PlainlyIpcChatDemo)}.{nameof(RpcDemo)}");
        //RemoteProxyCreator.CreateProxyClass<IRpcTestService>(outputPath, $"PlainlyIpcTests.Rpc");
        //return;

        try
        {
            Console.WriteLine($"Start demo with TCP instead of named pipes (y/n)?");
            string newServer = Console.ReadLine() ?? "";
            if (newServer.ToLower() == "y")
            {
                await TcpIpcDemo.Run(ipcFactory);
            }
            else
            {
                await NamedPipeIpcDemo.Run(ipcFactory);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine("Chat closed press key to exit app.");
        Console.ReadKey();
    }

}
