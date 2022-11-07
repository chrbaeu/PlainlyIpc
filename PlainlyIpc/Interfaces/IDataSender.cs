namespace PlainlyIpc.Interfaces;

/// <summary>
/// Interface for asynchronous sending of data.
/// </summary>
public interface IDataSender : IDisposable, IConnectionState
{

    /// <summary>
    /// Sends a byte array of data.
    /// </summary>
    /// <param name="data">The byte array of data.</param>
    /// <returns>Task to await the sending of the data.</returns>
    public Task SendAsync(byte[] data);

}
