﻿using PlainlyIpcChatDemo.IpcDemo;
using PlainlyIpcChatDemo.RpcDemo;

namespace PlainlyIpcChatDemo;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        IpcFactory ipcFactory = new();

        try
        {
            Console.WriteLine($"Start demo with TCP instead of named pipes (y/n)?");
            string newServer = Console.ReadLine() ?? "";
            if ("y".Equals(newServer, StringComparison.OrdinalIgnoreCase))
            {
                await TcpRpcDemo.Run(ipcFactory);
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
