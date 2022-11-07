namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for IPC sender implementations.
/// </summary>
public interface IIpcSender : IDisposable, IConnectionState
{

    /// <summary>
    /// Sends a byte array.
    /// </summary>
    /// <param name="data">The byte array to send.</param>
    /// <returns>Task to await the sending operation.</returns>
    /// <exception cref="IpcException"></exception>
    public Task SendAsync(byte[] data);

    /// <summary>
    /// Sends memory of bytes.
    /// </summary>
    /// <param name="data">The memory of bytes to send.</param>
    /// <returns>Task to await the sending operation.</returns>
    /// <exception cref="IpcException"></exception>
    public Task SendAsync(ReadOnlyMemory<byte> data);

    /// <summary>
    /// Sends a string.
    /// </summary>
    /// <param name="data">The string to send.</param>
    /// <returns>Task to await the sending operation.</returns>
    /// <exception cref="IpcException"></exception>
    public Task SendStringAsync(string data);

    /// <summary>
    /// Sends a object.
    /// </summary>
    /// <param name="data">The object to send.</param>
    /// <returns>Task to await the sending operation.</returns>
    /// <exception cref="IpcException"></exception>
    public Task SendObjectAsync<T>(T data);

}
