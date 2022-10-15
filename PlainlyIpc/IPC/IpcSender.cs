using PlainlyIpc.Enums;
using PlainlyIpc.Exceptions;
using PlainlyIpc.Interfaces;
using PlainlyIpc.Internal;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PlainlyIpc.IPC;

public class IpcSender : IIpcSender
{
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly IDataSender dataSender;
    private readonly IObjectConverter objectConverter;

    public IpcSender(IDataSender dataSender, IObjectConverter objectConverter)
    {
        this.dataSender = dataSender;
        this.objectConverter = objectConverter;
    }

    public async Task SendAsync(byte[] data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RawData);
            memoryStream.Write(data);
            await dataSender.SendAsync(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new IpcException("An error occurred while sending data.", e);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RawData);
            memoryStream.Write(data.Span);
            await dataSender.SendAsync(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new IpcException("An error occurred while sending data.", e);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task SendStringAsync(string data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.StringData);
            memoryStream.WriteUtf8String(data);
            await dataSender.SendAsync(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new IpcException("An error occurred while sending data.", e);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task SendObjectAsync<T>(T data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.ObjectData);
            memoryStream.WriteUtf8String(typeof(T).GetTypeString());
            memoryStream.WriteArray(objectConverter.Serialize(data));
            await dataSender.SendAsync(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new IpcException("An error occurred while sending data.", e);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

    public async Task SendRemoteMessageAsync(byte[] data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RemoteMessage);
            memoryStream.Write(data);
            await dataSender.SendAsync(memoryStream.ToArray());
        }
        catch (Exception e)
        {
            throw new IpcException("An error occurred while sending data.", e);
        }
        finally
        {
            semaphoreSlim.Release();
        }
    }

}
