# PlainlyIpc

A minimalistic, easy-to-use library for inter-process communication (IPC) with .NET.

The basic one-way IPC communication is realized via the `IIpcSender` and `IIpcReceiver` interfaces. The `IIpcHandler` interface relies on these interfaces and enables bidirectional communication and Remote Procedure Calls (RPC).

The `IpcFactory` class allows the easy creation of corresponding instances for these interfaces:
```csharp
IpcFactory ipcFactory = new IpcFactory();
IpcFactory ipcFactory = new IpcFactory(objectConverter);
```

Creating `IIpcSender` & `IIpcReceiver` instances based on named pipes or TCP:
```csharp
// Named pipe
IIpcReceiver receiver = await ipcFactory.CreateNamedPipeIpcReceiver(namedPipeName);
IIpcSender sender = await ipcFactory.CreateNamedPipeIpcSender(namedPipeName);

// TCP
IIpcReceiver receiver = await ipcFactory.CreateTcpIpcReceiver(namedPipeName);
IIpcSender sender = await ipcFactory.CreateTcpIpcSender(namedPipeName);
```

Creatting `IpcHandler` instances based on named pipes or TCP:
```csharp
// Named pipe
IIpcHandler server = await ipcFactory.CreateNamedPipeIpcServer(namedPipeName);
IIpcHandler client = await ipcFactory.CreateNamedPipeIpcClient(namedPipeName);

// TCP
IIpcHandler server = await ipcFactory.CreateTcpIpcServer(namedPipeName);
IIpcHandler client = await ipcFactory.CreateTcpIpcClient(namedPipeName);
```

The `IIpcSender` offers the following methods:
```csharp
Task SendAsync(byte[] data);
Task SendAsync(ReadOnlyMemory<byte> data);
Task SendStringAsync(string data);
Task SendObjectAsync<T>(T data);
```

The `IIpcReceiver` offers the following events:
```csharp
event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;
event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
```

The `IpcHandler` offers the features of `IIpcSender` and `IIpcReceiver` as well as the following RPC capabilities:
```csharp
void RegisterService(Type type, object service);
void RegisterService<TIService>(TIService service) where TIService : notnull;
Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, Task<TResult>>> func);
Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, TResult>> func);
Task ExecuteRemote<TIRemnoteService>(Expression<Action<TIRemnoteService>> func);
```

Minimal example:
```csharp
IpcFactory ipcFactory = new IpcFactory();

using IIpcHandler server = await ipcFactory.CreateNamedPipeIpcServer("MyNamedPipe");
server.MessageReceived += (sender, e) =>
{
	Console.WriteLine(e.Value);
};

using IIpcHandler client = await ipcFactory.CreateNamedPipeIpcClient("MyNamedPipe");
await client.SendStringAsync("Hello World!");

Console.ReadKey();
```
Additional usage examples can be found in the sample project "PlainlyIpcChatDemo" and the tests in the "PlainlyIpcTests" project.

For the easier use of the ExecuteRemote functionality of the `IIpcHandler` you can create proxy classes for RPC interfaces autmatically.

With the `RemoteProxyCreator`:
```csharp
RemoteProxyCreator.CreateProxyClass<TInterface>(string outputFolderPath, string baseNamespace);
```
Or via the "PlainlyIpc.SourceGenerator" package. This provides a `[GenerateProxy]` attribute that can be annotated to remote interfaces or partial classes that implement a remote interface. The source generator then generates the proxy implementations of the functions defined in the remote interface automatically.
```csharp
[GenerateProxy] // Generates 'MyRemoteFunctionsInterfaceRemoteProxy' class in 'MyRemoteFunctionsInterfaceRemoteProxy.g.cs'
public interface IMyRemoteFunctionsInterface {
	Task MyFunction();
}
// Or
[GenerateProxy] // Generates partial 'MyRemoteFuctionClass' class in 'MyRemoteFuctionClass.g.cs'
public partial class MyRemoteFuctionClass : IMyRemoteFunctionsInterface {
}
```

> Warning: The source generator is still a prototype and has only a very rudimentary implementation.

The basis for serialization and deserialization is the `IObjectConverter` interface. The defualt implementation ist the `System.Text.Json` based implementations for this interface.
```csharp
IObjectConverter objectConverter = new JsonObjectConverter();
```

The library is developed mainly for ".net6.0" and newer but also supports ".netstandard2.0" and is designed for completely asynchronous IPC and RPC communication. Only the ".netstandard2.0" version has dependencies to other NuGet packages (The packages "System.Memory" and "System.Text.Json" are required to add some functionalities which were introduced in later .NET versions.).

> Warning: PlainlyIPC is still under development. 
>
> A version 1.0 will be released as soon as all planed features are implemented and properly tested. Therefore, there may be some breaking changes before the release of version 1.0.

NuGet: https://www.nuget.org/packages/Chriffizient.PlainlyIpc
