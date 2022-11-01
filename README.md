# PlainlyIpc

A minimalistic, easy-to-use library for inter-process communication (IPC) with .NET.

> Warning
>
> PlainlyIPC is still under development. 
> * A version 1.0 will be released as soon as the remaining features are implemented and properly tested. Therefore, there may be some breaking changes before the release of version 1.0.
> * PlainlyIpc will support IPC over named pipes and TCP. However, TCP support is still under development.

The basis for serialization and deserialization is the IObjectConverter interface. PlanilyIPC provides three different implementations for this interface.
```csharp
IObjectConverter objectConverter;
objectConverter = new BinaryObjectConverter();
objectConverter = new XmlObjectConverter();
objectConverter = new JsonObjectConverter(); // only for net6.0 and newer
```

The basic one-way IPC communication is realized via the IIpcSender and IIpcReceiver interfaces. The IIpcHandler interface relies on these interfaces and enables bidirectional communication and Remote Procedure Calls (RPC).
The IpcFactory class allows the easy creation of corresponding instances for these interfaces.
```csharp
IpcFactory ipcFactory = new IpcFactory(objectConverter);
```

IIpcSender & IIpcReceiver
```csharp
IIpcReceiver receiver = await ipcFactory.CreateNampedPipeIpcReceiver(namedPipeName)
IIpcSender sender = await ipcFactory.CreateNampedPipeIpcSender(namedPipeName)
```

IpcHandler
```csharp
IIpcHandler server = await ipcFactory.CreateNampedPipeIpcServer(namedPipeName)
IIpcHandler client = await ipcFactory.CreateNampedPipeIpcClient(namedPipeName)
```

The IIpcSender offers the following methods:
```csharp
Task SendAsync(byte[] data);
Task SendAsync(ReadOnlyMemory<byte> data);
Task SendStringAsync(string data);
Task SendObjectAsync<T>(T data);
```

The IIpcReceiver offers the following events:
```csharp
event EventHandler<IpcMessageReceivedEventArgs>? MessageReceived;
event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
```

The IpcHandler offers the features of IIpcSender and IIpcReceiver as well as the following RPC capabilities:
```csharp
void RegisterService(Type type, object service);
void RegisterService<TIService>(TIService service) where TIService : notnull;
Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, Task<TResult>>> func);
Task<TResult> ExecuteRemote<TIRemnoteService, TResult>(Expression<Func<TIRemnoteService, TResult>> func);
Task ExecuteRemote<TIRemnoteService>(Expression<Action<TIRemnoteService>> func);
```

Minimal example:
```csharp
IObjectConverter objectConverter = new BinaryObjectConverter();
IpcFactory ipcFactory = new IpcFactory(objectConverter);

using IIpcHandler server = await ipcFactory.CreateNampedPipeIpcServer("MyNamedPipe");
server.MessageReceived += (sender, e) =>
{
	Console.WriteLine(e.Value);
};

using IIpcHandler client = await ipcFactory.CreateNampedPipeIpcClient("MyNamedPipe");
await client.SendStringAsync("Hello World!");
```
Additional usage examples can be found in the sample project "PlainlyIpcChatDemo" and the tests in the "PlainlyIpcTests" project.

NuGet: https://www.nuget.org/packages/Chriffizient.PlainlyIpc
