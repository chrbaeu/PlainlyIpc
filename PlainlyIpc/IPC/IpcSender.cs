using System.IO;
using System.Threading;

namespace PlainlyIpc.IPC;

/// <summary>
/// IPC sender class.
/// </summary>
public sealed class IpcSender : IIpcSender
{
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly IDataSender dataSender;
    private readonly IObjectConverter objectConverter;

    /// <summary>
    /// Creates a new IPC sender with the given data sender and object converter.
    /// </summary>
    /// <param name="dataSender">The data sender.</param>
    /// <param name="objectConverter">The object converter.</param>
    public IpcSender(IDataSender dataSender, IObjectConverter objectConverter)
    {
        this.dataSender = dataSender;
        this.objectConverter = objectConverter;
    }

    /// <inheritdoc/>
    public async Task SendAsync(byte[] data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RawData);
            memoryStream.Write(data);
            await dataSender.SendAsync(memoryStream.ToArray()).ConfigureAwait(false); ;
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

    /// <inheritdoc/>
    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RawData);
            memoryStream.Write(data.Span);
            await dataSender.SendAsync(memoryStream.ToArray()).ConfigureAwait(false);
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

    /// <inheritdoc/>
    public async Task SendStringAsync(string data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.StringData);
            memoryStream.WriteUtf8String(data);
            await dataSender.SendAsync(memoryStream.ToArray()).ConfigureAwait(false);
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

    /// <inheritdoc/>
    public async Task SendObjectAsync<T>(T data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.ObjectData);
            memoryStream.WriteUtf8String(typeof(T).GetTypeString());
            memoryStream.WriteArray(objectConverter.Serialize(data));
            await dataSender.SendAsync(memoryStream.ToArray()).ConfigureAwait(false);
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

    /// <summary>
    /// Sends a remote message.
    /// </summary>
    /// <param name="data">The byte data of the remote message to send.</param>
    /// <returns>Task to await the sending operation.</returns>
    /// <exception cref="IpcException"></exception>
    public async Task SendRemoteMessageAsync(byte[] data)
    {
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);
        try
        {
            using MemoryStream memoryStream = new();
            memoryStream.WriteByte((byte)IpcMessageType.RemoteMessage);
            memoryStream.Write(data);
            await dataSender.SendAsync(memoryStream.ToArray()).ConfigureAwait(false);
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

    /// <inheritdoc/>
    public void Dispose()
    {
        dataSender.Dispose();
        GC.SuppressFinalize(this);
    }

}
